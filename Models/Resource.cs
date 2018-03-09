using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ScreenViewer.Models
{
    public class UserResource
    {
        public string UserName { get; set; }
        public string UserValue { get; set; }
        public string UserColor { get; set; }
    }

    public class TaskTypeResource
    {
        public string TaskName { get; set; }
        public string TaskValue { get; set; }
        public string TaskColor { get; set; }
    }
}