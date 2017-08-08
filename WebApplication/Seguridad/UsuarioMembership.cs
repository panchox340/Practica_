using AutenticacionPersonalizada.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace WebApplication.Seguridad
{
    public class UsuarioMembership:MembershipUser
    {
        public string id_usu { get; set; }
        public string Nom_usu { get; set; }
        public string pass_usu { get; set; }
        public int id_cliente { get; set; }
        public int id_tipo_usu { get; set; }
        public string email { get; set; }
        public System.DateTime fechaIngreso { get; set; }


        //public UsuarioMembership(Usuario us)
        //{
        //    id_usu = us.id_usu;
        //    Nom_usu = us.Nom_usu;
        //    pass_usu = us.pass_usu;
        //    id_cliente = us.id_cliente;
        //    id_tipo_usu = us.id_tipo_usu;
        //    email = us.email;
        //    fechaIngreso = us.fechaIngreso;
        //}
    }
}