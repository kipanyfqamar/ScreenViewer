using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ScreenViewer.Models
{
    public enum NodeType { Start, Section, Workflow, ActionOnly, SignPost, PreviousWorkflow };
    public enum ConditionType { Default, Clause, Question, DataObject, None, LinkTo };

    //public struct wfHistory
    //{
    //    public int WorkFlowID;
    //    public string WorkFlowName;
    //    public int NodeID;
    //    public string SectionName;
    //}

    public class Workflow
    {
        public Int32 WorkflowID { get; set; }
        public string WorkflowName { get; set; }
        public string WorkflowTile { get; set; }
        public DateTime WorkflowStartDate { get; set; }
        public DateTime WorkflowEndDate { get; set; }
        public Int32 WorkflowVersion { get; set; }
        public string WorkflowAC { get; set; }
        public bool UseRange { get; set; }
        public bool CurrentVersion { get; set; }
        public WorkflowNodes WFNodes { get; set; }
        public bool Changed { get; set; }
        public Int32 DataObject { get; set; }
        public Int32 CurrentNode { get; set; }
        public bool AltLanguage { get; set; }
        public Int32 DefaultMenu { get; set; }
    }

    public class WorkflowNodes
    {
        public WFNodeInfo[] Nodes { get; set; }
    }

    public class WFNodeInfo
    {
        public NodeType nodeType { get; set; }
        public string nodeName { get; set; } //section or workflow name 
        public Int32 NodeUniqueID { get; set; }
        public bool startNode { get; set; }
        public bool Painted { get; set; }
        public string nodeActions { get; set; } //actions to run before section or workflow is loaded
        public WFCondition[] Conditions { get; set; }
        public int DocUID { get; set; }
    }

    public class WFReturn
    {
        public Int32 returnID { get; set; }
        public WFNodeInfo NewNode { get; set; }
        public Int32 CurrentNodeUniqueID { get; set; }
        public bool CurrentNode { get; set; }
    }

    public class WFCondition
    {
        public ConditionType conditionType { get; set; }
        public Int32 conditionID { get; set; }
        public Int32 conditionpriority { get; set; }
        public int ClauseID { get; set; }
        public string ClauseName { get; set; }
        public int Question { get; set; }
        public string QuestionResponse { get; set; }
        public string QText { get; set; }
        public string DataObject { get; set; }
        public string DataObjectValue { get; set; }
        public int Result { get; set; }
        public Int32 linktoNode { get; set; }
        public WFReturn ReturnNode { get; set; }
    }

    public class WorkFlowActionXMLObject
    {
        public string SetObj { get; set; }
        public string SetValue { get; set; }
    }

    public class WorkflowDisplay
    {
        public string workflowID { get; set; }
        public string workflowName { get; set; }
        public string sectionName { get; set; }
        public Boolean showNext { get; set; }
        public Boolean showPrevious { get; set; }
        public Boolean showLanguage { get; set; }
        
        public Boolean Search { get; set; }

        public WFNodeInfo nextNode { get; set; }
        public string callNotes { get; set; }
        public string Layout { get; set; }
        public string Notifications { get; set; }
        public string menuHTML { get; set; }

        public string ContactID { get; set; }
        public string CurrentNodeId { get; set; }
        public string ExceptionMessage { get; set; }
        public string ClassName { get; set; }
        public string MethodName { get; set; }
        public string StackTrace { get; set; }

    }

    public class DashBoardDisplay
    {
        public string DashboardID { get; set; }
        public string DashboardName { get; set; }
        public string sectionName { get; set; }
        public int sectionID { get; set; }
        public string Layout { get; set; }
        public string Notifications { get; set; }
        public string menuHTML { get; set; }
    }
    // GET: Dashboard/Create

}
