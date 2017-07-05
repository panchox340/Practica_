using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplicationModel;

namespace WebApplication.Areas.Solicitudes.Controllers
{
    public class SolGenericaController : MasterController
    {
        // GET: OtraSolicitudes/OtrasSol
        public ActionResult Index()
        {
            ViewBag.Otra_Solicitud = _db.ViewOtraSolicitud;
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Create(WebApplicationModel.Otra_solicitud model, string tipo_novedad)
        {
            WebApplicationModel.Novedad novedad = new WebApplicationModel.Novedad();

            novedad.tipo_nov = System.Convert.ToInt32(tipo_novedad);
            novedad.id_cliente = SesionLogin().id_cliente;
            novedad.fecha_mod = System.DateTime.Now;
            novedad.fecha_novedad = System.DateTime.Now;

            novedad.id_usu = SesionLogin().id_usu;
            novedad.estado = "1";
            novedad.usuario_mod = SesionLogin().id_usu;
            model.Novedad = novedad;
            model.id_novedad = novedad.id_novedad;
            //_db.beginTran();
            //_db.add(novedad);

            _db.Otra_solicitud.Add(model);
            //_db.commit
            //_db.Database.BeginTransaction();
            _db.SaveChanges();

            return JsonExito();
        }


        public ActionResult Delete( int id)
        {
            var model = _db.Otra_solicitud.FirstOrDefault(item => item.id_otro == id);
            var id_nov = model.id_novedad;
            var novedad = _db.Novedad.FirstOrDefault(c => c.id_novedad == id_nov);
            if (model == null) return JsonError("No existe el registro seleccionado");
            try
            {
                novedad.estado = "0";
                _db.SaveChanges();
                return JsonExito();
            }
            catch (System.Exception ex)
            {
                return JsonError("ocurrio un problema con su solicitud");
            }


        }
        public ActionResult OtraSolicitud()
        {

            int id = SesionLogin().id_cliente;

            IEnumerable<SelectListItem> myCollection = _db.Usuario.Where(u => u.id_cliente == id).ToList().Select(x =>
                                  new SelectListItem()
                                  {
                                      Text = x.Nom_usu.ToString()
                                      ,
                                      Value = x.id_usu
                                  }).ToList();
            ViewBag.usuarios = myCollection;

            //List<WebApplicationModel.parametros> parametros = new List<WebApplicationModel.parametros>();
            //foreach (var item in _db.parametros)
            //{
            //    if (item.detalle.Equals("TIPOSOL"))
            //    {
            //        parametros.Add(
            //         new parametros
            //         {
            //             id = item.id,
            //             texto = item.texto
            //         }
            //       );
            //    }

            //}

            //24(id) altas 20(is) bajas
            var datos = _db.Tipo_novedad.Where(c => c.id_tipo != 2 && c.id_tipo != 6).ToList();
            ViewBag.Tipo_novedad =  new SelectList(datos, "id_tipo", "nombre");
            ViewBag.Tipo_novedadOculto = new SelectList(datos, "valorizado", "id_tipo");
            return View();
        }
    }
}