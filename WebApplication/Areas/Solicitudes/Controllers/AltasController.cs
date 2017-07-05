using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication.App_Start;

namespace WebApplication.Areas.Solicitudes.Controllers
{
    public class AltasController : MasterController
    {
        // GET: Solicitudes/Altas
        public ActionResult Index()
        {
            //var novedades = (from nov in _db.Novedad
            //                 join ing in _db.Ingresos
            //                 on nov.id_novedad equals ing.id_novedad
            //                 where nov.estado == "1"
            //                 select new { fecha_novedad = nov.fecha_novedad, nombre = (ing.nombres + " " + ing.apellido), rut = ing.rut }).ToList();

            var novedades = LoadData("select ing.estado_aprov, ing.id_ing, fecha_novedad, (nombres + ' ' + apellido) as nombre, rut, cargo from Novedad nov " +
                                    "join Ingresos ing " +
                                    "on nov.id_novedad = ing.id_novedad " +
                                    "where estado = '1' " +
                                    "order by ing.id_novedad asc");
            return View(novedades);
        }

        // GET: Ingresos/Ingresos/Create
        public ActionResult Create()
        {
            ViewBag.selectlistdocus = new SelectList(_db.parametros.Where(item => item.grupo == "DOCUM").OrderBy(item => item.valor), "valor", "detalle");// se seleccionan tipos de documentos a pedir en el formulario

            return View("form");
        }

        // POST: Ingresos/Ingresos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(WebApplicationModel.Ingresos model, HttpPostedFileBase[] files, string[] documento)
        {
            
                WebApplicationModel.Novedad novedad = new WebApplicationModel.Novedad();
                novedad.id_usu = Helper.converRut(model.rut);
                novedad.id_cliente = SesionLogin().id_cliente;
                novedad.fecha_mod = DateTime.Now;
                novedad.estado = "1";
                novedad.tipo_nov = 2;
                novedad.fecha_novedad = DateTime.Now;
                novedad.usuario_mod = SesionLogin().id_usu;
                model.id_usu = Helper.converRut(model.rut);
                model.Novedad = novedad;
                model.cargo = "developer";
                model.estado_aprov = -1;
                if (ModelState.IsValid)
                {
                    model.id_novedad = novedad.id_novedad;
                    _db.Ingresos.Add(model);
                    _db.SaveChanges();
            
                    try
                    {
                        int i = 0;
                        foreach (var item in files)// TODO
                        {
                            if (helper.createFile(item, SesionCliente().id_cliente, model.id_novedad, novedad.tipo_nov, false, SesionCliente(), documento[i]) == null)
                            {
                                return JsonError("oops, no se ha podido guardar el archivo" + item.FileName.ToString());
                            }
                            i++;
                        }
                        return JsonExito();
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
                return JsonError("error en la solicitud");

        }

        // GET: Ingresos/Ingresos/Edit/5
        public ActionResult Edit(int id)
        {
            var model = _db.Ingresos.FirstOrDefault(item => item.id_ing == id);
            var id_nov = model.id_novedad;
            ViewBag.novedad = _db.Novedad.FirstOrDefault(p => p.id_novedad == id_nov);
            ViewBag.selectlistdocus = new SelectList(_db.parametros.Where(item => item.grupo == "DOCUM").OrderBy(item => item.valor), "valor", "detalle");// se seleccionan tipos de documentos a pedir en el formulario

            if (model == null) return RedirectToAction("Create");
            return View("form", model);
        }

        // POST: Ingresos/Ingresos/Edit/5
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(WebApplicationModel.Ingresos model, HttpPostedFileBase files, string[] documento)
        {
            
            WebApplicationModel.Novedad novedad = new WebApplicationModel.Novedad();
            novedad.usuario_mod = SesionLogin().id_usu;
            novedad.id_cliente = SesionLogin().id_cliente;
            novedad.fecha_mod = DateTime.Now;
            novedad.id_usu = Helper.converRut(model.rut);
            novedad.id_novedad = (int)model.id_novedad;
            novedad.estado = "1";
            novedad.tipo_nov = 2;
            model.id_usu = novedad.id_usu;
            novedad.fecha_novedad = DateTime.Now;
            model.Novedad = novedad;
            model.estado_aprov = -1;
            model.cargo = "developer";
            if (ModelState.IsValid)
            {
                try
                {
                    model.id_novedad = novedad.id_novedad;
                    _db.Ingresos.Add(model);
                    _db.SaveChanges();
                    var flag = helper.createFile(files, SesionCliente().id_cliente, model.id_novedad, novedad.tipo_nov, false, SesionCliente(), documento[0]);
                    if (flag!= null)
                    {
                        return JsonExito();
                    }
                    else
                    {
                        return JsonError("oops, ha ocurrido un problema con el archivo");
                    }
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
            return JsonError("error en la solicitud");
        }

            // GET: Ingresos/Ingresos/Delete/5
            public ActionResult Delete(int id)
        {
            var model = _db.Ingresos.FirstOrDefault(item => item.id_ing == id);
            var id_nov = model.id_novedad;
            var novedad = _db.Novedad.FirstOrDefault(c => c.id_novedad == id_nov);
            if (model == null) return JsonError("No existe el registro seleccionado");
            try
            {
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
            int id_registro = Int32.Parse(id);
            var model = _db.Ingresos.FirstOrDefault(item => item.id_ing == id_registro);
            var doc = model.id_novedad;
            var documento = _db.Documento.FirstOrDefault(item => item.id_novedad == doc);
            if (documento != null)
            {
                try
                {
                    model.estado_aprov = 1;
                    _db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();
                    return JsonExito();
                }
                catch (Exception e)
                {
                    return JsonError("oops, ocurrió un problema con su solicitud");
                }
            }
            else
            {
                return JsonError("No puede aprobar un registro sin documentos que lo validen");
            }
        }

        public ActionResult documentos(string id)
        {
            try
            {
                int id_reg = Int32.Parse(id);
                var comp = _db.Ingresos.FirstOrDefault(item => item.id_ing == id_reg).id_novedad;
                var model = _db.Documento.Where(item => item.id_novedad == comp && item.estado != 0).ToList();
                return View(model);

            }
            catch (Exception e)
            {
                return JsonError("no se encontró el registro con esa id");
            }

        }

        public ActionResult deleteDoc(string id)
        {
            try
            {
                int comp = Int32.Parse(id);
                var model = _db.Documento.FirstOrDefault(item => item.id_doc == comp);
                model.estado = 0;
                _db.SaveChanges();
                return JsonExito();
            }
            catch (Exception e)
            {
                return JsonError("no se encontró el registro con esa id");
            }
        }
    }
}