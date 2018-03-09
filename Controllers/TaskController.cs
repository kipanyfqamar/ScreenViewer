using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System.Web.Http.Results;
using ScreenViewer.Models;
using ScreenViewer.Data;

namespace ScreenViewer.Controllers
{
    public class TaskController : Controller
    {
        private SchedulerContactTaskService taskService;

        public TaskController()
        {
            taskService = new SchedulerContactTaskService();
        }

        public string Render(int ElementId, ControllerContext ContCont)
        {
            API.Elements.TaskController taskController = new API.Elements.TaskController();
            var actionResult = taskController.GetTask((int)ElementId);

            if (actionResult != null && actionResult != actionResult as System.Web.Http.Results.NotFoundResult)
            {
                ScriptTask task = (actionResult as OkNegotiatedContentResult<ScriptTask>).Content;
                ViewBag.TaskType = task.TaskType;
                ViewBag.WorkDayStart = DateTime.Parse(string.Format("{0} {1}", DateTime.Now.Date.ToShortDateString(), task.WorkStartTime));
                ViewBag.WorkDayEnd = DateTime.Parse(string.Format("{0} {1}", DateTime.Now.Date.ToShortDateString(), task.WorkEndTime));
                ViewBag.Timezone = task.Timezone;
                ViewBag.IsMultiUserTask = task.IsMultiUserTask;
                ViewBag.DeleteEnabled = task.DeleteEnabled;
                ViewBag.UpdateEnabled = task.UpdateEnabled;

                ViewBag.AddCustomButton = task.AddCustomButton;
                ViewBag.CustomButtonText = task.CustomButtonText;
                ViewBag.CustomButtonURL = task.CustomButtonURL;

                ViewBag.ShowStartDate = task.ShowStartDate;
                ViewBag.ShowEndDate = task.ShowEndDate;
                ViewBag.ShowAllDay = task.ShowAllDay;
                ViewBag.ShowRepeatRule = task.ShowRepeatRule;

                ViewBag.DayView = false;
                ViewBag.WorkWeekView = false;
                ViewBag.WeekView = false;
                ViewBag.MonthView = false;
                ViewBag.AgendaView = false;

                switch (task.DefaultView)
                {
                    case "DayView":
                        ViewBag.DayView = true;
                        break;
                    case "WorkWeekView":
                        ViewBag.WorkWeekView = true;
                        break;
                    case "WeekView":
                        ViewBag.WeekView = true;
                        break;
                    case "MonthView":
                        ViewBag.MonthView = true;
                        break;
                    case "AgendaView":
                        ViewBag.AgendaView = true;
                        break;
                    default:
                        ViewBag.WorkWeekView = true;
                        break;
                }

                ViewBag.LeadName = string.Empty;
                SessionControl.SessionManager.StoreScriptParameter("FilterTask", task.FilterTask, ContCont.HttpContext.Session);

                if (task.EndTimeMinute != 0)
                    SessionControl.SessionManager.StoreScriptParameter("EndTimeMinute", task.EndTimeMinute.ToString(), ContCont.HttpContext.Session);

                if (!string.IsNullOrEmpty(SessionControl.SessionManager.GetProgramParameterByKey("LeadID", ContCont.HttpContext.Session)))
                {
                    LeadRecord lead = taskService.GetLead(SessionControl.SessionManager.GetProgramParameterByKey("LeadID", ContCont.HttpContext.Session));
                    if (lead != null)
                    {
                        ViewBag.LeadName = string.Format("{0} {1}", lead.FirstName, lead.LastName);
                    }
                }

                string clientId = SessionControl.SessionManager.GetClientId(ContCont.HttpContext.Session);
                string userId = SessionControl.SessionManager.GetUserId(ContCont.HttpContext.Session);
                IQueryable<ContactTaskViewModel> lstTask = null;

                switch (task.FilterTask)
                {
                    case "CLIENT":
                        lstTask = taskService.GetAll(clientId);
                        break;
                    case "USER":
                        lstTask = taskService.GetAll(clientId, userId);
                        break;
                }

                return RenderHelper.RenderViewToString(ContCont, "~/Views/Task/Index.cshtml", lstTask, ViewData);
            }

            return "";
        }

        public ActionResult Index()
        {
            return PartialView();
        }

        public virtual JsonResult Read(DataSourceRequest request, FilterRange range)
        {
            if (range.Start == DateTime.MinValue  && range.End == DateTime.MinValue)
            {
                DateTime date = DateTime.Now;
                range.Start = FirstDayOfWeek(DateTime.Now);
                range.End = LastDayOfWeek(DateTime.Now);
            }

            string FilterTask = SessionControl.SessionManager.GetScriptParameterByKey("FilterTask", HttpContext.Session);
            string clientId = SessionControl.SessionManager.GetClientId(HttpContext.Session);
            string userId = SessionControl.SessionManager.GetUserId(HttpContext.Session);
            IQueryable<ContactTaskViewModel> lstTask = null;

            switch (FilterTask)
            {
                case "CLIENT":
                    lstTask = taskService.GetAll(clientId);
                    break;
                case "USER":
                    lstTask = taskService.GetAll(clientId, userId);
                    break;
            }

            return Json(lstTask.ToDataSourceResult(request));
        }

        public virtual JsonResult Destroy([DataSourceRequest] DataSourceRequest request, ContactTaskViewModel task)
        {
            if (ModelState.IsValid)
            {
                taskService.Delete(task, ModelState);
            }

            return Json(new[] { task }.ToDataSourceResult(request, ModelState));
        }

        public virtual JsonResult Create([DataSourceRequest] DataSourceRequest request, ContactTaskViewModel task)
        {
            if (ModelState.IsValid)
            {
                task.TaskType = "VIRTUAL TOUR";
                task.ContactId = SessionControl.SessionManager.GetContactId(Session);
                task.ClientID = SessionControl.SessionManager.GetClientId(HttpContext.Session);
                task.LeadID = SessionControl.SessionManager.GetProgramParameterByKey("LeadID", HttpContext.Session);
                task.CreatedDate = DateTime.Now;
                task.ModifiedDate = DateTime.Now;

                string EndTimeMinute = SessionControl.SessionManager.GetScriptParameterByKey("EndTimeMinute", HttpContext.Session);

                if (!string.IsNullOrEmpty(EndTimeMinute))
                {
                    task.End = task.Start.AddMinutes(Convert.ToDouble(EndTimeMinute));
                }

                if (taskService.IsExist(task.UserID, task.ClientID, task.Start, task.End))
                {
                    ModelState.AddModelError("errors", "This user is not available in this time period.");
                }
                else
                {
                    taskService.Insert(task, ModelState);

                    LeadRecord lead = taskService.GetLead(SessionControl.SessionManager.GetProgramParameterByKey("LeadID", HttpContext.Session));
                    lead.LeadOwner = task.UserID;
                    lead.ModifiedDate = DateTime.Now;
                    taskService.UpdateLead(lead);
                }
            }

            return Json(new[] { task }.ToDataSourceResult(request, ModelState));
        }

        public virtual JsonResult Update([DataSourceRequest] DataSourceRequest request, ContactTaskViewModel task)
        {
            if (ModelState.IsValid)
            {
                task.ModifiedDate = DateTime.Now;
                taskService.Update(task, ModelState);
            }

            return Json(new[] { task }.ToDataSourceResult(request, ModelState));
        }

        public ActionResult GetUsers()
        {
            List<UserResource> users = null;
            users = ScreenViewer.ClientHelper.GetAllUserByClientID(SessionControl.SessionManager.GetClientId(HttpContext.Session));

            if (users != null)
                users.Insert(0, new UserResource { UserName = string.Empty, UserValue = string.Empty, UserColor = string.Empty });

            return Json(users, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTaskType()
        {
            List<TaskTypeResource> types = null;
            types = ScriptHelper.GetTaskTypeByClientID(SessionControl.SessionManager.GetClientId(HttpContext.Session));

            return Json(types, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            taskService.Dispose();
            base.Dispose(disposing);
        }

        public static DateTime FirstDayOfWeek(DateTime date)
        {
            DayOfWeek fdow = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            int offset = fdow - date.DayOfWeek;
            DateTime fdowDate = date.AddDays(offset);
            return fdowDate;
        }

        public static DateTime LastDayOfWeek(DateTime date)
        {
            DateTime ldowDate = FirstDayOfWeek(date).AddDays(6);
            return ldowDate;
        }
    }
}