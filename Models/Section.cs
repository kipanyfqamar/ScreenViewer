using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ScreenViewer;
using ScreenViewer.Data;
using ScreenViewer.Models.Elements;

namespace ScreenViewer.Models
{
    public struct CriticalElements
    {
        public List<string> CriticalQuestions;
        public bool CriticalItems;
    }

    public class SectionViewLayout 
    {
        public ScriptSectionLayout SectionLayout { get; set; }
        public QuestionLayout QuestionLayout { get; set; }
        public ActionLayout ActionLayout { get; set; }
        public bool CriticalElement { get; set; }
        public string TextHTMLLayout { get; set; }
        public string ScriptLayout { get; set; }
        public string LinkLayout { get; set; }
        public string LinkGroupLayout { get; set; }
        public string OrderItemLayout { get; set; }
        public string OrderCartLayout { get; set; }
        public string DataViewLayout { get; set; }
        public string ImageLayout { get; set; }
        public string TaskLayout { get; set; }
    }

    public class ScriptLayoutE
    {
        public ScriptLayoutDetail Scriptlayoutdetail { get; set; }
        public bool CriticalElement { get; set; }

        public QuestionLayout QuestionLayout { get; set; }
  
        public string TextHTMLLayout { get; set; }
        public string LinkLayout { get; set; }

        public string AlignClass { get; set; }
     
    }
}