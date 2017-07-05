using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication.App_Start;

namespace WebApplication.Areas.Solicitudes.Controllers
{
    public class BajasController : MasterController
    {
        // GET: Solicitudes/Bajas
        public ActionResult Index()
        {
            var novedades = LoadData("select fin.id_baja, fecha_novedad, (nombres + ' ' + apellido) as nombre, fin.causal,p.detalle from ((Finiquito fin "+
                                    "join parametros p on fin.causal = p.valor) "+
                                    "join Novedad nov on nov.id_novedad = fin.id_novedad) "+
                                    "where nov.estado = 1 "+
                                    "order by fecha_mod asc");
            return View(novedades);
        }

        // GET: Finiquitos/Finiquito/Create
        public ActionResult Create()
        {
            ViewBag.selectlistParametros = new SelectList(_db.parametros.Where(m => m.grupo == "CAUSA").OrderBy(m => m.valor), "valor", "detalle"); // se seleccionan causales de despido
            ViewBag.selectlistdocus = new SelectList(_db.parametros.Where(item => item.grupo == "DOCUM").OrderBy(item => item.valor), "valor", "detalle");// se seleccionan tipos de documentos a pedir en el formulario
            return View("Form");
        }

        // POST: Finiquitos/Finiquito/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(WebApplicationModel.Finiquito model, HttpPostedFileBase[] files, string[] documento)
        {
                WebApplicationModel.Novedad novedad = new WebApplicationModel.Novedad();
                novedad.id_usu = Helper.converRut(model.rut);
                novedad.id_cliente = SesionLogin().id_cliente;
                novedad.fecha_mod = DateTime.Now;
                novedad.estado = "1";
                novedad.tipo_nov = 6;
                novedad.fecha_novedad = DateTime.Now;
                novedad.usuario_mod = SesionLogin().Nom_usu;
                model.Novedad = novedad;
                model.nombre_sol = novedad.id_usu;
                try
                {
                model.id_novedad = novedad.id_novedad;
                _db.Finiquito.Add(model);
                _db.SaveChanges();
                int i = 0;
                foreach (var item in files)
                    {
                        if (helper.createFile(item, SesionCliente().id_cliente, model.id_novedad, novedad.tipo_nov, false, SesionCliente(),documento[i])==null)
                        {
                            return JsonError("oops, no se ha podido guardar el archivo" + item.FileName.ToString());
                        }
                    }
                    
                }
                catch (Exception err)
                {
                    return JsonError("ocurrio un problema con su solicitud");
                }            
            return JsonError("ocurrio un problema con su solicitud");
        }

        // GET: Finiquitos/Finiquito/Edit/5
        public ActionResult Edit(int id)
        {
            var model = _db.Finiquito.FirstOrDefault(item => item.id_baja == id);
            var id_nov = model.id_novedad;
            ViewBag.novedad = _db.Novedad.FirstOrDefault(p => p.id_novedad == id_nov);
            ViewBag.selectlistParametros = new SelectList(_db.parametros.Where(m => m.grupo == "CAUSA").OrderBy(m => m.valor), "valor", "detalle");
            ViewBag.selectlistdocus = new SelectList(_db.parametros.Where(item => item.grupo == "DOCUM").OrderBy(item => item.valor), "valor", "detalle");// se seleccionan tipos de documentos a pedir en el formulario
            if (model == null) return RedirectToAction("Create");
            return View("Form", model);
        }

        // POST: Finiquitos/Finiquito/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(WebApplicationModel.Finiquito model, HttpPostedFileBase[] files, string[] documento)
        {
            
                WebApplicationModel.Novedad novedad = new WebApplicationModel.Novedad();
                novedad.usuario_mod = SesionLogin().Nom_usu;
                novedad.id_cliente = SesionLogin().id_cliente;
                novedad.fecha_mod = DateTime.Now;
                novedad.id_usu = Helper.converRut(model.rut);
                novedad.id_novedad = model.id_novedad;
                novedad.estado = "1";
                novedad.tipo_nov = 6;
                novedad.fecha_novedad = DateTime.Now;
            if (ModelState.IsValid)
            {
                model.nombre_sol = SesionLogin().Nom_usu;
                model.Novedad = novedad;
                model.estado_aprov = -1;
                model.id_novedad = novedad.id_novedad;
                _db.Finiquito.Attach(model);
                _db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
                try
                {
                    int i = 0;
                    foreach (var item in files)// TODO
                    {
                        if (helper.createFile(item, SesionCliente().id_cliente, model.id_novedad, novedad.tipo_nov, false, SesionCliente(), documento[i])==null)
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
            return JsonError("oopa ocurrio un problema con su solicitud");
        }

        // GET: Finiquitos/Finiquito/Delete/5
        public ActionResult Delete(string id)
        {
            var comp = Int32.Parse(id);
            var model = _db.Finiquito.FirstOrDefault(item => item.id_baja == comp);
            if (model == null) return JsonError("No existe el registro seleccionado");
            var novedad = _db.Novedad.FirstOrDefault(c => c.id_novedad == model.id_novedad);
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
            var model = _db.Finiquito.FirstOrDefault(item => item.id_baja == id_registro);
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
                var comp = _db.Finiquito.FirstOrDefault(item => item.id_baja == id_reg).id_novedad;
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