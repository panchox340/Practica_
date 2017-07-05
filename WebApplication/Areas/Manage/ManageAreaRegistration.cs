using System.Web.Mvc;

namespace WebApplication.Areas.Manage
{
    public class ManageAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Manage";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Manage_default",
                "{cliente}/Manage/{controller}/{action}/{id}",
                new { cliente = "PSO", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}