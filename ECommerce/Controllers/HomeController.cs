using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ECommerce.Entity;
using ECommerce.Models;
using System.Configuration;
using System.IO;
using HRMGMTEF.Services.PDFGenerator;
namespace ECommerce.Controllers
{
    public class HomeController : Controller
    {
        UserRepository repo = new UserRepository();
        EloadEntities entities = new EloadEntities();

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult PartialLogin(string returnUrl)
        {
            if (Session["user"] != null)
            {
                ViewBag.LoggedIn = true;
                ViewBag.Name = ((TB_User)Session["user"]).UserName;
            }
            else
            {
                ViewBag.LoggedIn = false;
            }
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_LoginPartial");
        }

        public ActionResult Index()
        {
            RechargeModel model = new RechargeModel();
            List<TB_Operator> Operators = entities.TB_Operator.ToList<TB_Operator>();
            ViewBag.OperatorsList = new SelectList(Operators, "OperatorId", "OperatorName");

            return View(model);
        }
        [HttpPost]
        public ActionResult Index(FormCollection forms)
        {
            string phone = forms["phone"];
            string product = forms["Operators"];
            string amount = forms["amount"];
            string ProductId = forms["ProductId"];



            if (phone == "" || amount == "" || product == "")
            {
                List<TB_Operator> Operators = entities.TB_Operator.ToList<TB_Operator>();
                ViewBag.OperatorsList = new SelectList(Operators, "OperatorId", "OperatorName");
                ViewBag.Error = "Please enter required details !";
                return View();
            }
            else
            {
                RechargeModel model = new RechargeModel();
                model.phone = phone;
                model.amount = amount;
                model.Operators = product;
                model.ProductId = ProductId;
                int prdId = Convert.ToInt32(ProductId);
                model.Operator = entities.TB_Product.Where(x => x.ProductId == prdId).FirstOrDefault().Product_Name;
                Session["recharge"] = model;
                return RedirectToAction("Checkout");
            }

        }
        public ActionResult RechargeStatus()
        {
            return View();
        }
        [HttpPost]
        public ActionResult RechargeStatus(FormCollection forms)
        {
            string txn = forms["txn"];
            ServiceReference1.ClientAPIServiceSoapClient s = new ServiceReference1.ClientAPIServiceSoapClient();
            string clientId = ConfigurationManager.AppSettings["ClientId"];
            string clientPwd = ConfigurationManager.AppSettings["ClientPwd"];
            string responseid = "";
            string resstatus = "";
            string txnstatus = "";
            string errorcode = "";
            string SMTMsg = "";

            TB_Transaction tx = entities.TB_Transaction.Where(x => x.TransactionNumber == txn).FirstOrDefault();

            if (tx != null)
            {
                RechargeModel model = new RechargeModel();
                model.phone = tx.Phone;
                model.amount = tx.Denomination;
                int prdId = Convert.ToInt32(tx.ProductId);
                model.Operator = entities.TB_Product.Where(x => x.ProductId == prdId).FirstOrDefault().Product_Name;

                if (tx.Status != null)
                {
                    if (txnstatus == "SUBMIT_SUCCESS")
                    {
                        ViewBag.Message = "Recharge was successful";
                    }
                    else
                    {
                        ViewBag.Message = "Recharge was un-successful";
                    }
                }
                else
                {
                    s.CheckTransactionStatus(clientId, clientPwd, txn, ref txnstatus, ref errorcode, ref SMTMsg, ref responseid, ref resstatus);
                    if (txnstatus == "SUBMIT_SUCCESS")
                    {
                        ViewBag.Message = "Recharge was successful";
                    }
                    else
                    {
                        ViewBag.Message = "Recharge was un-successful";
                    }
                }

                return View("RechargeStatusResult", model);
            }
            else
            {
                ViewBag.Message = "Invalid Transaction Number !";
                return View();
            }
        }


        [HttpGet]
        public JsonResult GetProducts(string id)
        {
            List<GetProductByOpId1_Result> prjs = repo.GetProductByOperatorId(Convert.ToInt32(id)).Where(x => x.IsActive == "true").ToList();

            return Json(prjs, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PayUsingWallet()
        {

            RechargeModel model = (RechargeModel)Session["recharge"];
            EWallet wallet = null;

            if (Session["user"] != null)
            {
                TB_User user = (TB_User)Session["user"];
                wallet = user.EWallets.FirstOrDefault();
                if (Convert.ToDecimal(model.amount) < wallet.Amount)
                {
                    ServiceReference1.ClientAPIServiceSoapClient s = new ServiceReference1.ClientAPIServiceSoapClient();
                    string clientId = ConfigurationManager.AppSettings["ClientId"];
                    string clientPwd = ConfigurationManager.AppSettings["ClientPwd"];
                    string responseid = "";
                    string status = "";
                    string balances = "";
                    string number = Guid.NewGuid().ToString().Substring(0, 6);
                    TB_Transaction tx = repo.createTransaction(number, model.amount, wallet.UserID ?? 0, Convert.ToInt32(model.ProductId), model.phone);
                    bool result = s.RequestInput(clientId, clientPwd, number, model.Operators, Convert.ToDecimal(model.amount), model.phone, ref responseid, ref status, ref balances);
                    tx.Status = status;
                    tx.ResponseId = responseid;
                    repo.updateTrasnaction(tx);

                    if (status == "SUBMIT_SUCCESS")
                    {
                        TB_Transaction txu = entities.TB_Transaction.Where(x => x.TransactionId == tx.TransactionId).FirstOrDefault();
                        txu.PaymentStatus = "Payment Recieved.";
                        entities.SaveChanges();
                        repo.DeductMoneyFromWallet(((TB_User)Session["user"]).UserId, Convert.ToDecimal(model.amount));
                        TB_Paypal paypal = new TB_Paypal();
                        paypal.Amount = Convert.ToDecimal(model.amount);
                        paypal.date = DateTime.Now;
                        paypal.EWalletID = wallet.EWalletID;
                        paypal.Type = "Mobile Recharge";
                        paypal.UserID = user.UserId;
                        paypal.TransactionID = txu.TransactionId;
                        entities.TB_Paypal.Add(paypal);
                        entities.SaveChanges();
                        ViewBag.Message = "Recharge Succesful !";
                    }
                    else
                    {
                        ViewBag.Message = "Recharge was UnSuccesful !";
                        ViewBag.MessageDesc = "Some issue occured in the backend. We haven't deducted money from your wallet. Please try again later";
                    }
                }
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Login(FormCollection forms)
        {
            TB_User user = repo.Login(forms["username"], forms["password"]);
            if (user != null)
            {
                Session["user"] = user;
                return RedirectToAction("MakePayment");
            }
            else
            {
                ViewBag.ErrorLogin = "Username/Password was incorrect !";
                return View("AskForLogin");
            }
        }

        [AllowAnonymous]
        public ActionResult AboutUs()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult ContactUs()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(FormCollection forms)
        {
            string email = forms["email"];
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Please enter email address";
                return View();
            }


            TB_User u = repo.GetUserEmail(email);
            if (u == null)
            {

                ViewBag.Error = "'" + email + "' Email address is not registered";
                return View();

            }

            string body = string.Empty;
            using (StreamReader reader = new StreamReader(System.Web.HttpContext.Current.Server.MapPath(@"~/App_Data/../Templates/ForgotPassword.html")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{Name}", u.UserName);
            body = body.Replace("{Password}", u.Password);


            MailSystem mail = new MailSystem();
            mail.SendMail(body, "Your Eload Details !!");

            ViewBag.message = "ad";

            ViewBag.Error = "Mail containing your user name and password has been sent !";

            return View();
        }





        [HttpPost]
        public ActionResult ContactUs(FormCollection forms)
        {
            string name = forms["name"];
            string email = forms["email"];
            string phone = forms["phone"];
            string subject = forms["subject"];
            string message = forms["message"];

            string body = string.Empty;
            using (StreamReader reader = new StreamReader(System.Web.HttpContext.Current.Server.MapPath(@"~/App_Data/../Templates/ContactUsMail.html")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{Name}", name);
            body = body.Replace("{Email}", email);



            body = body.Replace("{Phone}", phone);
            body = body.Replace("{Message}", message);

            MailSystem mail = new MailSystem();
            mail.SendMail(body, subject);

            ViewBag.message = "ad";

            return RedirectToAction("ContactUsThanks");

        }

        public ActionResult ContactUsThanks()
        {
            ViewBag.message = "Thanks you for contacting us! We will get back to you soon.";

            return View("ContactUsSubmitted");
        }

        [HttpPost]
        public ActionResult Register(FormCollection forms)
        {
            string username = forms["username"];
            string email = forms["email"];
            string password = forms["password"];
            string phone = forms["phone"];
            string repassword = forms["repassword"];

            TB_User conflict = entities.TB_User.Where(x => x.UserName == username).FirstOrDefault();
            if (conflict != null)
            {
                ViewBag.Message = "Username already taken !";
                return View("AskForLogin");
            }

            if (password != repassword)
            {

                ViewBag.Message = "Passwords do not match!";
                return View("AskForLogin");
            }

            TB_User u = new TB_User();
            u.UserName = username;
            u.Email = email;
            u.Password = password;
            u.MobileNumber = phone;
            u.Active = "true";
            entities.TB_User.Add(u);
            entities.SaveChanges();

            EWallet w = new EWallet();
            w.Amount = 0;
            w.Active = "true";
            w.UserID = u.UserId;
            entities.EWallets.Add(w);
            entities.SaveChanges();
            TB_User newuser = entities.TB_User.Where(x => x.UserId == u.UserId).FirstOrDefault();
            Session["user"] = newuser;
            return RedirectToAction("MakePayment");

        }





        public ActionResult AskForLogin()
        {
            if (Session["user"] != null)
            {
                return RedirectToAction("MakePayment");
            }
            return View();

        }

        public ActionResult Checkout()
        {
            RechargeModel model = (RechargeModel)Session["recharge"];
            if (model == null)
            {
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult MakePayment()
        {
            PaymentViewModel vm = new PaymentViewModel();

            RechargeModel model = (RechargeModel)Session["recharge"];

            EWallet wallet = null;
            if (model == null)
            {
                return RedirectToAction("Index");
            }
            vm.recharge = model;

            if (Session["user"] != null)
            {
                wallet = ((TB_User)Session["user"]).EWallets.FirstOrDefault();
                vm.eWallet = wallet;
            }
            ViewBag.rechargeModel = model;

            return View(vm);
        }



        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
