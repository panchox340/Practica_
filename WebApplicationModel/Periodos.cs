//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebApplicationModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class Periodos
    {
        public int id_periodo { get; set; }
        public Nullable<System.DateTime> fecha_fin_novedades { get; set; }
        public Nullable<System.DateTime> fecha_inicio { get; set; }
        public Nullable<System.DateTime> fecha_fin { get; set; }
        public Nullable<int> id_cliente { get; set; }
    
        public virtual Cliente Cliente { get; set; }
    }
}