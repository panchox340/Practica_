using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using System.Web.Mvc;
using WebApplication.Seguridad;
using System.Web.Security;

namespace WebApplication.App_Start
{
    public  class Helper : Controller
    {

        private Db_Entities _db;

        public Helper(Db_Entities _db) {
            this._db = _db;
        }

        public static string converRut(string rut) {
            Regex rgx = new Regex("^(0{1}){1}([1-9])");
            Regex rgx2 = new Regex("^(0{2}){1}([1-9])");
            try
            {

                if (rgx.IsMatch(rut))
                {
                    rut = rut.Substring(1, (rut.Length - 2)).Replace(".", "").Replace("-", "");
                    return rut;
                }
                else if (rgx2.IsMatch(rut))
                {
                    rut = rut.Substring(2, (rut.Length - 3)).Replace(".", "").Replace("-", "");
                    return rut;
                }
                else
                {
                    return rut;
                }
            }
            catch (Exception e) {
                MvcApplication.LogError(e);
                return "";
            }           
        }

        public Documento createFile(HttpPostedFileBase file, int id_cliente, int id_novedad, int? tipo_nov, bool operador_payrrol, Cliente cliente_sesion, string tipo_documento)
        {
            Documento documento = new Documento();
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            string fileName = new string(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray());
            DateTime dateTime = DateTime.Now;
            string year = dateTime.ToString("yyyy");
            string month = dateTime.ToString("MM");
            string relative_path = "";
            string path = "";
            string route = "";
            try
            {
                if (!IsValidDocument(file)) { return null; }
                if(tipo_nov == null)
                {
                    fileName = tipo_documento + "_" + dateTime.ToString("yyyyMMddHHmmssfff") + "_" + fileName + Path.GetExtension(file.FileName);
                    relative_path = "~/Files/" + cliente_sesion.Nom_cor_emp + "/" + tipo_documento + "/" + fileName;
                    path = System.Web.Hosting.HostingEnvironment.MapPath("~/Files/" + cliente_sesion.Nom_cor_emp + "/" + tipo_documento + "/" + fileName);
                    route = System.Web.Hosting.HostingEnvironment.MapPath("~/Files/" + cliente_sesion.Nom_cor_emp + "/" + tipo_documento);
                }
                else
                {
                    fileName = _db.Tipo_novedad.SingleOrDefault(p => p.id_tipo == tipo_nov).nombre + "_" + dateTime.ToString("yyyyMMddHHmmssfff") + "_" + fileName + Path.GetExtension(file.FileName);
                    relative_path = "~/Files/" + cliente_sesion.Nom_cor_emp + "/" + year + "/" + month + "/" + fileName;
                    path = System.Web.Hosting.HostingEnvironment.MapPath("~/Files/" + cliente_sesion.Nom_cor_emp + "/" + year + "/" + month + "/" + fileName);
                    route = System.Web.Hosting.HostingEnvironment.MapPath("~/Files/" + cliente_sesion.Nom_cor_emp + "/" + year + "/" + month);
                    if (operador_payrrol)
                    {
                        string client = _db.Cliente.FirstOrDefault(p => p.id_cliente == id_cliente).Nom_cor_emp;
                        relative_path = "~/Files/" + cliente_sesion.Nom_cor_emp + "/" + year + "/" + month + "/" + fileName;
                        path = System.Web.Hosting.HostingEnvironment.MapPath("~/Files/" + client + "/" + year + "/" + month + "/" + fileName);
                        route = System.Web.Hosting.HostingEnvironment.MapPath("~/Files/" + client + "/" + year + "/" + month);
                    }
                }
                Directory.CreateDirectory(route);
                var data = new byte[file.ContentLength];
                file.InputStream.Read(data, 0, file.ContentLength);
                using (var sw = new FileStream(path, FileMode.Create))
                {
                    sw.Write(data, 0, data.Length);
                }
                if (System.IO.File.Exists(path))
                {
                    documento.categoria = tipo_documento;
                    if (tipo_documento.Equals("DOCUMLIQUI"))
                    {
                        documento.id_novedad = null;
                    }else
                    {
                        documento.id_novedad = id_novedad;
                    }
                    documento.nombre_asignado = fileName;
                    documento.nombre_original = file.FileName;
                    documento.ruta = relative_path;
                    _db.Documento.Add(documento);
                    _db.SaveChanges();
                    return documento;
                }else
                {
                    return null;
                }

            }
            catch(Exception ex)
            {
                MvcApplication.LogError(ex);
                return null;
            }
        }


        private bool IsValidDocument(HttpPostedFileBase file)
        {
            string[] formats = new string[] { ".jpg", ".png", ".gif", ".jpeg", ".pdf", ".xls", ".xlsx", ".doc", ".docx" };
            string[] contentType = new string[] { "image",
                "application/vnd.ms-excel",
                "application/msword",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "application/pdf" };
            if (contentType.Any(item => file.ContentType.EndsWith(item, StringComparison.OrdinalIgnoreCase)) && 
                formats.Any(item => file.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string digitoVerificador(Int64 rut)
        {
            Int64 Digito;
            int Contador;
            Int64 Multiplo;
            Int64 Acumulador;
            string RutDigito;

            Contador = 2;
            Acumulador = 0;

            while (rut != 0)
            {
                Multiplo = (rut % 10) * Contador;
                Acumulador = Acumulador + Multiplo;
                rut = rut / 10;
                Contador = Contador + 1;
                if (Contador == 8)
                {
                    Contador = 2;
                }

            }

            Digito = 11 - (Acumulador % 11);
            RutDigito = Digito.ToString().Trim();
            if (Digito == 10)
            {
                RutDigito = "K";
            }
            if (Digito == 11)
            {
                RutDigito = "0";
            }
            return (RutDigito);
        }


        public bool validaDV(string numVal, string modulo11)
        {
            Int64 numVal_int = 0;
            if(Int64.TryParse(numVal, out numVal_int))
            {
                if(digitoVerificador(numVal_int) == modulo11)
                {
                    return true;
                }
            }
            return false;
        }

        //public string Get_TextCertificado(string tag, List<Dictionary<string, object>> datosCertAntiguedad, Usuario usuario)
        //{
        //    int id_cliente = usuario.id_cliente;
        //    XmlTextReader reader = new XmlTextReader(System.Web.Hosting.HostingEnvironment.MapPath(_db.Documento.FirstOrDefault(p => p.categoria == "DOCUMTXTCERTANT" && p.estado == 1 && p.id_cliente == id_cliente).ruta));
        //    string Texto_Super = null;
        //    while (reader.Read())
        //    {
        //        if (reader.Name == tag)
        //        {
        //            Texto_Super = reader.ReadString();
        //        }

        //    }
        //    foreach (var row in datosCertAntiguedad)
        //    {
        //        foreach (var item in row)
        //        {
        //            Texto_Super = Texto_Super.Replace("{" + item.Key + "}", item.Value.ToString());
        //        }
        //    }
        //    return Texto_Super;
        //}
    }
}