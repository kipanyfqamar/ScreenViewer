using System;
using System.Web;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using ScreenViewer.Data;

namespace ScreenViewer
{
    public static partial class Log
    {
        public static Data.Log Save(this Exception ex, HttpSessionStateBase Session)
        {
            return Save(ex, Session, "", ImpactLevel.Low, "");
        }

        public static Data.Log Save(this Exception ex, HttpSessionStateBase Session, string callDetail)
        {
            return Save(ex, Session, callDetail, ImpactLevel.Low, "");
        }

        public static Data.Log Save(this Exception ex, HttpSessionStateBase Session, string callDetail, ImpactLevel impactLevel)
        {
            return Save(ex, Session, callDetail, impactLevel, "");
        }
        
        public static Data.Log Save(this Exception ex, HttpSessionStateBase Session, string callDetail,  ImpactLevel impactLevel, string errorDescription)
        {
            using (var db = new ScreenPlayCRMEntities())
            {
                Data.Log log = new Data.Log();
                
                log.ContactId = SessionControl.SessionManager.GetContactId(Session);
                log.ClientId = SessionControl.SessionManager.GetClientId(Session);
                log.UserId = SessionControl.SessionManager.GetUserId(Session);
                log.CallDetail = callDetail;
                log.ErrorDate = DateTime.Now;
                log.LastModified = DateTime.Now;

                if (errorDescription != null && errorDescription != "")
                {
                    log.ErrorShortDescription = errorDescription;
                }

                log.ExceptionType = ex.GetType().FullName;
                var stackTrace = new StackTrace(ex, true);
                var allFrames = stackTrace.GetFrames().ToList();

                foreach (var frame in allFrames)
                {
                    log.FileName = frame.GetFileName();
                    log.LineNumber = frame.GetFileLineNumber();
                    var method = frame.GetMethod();
                    log.MethodName = method.Name;
                    log.ClassName = frame.GetMethod().DeclaringType.ToString();
                }

                log.ImpactLevel = impactLevel.ToString();
                try
                {
                    log.ApplicationName = Assembly.GetCallingAssembly().GetName().Name;
                }
                catch
                {
                    log.ApplicationName = "";
                }

                log.ErrorMessage = ex.Message;
                log.StackTrace = ex.StackTrace;
                if (ex.InnerException != null)
                {
                    log.InnerException = ex.InnerException.ToString();
                    log.InnerExceptionMessage = ex.InnerException.Message;
                }

                log.IpAddress = ""; 


                try
                {
                    db.Logs.Add(log);
                    db.SaveChanges();
                }
                catch (Exception eex)
                {

                }

                return log;
            }
        }
    }

    public enum ImpactLevel
    {
        High = 0,
        Medium = 1,
        Low = 2,
    }
}