using ECommerce.Entity;
using ECommerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ECommerce.Controllers
{
    public class AdminController : Controller
    {
        EloadEntities entities = new EloadEntities();
        UserRepository repo = new UserRepository();
        //
        // GET: /Admin/

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult PartialLogin(string returnUrl)
        {
            if (Session["Admin"] != null)
            {
                ViewBag.LoggedIn = true;
                ViewBag.Name = ((TB_User)Session["Admin"]).UserName;
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
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            TB_User user = repo.LoginAdmin(model.UserName, model.Password);
            if (user != null)
            {
                FormsAuthentication.SetAuthCookie(model.UserName, false);
                Session["Admin"] = user;

                if (user.Active == "Admin")
                {
                    return RedirectToAction("Products", "Admin");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            //if (ModelState.IsValid && WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
            //{
            //    return RedirectToLocal(returnUrl);
            //}

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "The user name or password provided is incorrect.");
            return View("Index", model);
        }


        public ActionResult EditPrpduct(int id)
        {
            TB_Product prd = entities.TB_Product.Where(x => x.TB_ProductID == id).FirstOrDefault();
            return View(prd);

        }
        public ActionResult AddProduct()
        {
            TB_Product prd = new TB_Product();
            prd.TB_ProductID = -1;
            List<TB_Operator> Operators = entities.TB_Operator.ToList<TB_Operator>();
            ViewBag.OperatorsList = new SelectList(Operators, "OperatorId", "OperatorName");

            return View("AddProduct", prd);

        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddProduct(TB_Product model,FormCollection formsq)
        {
            TB_Product prd = new TB_Product();
            prd.Product_Name = model.Product_Name;
            prd.Product_Price = model.Product_Price;
            prd.OperatorId = model.OperatorId;
            prd.ProductId = model.ProductId;
            prd.IsActive = "true";
            entities.TB_Product.Add(prd);
            entities.SaveChanges();
            return RedirectToAction("Products");

        }

        [HttpPost]
        public ActionResult EditPrpduct(TB_Product model)
        {
            TB_Product prd = entities.TB_Product.Where(x => x.TB_ProductID == model.TB_ProductID).FirstOrDefault();
            prd.Product_Name = model.Product_Name;
            prd.Product_Price = model.Product_Price;
            prd.ProductId = model.ProductId;
            prd.IsActive = model.IsActive;
            entities.SaveChanges();

            return RedirectToAction("Products");
        }

        public ActionResult Products()
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Index", "Admin");
            }

            List<TB_Product> products = entities.TB_Product.ToList();

            return View(products);
        }

        public ActionResult History()
        {
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Index", "Admin");
            }

            TB_User user = (TB_User)Session["Admin"];
            List<TB_Transaction> transactions = entities.TB_Transaction.Where(x => x.UserId == user.UserId).ToList();
            return View(transactions);
        }

    }
}
