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
    
    public partial class Contrato
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Contrato()
        {
            this.ProdxContrato = new HashSet<ProdxContrato>();
        }
    
        public int id_Contrato { get; set; }
        public Nullable<System.DateTime> FecInicSoportePactado { get; set; }
        public Nullable<System.DateTime> FecFinSoportePactado { get; set; }
        public Nullable<bool> bActivo { get; set; }
        public Nullable<int> HorasContrato { get; set; }
        public Nullable<int> id_Empresa { get; set; }
    
        public virtual Empresa Empresa { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProdxContrato> ProdxContrato { get; set; }
    }
}