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
    
    public partial class EstadoTicket
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public EstadoTicket()
        {
            this.HistorialRespuesta = new HashSet<HistorialRespuesta>();
            this.Ticket = new HashSet<Ticket>();
        }
    
        public int id_EstadoTicket { get; set; }
        public Nullable<int> id_Responsable { get; set; }
        public string nomEstadoTicket { get; set; }
    
        public virtual ResponsableTicket ResponsableTicket { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<HistorialRespuesta> HistorialRespuesta { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Ticket> Ticket { get; set; }
    }
}
