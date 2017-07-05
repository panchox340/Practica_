using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplicationModel;

namespace WebApplication.Areas.Remuneraciones.Controllers
{
    public class LibroRemuController : MasterController
    {
        // GET: Remuneraciones/LibroRemu
        public ActionResult Index()
        {
            ViewBag.ClientesByUsuario = new SelectList(getClientesByUsuarioHerencia("10925738"), "id_cliente", "nom_emp");
            return View();
        }


        public ActionResult Create(int? id)
        {
            if (id == null) return JsonError("");
            ViewBag.id_emp = id;
            return View("Form");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Create(Remuneracion model, HttpPostedFileBase documento, int id_emp)
        {
            try
            {
                Novedad novedad = new Novedad();
                novedad.fecha_mod = DateTime.Now;
                novedad.fecha_novedad = DateTime.Now;
                novedad.id_cliente = id_emp;
                novedad.id_usu = SesionLogin().id_usu;
                novedad.tipo_nov = 15;
                novedad.usuario_mod = SesionLogin().Nom_usu;
                novedad.estado = "P";
                model.Novedad = novedad;
                _db.Remuneracion.Add(model);
                _db.SaveChanges();
                helper.createFile(documento, id_emp, (int)model.id_novedad, 15, true, SesionCliente(),"");
                return JsonExito();
            }
            catch(Exception ex)
            {
                MvcApplication.LogError(ex);
                return JsonError("");
            }
            
        }


        public ActionResult Edit(string id)
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Edit(Remuneracion model)
        {
            return JsonExito();
        }



        public List<Cliente> getClientesByUsuarioHerencia (string id_usu)
        {
            return _db.Database.SqlQuery<Cliente>("select distinct c.* from fn_subordinadosPorUsuario(@param1,-1) tb1 left join Usuario_Cliente uc on tb1.id_usu = uc.id_usu left join Cliente c on uc.id_cliente = c.id_cliente", new SqlParameter("param1", id_usu)).ToList();
        }


        public JsonResult getLibroByCliente(string id)
        {
            int id_;
            if(Int32.TryParse(id, out id_))
            {
                var list = (from r in _db.Remuneracion
                            join n in _db.Novedad on r.id_novedad equals n.id_novedad
                            where n.id_cliente == id_
                            select new {n.id_usu, n.id_novedad});
                return Json(list);
            }
            return Json("");
        }
    }
}