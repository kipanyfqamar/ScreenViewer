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
    
    public partial class ScriptClientUser
    {
        public int ScriptClientUserID { get; set; }
        public string ScriptClientID { get; set; }
        public string UserID { get; set; }
        public System.DateTime DateCreated { get; set; }
        public System.DateTime DateLastModified { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual ScriptClient ScriptClient { get; set; }
    }
}
