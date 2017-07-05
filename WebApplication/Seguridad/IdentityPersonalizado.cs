using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Security;

namespace WebApplication.Seguridad
{
    public class IdentityPersonalizado : IIdentity
    {
        public string Name
        {
            get { return id_usu; }
        }

        public string AuthenticationType
        {
            get { return Identity.AuthenticationType; }
        }

        public bool IsAuthenticated
        {
            get { return Identity.IsAuthenticated; }
        }

        public string id_usu { get; set; }
        public string Nom_usu { get; set; }
        public string pass_usu { get; set; }
        public int id_cliente { get; set; }
        public int id_tipo_usu { get; set; }

        public static explicit operator IdentityPersonalizado(ClaimsIdentity v)
        {
            throw new NotImplementedException();
        }

        public string email { get; set; }
        public System.DateTime fechaIngreso { get; set; }

        public IIdentity Identity { get; set; }

        public IdentityPersonalizado(IIdentity identity)
        {
            this.Identity = identity;
            var us = Membership.GetUser(identity.Name) as UsuarioMembership;

            id_usu = us.id_usu;
            Nom_usu = us.Nom_usu;
            pass_usu = us.pass_usu;
            id_cliente = us.id_cliente;
            id_tipo_usu = us.id_tipo_usu;
            email = us.email;
            fechaIngreso = us.fechaIngreso;
        }
    }
}