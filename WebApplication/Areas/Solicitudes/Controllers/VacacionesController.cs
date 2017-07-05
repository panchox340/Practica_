using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication.App_Start;

namespace WebApplication.Areas.Solicitudes.Controllers
{
    public class VacacionesController : MasterController
    {
        // GET: Solicitudes/Vacaciones
        public ActionResult Index()
        {
            
            var user = SesionLogin().id_usu;
            var novedades = LoadData("select vac.id_vac, Nom_usu, vac.fecha_ini, vac.fecha_fin, cant_dias, nov.fecha_novedad, vac.estado_aprov " +
                                    "from (Novedad nov join Vacaciones vac on nov.id_novedad = vac.id_novedad) " +
                                    "join Usuario u on u.id_usu = nov.id_usu "+
                                     "where vac.id_usu = '" + user + "' and nov.estado != 0 ");
            return View(novedades);
        }

        public ActionResult Create()
        {
            var user = SesionLogin().id_usu;
            ViewBag.Empresa = (from a in _db.Cliente
                               join b in _db.Usuario on user equals b.id_usu
                               where a.id_cliente == b.id_cliente
                               select new { a.Nom_cor_emp }).Single().Nom_cor_emp;

            ViewBag.Rut = (from a in _db.Ingresos where a.id_usu == user select new { a.rut }).First().rut;

            var cantdias = LoadData("select(((DATEDIFF(DAYOFYEAR, FecCalVac, getdate()) / 30) * 1.25) - sum(NDiasAp)) as total " +
                                    "from([PSURSOFTSQL].[PSO].softland.sw_personal p join[PSURSOFTSQL].[PSO].softland.sw_vacsolic v " +
                                    "on p.ficha = v.Ficha) " +
                                    "where p.ficha = '" + user + "' " +
                                    "group by FecCalVac").First()["total"];
           ViewBag.dias = cantdias;

            return View("form");
        }

        // POST: Ingresos/Ingresos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(WebApplicationModel.Vacaciones model, string reservation)
        {
            DateTime fecha_ini = new DateTime();
            DateTime fecha_fin = new DateTime();
            if (!DateTime.TryParse(reservation.Substring(0, 10), out fecha_ini) || !DateTime.TryParse(reservation.Substring(13, 10), out fecha_fin))
            {
                fecha_ini = Convert.ToDateTime(reservation.Substring(0, 10), System.Globalization.CultureInfo.GetCultureInfo("en-Us").DateTimeFormat);
                fecha_fin = Convert.ToDateTime(reservation.Substring(13, 10), System.Globalization.CultureInfo.GetCultureInfo("en-Us").DateTimeFormat);
            }

            foreach (var key in ModelState.Keys)
            {
                ModelState[key].Errors.Clear();
            }

            if (ModelState.IsValid)
            {
                WebApplicationModel.Novedad novedad = new WebApplicationModel.Novedad();
                novedad.id_usu = Helper.converRut(SesionLogin().id_usu);
                novedad.id_cliente = SesionLogin().id_cliente;
                novedad.fecha_mod = DateTime.Now;
                novedad.estado = "1";
                novedad.tipo_nov = 13;
                novedad.fecha_novedad = DateTime.Now;
                novedad.usuario_mod = SesionLogin().id_usu;
                model.id_usu = novedad.id_usu;
                model.Novedad = novedad;
                model.fecha_ini = fecha_ini;
                model.fecha_fin = fecha_fin;
                model.estado_aprov = -1;
                try
                {
                    model.id_novedad = novedad.id_novedad;
                    _db.Vacaciones.Add(model);
                    _db.SaveChanges();
                }
                catch (Exception err)
                {
                    if ((err.InnerException.InnerException).GetType().ToString().Equals("System.Data.SqlClient.SqlException") && ((SqlException)(err.InnerException.InnerException)).ErrorCode == -2146232060)
                    {
                        return JsonError("Ya existe un registro con estos datos");
                    }
                    //captura cualquier otra excepcion
                    return JsonError("Opps, ocurrio un problema");
                }
            }
            return JsonExito();

        }

        // GET: Ingresos/Ingresos/Edit/5
        public ActionResult Edit(int id)
        {
            var model = _db.Vacaciones.FirstOrDefault(item => item.id_vac == id);
            var id_nov = model.id_novedad;
            ViewBag.novedad = _db.Novedad.FirstOrDefault(p => p.id_novedad == id_nov);
            var user = SesionLogin().id_usu;
            ViewBag.Empresa = (from a in _db.Cliente
                               join b in _db.Usuario on user equals b.id_usu
                               where a.id_cliente == b.id_cliente
                               select new { a.Nom_cor_emp }).Single().Nom_cor_emp;

            ViewBag.Rut = (from a in _db.Ingresos where a.id_usu == user select new { a.rut }).First().rut;

            var cantdias = LoadData("select(((DATEDIFF(DAYOFYEAR, FecCalVac, getdate()) / 30) * 1.25) - sum(NDiasAp)) as total " +
                                    "from([PSURSOFTSQL].[PSO].softland.sw_personal p join[PSURSOFTSQL].[PSO].softland.sw_vacsolic v " +
                                    "on p.ficha = v.Ficha) " +
                                    "where p.ficha = '" + user + "' " +
                                    "group by FecCalVac").First()["total"];
            ViewBag.dias = cantdias;
            if (model == null) return RedirectToAction("Create");
            return View("form", model);
        }

        // POST: Ingresos/Ingresos/Edit/5
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(WebApplicationModel.Vacaciones model, string reservation)
        {
            DateTime fecha_ini = new DateTime();
            DateTime fecha_fin = new DateTime();
            if (!DateTime.TryParse(reservation.Substring(0, 10), out fecha_ini) || !DateTime.TryParse(reservation.Substring(13, 10), out fecha_fin))
            {
                fecha_ini = Convert.ToDateTime(reservation.Substring(0, 10), System.Globalization.CultureInfo.GetCultureInfo("en-Us").DateTimeFormat);
                fecha_fin = Convert.ToDateTime(reservation.Substring(13, 10), System.Globalization.CultureInfo.GetCultureInfo("en-Us").DateTimeFormat);
            }

            foreach (var key in ModelState.Keys)
            {
                ModelState[key].Errors.Clear();
            }

            if (ModelState.IsValid)
            {
                WebApplicationModel.Novedad novedad = new WebApplicationModel.Novedad();
                novedad.id_usu = Helper.converRut(SesionLogin().id_usu);
                novedad.usuario_mod = SesionLogin().id_usu;
                novedad.id_cliente = SesionLogin().id_cliente;
                novedad.fecha_mod = DateTime.Now;
                novedad.id_novedad = model.id_novedad;
                novedad.estado = "1";
                novedad.tipo_nov = 13;
                novedad.fecha_novedad = DateTime.Now;

                model.id_usu = novedad.id_usu;
                model.Novedad = novedad;
                model.fecha_ini = fecha_ini;
                model.fecha_fin = fecha_fin;
                model.estado_aprov = -1;
                try
                {
                    model.id_novedad = novedad.id_novedad;
                    _db.Vacaciones.Attach(model);
                    _db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();

                    return JsonExito();
                }
                catch (Exception e)
                {
                    return JsonError("ocurrio un problema con su solicitud");
                }
            }
            return JsonError("ocurrio un problema con su solicitud");
        }

        // GET: Ingresos/Ingresos/Delete/5
        public ActionResult Delete(int id)
        {
            var model = _db.Vacaciones.FirstOrDefault(item => item.id_vac == id);
            var id_nov = model.id_novedad;
            var novedad = _db.Novedad.FirstOrDefault(c => c.id_novedad == id_nov);
            if (model == null) return JsonError("No existe el registro seleccionado");
            try
            {
                model.estado_aprov = -1;
                novedad.estado = "0";
                _db.SaveChanges();
                return JsonExito();
            }
            catch (Exception e)
            {
                return JsonError("ocurrio un problema con su solicitud");
            }
        }

        public ActionResult aprobacion(string id)
        {
            int id_vacas = Int32.Parse(id);
            var model = _db.Vacaciones.FirstOrDefault(item => item.id_vac == id_vacas);
            model.estado_aprov = 1;
            try
            {
                _db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
                return JsonExito();
            }
            catch (Exception e)
            {
                return JsonError("oops, hubo un problema con su solicitud");
            }
        }


        // TODO
        public ActionResult documentos(string id)
        {
            try
            {
                int id_reg = Int32.Parse(id);
                ViewBag.documentos = _db.Documento.FirstOrDefault(item => item.id_novedad == id_reg);
                return View("documentos");

            } catch (Exception e)
            {
                return JsonError("no se encontró el registro con esa id");
            }
            
        }
    }
}