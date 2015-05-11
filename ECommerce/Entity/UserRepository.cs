using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Web;

namespace ECommerce.Entity
{
    public class UserRepository
    {
        EloadEntities entities = new EloadEntities();

        public TB_User GetUserEmail(string email)
        {
            return entities.TB_User.Where(x => x.Email == email).FirstOrDefault();
        }

        public TB_User Login(string userName, string password)
        {
            return entities.TB_User.Where(x => x.UserName == userName && x.Password == password && x.Active == "true").FirstOrDefault();
        }
        public TB_User LoginAdmin(string userName, string password)
        {
            return entities.TB_User.Where(x => x.UserName == userName && x.Password == password && x.Active == "Admin").FirstOrDefault();
        }
        public TB_Transaction createTransaction(string txnNumber, string amount, int? userid, int productid,string phone)
        {
            TB_Transaction tx = new TB_Transaction();
            tx.TransactionNumber = txnNumber;
            tx.Phone = phone;
            tx.UserId = userid;
            tx.Denomination = amount;
            tx.ProductId = productid;
            tx.date = DateTime.Now;
            entities.TB_Transaction.Add(tx);
            entities.SaveChanges();
            return tx;

        }

        public bool UpdateWallet(int user, decimal amount)
        {
            EWallet w= entities.EWallets.Where(x => x.UserID == user).FirstOrDefault();
            if (w != null)
            {
                w.Amount = w.Amount + amount;
                entities.SaveChanges();
                return true;
            }
            return false;
        }

        public void updateTrasnaction(TB_Transaction tx)
        {
            TB_Transaction old = entities.TB_Transaction.Where(x => x.TransactionId == tx.TransactionId).FirstOrDefault();
            old.Status = tx.Status;
            old.ResponseId = tx.ResponseId;
            entities.SaveChanges();
        }

        public bool DeductMoneyFromWallet(int userId, decimal amount)
        {
            EWallet old = entities.EWallets.Where(x => x.UserID == userId).FirstOrDefault();
            old.Amount = Convert.ToDecimal(old.Amount) - amount;
            entities.SaveChanges();
            return false;
        }

        public List<GetProductByOpId1_Result> GetProductByOperatorId(int id)
        {
            ObjectParameter orderidParameter;

            orderidParameter = new ObjectParameter("id", id);
            return entities.GetProductByOpId1(id).ToList();
            
        }
    }
}