using System.Web.Mvc;

namespace WebApplication.Areas.Mantencion
{
    public class MantencionAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Mantencion";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Mantencion_default",
                "{cliente}/Mantencion/{controller}/{action}/{id}",
                new { cliente = "PSO", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}