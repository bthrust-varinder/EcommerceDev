using ECommerce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECommerce.Controllers
{
    public class ProductsController : Controller
    {
        //
        // GET: /Products/

        public ActionResult Index()
        {
            return View();
        }

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

    }
}
