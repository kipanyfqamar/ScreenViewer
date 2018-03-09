using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ScreenViewer.Models
{
    public class SessionModel
    {

        public List<wfHistory> WorkflowHistory {get;set;}

        public List<ItemKeyVal> ItemKeys  {get;set;}

        public List<QuestVal> questionResponses {get;set;}

        public List<ItemOrdered> ItemsInCart { get; set; }


    }

        [Serializable]
        public struct ItemKeyVal
        {
            public string ItemKeyCode;
            public string[] ItemKeyValues;
        }

        [Serializable]
        public struct QuestVal
        {
            public string QuestionID;
            public string Response;
            public string DisplayResponse;
            public string QuestionText;
            public string QuestKeyCodes;
            [System.Xml.Serialization.XmlIgnoreAttribute]
            public QuestResponse[] Responses;
        }
        [Serializable]
        public struct QuestResponse
        {
            public string Response;
            public string DisplayResponse;
            public int ScriptPage;
            public int Workflow;
            public int Section;
        }



        [Serializable]
        public struct wfHistory
        {
            public int WorkFlowID;
            public string WorkFlowName;
            public int NodeID;
            public string SectionName;
        }

        [Serializable]
        public class ItemOrdered
        {
            public string ItemCode { get; set; }
            public string ItemName { get; set; }
            public string ItemDesc { get; set; }
            public int ItemQuantity { get; set; }
            public decimal ItemPrice { get; set; }
            public decimal ItemShipping { get; set; }
            public decimal LineTotal 
            { get
                {
                    return (ItemPrice + ItemShipping) * ItemQuantity;
                }
            }
            public string Category { get; set; }
            public string SubCategory { get; set; }
            public string SetKeys { get; set; }
            public bool setKey { get; set; }
            public Int32 oiOwner { get; set; }
        }

        [Serializable]
        public class ProgramParamete
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }
        
    }
