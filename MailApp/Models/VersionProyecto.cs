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
    
    public partial class VersionProyecto
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public VersionProyecto()
        {
            this.TicketVersionAfectada = new HashSet<TicketVersionAfectada>();
        }
    
        public int id_VersionProyecto { get; set; }
        public string nomVersion { get; set; }
        public Nullable<int> ActivaEnProd { get; set; }
        public Nullable<int> id_ProdxProy { get; set; }
    
        public virtual ProdxProy ProdxProy { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TicketVersionAfectada> TicketVersionAfectada { get; set; }
    }
}