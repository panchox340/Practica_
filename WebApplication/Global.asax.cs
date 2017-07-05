using Circon.Mvc.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using WebApplication.Controllers;
using WebApplication.Seguridad;
using WebApplicationModel;

namespace WebApplication
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            BundleTable.EnableOptimizations = false;
        }

        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            if (Request.IsAuthenticated)
            {
                var identity =
                    new IdentityPersonalizado(
                        HttpContext.Current.User.Identity);
                var principal = new PrincipalPersonalizado(identity);
                HttpContext.Current.User = principal;
                
            }
        }




        protected void Application_Error(object sender, EventArgs e)
        {


            Exception exception = Server.GetLastError();
            string log = LogError(exception);
            HttpException httpException = exception as HttpException;

            RouteData routeData = new RouteData();
            routeData.Values.Add("controller", "Error");
            routeData.Values.Add("action", "Error");
            routeData.Values.Add("Area", "");
            routeData.Values.Add("cliente", "");
            routeData.Values.Add("id", log);
            if (httpException == null)
            {
                routeData.Values.Add("http_code", "");
            }
            else
            {
                routeData.Values.Add("http_code", httpException.GetHttpCode());
            }
            Response.Clear();
            Server.ClearError();
            Response.TrySkipIisCustomErrors = true;
            IController errorController = new ErrorController();
            //HttpContextWrapper Context_ = new HttpContextWrapper(Context);
            errorController.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));
        }


        public static string LogError(Exception ex)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            string code = new string(Enumerable.Repeat(chars, 20).Select(s => s[random.Next(s.Length)]).ToArray());
            DateTime actual = DateTime.Now;
            string message = string.Format("Time: {0}", actual.ToString("dd/MM/yyyy HH:mm:ss"));
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;
            message += string.Format("Code: {0}", code + "_" + actual.ToString("ddMMyyyyHHmmss"));
            message += Environment.NewLine;
            message += string.Format("Message: {0}", ex.Message);
            message += Environment.NewLine;
            message += string.Format("StackTrace: {0}", ex.StackTrace);
            message += Environment.NewLine;
            message += string.Format("Source: {0}", ex.Source);
            message += Environment.NewLine;
            message += string.Format("TargetSite: {0}", ex.TargetSite.ToString());
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;
            string path = System.Web.Hosting.HostingEnvironment.MapPath("~/ErrorLog/"+ actual.ToString("dd-MM-yyyy") + ".txt");
            using (System.IO.StreamWriter writer = System.IO.File.AppendText(path))
            {
                writer.WriteLine(message);
                writer.Close();
            }
            return code + "_" + actual.ToString("ddMMyyyyHHmmss");
        }

        protected void Application_AcquireRequestState (object sender, EventArgs e)
        {
            HttpContext context = HttpContext.Current;
            if (context != null & context.Request.Cookies != null)
            {
                HttpCookie cookie = HttpContext.Current.Request.Cookies["_lang"];
                if(cookie != null && cookie.Value != null)
                {
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(cookie.Value);
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(cookie.Value);
                }
                else
                {
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("es-CL");
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("es-CL");
                }
            }
        }

    }
}
