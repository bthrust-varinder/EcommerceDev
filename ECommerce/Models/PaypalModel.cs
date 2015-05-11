using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ECommerce.Models
{

    public class PayPalModel
    {
        public string cmd { get; set; }
        public string business { get; set; }
        public string no_shipping { get; set; }
        public string successurl { get; set; }
        public string cancel_return { get; set; }
        public string notify_url { get; set; }
        public string currency_code { get; set; }
        public string item_name { get; set; }
        public string amount { get; set; }
        public string actionURL { get; set; }
        public string signature{get;set;}
        public int refid { get; set; }
        public PayPalModel(bool useSandbox,int id,bool wallet=false,int amount=0)
        {
            this.cmd = "_xclick";
            this.refid = id;
            this.business = ConfigurationManager.AppSettings["business"];
            string secret = ConfigurationManager.AppSettings["secret"];

            string dataToBeHashed = secret
                + this.business
                + "pay"
                + id.ToString()
                + amount.ToString()
                + ConfigurationManager.AppSettings["currency_code"];

            var sha1 = new SHA1CryptoServiceProvider();
            var passwordBytes = Encoding.UTF8.GetBytes(dataToBeHashed);
            var passwordHash = sha1.ComputeHash(passwordBytes);
            this.signature = BitConverter.ToString(passwordHash).Replace("-", string.Empty).ToLowerInvariant();

            if (wallet)
            {
                
                this.cancel_return = ConfigurationManager.AppSettings["walletcancel_return"];
                this.successurl = ConfigurationManager.AppSettings["walletreturn"] + "/" + id;
                this.notify_url = ConfigurationManager.AppSettings["walletnotify_url"] + "/" + id;
            }
            else
            {
                this.cancel_return = ConfigurationManager.AppSettings["cancel_return"];
                this.successurl = ConfigurationManager.AppSettings["return"] +"/" + id;
                this.notify_url = ConfigurationManager.AppSettings["notify_url"] +"/" + id;
            }
            if (useSandbox)
            {
                this.actionURL = ConfigurationManager.AppSettings["test_url"];
            }
            else
            {
                this.actionURL = ConfigurationManager.AppSettings["Prod_url"];
            }
            // We can add parameters here, for example OrderId, CustomerId, etc….
            
            // We can add parameters here, for example OrderId, CustomerId, etc….
            this.currency_code = ConfigurationManager.AppSettings["currency_code"];
        }
    }

}