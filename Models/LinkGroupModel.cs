using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ScreenViewer.Models
{
    public class LinkGroupModel
    {
    }

    public enum ElementTypes
    {
        None,
        Question,
        Link,
        LinkGroup,
        Redirect,
        TextHTML,
        Image,
        Action,
        Questionare,
        OrderItem,
        FAQSearch
    }

    [Serializable]
    [XmlRoot("ul")]
    public partial class ScriptUL
    {
        public ScriptUL() { LIArray = new ScriptLI[0]; }

        [XmlElement(ElementName = "li")]
        public ScriptLI[] LIArray
        {
            get;
            set;
        }
    }
    [Serializable]
    public class ScriptLI
    {
        [XmlAttribute(AttributeName = "IsListItem")]
        public bool IsListItem { get; set; }

        [XmlAttribute(AttributeName = "Text")]
        public string Text { get; set; }

        [XmlAttribute(AttributeName = "ElementType")]
        public ElementTypes ElementType { get; set; }

        [XmlAttribute(AttributeName = "ElementID")]
        public string ElementID { get; set; }

        [XmlElement("ul")]
        public ScriptUL theUL { get; set; }
    }
}