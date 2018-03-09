using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kendo.Mvc.UI;
using ScreenViewer.Data;

namespace ScreenViewer.Models
{
    public class ContactTaskViewModel : ISchedulerEvent
    {
        public int TaskID { get; set; }
        public int ContactId { get; set; }
        public string TaskType { get; set; }
        public string TaskColor { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

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

        public string StartTimezone { get; set; }

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

        public string EndTimezone { get; set; }
        public string RecurrenceRule { get; set; }
        public int? RecurrenceID { get; set; }
        public string RecurrenceException { get; set; }
        public bool IsAllDay { get; set; }
        public string UserID { get; set; }
        public string LeadID { get; set; }
        public string ClientID { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        public ContactTask ToEntity()
        {
            return new ContactTask
            {
                TaskId = TaskID,
                TaskType = TaskType,
                TaskColor = TaskColor,
                ContactId = ContactId,
                Title = Title,
                Start = Start,
                StartTimezone = StartTimezone,
                End = End,
                EndTimezone = EndTimezone,
                Description = Description,
                RecurrenceRule = RecurrenceRule,
                RecurrenceException = RecurrenceException,
                IsAllDay = IsAllDay,
                RecurrenceID = RecurrenceID,
                UserId = UserID,
                LeadId = LeadID,
                ClientId = ClientID,
                CreatedDate = CreatedDate,
                ModifiedDate = ModifiedDate
            };
        }
    }
}