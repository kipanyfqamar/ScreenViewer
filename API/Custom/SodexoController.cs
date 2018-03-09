using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using System.Collections.Specialized;
using System.Configuration;
using System.Text;
using System.Web.Http.Description;
using System.Net.Http.Formatting;
using System.Data.SqlClient;
using ScreenViewer.API.CRM;

namespace ScreenViewer.API.Custom
{
    public class SodexoController : ApiController
    {
        private SodexoEntities db = new SodexoEntities();

        [HttpGet]
        public IHttpActionResult GetPostCreateCall(int contactId, string siteNo, string custNo, string custName)
        {
            SodexoCall call = null;
            ApiResult result = null;

            try
            {
                result = new ApiResult();
                call = new SodexoCall();

                call.ContactID = contactId;
                call.SiteNo = siteNo;
                call.CustNo = custNo;
                call.CustName = custName;

                db.SodexoCalls.Add(call);
                db.SaveChanges();

                result.Status = "SUCCESS";
                return Ok(result);

            }
            catch (Exception ex)
            {
                result = new ApiResult();
                result.Status = "FAILED";
                result.ErrorMessage1 = ex.Message;

                if (ex.InnerException != null)
                    result.ErrorMessage2 = ex.InnerException.Message;
                else
                    result.ErrorMessage2 = string.Empty;

                return Ok(result);
            }
        }

        [HttpPost]
        public IHttpActionResult CreateCall(FormDataCollection formData)
        {
            SodexoCall call = null;
            ApiResult result = null;
            string sender = string.Empty;
            string senderName = string.Empty;

            try
            {
                result = new ApiResult();
                call = new SodexoCall();

                call.ContactID = Convert.ToInt32(formData.Get("ContactID"));
                call.SiteNo = formData.Get("SiteNo") != null ? formData.Get("SiteNo") : string.Empty;
                call.CustNo = formData.Get("CustNo") != null ? formData.Get("CustNo") : string.Empty;
                call.CustName = formData.Get("CustName") != null ? formData.Get("CustName") : string.Empty;
                call.SiteAddress1 = formData.Get("SiteAddress1") != null ? formData.Get("SiteAddress1") : string.Empty;
                call.SiteAddress2 = formData.Get("SiteAddress2") != null ? formData.Get("SiteAddress2") : string.Empty;
                call.SiteCity = formData.Get("SiteCity") != null ? formData.Get("SiteCity") : string.Empty;
                call.SiteState = formData.Get("SiteState") != null ? formData.Get("SiteState") : string.Empty;
                call.SiteZip = formData.Get("SiteZip") != null ? formData.Get("SiteZip") : string.Empty;
                call.CallType = formData.Get("CallType") != null ? formData.Get("CallType") : string.Empty;
                call.NatureofCall = formData.Get("NatureofCall") != null ? formData.Get("NatureofCall") : string.Empty;
                call.Dept = formData.Get("Dept") != null ? formData.Get("Dept") : string.Empty;
                call.Division = formData.Get("Division") != null ? formData.Get("Division") : string.Empty;

                if (formData.Get("OvertimeApproved") != null)
                    call.OvertimeApproved = Convert.ToBoolean(formData.Get("OvertimeApproved"));

                call.PONumbrt = formData.Get("PONumbrt") != null ? formData.Get("PONumbrt") : string.Empty;
                call.NTX = formData.Get("NTX") != null ? formData.Get("NTX") : string.Empty;

                if (formData.Get("ScheduledDate") != null)
                    call.ScheduledDate = Convert.ToDateTime(formData.Get("OvertimeApproved"));
                
                call.VendorNumber = formData.Get("VendorNumber") != null ? formData.Get("VendorNumber") : string.Empty;
                call.Priority = formData.Get("Priority") != null ? formData.Get("Priority") : string.Empty;
                call.ProblemCode = formData.Get("ProblemCode") != null ? formData.Get("ProblemCode") : string.Empty;
                call.CallerPhone = formData.Get("CallerPhone") != null ? formData.Get("CallerPhone") : string.Empty;
                call.CallerEmail = formData.Get("CallerEmail") != null ? formData.Get("CallerEmail") : string.Empty;
                call.WorkOrderID = formData.Get("WorkOrderID") != null ? formData.Get("WorkOrderID") : string.Empty;
                call.CallerName = formData.Get("CallerName") != null ? formData.Get("CallerName") : string.Empty;
                call.CallerTitle = formData.Get("CallerTitle") != null ? formData.Get("CallerTitle") : string.Empty;

                db.SodexoCalls.Add(call);
                db.SaveChanges();

                result.Status = "SUCCESS";
                return Ok(result);

            }
            catch (Exception ex)
            {
                result = new ApiResult();
                result.Status = "FAILED";
                result.ErrorMessage1 = ex.Message;

                if (ex.InnerException != null)
                    result.ErrorMessage2 = ex.InnerException.Message;
                else
                    result.ErrorMessage2 = string.Empty;

                return Ok(result);
            }
        }

        [HttpPost]
        public IHttpActionResult UpdateCall(FormDataCollection formData)
        {
            SodexoCall call = null;
            ApiResult result = null;
            int contactID = 0;
            string sender = string.Empty;
            string senderName = string.Empty;

            try
            {
                result = new ApiResult();
                call = new SodexoCall();

                contactID = Convert.ToInt32(formData.Get("ContactID"));
                call = db.SodexoCalls.Where(x=> x.ContactID == contactID).FirstOrDefault();

                if (call != null)
                {
                    call.SiteNo = formData.Get("SiteNo") != null ? formData.Get("SiteNo") : string.Empty;
                    call.SiteAddress1 = formData.Get("SiteAddress1") != null ? formData.Get("SiteAddress1") : string.Empty;
                    call.SiteAddress2 = formData.Get("SiteAddress2") != null ? formData.Get("SiteAddress2") : string.Empty;
                    call.SiteCity = formData.Get("SiteCity") != null ? formData.Get("SiteCity") : string.Empty;
                    call.SiteState = formData.Get("SiteState") != null ? formData.Get("SiteState") : string.Empty;
                    call.SiteZip = formData.Get("SiteZip") != null ? formData.Get("SiteZip") : string.Empty;
                    call.CallType = formData.Get("CallType") != null ? formData.Get("CallType") : string.Empty;
                    call.NatureofCall = formData.Get("NatureofCall") != null ? formData.Get("NatureofCall") : string.Empty;
                    call.Dept = formData.Get("Dept") != null ? formData.Get("Dept") : string.Empty;
                    call.Division = formData.Get("Division") != null ? formData.Get("Division") : string.Empty;

                    if (formData.Get("OvertimeApproved") != null)
                        call.OvertimeApproved = Convert.ToBoolean(formData.Get("OvertimeApproved"));

                    call.PONumbrt = formData.Get("PONumbrt") != null ? formData.Get("PONumbrt") : string.Empty;
                    call.NTX = formData.Get("NTX") != null ? formData.Get("NTX") : string.Empty;

                    if (formData.Get("ScheduledDate") != null)
                        call.ScheduledDate = Convert.ToDateTime(formData.Get("OvertimeApproved"));

                    call.VendorNumber = formData.Get("VendorNumber") != null ? formData.Get("VendorNumber") : string.Empty;
                    call.Priority = formData.Get("Priority") != null ? formData.Get("Priority") : string.Empty;
                    call.ProblemCode = formData.Get("ProblemCode") != null ? formData.Get("ProblemCode") : string.Empty;
                    call.CallerPhone = formData.Get("CallerPhone") != null ? formData.Get("CallerPhone") : string.Empty;
                    call.CallerEmail = formData.Get("CallerEmail") != null ? formData.Get("CallerEmail") : string.Empty;
                    call.WorkOrderID = formData.Get("WorkOrderID") != null ? formData.Get("WorkOrderID") : string.Empty;
                    call.CallerName = formData.Get("CallerName") != null ? formData.Get("CallerName") : string.Empty;
                    call.CallerTitle = formData.Get("CallerTitle") != null ? formData.Get("CallerTitle") : string.Empty;

                    db.Entry(call).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    result.Status = "SUCCESS";
                }
                else
                {
                    result.Status = "FAILED";
                    result.ErrorMessage1 = "Site Not Found";
                }

                return Ok(result);

            }
            catch (Exception ex)
            {
                result = new ApiResult();
                result.Status = "FAILED";
                result.ErrorMessage1 = ex.Message;

                if (ex.InnerException != null)
                    result.ErrorMessage2 = ex.InnerException.Message;
                else
                    result.ErrorMessage2 = string.Empty;

                return Ok(result);
            }
        }

        [HttpGet]
        [ResponseType(typeof(IQueryable<SodexoCall>))]
        public IHttpActionResult GetCallsByPhone(string ani)
        {
            var siteCalls = db.SodexoCalls.Where(b => b.CallerPhone == ani);
            return Ok(siteCalls);
        }

        [HttpGet]
        [ResponseType(typeof(IQueryable<SodexoCall>))]
        public IHttpActionResult GetLatestCallByPhone(string ani)
        {
            var siteCall = db.SodexoCalls.Where(b => b.CallerPhone == ani).OrderByDescending(o=> o.SodexoCallId).First();
            return Ok(siteCall);
        }

        [HttpGet]
        [ResponseType(typeof(IQueryable<SodexoCall>))]
        public IHttpActionResult GetCallsBySiteNo(string siteNo)
        {
            var siteCalls = db.SodexoCalls.Where(b => b.SiteNo.Equals(siteNo));
            return Ok(siteCalls);
        }

        [HttpGet]
        [ResponseType(typeof(IQueryable<SodexoCall>))]
        public IHttpActionResult GetCallsByCustNo(string CustNo)
        {
            var siteCalls = db.SodexoCalls.Where(b => b.CustNo.Equals(CustNo)).OrderByDescending(o => o.SodexoCallId).Take(5);
            return Ok(siteCalls);
        }




        [HttpGet]
        [ResponseType(typeof(SodexoCall))]
        public IHttpActionResult GetCall(int contactId)
        {
            var siteCall = db.SodexoCalls.Where(b => b.ContactID.Equals(contactId)).FirstOrDefault();

            if (siteCall == null)
            {
                return NotFound();
            }

            return Ok(siteCall);
        }

        public class ApiResult
        {
            public string Status { get; set; }
            public string StatusDescription { get; set; }
            public string ErrorMessage1 { get; set; }
            public string ErrorMessage2 { get; set; }

        }
    }
}