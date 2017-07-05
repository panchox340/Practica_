using System.Web.Mvc;

namespace WebApplication.Areas.Liquidaciones
{
    public class LiquidacionesAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Liquidaciones";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Liquidaciones_default",
                "{cliente}/Liquidaciones/{controller}/{action}/{id}",
                new { cliente = "PSO", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}