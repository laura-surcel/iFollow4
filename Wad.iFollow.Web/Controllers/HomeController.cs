using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wad.iFollow.Web.Models;
using WebMatrix.WebData;

namespace Wad.iFollow.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string message = " ")
        {
            ViewBag.Message = message;
            return View();
        }
    }
}
