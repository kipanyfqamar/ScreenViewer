//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ScreenViewer.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class ScriptLayout
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ScriptLayout()
        {
            this.ScriptLayoutDetails = new HashSet<ScriptLayoutDetail>();
        }
    
        public int ScriptLayoutID { get; set; }
        public string LayoutDesc { get; set; }
        public string LayoutWidth { get; set; }
        public System.DateTime DateCreated { get; set; }
        public System.DateTime DateLastModified { get; set; }
        public string UserLastModified { get; set; }
        public string ClientID { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ScriptLayoutDetail> ScriptLayoutDetails { get; set; }
    }
}