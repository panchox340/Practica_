using AutenticacionPersonalizada.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WebApplication;
using WebApplication.App_Start;


namespace webapplication.controllers
{
    public class validadocumentocontroller : MasterController
    {
        [HttpGet]
        [AllowAnonymous]
        public ActionResult validaQR(string token, string cliente)
        {
            try
            {
                ViewBag.NeedLayaout = "N";
                var codigodesencriptado = SeguridadUtilidades.Desencriptar(token);
                string[] codqr = codigodesencriptado.Split('_');
                var numVal = codqr[0];
                var letra = codqr[1];
                var modulo11 = codqr[2];
                var idusu = codqr[3];
                var idcliente = codqr[4];
                if (helper.validaDV(numVal, modulo11))
                {
                    if (cliente.Equals(idcliente) && letra == "o")
                    {
                        if(_db.Usuario.Where(p => p.id_usu == idusu).Any())
                        {

                            string value = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                            string modulo11_ = helper.digitoVerificador(Int64.Parse(value));
                            string code = SeguridadUtilidades.Encriptar(value + ":" + modulo11_ + ":0:" + TIPO_CERTIFICADO_ANTIGUEDAD + ":" + idcliente + ":" + "" + ":"+ idusu + ":");
                            RouteValueDictionary values = new RouteValueDictionary();
                            values.Add("code", HttpUtility.UrlEncode(code));
                            values.Add("Area", "");
                            ViewBag.Routes_values = values;
                            return View();
                        }
                    }
                }
                return View("Error");
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }
    }
}