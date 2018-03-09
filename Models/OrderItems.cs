using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ScreenViewer.SessionControl;
using System.Web.Http.Results;

namespace ScreenViewer.Models
{
    public class DisplayOrderItem
    {
        public Data.ScriptItem OrderItem { get; set; }
        public string setKey { get; set; }
        public string filterKey { get; set; }
        public Int32 quantity { get; set; }
    }




}