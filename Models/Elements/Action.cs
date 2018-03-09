using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ScreenViewer.Models
{
    public class ActionLayout
    {
        public string actionId { get; set; }
        public string actionName { get; set; }
        public string actionDisplay { get; set; }
        public bool IsActionClicked { get; set; }
        public string AgentId { get; set; }
        public string LeadId { get; set; }
    }
}