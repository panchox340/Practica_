using System.Web.Mvc;

namespace WebApplication.Areas.Certificados
{
    public class CertificadosAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Certificados";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Certificados_default",
                "{cliente}/Certificados/{controller}/{action}/{id}",
                new { cliente = "PSO", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}