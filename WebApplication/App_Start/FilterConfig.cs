using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WebApplication.Controllers;
using Circon.Mvc.Helpers;
namespace WebApplication
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new ClientAuthorizeAttribute());
        }
    }

    public class CustomLayoutAjaxAttribute : ActionFilterAttribute, IResultFilter
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var viewResult = filterContext.Result as ViewResult;
            if (viewResult != null)
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                  
                    viewResult.MasterName = null;
                }
                else
                {
                    try
                    {
                        if(filterContext.Controller.ViewBag.NeedLayaout != null)
                        {
                            viewResult.MasterName = null;
                        }
                        else
                        {
                            if (filterContext.HttpContext.Session[2].ToString() == (filterContext.HttpContext.Session["usuario"] as Usuario).id_usu.ToString())
                            {
                                viewResult.MasterName = "~/Views/Shared/_Layout2.cshtml";
                            }
                            else
                            {
                                viewResult.MasterName = "~/Views/Shared/_Layout.cshtml";
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        viewResult.MasterName = "~/Views/Shared/_Layout.cshtml";
                        MvcApplication.LogError(e);
                    }
                }
            }
        }
    }
              
    public class ClientAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            if (filterContext.Result is HttpUnauthorizedResult)
            {
                var redirect = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                    { "cliente", filterContext.RouteData.Values[ "cliente" ] },
                    { "area", "" },
                    { "controller", "Account" },
                    { "action", "Login" },
                    { "ReturnUrl", filterContext.HttpContext.Request.RawUrl }
                    });
                filterContext.Result = redirect;
            }
        }
    }
}
