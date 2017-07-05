using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplicationModel;

namespace WebApplication.Areas.Manage.Controllers
{
    public class ManageController : MasterController
    {
        // GET: Manage/Manage
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(Usuario model)
        {
            string usuario_id = SesionLogin().id_usu;
            var model_a_editar = (Usuario)_db.Usuario.FirstOrDefault(p => p.id_usu == usuario_id);
            model_a_editar.pass_usu = model.pass_usu;
            try
            {
                if (ModelState.IsValid)
                {
                    _db.Usuario.Attach(model_a_editar);
                    _db.Entry(model_a_editar).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();
                    return JsonExito();
                }
                return JsonError("Opps, ocurrio un problema");
            }
            catch (Exception e)
            {
                MvcApplication.LogError(e);
                return JsonError("Opps, ocurrio un problema");
            }

        }

    }
}