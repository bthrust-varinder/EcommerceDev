using ECommerce.Entity;
using ECommerce.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECommerce.Controllers
{
    public class PayPalController : Controller
    {
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
        UserRepository repo = new UserRepository();
        public ActionResult RedirectFromPaypal(int id)
        {
            ServiceReference1.ClientAPIServiceSoapClient s = new ServiceReference1.ClientAPIServiceSoapClient();
            string clientId = ConfigurationManager.AppSettings["ClientId"];
            string clientPwd = ConfigurationManager.AppSettings["ClientPwd"];
            string responseid = "";
            string status = "";
            string balances = "";
            TB_Transaction tx = entities.TB_Transaction.Where(x => x.TransactionId == id).FirstOrDefault();
            bool result = s.RequestInput(clientId, clientPwd, tx.TransactionNumber, tx.ProductId.ToString(), Convert.ToDecimal(tx.Denomination), tx.Phone, ref responseid, ref status, ref balances);

            RechargeModel model = new RechargeModel();
            model.phone = tx.Phone;
            model.amount = tx.Denomination;
            model.Operator = entities.TB_Product.Where(x => x.ProductId == tx.ProductId).FirstOrDefault().Product_Name;
            tx.Status = status;
            tx.ResponseId = responseid;
            repo.updateTrasnaction(tx);

            if (status == "SUBMIT_SUCCESS")
            {
                repo.DeductMoneyFromWallet(tx.UserId ?? 0, Convert.ToDecimal(tx.Denomination));
                ViewBag.Message = "Recharge Succesful ! Transaction Number is :" + tx.TransactionNumber;
                ViewBag.MessageDesc = "";
            }
            else
            {
                ViewBag.Message = "Recharge was UnSuccesful ! Transaction Number is :" + tx.TransactionNumber + "";
                ViewBag.MessageDesc = "Some issue occured in the backend. We haven't deducted money from your wallet. Please try again later. Please note down transaction number for refund process.";
            }
            return View(model);
        }
        EloadEntities entities = new EloadEntities();
        public ActionResult CancelFromPaypal()
        {
            return View();
        }
        public ActionResult NotifyFromPaypal(int id)
        {
            TB_Transaction tx = entities.TB_Transaction.Where(x => x.TransactionId == id).FirstOrDefault();
            if (tx != null)
            {
                tx.PaymentStatus = "Payment Recieved";
                entities.SaveChanges();
                repo.UpdateWallet(tx.UserId ?? 0, Convert.ToDecimal(tx.Denomination));
            }
            return View();
        }
        
        public ActionResult ValidateCommand()
        {

            string url = ConfigurationManager.AppSettings["redirect"];
            string email = ConfigurationManager.AppSettings["business"];
            string returnurl = ConfigurationManager.AppSettings["return"];
            string cancel_return = ConfigurationManager.AppSettings["cancel_return"];
            Models.HttpRequest hr = new Models.HttpRequest();

            


            RechargeModel model = (RechargeModel)Session["recharge"];
            string number = Guid.NewGuid().ToString().Substring(0, 6);
            TB_User user = (TB_User)Session["user"];
            TB_Transaction tx = null;
            if (user != null)
            {

                tx = repo.createTransaction(number, model.amount, user.UserId, Convert.ToInt32(model.ProductId), model.phone);
            }
            else
            {
                tx = repo.createTransaction(number, model.amount, null, Convert.ToInt32(model.ProductId), model.phone);
            }

            //Dictionary<string, string> post = new Dictionary<string, string>();
            //post.Add("version", "2.0");
            //post.Add("action", "pay");
            //post.Add("merchant", email);
            //post.Add("ref_id", tx.TransactionId.ToString());
            //post.Add("item_name_1", model.phone+ " Mobile Recharge" );
            //post.Add("item_description_1", "Mobile Recharge");
            //post.Add("item_quantity_1", "1");
            //post.Add("item_amount_1", model.amount);
            //post.Add("currency", "SGD");
            //post.Add("total_amount", model.amount);
            //post.Add("success_url", returnurl);
            //post.Add("cancel_url", cancel_return);

            //hr.HttpPostRequest(url, post);

            bool useSandbox = Convert.ToBoolean(ConfigurationManager.AppSettings["IsSandbox"]);
            var paypal = new PayPalModel(useSandbox, tx.TransactionId,false, Convert.ToInt32( model.amount));
            paypal.item_name = model.phone;
            paypal.amount = model.amount;
            return View(paypal);

        }
    }
}
