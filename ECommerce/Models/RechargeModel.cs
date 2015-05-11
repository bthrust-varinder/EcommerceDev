using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ECommerce.Models
{
    public class RechargeModel
    {
        [RegularExpression(@"^\d+$", ErrorMessage = "Please enter proper contact details.")]
        [Required(ErrorMessage="Please enter phone number !")]
        public string phone { get; set; }

        [Compare("phone",ErrorMessage="Phone Numbers do not match !")]
        public string confirmPhone { get; set; }

        [Required]
        public string Operators { get; set; }

        [Required(ErrorMessage = "Please enter recharge amount !")]
        [Range(0, double.MaxValue, ErrorMessage = "Please enter valid amount !")]
        public string amount { get; set; }

        public string ProductId { get; set; }

        public int TxnId { get; set; }

        public string Operator { get; set; }
    }
}