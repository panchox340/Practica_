using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication.Controllers
{
    public class ErrorController : _Controller
    {
        // GET: Error
        public ActionResult Error(string id, int http_code)
        {
            ViewBag.NeedLayaout = "N";
            ViewBag.codeError = id;
            ViewBag.http_code = http_code;
            return View("Error");
        }
    }
}