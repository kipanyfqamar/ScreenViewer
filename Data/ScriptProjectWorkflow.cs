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
    
    public partial class ScriptProjectWorkflow
    {
        public int ScriptProjectWorkflowID { get; set; }
        public int ScriptProjectID { get; set; }
        public int ScriptWorkflowID { get; set; }
        public System.DateTime ScriptWorkflowActiveDate { get; set; }
    
        public virtual ScriptProject ScriptProject { get; set; }
        public virtual ScriptWorkflow ScriptWorkflow { get; set; }
    }
}
