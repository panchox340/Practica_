using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Circon.Mvc.Helpers;
using WebApplication.Seguridad;
using System.Web.Security;
using iTextSharp.text.pdf;
using System.Data.SqlClient;
using System.Data;
using WebApplication.App_Start;
using System.Text.RegularExpressions;
using System.Web.Routing;
using System.IO;
using iTextSharp.text;
using AutenticacionPersonalizada.Utilidades;
using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using System.Drawing.Imaging;
using System.Drawing;

namespace WebApplication
{
    public class _Controller : Controller
    {
        protected const string TIPO_LIQUIDACION = "liquidacion";
        protected const string TIPO_CERTIFICADO_ANTIGUEDAD = "cert_ant";
        protected const string TIPO_OTROS = "other";
        //protected Db_Entities _db = new Db_Entities();

        //protected Usuario SesionLogin()
        //{
        //    if ((Session["usuario"] as Usuario) == null)
        //    {
        //        UsuarioMembership mu = (UsuarioMembership)Membership.GetUser(HttpContext.User.Identity.Name, false);
        //        Session["usuario"] = _db.Usuario.SingleOrDefault(p => p.id_usu == mu.id_usu) as Usuario;
        //    }
        //    return Session["usuario"] as Usuario;
        //}

        //protected Cliente SesionCliente()
        //{
        //    if ((Session["cliente"] as Cliente) == null)
        //    {
        //        var mu = HttpContext.Request.Cookies["AuthCookieClient"].Values["client_auth"].Trim();
        //        Session["cliente"] = _db.Cliente.SingleOrDefault(p => p.Nom_cor_emp == mu) as Cliente;
        //    }
        //    return Session["cliente"] as Cliente;
        //}
        //protected Cliente SesionCliente(string cliente)
        //{
        //    if ((Session["cliente"] as Cliente) == null)
        //    {
        //        Session["cliente"] = _db.Cliente.SingleOrDefault(p => p.Nom_cor_emp == cliente);
        //    }
        //    return (Session["cliente"] as Cliente) == null? new Cliente(): Session["cliente"] as Cliente;
        //}

        protected ActionResult RedirectToLocal(string cliente, string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Home", "Home", new { cliente = cliente, area = "", returnUrl = returnUrl });
        }
    }

    [CustomLayoutAjax]
    [ClientAuthorize]
    public class MasterController : _Controller
    {

        protected Helper helper;

        public MasterController()
        {
            helper = new Helper(_db);
        }

        protected List<Dictionary<string, object>> LoadData(string sqlSelect, params object[] sqlParameters)
        {
            var table = new List<Dictionary<string, object>>();
            _db.Database.Connection.Open();
            using (var cmd = _db.Database.Connection.CreateCommand())
            {
                cmd.CommandText = sqlSelect;
                foreach (var param in sqlParameters)
                    cmd.Parameters.Add(param);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                            row[reader.GetName(i)] = reader[i];
                        table.Add(row);
                    }
                }
            }
            _db.Database.Connection.Close();
            return table;
        }

        protected JsonResult JsonExito()
        {
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
        protected JsonResult JsonError(string msg)
        {
            return Json(new { success = false, error = true, mensaje = msg }, JsonRequestBehavior.AllowGet);
        }

        protected ActionResult getFile(int id_doc, string type, string client, string fecha)
        {
            string value = DateTime.Now.ToString("yyyyMMddHHmmssffff");
            string modulo11 = helper.digitoVerificador(Int64.Parse(value));
            string code = SeguridadUtilidades.Encriptar(value + ":" + modulo11 + ":" + id_doc.ToString() + ":" + type + ":" + client + ":" + fecha + "::") ;
            RouteValueDictionary values = new RouteValueDictionary();
            values.Add("code", HttpUtility.UrlEncode(code));
            values.Add("Area", "");
            ViewBag.Routes_values = values;
            if (type.Equals("other"))
            {
                ViewBag.MimeType = "other";
            }
            else
            {
                ViewBag.MimeType = "application/pdf";
            }
            return View("getFile");
        }

        [AllowAnonymous]
        public FileResult getFileData(string code)
        {
            var code_array = SeguridadUtilidades.Desencriptar(HttpUtility.UrlDecode(code)).Split(':');
            string validacion = code_array[0];
            string modulo11 = code_array[1];
            if (!helper.validaDV(validacion, modulo11)) { return null; } 
            int id_doc = Int32.Parse(code_array[2]);
            string type = code_array[3];
            string client = code_array[4];
            string fecha = code_array[5];
            string id_usu = code_array[6];
            MemoryStream ms = new MemoryStream();
            PdfReader pdfReader;
            PdfStamper pdfStamper;
            AcroFields pdfFormFields;
            Cliente cliente = id_usu == "" ? SesionCliente() : _db.Cliente.FirstOrDefault(p => p.Nom_cor_emp == client);
            Usuario usuario = id_usu == "" ? SesionLogin() : _db.Usuario.FirstOrDefault(p => p.id_usu == id_usu);
            switch (type)
            {
                case "liquidacion":
                    pdfReader = new PdfReader(System.Web.Hosting.HostingEnvironment.MapPath(_db.Documento.FirstOrDefault(p => p.categoria == "DOCUMLIQUI" && p.estado == 1 && p.id_cliente == usuario.id_cliente).ruta));
                    pdfStamper = new PdfStamper(pdfReader, ms);
                    pdfFormFields = pdfStamper.AcroFields;
                    var periodo = PeriodoVigente(new List<SqlParameter>()
                    {
                        new SqlParameter ("@Fecha",SqlDbType.Date) {Value = fecha },
                        new SqlParameter ("@NOM_CLIENTE_DB",SqlDbType.VarChar) {Value = cliente.Nom_cor_emp },
                        new SqlParameter ("@VALOR_RET",SqlDbType.Int) { Value = 0 }
                    });
                    var datos_liquidacion = MultiLoadDataToDictionary(new List<SqlParameter>()
                    {
                        new SqlParameter ("@USU_ID",SqlDbType.VarChar) {Value = usuario.id_usu },
                        new SqlParameter ("@NOM_CLIENTE_DB",SqlDbType.VarChar) {Value = cliente.Nom_cor_emp },
                        new SqlParameter ("@Mes_consul",SqlDbType.VarChar) {Value = periodo },
                    }, "pa_selDatosLiquidacion");
                    var campo = LoadData("select tag, campo,label,id_cliente, orden  from configPDF where id_cliente = -1 order by tag, orden").ToList();
                    Fill_pdf(datos_liquidacion, campo, pdfFormFields, periodo); // funcion para llenado de pdf
                    pdfStamper.FormFlattening = false;
                    pdfStamper.MoreInfo = new Dictionary<string, string>() { { "Title", "Liquidación_" + DateTime.Now.ToString("dd-MM-yyyy") + ".pdf" } };
                    pdfStamper.Close();
                    pdfReader.Close();
                    Response.AppendHeader("Content-Disposition", "inline; filename=Liquidación_" + DateTime.Now.ToString("dd-MM-yyyy") + ".pdf");
                    return File(ms.ToArray(), System.Net.Mime.MediaTypeNames.Application.Pdf);
                case "cert_ant":
                    pdfReader = new PdfReader(System.Web.Hosting.HostingEnvironment.MapPath(_db.Documento.FirstOrDefault(p => p.categoria == "DOCUMCERTANT" && p.estado == 1 && p.id_cliente == usuario.id_cliente).ruta));
                    pdfStamper = new PdfStamper(pdfReader, ms);
                    pdfFormFields = pdfStamper.AcroFields;
                    var datosCertAntiguedad = LoadData("EXEC  [dbo].[pa_selDatosCertAntiguedad] @USU_ID = N'" + usuario.id_usu + "',@NOM_CLIENTE_DB = N'" + cliente.Nom_cor_emp + "' ");
                    var Texto_Super = helper.Get_TextCertificado("C_ANTIGUEDAD", datosCertAntiguedad, usuario);
                    var fec_actu = DateTime.Now.ToString("dd MMMM yyyy");
                    fec_actu = "En Santiago a " + fec_actu;
                    pdfFormFields.SetField("Texto_Super", Texto_Super);
                    pdfFormFields.SetField("fec_actu", fec_actu);

                    var pdfContentByte2 = pdfStamper.GetOverContent(1);
                    iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(System.Web.Hosting.HostingEnvironment.MapPath(_db.Documento.FirstOrDefault(p => p.categoria == "DOCUMLOGOCLI" && p.estado == 1 && p.id_cliente == usuario.id_cliente).ruta));
                    image.SetAbsolutePosition(20, 730);
                    pdfContentByte2.AddImage(image);
                    if (id_usu == "")
                    {
                        var fechaQR = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                        var mod11 = helper.digitoVerificador(Int64.Parse(fechaQR));
                        var cadena = fechaQR + "_" + "o" + "_" + mod11 + "_" + usuario.id_usu + "_" + cliente.Nom_cor_emp;
                        var encriptado = SeguridadUtilidades.Encriptar(cadena);
                        string url = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
                        var QRfinal = url + "/" + cliente.Nom_cor_emp + "/" + "ValidaDocumento" + "/" + "validaQR" + "?" + "token=" + HttpUtility.UrlEncode(encriptado);
                        QrEncoder qrEncoder = new QrEncoder(ErrorCorrectionLevel.M);
                        QrCode qrcode = new QrCode();
                        qrEncoder.TryEncode(QRfinal, out qrcode);
                        GraphicsRenderer Renderder = new GraphicsRenderer(new FixedCodeSize(50, QuietZoneModules.Zero), Brushes.Black, Brushes.White);
                        MemoryStream MS = new MemoryStream();
                        Renderder.WriteToStream(qrcode.Matrix, ImageFormat.Jpeg, MS);
                        var imagentemporal = new Bitmap(MS);
                        var pdfContentByte = pdfStamper.GetOverContent(1);
                        iTextSharp.text.Image QR = iTextSharp.text.Image.GetInstance(imagentemporal, System.Drawing.Imaging.ImageFormat.Jpeg);
                        QR.SetAbsolutePosition(500, 730);
                        pdfContentByte.AddImage(QR);
                    }
                    pdfStamper.FormFlattening = false;
                    pdfStamper.MoreInfo = new Dictionary<string, string>() { { "Title", "Certificado_" + DateTime.Now.ToString("dd-MM-yyyy") + ".pdf" } };
                    pdfStamper.Close();
                    pdfReader.Close();
                    Response.AppendHeader("Content-Disposition", "inline; filename=" + "Certificado_" + DateTime.Now.ToString("dd-MM-yyyy")+".pdf");
                    return File(ms.ToArray(), System.Net.Mime.MediaTypeNames.Application.Pdf);
                case "other":
                    var documento = _db.Documento.FirstOrDefault(p => p.id_doc == id_doc);
                    Response.AppendHeader("Content-Disposition", "inline; filename=" + documento.nombre_original);
                    using (FileStream file = new FileStream(System.Web.Hosting.HostingEnvironment.MapPath(documento.ruta), FileMode.Open, FileAccess.Read))
                    {
                        byte[] bytes = new byte[file.Length];
                        file.Read(bytes, 0, (int)file.Length);
                        ms.Write(bytes, 0, (int)file.Length);
                    }
                    return File(ms.ToArray(), MimeMapping.GetMimeMapping(System.Web.Hosting.HostingEnvironment.MapPath(documento.ruta)));
                default:
                    return null;
            }
        }




        protected Dictionary<int,List<Dictionary<string, string>>> MultiLoadData(List<SqlParameter> parametros,string NombreProcedimiento)
        {
            var table = new List<Dictionary<string, string>>();
            var multi_list = new Dictionary<int, List<Dictionary<string, string>>>();
            _db.Database.Connection.Open();
            using (var cmd = _db.Database.Connection.CreateCommand())
            {
                cmd.CommandText = NombreProcedimiento;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(parametros.ToArray());
                cmd.ExecuteNonQuery();
                using (var reader = cmd.ExecuteReader())
                {
                    int y = 0;
                    while (reader.HasRows)
                    {
                        var row = new Dictionary<string, string>();
                        while (reader.Read())
                        {
                            if (reader.GetName(0).Equals("valor", StringComparison.InvariantCultureIgnoreCase))
                            {
                                row[reader[1].ToString()] = reader[0].ToString();
                            }
                            else
                            {
                                row = new Dictionary<string, string>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row[reader.GetName(i)] = reader[i].ToString();
                                }
                            }
                            table.Add(row);
                        }
                        multi_list[y] = table;
                        y++;
                        reader.NextResult();
                    }
                }
            }
            _db.Database.Connection.Close();
            return multi_list;
        }

        protected Dictionary<string, string> MultiLoadDataToDictionary(List<SqlParameter> parametros, string NombreProcedimiento)
        {
            var table = new Dictionary<string, string>();
            _db.Database.Connection.Open();
            using (var cmd = _db.Database.Connection.CreateCommand())
            {
                cmd.CommandText = NombreProcedimiento;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(parametros.ToArray());
                cmd.ExecuteNonQuery();
                using (var reader = cmd.ExecuteReader())
                {
                    try
                    {
                        while (reader.HasRows)
                        {

                            while (reader.Read())
                            {
                                if (reader.GetName(0).Equals("valor", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    table[reader[1].ToString()] = reader[0].ToString();
                                }
                                else
                                {
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        table[reader.GetName(i)] = reader[i].ToString();
                                    }
                                }
                            }
                            reader.NextResult();
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.Out.WriteLine(ex.Message);
                    }
                    
                }
            }
            _db.Database.Connection.Close();
            return table;
        }

        protected void Fill_pdf(Dictionary<string, string> select_usu,List<Dictionary<string,object>> config_pdf, AcroFields pdfFormFields, int periodo)
        {
            int pdf_index = 1;
            string tipo = "";
            foreach(var item in config_pdf)
            {
                
                var tag = item["tag"].ToString();
                var campo = item["campo"].ToString();
                var label = item["label"].ToString();
                var orden = item["orden"].ToString();

                string value = "";
                if(!tipo.Equals(tag)) {
                    pdf_index = 1;
                    tipo = tag;
                }


                if (select_usu.TryGetValue(campo, out value))
                {
                    if (!value.Equals(""))
                    {
                        if (item["orden"].ToString().Equals(""))
                        {
                            pdfFormFields.SetField(item["tag"].ToString(), value);
                        }
                        else 
                        {
                            pdfFormFields.SetField(tipo.ToUpper()+".VALOR" + pdf_index.ToString(), value);
                            pdfFormFields.SetField(tipo.ToUpper()+".LABEL" + pdf_index.ToString(), item["label"].ToString());
                            pdf_index++;
                        }                  
                    }
                }


            }
        }

        //funcion para leer el periodo vigente al cual pertenece el usuario consultando
        protected int PeriodoVigente(List<SqlParameter> parametros) 
        {
            int result = new int();
            _db.Database.Connection.Open();
            using (var cmd = _db.Database.Connection.CreateCommand())
            {
                cmd.CommandText = "sp_selPeriodoUsuario";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(parametros.ToArray());
                cmd.Parameters["@VALOR_RET"].Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                result = (int)cmd.Parameters["@VALOR_RET"].Value;
            }
            _db.Database.Connection.Close();
            return result;
        }
    }
}