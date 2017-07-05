using AutenticacionPersonalizada.Utilidades;
using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using WebApplication.Controllers;


namespace WebApplication.Areas.Certificados.Controllers
{
    public class AntiguedadController : MasterController
    {
        // GET: Certificados/Antiguedad
        public ActionResult Index()
        {
            int empresa = SesionLogin().id_cliente;
            ViewBag.Empresa = _db.Cliente.Where(item => item.id_cliente == empresa).SingleOrDefault().nom_emp;
            ViewBag.fechaIngreso = SesionLogin().fechaIngreso;
            return View();
        }

        public ActionResult PDF()
        {
            return getFile(0,TIPO_CERTIFICADO_ANTIGUEDAD, SesionCliente().Nom_cor_emp,"");
        }



    }
}