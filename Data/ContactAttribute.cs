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
    
    public partial class ContactAttribute
    {
        public int ContactAttributeId { get; set; }
        public int ContactId { get; set; }
        public string ContactAttributeName { get; set; }
        public string ContactAttributeValue { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string ClientId { get; set; }
    
        public virtual ContactRecord ContactRecord { get; set; }
    }
}
