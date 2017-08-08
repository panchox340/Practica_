using Circon.Mvc.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using WebApplication.Seguridad;

namespace WebApplication.Controllers
{
    
    public class AccountController : _Controller
    {
        [AllowAnonymous]
        public ActionResult Login(string cliente, string returnUrl)
        {
            var _cliente = _db.Cliente.SingleOrDefault(p => p.Nom_cor_emp == cliente);
            if(_cliente == null)
            {
                ViewBag.Cliente = "N";
            }else
            {
                ViewBag.Cliente = "S";
            }
            ViewBag.returnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string cliente, /*Usuario model,*/ string returnUrl, string RememberMe) {

            if (Membership.ValidateUser("",""/*model.id_usu, model.pass_usu)*/))
            {
                //SesionCliente(cliente);
                FormsAuthentication.SetAuthCookie(""/*model.id_usu*/, false);

                var AuthCookie = Response.Cookies[FormsAuthentication.FormsCookieName];

                var AuthCookieClient = new HttpCookie("AuthCookieClient");
                AuthCookieClient.Values["client_auth"] = cliente;
                AuthCookieClient.Expires = AuthCookieClient.Expires;
                Response.Cookies.Add(AuthCookieClient);
                return RedirectToLocal(cliente,  returnUrl);
            }
            return View(/*model*/);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {

            string cliente = "";//SesionCliente().Nom_cor_emp;

            FormsAuthentication.SignOut();
            Session.Abandon();

            HttpCookie cookie1 = new HttpCookie(FormsAuthentication.FormsCookieName, "");
            cookie1.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(cookie1);

            HttpCookie cookie2 = new HttpCookie("AuthCookieClient", "");
            cookie2.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(cookie2);


            SessionStateSection sessionStateSection = (SessionStateSection)WebConfigurationManager.GetSection("system.web/sessionState");
            HttpCookie cookie3 = new HttpCookie(sessionStateSection.CookieName, "");
            cookie3.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(cookie3);

            return RedirectToLocal(cliente, "");
        }


        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }
        }
    }
}