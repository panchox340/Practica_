using System.Web.Mvc;

namespace WebApplication.Areas.Solicitudes
{
    public class SolicitudesAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Solicitudes";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Solicitudes_default",
                "{cliente}/Solicitudes/{controller}/{action}/{id}",
                new {cliente = "PSO", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}