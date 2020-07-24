using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Lear.Mvc.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {

        }
        public int a = 1;
        // GET: Home
        public ActionResult Index()
        {
            TempData["a"] = 1;

            ViewData["a"] = 1;
            a = 2;
            string name = Request.QueryString["name"];
            string age = Request.QueryString["age"];
            ViewBag.UrlText = "acanalytype=z2&subunit=z0.总部&analysdate=2020-04-22&subrec=lwh001.lwh科目对账&summaryitem=account.科目&t0=&t10=&t11=&billtype=zsk&cd=0&account=lwh001.lwh对账收款单       &edate=2020-03-22:2020-04-20,2020-02-21:2020-03-21";
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
            var d = TempData["a"];
            return null;
        }

        public ActionResult NtDo(int i)
        {
            var t = new Random().Next(5000);
            Thread.Sleep(t);
            return Content(i.ToString());
        }

        public ActionResult AsynXTest()
        {
            return View();
        }
    }
}