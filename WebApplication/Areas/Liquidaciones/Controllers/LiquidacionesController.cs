using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplicationModel;

namespace WebApplication.Areas.Liquidaciones.Controllers
{
    public class LiquidacionesController : MasterController
    {
        // GET: Liquidaciones/Liquidaciones
        public ActionResult Index()
        {
            Usuario usuario = SesionLogin();
            var per_liqui = _db.Periodo_liquidaciones.Where(x => x.id_cliente == usuario.id_cliente).Where(x => x.liberado).OrderBy(c => c.ano).ThenByDescending(c => c.mes).FirstOrDefault();
            ViewBag.fechaUltimaLiberacion = new DateTime(per_liqui.ano, per_liqui.mes, 1);
            return View();
        }

        public ActionResult Liquidacion(string fecha)
        {
            return getFile(0, TIPO_LIQUIDACION, SesionCliente().Nom_cor_emp, fecha);
        }

        public ActionResult Create()
        {
            return View("Form");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Create(Usuario model)
        {
            return JsonExito();
        }
        
        public ActionResult LiberaLiquidacion()
        {
            var clientes = _db.Cliente;
            ViewBag.Clientes = new SelectList(clientes, "id_cliente", "nom_emp");
            Usuario usuario = SesionLogin();
            var per_liqui = _db.Periodo_liquidaciones.Where(x => x.id_cliente == usuario.id_cliente).Where(x => x.liberado).OrderBy(c => c.ano).ThenByDescending(c => c.mes).FirstOrDefault();
            ViewBag.fechaUltimaLiberacion = new DateTime(per_liqui.ano, per_liqui.mes, 1);
            return View();
        }
        public ActionResult Solicitud()
        {
            return View("Form");
        }

        //se declara explicitamente que este action sera invocado de manera POST
        [HttpPost]
        //Modulo de solicitud de vacaciones
        public ActionResult Solicitud(string fechaInicio, string fechaFin)
        {
            var user = SesionLogin().id_usu.ToString();
            var empresa = (from a in _db.Cliente
                           join b in _db.Usuario on user equals b.id_usu
                           where a.id_cliente == b.id_cliente
                           select new { a.Nom_cor_emp }).Single().Nom_cor_emp;
            var rut = (from a in _db.Ingresos where a.id_usu == user select new { a.rut }).Single().rut;

            try
            {

                int largo = rut.Length;
                var ficha = rut.Remove((largo - 2), 2);
                if (largo == 10)
                { // verifica si el rut es de 9 o 10 digitos
                    rut = (rut.Remove((largo - 2), 2)).Insert(0, "0"); // remueve el guion y digito verificador al rut y agrega 0 para seguir el formato de la bdd
                }
                else
                {
                    rut = (rut.Remove((largo - 2), 2)).Insert(0, "00");
                }

                var Datos_Usu = LoadData("select fechaPrimerCon from [PSURSOFTSQL].[" + empresa + "].softland.sw_personal where left(replace(replace(rut,'.',''),'-',''),9) =" + rut); // obtiene los datos del usuario a 
                var fec_usu = DateTime.Parse((Datos_Usu.FirstOrDefault())["fechaPrimerCon"].ToString());

                var saldo = LoadData("select sum(Ndias)as total_dias from [PSURSOFTSQL].[" + empresa + "].[softland].[sw_vacadic] where ficha =" + rut);
                Double saldo_dias;
                if (saldo.Any())
                {
                    saldo_dias = 0;
                }
                else {
                    saldo_dias = Double.Parse((saldo.FirstOrDefault())["DiasVacAnual"].ToString()); // cantidad de dias que llegan como saldo de apertura
                }
                var DiasVac = LoadData("select DiasVacAnual from [PSURSOFTSQL].[" + empresa + "].[softland].[sw_diasvacanuper] where left(replace(replace(ficha,'.',''),'-',''),9) =" + rut + " order by DiasVacAnual desc");// cantidad de dias de vacaciones por año del usuario    

                var Vac = Double.Parse((DiasVac.FirstOrDefault())["DiasVacAnual"].ToString()); // cantidad de dias que llegan como saldo de apertura
                var vacas_solicitadas = LoadData("select sum(NDiasAp) as t_dias_solicitados from [PSURSOFTSQL].[" + empresa + "].[softland].[sw_vacsolic] where left(replace(replace(ficha,'.',''),'-',''),9) =" + rut); //total vacaciones autorizadas para el usuario
                Double vacasSolicitadas;
                if (vacas_solicitadas.Any())
                {
                    vacasSolicitadas = 0;
                }
                else {
                    vacasSolicitadas = Double.Parse(vacas_solicitadas.FirstOrDefault()["t_dias_solicitados"].ToString());
                }

                if (Vac == 0) Vac = 15;

                Double Factor = (Vac / 12); // cantidad de dias que gana el usuario por mes
                var meses_trabajados = DateTime.Now.Subtract(fec_usu).Days / (365.25 / 12); // cantidad de meses trabajados por el usuario
                var dias_vacas = (Factor * meses_trabajados) + saldo_dias; // cantidad de dias de vacaciones que tiene el usuario
                DateTime FI = DateTime.Parse(fechaInicio);
                DateTime FF = DateTime.Parse(fechaFin);
                Double dias = (FF - FI).TotalDays; //calcula dias que solicita el usuario

                var Vac_totales = dias_vacas - vacasSolicitadas; // total dias que le quedan de vacaciones antes de solcitar
                var Vac_totales_sol = Vac_totales - dias; //total dias luego de la solicitud

                ViewBag.usuario = LoadData("select nombres from [PSURSOFTSQL].[" + empresa + "].softland.sw_personal where left(replace(replace(ficha,'.',''),'-',''),9) =" + rut);
                ViewBag.vacas = Vac_totales_sol;

            }
            catch
            {
                return View("index");
            }
            return View("index");
        }

        public JsonResult GetUsuarios(int id)
        {
           var selectList =new SelectList(_db.Usuario.Where(c=> c.id_cliente==id), "id_usu", "Nom_usu");
      
            return Json(selectList);
        }


        public JsonResult LiberarLiquidacion()
        {
            var usuario = SesionLogin();
            var per_liqui=_db.Periodo_liquidaciones.Where(x => x.id_cliente == usuario.id_cliente).Where(x=>!x.liberado).OrderBy(c => c.ano).ThenBy(c => c.mes).FirstOrDefault();
            per_liqui.liberado = true;
            _db.SaveChanges();
            return JsonExito();

        }
    }
}