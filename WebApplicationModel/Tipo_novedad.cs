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
    
    public partial class Tipo_novedad
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Tipo_novedad()
        {
            this.Novedad = new HashSet<Novedad>();
        }
    
        public int id_tipo { get; set; }
        public string nombre { get; set; }
        public bool valorizado { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Novedad> Novedad { get; set; }
    }
}
