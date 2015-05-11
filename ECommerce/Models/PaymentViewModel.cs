using ECommerce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECommerce.Models
{
    public class PaymentViewModel
    {
        public RechargeModel recharge { get; set; }
        public EWallet eWallet { get; set; }
    }
}
