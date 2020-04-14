using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Lear.Mvc.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            Session["u"] = "ly";
            return View();

        }

        public ActionResult MySession()
        {
            Session.Clear(); 
            return View();
        }
        
        [ValidateInput(false)]
        public ActionResult Do1(string name, int age)
        {
            Session["D"] = 1;
            return null;
        }

        public ActionResult Do2()
        {
            var t = Session["D"];
            return null;
        }
    }
}