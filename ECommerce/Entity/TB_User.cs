//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ECommerce.Entity
{
    using System;
    using System.Collections.Generic;
    
    public partial class TB_User
    {
        public TB_User()
        {
            this.TB_Transaction = new HashSet<TB_Transaction>();
            this.EWallets = new HashSet<EWallet>();
        }
    
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Active { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
    
        public virtual ICollection<TB_Transaction> TB_Transaction { get; set; }
        public virtual ICollection<EWallet> EWallets { get; set; }
    }
}
