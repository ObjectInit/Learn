﻿using System;
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
            //Session["u"] = "ly";
            OtherController other = new OtherController();
            return other.Index();
        }

        public ActionResult MySession()
        {
            //ViewBag.Session = Session["u"]; 
            return View();
        }
    }
}