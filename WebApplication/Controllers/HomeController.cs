
﻿using System.Web.Mvc;
using System.Threading;
using System.Globalization;
using System.Web;

namespace WebApplication.Controllers
{

    public class HomeController : MasterController
    {
        public ActionResult Home()
        {
            string var = HttpContext.User.Identity.Name;
            var cumple = LoadData(" SELECT (nombre+ ' ' + appaterno + ' ' + apmaterno) as nombre_completo,fechaFiniquito, concat (year(GETDATE ()),'-',month(fechaNacimient),'-',day(fechaNacimient)) as fecha_nacimiento from [PSURSOFTSQL].[PSO].softland.sw_personal where fechaFiniquito > getdate() ");
            var privilegio = LoadData(" SELECT  [id_usu],[Nom_usu],[pass_usu] ,[id_cliente],[id_tipo_usu],[email],[fechaIngreso] FROM[PAYROLL_PreProd].[dbo].[Usuario] where id_usu='13029615'");
            
            ViewBag.TS = cumple;
            ViewBag.cliente = SesionCliente().Nom_cor_emp;
            
            SesionLogin();
            Session.Add("privilegiado", privilegio[0]["id_usu"].ToString());

            if (SesionLogin().id_usu== "13029615")
            {
                return RedirectToAction("Index", "MantFechasCalendario", new { Area = "Mantencion" });
            }
            return View(cumple);
        }



        //Se Supone que va a cambiar el idioma
        public ActionResult ChangeLanguage(string lang)
        {
            if (lang != null)
            {

                //return View("Home");
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(lang);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);
            }
            HttpCookie cookie = new HttpCookie("_lang");
            cookie.Value = lang;
            Response.Cookies.Add(cookie);
            return RedirectToLocal(SesionCliente().Nom_cor_emp, "");
        }

    }
}