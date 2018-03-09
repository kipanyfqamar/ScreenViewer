using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Web.Mvc;
using System.Data.Entity;

using ScreenViewer.Data;

namespace ScreenViewer.Models
{
    public class SchedulerContactTaskService
    {
        private ScreenPlayCRMEntities db;

        public SchedulerContactTaskService(ScreenPlayCRMEntities context)
        {
            db = context;
        }

        public SchedulerContactTaskService()
            : this(new ScreenPlayCRMEntities())
        {
        }

        public virtual IQueryable<ContactTaskViewModel> GetRange(string clientId, DateTime start, DateTime end)
        {
            return GetAll(clientId).Where(t => (t.Start >= start || t.Start <= start) && t.Start <= end
                && t.End >= start && (t.End >= end || t.End <= end) || t.RecurrenceRule != null);
        }

        public virtual IQueryable<ContactTaskViewModel> GetRange(string clientId, string userId, DateTime start, DateTime end)
        {
            return GetAll(clientId, userId).Where(t => (t.Start >= start || t.Start <= start) && t.Start <= end
                && t.End >= start && (t.End >= end || t.End <= end) || t.RecurrenceRule != null);
        }

        public virtual IQueryable<ContactTaskViewModel> GetAll(string clientId)
        {
            return db.ContactTasks.Where(x=> x.ClientId == clientId).ToList().Select(task => new ContactTaskViewModel
            {
                TaskID = task.TaskId,
                TaskType = task.TaskType,
                ContactId = task.ContactId,
                Title = task.Title,
                Start = DateTime.SpecifyKind(task.Start.Value, DateTimeKind.Utc),
                End = DateTime.SpecifyKind(task.End.Value, DateTimeKind.Utc),
                StartTimezone = task.StartTimezone,
                EndTimezone = task.EndTimezone,
                Description = task.Description,
                IsAllDay = task.IsAllDay.Value,
                RecurrenceRule = task.RecurrenceRule,
                RecurrenceException = task.RecurrenceException,
                UserID = task.UserId,
                LeadID = task.LeadId,
                ClientID = task.ClientId,
                CreatedDate = task.CreatedDate,
                ModifiedDate = task.ModifiedDate
            }).AsQueryable();
        }

        public virtual IQueryable<ContactTaskViewModel> GetAll(string clientId, string userId)
        {
            return db.ContactTasks.Where(x => x.ClientId == clientId && x.UserId == userId).ToList().Select(task => new ContactTaskViewModel
            {
                TaskID = task.TaskId,
                TaskType = task.TaskType,
                TaskColor = task.TaskColor,
                ContactId = task.ContactId,
                Title = task.Title,
                Start = DateTime.SpecifyKind(task.Start.Value, DateTimeKind.Utc),
                End = DateTime.SpecifyKind(task.End.Value, DateTimeKind.Utc),
                StartTimezone = task.StartTimezone,
                EndTimezone = task.EndTimezone,
                Description = task.Description,
                IsAllDay = task.IsAllDay.Value,
                RecurrenceRule = task.RecurrenceRule,
                RecurrenceException = task.RecurrenceException,
                UserID = task.UserId,
                LeadID = task.LeadId,
                ClientID = task.ClientId,
                CreatedDate = task.CreatedDate,
                ModifiedDate = task.ModifiedDate
            }).AsQueryable();
        }

        public virtual bool IsExist(string userId, string clientId, DateTime start, DateTime end)
        {
            return db.ContactTasks.Any(x => x.UserId == userId && x.ClientId == clientId && x.Start <= end && x.End >= start);
        }

        public virtual LeadRecord GetLead(string leadId)
        {
            return db.LeadRecords.Find(leadId);
        }

        public virtual void UpdateLead(LeadRecord leadRecord)
        {
            try
            {
                db.Entry(leadRecord).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public virtual List<Resource> GetTaskTypeByClientID(string clientId)
        //{

        //    List<Resource> resource = (from cu in db.ContactTasks

        //                               where cu.ClientId == clientId
        //                               select new Resource
        //                               {
        //                                   Value = cu.TaskType,
        //                                   Name = cu.TaskType,
        //                                   Color = cu.TaskColor
        //                               }).Distinct().ToList<Resource>();

        //    return resource;

        //}

        public virtual void Insert(ContactTaskViewModel task, ModelStateDictionary modelState)
        {
            if (ValidateModel(task, modelState))
            {
                var entity = task.ToEntity();

                db.ContactTasks.Add(entity);
                db.SaveChanges();

                task.TaskID = entity.TaskId;
            }
        }

        public virtual void Update(ContactTaskViewModel task, ModelStateDictionary modelState)
        {
            if (ValidateModel(task, modelState))
            {
                var entity = task.ToEntity();
                db.ContactTasks.Attach(entity);
                db.Entry(entity).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public virtual void Delete(ContactTaskViewModel task, ModelStateDictionary modelState)
        {
            var entity = task.ToEntity();
            db.ContactTasks.Attach(entity);

            var recurrenceExceptions = db.ContactTasks.Where(t => t.TaskId == task.TaskID);

            foreach (var recurrenceException in recurrenceExceptions)
            {
                db.ContactTasks.Remove(recurrenceException);
            }

            db.ContactTasks.Remove(entity);
            db.SaveChanges();
        }

        private bool ValidateModel(ContactTaskViewModel appointment, ModelStateDictionary modelState)
        {
            if (appointment.Start > appointment.End)
            {
                modelState.AddModelError("errors", "End date must be greater or equal to Start date.");
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }

    public class FilterRange
    {
        private DateTime start;
        public DateTime Start
        {
            get
            {
                return start;
            }
            set
            {
                start = value.ToUniversalTime();
            }
        }

        private DateTime end;
        public DateTime End
        {
            get
            {
                return end;
            }
            set
            {
                end = value.ToUniversalTime();
            }
        }


    }
}