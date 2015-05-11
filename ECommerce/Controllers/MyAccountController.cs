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
    public class MyAccountController : Controller
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
        //
        // GET: /MyAccount/

        public ActionResult Index()
        {
            if (Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        public ActionResult RedirectFromPaypal(int id)
        {
            TB_Paypal p = entities.TB_Paypal.Where(x => x.PaypalId == id).FirstOrDefault();
            EWallet wa = entities.EWallets.Where(x => x.EWalletID == p.EWalletID).FirstOrDefault();
            p.TransactionID = 1;
            wa.Amount = wa.Amount + p.Amount;
            entities.SaveChanges();

            ViewBag.Message = "Recharge Succesful !";
            ViewBag.MessageDesc = "";

            return View();
        }
        public ActionResult CancelFromPaypal()
        {
            return View();
        }
        public ActionResult NotifyFromPaypal(int id)
        {
            //TB_Transaction tx = entities.TB_Transaction.Where(x => x.TransactionId == id).FirstOrDefault();
            //if (tx != null)
            //{
            //    tx.PaymentStatus = "Payment Recieved";
            //    entities.SaveChanges();
            //    repo.UpdateWallet(tx.UserId ?? 0, Convert.ToDecimal(tx.Denomination));
            //}
            return View();
        }

        public ActionResult RechargeWallet()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RechargeWallet(FormCollection forms)
        {
            TB_User user = (TB_User)Session["user"];
            string txn = forms["txn"];
            string number = Guid.NewGuid().ToString().Substring(0, 6);
            bool useSandbox = Convert.ToBoolean(ConfigurationManager.AppSettings["IsSandbox"]);
            TB_Paypal paypal = new TB_Paypal();
            paypal.Amount = Convert.ToDecimal(txn);
            paypal.date = DateTime.Now;
            paypal.EWalletID = user.EWallets.FirstOrDefault().EWalletID;
            paypal.Type = "Wallet Recharge";
            paypal.UserID = user.UserId;
            entities.TB_Paypal.Add(paypal);
            entities.SaveChanges();
            var pay = new PayPalModel(useSandbox, paypal.PaypalId, true,Convert.ToInt32(txn));
            pay.item_name = "Wallet Recharge";
            pay.amount = txn;
            return View("RechargeStatusSubmit",pay);
        }
        public ActionResult EWallet()
        {
            if (Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            TB_User user = (TB_User)Session["user"];
            EWallet wallet = entities.EWallets.Where(x => x.UserID == user.UserId).FirstOrDefault();
            return View(wallet);
        }

        public ActionResult History()
        {
            if (Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            TB_User user = (TB_User)Session["user"];
            List<TB_Transaction> transactions = entities.TB_Transaction.Where(x => x.UserId == user.UserId).ToList();
            return View(transactions);
        }

    }
}
