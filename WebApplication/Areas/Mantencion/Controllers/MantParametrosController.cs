using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication.Areas.Mantencion.Controllers
{
    public class MantParametrosController : MasterController
    {
        // GET: Mantencion/MantParametros
        public ActionResult Index()
        {
            return View();
        }
    }
}