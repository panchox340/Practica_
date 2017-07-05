using System.Web.Mvc;

namespace WebApplication.Areas.Remuneraciones
{
    public class RemuneracionesAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Remuneraciones";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Remuneraciones_default",
                "{cliente}/Remuneraciones/{controller}/{action}/{id}",
                new { cliente = "PSO", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}