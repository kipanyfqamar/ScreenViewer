using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using ScreenViewer.Data;
using System.Data.Entity.Validation;

namespace ScreenViewer.API.ExternalData
{
    [RoutePrefix("api/ContactRecord")]
    public class ContactRecordController : ApiController
    {
        private ScreenPlayCRMEntities db = new ScreenPlayCRMEntities();

        // GET api/ContactRecord/5
        [ResponseType(typeof(ContactRecord))]
        public IHttpActionResult GetContactRecordByContactId(int contactId)
        {
            ContactRecord contactRecord = db.ContactRecords.Find(contactId);
            if (contactRecord == null)
            {
                return NotFound();
            }

            return Ok(contactRecord);
        }

        // GET api/ContactRecord/5
        [ResponseType(typeof(ContactRecord))]
        public IHttpActionResult GetContactRecordByLeadClient(string leadId, string clientCallId)
        {
            var today = System.DateTime.Now.AddMinutes(-5);

            //ContactRecord contactRecord = db.ContactRecords.Where(x=> x.LeadRecordId == leadId && x.ClientCallId == clientCallId && x.DispositionCode == null &&  x.CallStartDateTime.Value > today).FirstOrDefault();
            ContactRecord contactRecord = db.ContactRecords.Where(x => x.LeadRecordId == leadId && x.ClientCallId == clientCallId && x.CallStartDateTime.Value > today).FirstOrDefault();

            if (contactRecord == null)
            {
                return NotFound();
            }

            return Ok(contactRecord);
        }

        // GET api/ContactRecord/5
        public Int32 GetLatestContactRecord(string leadID)
        {
            Int32 crec = (from ContactRecord in db.ContactRecords
                          where ContactRecord.LeadRecordId == leadID
                          orderby ContactRecord.CallStartDateTime descending
                          select ContactRecord.ContactId).First();

            return crec;
        }


        [ResponseType(typeof(ContactRecord))]
        public IHttpActionResult GetContactRecordByLeadId(string leadId)
        {
            ContactRecord contactRecord = db.ContactRecords.Where(x => x.LeadRecordId == leadId).OrderByDescending(y => y.ContactId).LastOrDefault(); // Remove condition && x.DispositionCode != null

            if (contactRecord == null)
            {
                return NotFound();
            }

            return Ok(contactRecord);
        }

        [ResponseType(typeof(ContactRecordDetail[]))]
        public IHttpActionResult GetContactRecordDetailByLeadId(string leadId)
        {
            int[] contactIds = (from t in db.ContactRecords
                                    where t.LeadRecordId == leadId
                                    select new 
                                    {
                                        t.ContactId,
                                    }.ContactId).ToArray();

            ContactRecordDetail[] contactRecordDetail = db.ContactRecordDetails.Where(x => contactIds.Contains(x.ContactId)).ToArray();

            if (contactRecordDetail == null)
            {
                return NotFound();
            }

            return Ok(contactRecordDetail);
        }

        [Route("RegisterContact")]
        [ResponseType(typeof(ContactRecord))]
        public IHttpActionResult PostContactRecord(ContactRecord contactRecord)
        {
            //LeadRecord leadRecord = db.LeadRecords.Find(contactRecord.LeadRecordID);


                db.ContactRecords.Add(contactRecord);
                db.SaveChanges();
                return Ok(contactRecord);

        }

        [Route("UpdateContact")]
        public IHttpActionResult PutContactRecord(ContactRecord contactRecord)
        {
            string validationErrors = "";
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentContactRecord = db.ContactRecords.Find(contactRecord.ContactId);
                db.Entry(currentContactRecord).CurrentValues.SetValues(contactRecord);
                db.Entry(currentContactRecord).State = EntityState.Modified;
                db.SaveChanges();
                
            }

            catch (DbEntityValidationException e)
            {

                foreach (DbEntityValidationResult eve in e.EntityValidationErrors)
                {


                    foreach (DbValidationError ve in eve.ValidationErrors)
                    {
                        string error = ve.PropertyName + " - " + ve.ErrorMessage;
                        validationErrors += System.Environment.NewLine + error;
                    }

                }

                string x = validationErrors;
            }
            return StatusCode(HttpStatusCode.NoContent);

        }

        [Route("UpdateContactDetails")]
        public IHttpActionResult PutContactRecord(ICollection<ContactRecordDetail> contactRecordDetail)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            int contactId = contactRecordDetail.FirstOrDefault().ContactId;

            List<ContactRecordDetail> lstDetails = db.ContactRecordDetails.Where(x => x.ContactId.Equals(contactId)).ToList();

            foreach (var item in contactRecordDetail)
            {
                ContactRecordDetail detail = lstDetails.Where(x => x.QuestionId.Equals(item.QuestionId) && x.QuestionText.Equals(item.QuestionText)).FirstOrDefault();
                if (detail != null)
                {
                    item.ContactRecordDetailId = detail.ContactRecordDetailId;
                    db.Entry(detail).CurrentValues.SetValues(item);
                    db.Entry(detail).State = EntityState.Modified;
                }
                else
                {
                    if (item.ContactRecordDetailId.Equals(0) && !string.IsNullOrEmpty(item.QuestionResponseText))
                        db.ContactRecordDetails.Add(item);
                }
            }

            db.SaveChanges();
            return StatusCode(HttpStatusCode.NoContent);
        }


        [ResponseType(typeof(ContactVariable))]
        public IHttpActionResult PostContactVariable(ContactVariable contactVariable)
        {
            string validationErrors = "";
            try
            {
                db.ContactVariables.Add(contactVariable);
                db.SaveChanges();
                return Ok(contactVariable);
            }
            catch (DbEntityValidationException e)
            {
                foreach (DbEntityValidationResult eve in e.EntityValidationErrors)
                {
                    foreach (DbValidationError ve in eve.ValidationErrors)
                    {
                        string error = ve.PropertyName + " - " + ve.ErrorMessage;
                        validationErrors += System.Environment.NewLine + error;
                    }
                }

                string x = validationErrors;
            }
            return StatusCode(HttpStatusCode.NoContent);
        }

        [ResponseType(typeof(ContactNavigation))]
        public IHttpActionResult PostContactNavigation(ContactNavigation contactNavigation)
        {
            string validationErrors = "";
            try
            {
                db.ContactNavigations.Add(contactNavigation);
                db.SaveChanges();
                return Ok(contactNavigation);
            }
            catch (DbEntityValidationException e)
            {
                foreach (DbEntityValidationResult eve in e.EntityValidationErrors)
                {
                    foreach (DbValidationError ve in eve.ValidationErrors)
                    {
                        string error = ve.PropertyName + " - " + ve.ErrorMessage;
                        validationErrors += System.Environment.NewLine + error;
                    }
                }

                string x = validationErrors;
            }
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}