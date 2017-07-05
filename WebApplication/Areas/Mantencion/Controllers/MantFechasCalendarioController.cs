using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplicationModel;

namespace WebApplication.Areas.Mantencion.Controllers
{
    public class MantFechasCalendarioController : MasterController
    {
        // GET: Mantencion/MantFechasCalendario

      /*  public ActionResult MantAvisos(string titulo) {

            int id = Convert.ToInt32(titulo.Substring(1, 1));
            Avisos aviso=  _db.Avisos.Where(a => a.id_avisos == id).FirstOrDefault();
            return View(aviso);
        }*/
        public ActionResult MantAvisos(string titulo)
        {
            if (titulo==null)
            {
                List<Cliente> clientes = _db.Cliente.ToList();
                ViewBag.clientes = clientes;
                return View();
            }
            else
            {

                List<Cliente> clientes = _db.Cliente.ToList();
                ViewBag.clientes = clientes;
                int id = Convert.ToInt32(titulo.Split('.')[0]);
                Avisos aviso=_db.Avisos.Where(a => a.id_avisos == id).FirstOrDefault();
                return View(aviso);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Create(Avisos model)
        {

            try
            {
                Cliente cliente = new Cliente
                {
                    id_cliente = model.id_cliente
                };

                if (model.id_avisos == 0)
                {
                    _db.Avisos.Add(model);
                }
                else
                {
                    _db.Avisos.Attach(model);
                    _db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                }
                _db.SaveChanges();
                return JsonExito();
            }
            catch (Exception)
            {

                return JsonError("Error al guardar cambios");
            }
           

           
        }

        public JsonResult Delete(int id) {
            var model = _db.Avisos.FirstOrDefault(item => item.id_avisos == id);

            if (model == null) return JsonError("No existe el registro seleccionado");
            try
            {
                _db.Avisos.Remove(model);
                _db.SaveChanges();
                return JsonExito();
            }
            catch (System.Exception ex)
            {
                return JsonError("ocurrio un problema con su solicitud");
            }
        }

        public ActionResult Index() {


            //se debe implementar además que si el usuario logeado es payroll se omite el filtro de esta consulta
            var avisos = LoadData(" SELECT  [id_avisos] ,[titulo_aviso]  ,[desc_aviso] ,concat (year(GETDATE ()),'-',month([fecha_aviso]),'-',day([fecha_aviso]))  as [fecha_aviso] ,[id_cliente] FROM [PAYROLL_PreProd].[dbo].[avisos] where [id_cliente]="+SesionLogin().id_cliente);
            return View(avisos);
        }
    }
}


