//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MailApp.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class ProdxProy
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ProdxProy()
        {
            this.VersionProyecto = new HashSet<VersionProyecto>();
        }
    
        public int id_ProdxProy { get; set; }
        public Nullable<int> Principal { get; set; }
        public Nullable<System.DateTime> FechaUltimoDeploy { get; set; }
        public Nullable<System.DateTime> FechaVencimientoSoporte { get; set; }
        public Nullable<int> SoporteActivo { get; set; }
        public Nullable<int> id_Proyectos { get; set; }
        public Nullable<int> id_Producto { get; set; }
    
        public virtual Producto Producto { get; set; }
        public virtual Proyecto Proyecto { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VersionProyecto> VersionProyecto { get; set; }
    }
}
