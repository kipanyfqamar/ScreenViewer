using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Description;
using ScreenViewer.Data;
using ScreenViewer.Models;
using System.Text.RegularExpressions;
using System.Web.Http.Results;

namespace ScreenViewer.API
{
    public class DataObjectController : ApiController
    {
        private ScreenPlayEntities db = new ScreenPlayEntities();

        // GET api/DataObject
        public IQueryable<ScriptObject> GetDataObjects()
        {
            return db.ScriptObjects;
        }

        // GET api/DataObject/5
        [ResponseType(typeof(ScriptObject))]
        public IHttpActionResult returnDataObject(int id)
        {
            ScriptObject dataobject = db.ScriptObjects.Find(id);
            if (dataobject == null)
            {
                return NotFound();
            }

            return Ok(dataobject);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool DataObjectExists(int id)
        {
            return db.ScriptObjects.Count(e => e.ScriptObjectID == id) > 0;
        }

        [ResponseType(typeof(DataObjects))]
        public IHttpActionResult GetDataObject(Int32 objectID)
        {
            DataObjects DO = new DataObjects();
            DO.DOID = objectID;
            DO.DOName = GetNameByID(objectID);
            DO.Details = FillDetails(objectID);

            return Ok(DO);
        }


        public ScriptObject GetDataObject(string DataObjectName)
        {
            ScriptObject dob = db.ScriptObjects.Where(b => b.ObjectName.Equals(DataObjectName)).FirstOrDefault();
            if (dob == null)
            {
                return null;
            }

            return dob;
        }
        public ScriptObject GetDataObjectbyID(Int32 DataObjectID)
        {
            ScriptObject dob = db.ScriptObjects.Where(b => b.ScriptObjectID.Equals(DataObjectID)).FirstOrDefault();
            if (dob == null)
            {
                return null;
            }

            return dob;
        }

        [ResponseType(typeof(DataObjects))]
        public IHttpActionResult GetDataObject(string DataObjectName, Object LoadObject)
        {
            ScriptObject dob = db.ScriptObjects.Where(b => b.ObjectName.Equals(DataObjectName)).FirstOrDefault();
            return GetDataObject(dob.ScriptObjectID, LoadObject);
        }

        [ResponseType(typeof(DataObjects))]
        public IHttpActionResult GetDataObject(Int32 objectID, Object LoadObject)
        {
            DataObjects DO = new DataObjects();
            DO.DOID = objectID;
            DO.DOName = GetNameByID(objectID);
            DO.Details = FillDetails(objectID);
            DO.ObjectMatch("", LoadObject);
            return Ok(DO);
        }

        public DataObjects GetDO(Int32 objectID, Object LoadObject)
        {
            DataObjects DO = new DataObjects();
            DO.DOID = objectID;
            DO.DOName = GetNameByID(objectID);
            DO.Details = FillDetails(objectID);
            DO.ObjectMatch("", LoadObject);
            return DO;
        }

        [ResponseType(typeof(DataObjects))]
        public IHttpActionResult GetDataObjectDictionary(string objectname, dynamic DOdictionary)
        {
            return Ok(GetDataObjectDD(objectname, DOdictionary));
        }

        //Added By FQ
        [ResponseType(typeof(DataObjects))]
        public IHttpActionResult GetDataObjectDictionary(int objectID, dynamic DOdictionary)
        {
            return Ok(GetDataObjectDD(objectID, DOdictionary));
        }
        public IHttpActionResult GetDataObjectCRM(int objectid, LeadRecord LR)
        {
            return Ok(GetDataObjectCRMLead(objectid, LR));
        }

        public DataObjects GetDataObjectDD(string objectname, dynamic DOdictionary)
        {
            List<string> doParts = Regex.Split(objectname, "\\.").ToList();
            ScriptObject Dob = GetDataObject(doParts[0]);

            DataObjects DO = new DataObjects();
            DO.DOID = Dob.ScriptObjectID;
            DO.DOName = Dob.ObjectName;
            DO.Details = FillDetails(Dob.ScriptObjectID);

            if (doParts.Count > 1)
            {
                DataDetails thisDataDetail = ReturnObjectDetail(DO.Details, objectname);

            }
            else
            {
                DO.DictionaryMatch(DOdictionary);
            }
            return DO;
        }

        //Added By FQ
        public DataObjects GetDataObjectDD(int objectID, dynamic DOdictionary)
        {
            ScriptObject Dob = GetDataObjectbyID(objectID);

            DataObjects DO = new DataObjects();
            DO.DOID = Dob.ScriptObjectID;
            DO.DOName = Dob.ObjectName;

            if (!Dob.IsCollection)
            {
                DO.Details = FillDetails(Dob.ScriptObjectID);
                DO.DictionaryMatch(DOdictionary);
            }
            else
            {
                List<DataObjects> theObs = new List<DataObjects>();
                Int32 thecount = 0;

                foreach (dynamic dyndet in DOdictionary)
                {
                    thecount++;
                    DataObjects doS = GetDataObjectDD(DO.DOName, dyndet);
                    theObs.Add(doS);

                }

                DataDetails[] subdets = new DataDetails[thecount];
                for (int i = 0; i < thecount; i++)
                {
                    DataDetails d1 = new DataDetails();
                    d1.SubDetails = new DataDetails[theObs[i].Details.Length];
                    theObs[i].Details.CopyTo(d1.SubDetails, 0);

                    subdets[i] = d1;
                }

                DO.Details = subdets;
            }

            return DO;
        }

        public DataObjects GetDataObjectCRMLead(int objectID, LeadRecord LeadRec)
        {
            ScriptObject Dob = GetDataObjectbyID(objectID);

            DataObjects DO = new DataObjects();
            DO.DOID = Dob.ScriptObjectID;
            DO.DOName = Dob.ObjectName;

            List<DataObjects> theObs = new List<DataObjects>();
            Int32 thecount = 0;

            Type objtype = LeadRec.GetType();

            PropertyInfo[] FIS = objtype.GetProperties();
            string stringval;

            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            foreach (PropertyInfo fi in FIS)
            {
                switch (fi.PropertyType.Name)
                {
                    case "String":
                        stringval = (string)objtype.InvokeMember(fi.Name, BindingFlags.GetProperty, null, LeadRec, null);
                        dictionary.Add(fi.Name, stringval);

                        break;

                    case "DateTime":
                        DateTime datetimecol = (DateTime)objtype.InvokeMember(fi.Name, BindingFlags.GetProperty, null, LeadRec, null);
                        stringval = datetimecol.ToString();
                        dictionary.Add(fi.Name, stringval);

                        break;
                }
            }
            foreach(LeadRecordAttribute LRA in LeadRec.LeadRecordAttributes)
            {
                dictionary.Add(LRA.LeadAttributeName, LRA.LeadAttributeValue);
            }

            DO.Details = FillDetails(Dob.ScriptObjectID);
            DO.DictionaryMatch(dictionary);

            return DO;
        }

        public string GetNameByID(Int32 ObjectID)
        {
            var doName = db.ScriptObjects
                                .Where(dataObject => dataObject.ScriptObjectID == ObjectID)
                                .Select(ret => new { ret.ObjectName });

            return doName.FirstOrDefault().ObjectName;
        }

        private DataDetails[] FillDetails(Int32 theID)
        {
            var doDetails = db.ScriptObjectDetails
                .Where(dataObjectDetails => dataObjectDetails.ScriptObjectID == theID)
                .Select(ret => new { ret.ObjectDetailName, ret.ObjectDetailDataType, ret.Sequence, })
                .OrderBy(ob => ob.Sequence);

            if (doDetails != null)
            {
                List<DataDetails> ddList = new List<DataDetails>();

                foreach (var dodetail in doDetails)
                {
                    DataDetails DD = new DataDetails();

                    DD.DetailName = dodetail.ObjectDetailName;
                    DD.DetailID = System.Convert.ToInt32(dodetail.Sequence);
                    DD.DetailType = dodetail.ObjectDetailDataType;

                    //if (dodetail.InheritedID != null)
                    //{
                    //    DD.InheritedID = Convert.ToInt32(dodetail.InheritedID);
                    //}
                    //try
                    //{
                    //    DD.Collection = (bool)dodetail.Collection;
                    //}
                    //catch
                    //{
                    //    DD.Collection = false;
                    //}
                    //if (DD.Collection)
                    //{
                    //    if (dodetail.DetailType == "Object")
                    //    {
                    //        DD.DetailType = dodetail.DetailType;
                    //    }
                    //    else
                    //    {
                    //        DD.DetailType = dodetail.DetailType;
                    //    }
                    //}
                    //else
                    //{
                    //    if (dodetail.DetailType == "Object")
                    //    {
                    //        DD.DetailType = dodetail.DetailType;
                    //        DataDetails[] ddets = FillDetails(System.Convert.ToInt32(dodetail.InheritedID));
                    //        DD.SubDetails = ddets;
                    //    }
                    //    else
                    //    {
                    //        DD.DetailType = dodetail.DetailType;
                    //    }
                    //}
                    ddList.Add(DD);
                }
                DataDetails[] DO = new DataDetails[ddList.Count];
                ddList.CopyTo(DO);
                return DO;
            }
            return null;
        }

        public DataDetails ReturnObjectDetail(DataDetails[] DD, string ObjectName)
        {
            string[] LevelName = ObjectName.Split(new char[] { '.' });

            for (int i = 1; i < LevelName.Length; i++)
            {
                bool foundit = false;
                foreach (DataDetails DataDet in DD)
                {
                    if (DataDet.DetailName == LevelName[i])
                    {
                        foundit = true;
                        if (i == LevelName.Length - 1)
                        {
                            return DataDet;
                        }
                        else
                        {
                            DD = DataDet.SubDetails;
                        }
                        break;
                    }
                }
                if (foundit == false)
                {
                    return null;
                }
            }
            return null;
        }
    }
}