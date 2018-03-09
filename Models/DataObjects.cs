using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using ScreenViewer.API;

namespace ScreenViewer.Models
{
    [Serializable]
    public class DataObjects
    {
        protected string DONameField;
        protected Int32 DOIDField;
        protected DataDetails[] DetailsArray;

        public string DOName
        {
            get
            {
                return DONameField;
            }
            set
            {
                DONameField = value;
            }
        }
        public Int32 DOID
        {
            get
            {
                return DOIDField;
            }
            set
            {
                DOIDField = value;
            }
        }
        public DataDetails[] Details
        {
            get
            {
                return DetailsArray;
            }
            set
            {
                DetailsArray = value;
            }
        }
        public Int32 CreateDataObjects(string TheName)
        {
            return 0;
        }
        public DataDetails ReturnObjectDetail(string ObjectName,DataDetails[] detailList)
        {
            string[] LevelName = ObjectName.Split(new char[] { '.' });
            DataDetails[] DD = detailList;

            if (LevelName[0] != DONameField)
            {
                return null;
            }

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
        public static DataDetails[] SetObjectDetail(string ObjectName,string value, DataDetails[] detailList)
        {
            string[] LevelName = ObjectName.Split(new char[] { '.' });
            DataDetails[] DD = detailList;

            for (int i = 1; i < LevelName.Length; i++)
            {
                foreach (DataDetails DataDet in DD)
                {
                    if (DataDet.DetailName == LevelName[i])
                    {

                        bool itsValid = false;

                        switch (DataDet.DetailType)
                        {

                            case "String" :

                                itsValid = true;
                                break;

                            case "Integer" :
                                int v;
                                itsValid = Int32.TryParse(value,out v);
                                break;
                            case "Decimal" :
                                Decimal d;
                                itsValid = Decimal.TryParse(value,out d);
                                break;
                            case "Date" :
                            case "Time" :
                            case "Date-Time":

                                DateTime dt;
                                itsValid = DateTime.TryParse(value, out dt);
                                break;    

                            default:
                                break;
                        }
                        if (itsValid)
                        {
                            DataDet.DetailValue = value;
                        }
                        break;
                    }

                }
 

            }
            return detailList;

        }
        public bool IsDataObject(string ObjectName,HttpSessionStateBase session)
        {
            DataDetails DD = null;

            string dataobname = System.Text.RegularExpressions.Regex.Split(ObjectName, "\\.")[0];
            DataObjects ScriptDataObject = (DataObjects)SessionControl.SessionManager.GetDataObject(session, dataobname);

            DD = ScriptDataObject.ReturnObjectDetail(ObjectName, ScriptDataObject.Details);

            if (DD == null)
            {
                return false;
            }
            else
            {
                if (DD.DetailType == "Object")
                { return true; }
                else { return false; }
            }
        }
        public static object ReturnValue(string ObjectName,HttpSessionStateBase session)
        {
            DataDetails DD = null;

            string dataobname = System.Text.RegularExpressions.Regex.Split(ObjectName, "\\.")[0];
            DataObjects ScriptDataObject = (DataObjects)SessionControl.SessionManager.GetDataObject(session, dataobname);

            DD = ScriptDataObject.ReturnObjectDetail(ObjectName, ScriptDataObject.Details);


            if (DD != null)
            {
                return DD.DetailValue;
            }

            return DD;

            //else
            //{
            //    if (DD.Collection)
            //    {
            //        return DD.SubDetails;
            //    }

            //    return DD.DetailValue;
            //}
        }
        public static List<DataObjects> ReturnObjects(string ObjectName, HttpSessionStateBase session)
        {
            DataDetails DD = null;

            string dataobname = System.Text.RegularExpressions.Regex.Split(ObjectName, "\\.")[0];
            DataObjects ScriptDataObject = (DataObjects)SessionControl.SessionManager.GetDataObject(session, dataobname);

            DD = ScriptDataObject.ReturnObjectDetail(ObjectName, ScriptDataObject.Details);
            return null;
            //if (DD == null)
            //{
            //    return null;
            //}
            //else
            //{
            //    return DD.objCollection;
            //}
        }

        public static void SetValue(string ObjectName, string thevalue, HttpSessionStateBase session)
        {
            string dataobname = System.Text.RegularExpressions.Regex.Split(ObjectName, "\\.")[0];
            DataObjects ScriptDataObject = (DataObjects)SessionControl.SessionManager.GetDataObject(session, dataobname);

            if (ScriptDataObject == null)
            {
                DataObjectController dataObjectController = new DataObjectController();
                object loadObject = null;
                var actionResult = dataObjectController.GetDataObject(dataobname, loadObject);

                if (actionResult != null)
                {
                    var objectResponse = actionResult as System.Web.Http.Results.OkNegotiatedContentResult<DataObjects>;
                    DataObjects DisplayObject = (DataObjects)objectResponse.Content;
                    SessionControl.SessionManager.StoreDataObject(session, DisplayObject.DOName, DisplayObject);
                    ScriptDataObject = DisplayObject;
                }
            }

            ScriptDataObject.DetailsArray = SetObjectDetail(ObjectName, thevalue, ScriptDataObject.DetailsArray);
            SessionControl.SessionManager.StoreDataObject(session, dataobname, ScriptDataObject);
        }

        public object ReverseObjectMatch(string parent, object theobject)
        {
            if (theobject == null)
            {
                return null;
            }
            string temp = theobject.ToString();
            DataDetails[] thedetails;

            if (parent == "")
            {
                thedetails = this.DetailsArray;
            }
            else
            {
                string[] LevelName = parent.Split(new char[] { '.' });

                thedetails = this.DetailsArray;

                for (int i = 0; i < LevelName.Length; i++)
                {
                    bool foundit = false;
                    foreach (DataDetails DataDet in thedetails)
                    {
                        if (DataDet.DetailName.ToUpper() == LevelName[i].ToUpper() && DataDet.DetailType == "Object")
                        {
                            foundit = true;
                            thedetails = DataDet.SubDetails;
                            break;
                        }
                    }
                    if (foundit == false)
                    {
                        return null;
                    }
                }
            }

            Type objtype = theobject.GetType();

            PropertyInfo[] FIS = objtype.GetProperties();
 
            Object[] objcol;

            foreach (PropertyInfo fi in FIS)
            {
                DataDetails dade;
                switch (fi.PropertyType.Name)
                {
                    case "String":
                        dade = GrabDetail(thedetails, fi.Name);
                        if (dade != null)
                        {
                            try
                            {
                                fi.SetValue(theobject, dade.DetailValue.ToString());
                            }
                            catch
                            {

                            }

                        }
                        break;
                    case "String[]":
                        dade = GrabDetail(thedetails, fi.Name);
                        if (dade != null)
                        {
                            try
                            {
                            fi.SetValue(theobject, (string[])dade.DetailValue);
                            }
                            catch
                            {

                            }
                        }
                        break;

                    case "Int32":
                        dade = GrabDetail(thedetails, fi.Name);
                        if (dade != null)
                        {
                            try
                            {
                                fi.SetValue(theobject, (Int32)dade.DetailValue);
                            }
                            catch
                            {

                            }

                        }
                        break;
                    case "Int32[]":
                        dade = GrabDetail(thedetails, fi.Name);
                        if (dade != null)
                        {
                            try
                            {
                                fi.SetValue(theobject, (Int32[])dade.DetailValue);
                            }
                            catch
                            {

                            }
                        }
                        break;
                    case "Int64":
                        dade = GrabDetail(thedetails, fi.Name);
                        if (dade != null)
                        {
                            try
                            {
                                fi.SetValue(theobject, (Int64)dade.DetailValue);
                            }
                            catch
                            {

                            } 
                        }
                        break;
                    case "Int64[]":
                        dade = GrabDetail(thedetails, fi.Name);
                        if (dade != null)
                        {
                            try
                            {
                                fi.SetValue(theobject, (Int64[])dade.DetailValue);
                            }
                            catch
                            {

                            } 
                        }
                        break;
                    case "Decimal":
                        dade = GrabDetail(thedetails, fi.Name);
                        if (dade != null)
                        {
                            try
                            {
                                fi.SetValue(theobject, (Decimal)dade.DetailValue);
                            }
                            catch
                            {

                            } 
                        }
                        break;
                    case "Double":
                        dade = GrabDetail(thedetails, fi.Name);
                        if (dade != null)
                        {
                            try
                            {
                                fi.SetValue(theobject, (Double)dade.DetailValue);
                            }
                            catch
                            {

                            } 
                        }
                        break;
                    case "Decimal[]":
                        dade = GrabDetail(thedetails, fi.Name);
                        if (dade != null)
                        {
                            try
                            {
                                fi.SetValue(theobject, (Decimal[])dade.DetailValue);
                            }
                            catch
                            {

                            } 
                        }
                        break;

                    case "DateTime":
                        dade = GrabDetail(thedetails, fi.Name);
                        if (dade != null)
                        {
                            try
                            {
                                fi.SetValue(theobject, (DateTime)dade.DetailValue);
                            }
                            catch
                            {

                            } 
                        }
                        break;

                    case "DateTime[]":
                        dade = GrabDetail(thedetails, fi.Name);
                        if (dade != null)
                        {
                            try
                            {
                                fi.SetValue(theobject, (DateTime[])dade.DetailValue);
                            }
                            catch
                            {

                            } 
                        }
                        break;

                    //default:
                    //    string obname;

                    //    if (fi.PropertyType.Name.Contains("[]"))
                    //    {

                    //        objcol = (object[])objtype.InvokeMember(fi.Name, BindingFlags.GetProperty, null, theobject, null);
                    //        dade = GrabDetail(thedetails, fi.Name);


                    //        if (dade != null)
                    //        {
                    //            if (dade.DetailType == "Object")
                    //            {
                    //                dade.objCollection = new List<DataObjects>();
                    //                foreach (object ob in objcol)
                    //                {
                    //                    DataObjectController doc = new DataObjectController();
                    //                    DataObjects DO = doc.GetDO(dade.InheritedID, ob);
                    //                    dade.objCollection.Add(DO);
                    //                }

                    //            }
                    //            else
                    //            {
                    //                dade.DetailValue = objcol;
                    //            }
                    //        }
                    //        break;

                    //    }
                    //    else if (fi.PropertyType.FullName.Contains("System.Collections.Generic.List"))
                    //    {
                    //        dade = GrabDetail(thedetails, fi.Name);
                    //        if (dade != null)
                    //        {
                    //            try
                    //            {
                    //                fi.SetValue(theobject, (List<Object>)dade.DetailValue);
                    //            }
                    //            catch
                    //            {

                    //            } 
                    //        }
                    //        break;

                    //    }
                    //    else
                    //    {
                    //        string protype = fi.PropertyType.ToString();
                    //        if (parent != "")
                    //        {
                    //            obname = parent + "." + fi.Name;
                    //        }
                    //        else
                    //        {
                    //            obname = fi.Name;
                    //        }
                    //        object thob = objtype.InvokeMember(fi.Name, BindingFlags.GetProperty, null, theobject, null);
                    //        ReverseObjectMatch(obname, thob);
                    //    }
                    //    break;
                }
            }
            return theobject;
        }



        public void ObjectMatch(string parent, object theobject)
        {
            if (theobject == null)
            {
                return;
            }
            string temp = theobject.ToString();
            DataDetails[] thedetails;

            if (parent == "")
            {
                thedetails = this.DetailsArray;
            }
            else
            {
                string[] LevelName = parent.Split(new char[] { '.' });

                thedetails = this.DetailsArray;

                for (int i = 0; i < LevelName.Length; i++)
                {
                    bool foundit = false;
                    foreach (DataDetails DataDet in thedetails)
                    {
                        if (DataDet.DetailName.ToUpper() == LevelName[i].ToUpper() && DataDet.DetailType == "Object")
                        {
                            foundit = true;
                            thedetails = DataDet.SubDetails;
                            break;
                        }
                    }
                    if (foundit == false)
                    {
                        return;
                    }
                }
            }

            Type objtype = theobject.GetType();

            PropertyInfo[] FIS = objtype.GetProperties();
            string stringval;
            string[] stringcol;
            Int32 intval;
            Int32[] intcol;
            Int64 intlongval;
            Int64[] intlongcol;
             DateTime datetimeval;
            DateTime[] datetimecol;
            Decimal decval;
            Double douval;
            Decimal[] deccol;
            Object[] objcol;
            List<object> ObjectList;

            foreach (PropertyInfo fi in FIS)
            {
                DataDetails dade;
                switch (fi.PropertyType.Name)
                {
                    case "String":
                        stringval = (string)objtype.InvokeMember(fi.Name, BindingFlags.GetProperty, null, theobject, null);
                        dade = GrabDetail(thedetails, fi.Name);
                        if (dade != null)
                        {
                            dade.DetailValue = stringval;
                        }
                        break;
                    case "String[]":
                        stringcol = (string[])objtype.InvokeMember(fi.Name, BindingFlags.GetProperty, null, theobject, null);
                        dade = GrabDetail(thedetails, fi.Name);
                        if (dade != null)
                        {
                            dade.DetailValue = stringcol;
                        }
                        break;

                    case "Int32":
                        intval = (Int32)objtype.InvokeMember(fi.Name, BindingFlags.GetProperty, null, theobject, null);
                        dade = GrabDetail(thedetails, fi.Name);
                        if (dade != null)
                        {
                            dade.DetailValue = intval;
                        }
                        break;
                    case "Int32[]":
                        intcol = (Int32[])objtype.InvokeMember(fi.Name, BindingFlags.GetProperty, null, theobject, null);
                        dade = GrabDetail(thedetails, fi.Name);
                        if (dade != null)
                        {
                            dade.DetailValue = intcol;
                        }
                        break;
                    case "Int64":
                        intlongval = (Int64)objtype.InvokeMember(fi.Name, BindingFlags.GetProperty, null, theobject, null);
                        dade = GrabDetail(thedetails, fi.Name);
                        if (dade != null)
                        {
                            dade.DetailValue = intlongval;
                        }
                        break;
                    case "Int64[]":
                        intlongcol = (Int64[])objtype.InvokeMember(fi.Name, BindingFlags.GetProperty, null, theobject, null);
                        dade = GrabDetail(thedetails, fi.Name);
                        if (dade != null)
                        {
                            dade.DetailValue = intlongcol;
                        }
                        break;
                    case "Decimal":
                        decval = (Decimal)objtype.InvokeMember(fi.Name, BindingFlags.GetProperty, null, theobject, null);
                        dade = GrabDetail(thedetails, fi.Name);
                        if (dade != null)
                        {
                            dade.DetailValue = decval;
                        }
                        break;
                    case "Double":
                        douval = (Double)objtype.InvokeMember(fi.Name, BindingFlags.GetProperty, null, theobject, null);
                        dade = GrabDetail(thedetails, fi.Name);
                        if (dade != null)
                        {
                            dade.DetailValue = douval;
                        }
                        break;
                    case "Decimal[]":
                        deccol = (Decimal[])objtype.InvokeMember(fi.Name, BindingFlags.GetProperty, null, theobject, null);
                        dade = GrabDetail(thedetails, fi.Name);
                        if (dade != null)
                        {
                            dade.DetailValue = deccol;
                        }
                        break;

                    case "DateTime":
                        datetimeval = (DateTime)objtype.InvokeMember(fi.Name, BindingFlags.GetProperty, null, theobject, null);
                        dade = GrabDetail(thedetails, fi.Name);
                        if (dade != null)
                        {
                            dade.DetailValue = datetimeval;
                        }
                        break;

                    case "DateTime[]":
                        datetimecol = (DateTime[])objtype.InvokeMember(fi.Name, BindingFlags.GetProperty, null, theobject, null);
                        dade = GrabDetail(thedetails, fi.Name);
                        if (dade != null)
                        {
                            dade.DetailValue = datetimecol;
                        }
                        break;

                    //default:
                    //    string obname;

                    //    if (fi.PropertyType.Name.Contains("[]"))
                    //    {
                            
                    //        objcol = (object[])objtype.InvokeMember(fi.Name, BindingFlags.GetProperty, null, theobject, null);
                    //        dade = GrabDetail(thedetails, fi.Name);


                    //        if (dade != null)
                    //        {
                    //            if (dade.DetailType == "Object")
                    //            {
                    //                dade.objCollection = new List<DataObjects>();
                    //                foreach (object ob in objcol)
                    //                {
                    //                    DataObjectController doc = new DataObjectController();
                    //                    DataObjects DO = doc.GetDO(dade.InheritedID, ob);
                    //                    dade.objCollection.Add(DO);
                    //                }
                                    
                    //            }
                    //            else
                    //            {
                    //                dade.DetailValue = objcol;
                    //            }
                    //        }
                    //        break;

                    //    }
                    //    else if ( fi.PropertyType.FullName.Contains("System.Collections.Generic.List"))
                    //    {
                    //        ObjectList = (List<object>)objtype.InvokeMember(fi.Name, BindingFlags.GetProperty, null, theobject, null);
                    //        dade = GrabDetail(thedetails, fi.Name);
                    //        if (dade != null)
                    //        {
                    //            dade.DetailValue = ObjectList;
                    //        }
                    //        break;

                    //    }
                    //    else
                    //    {
                    //        string protype = fi.PropertyType.ToString();
                    //        if (parent != "")
                    //        {
                    //            obname = parent + "." + fi.Name;
                    //        }
                    //        else
                    //        {
                    //            obname = fi.Name;
                    //        }
                    //        object thob = objtype.InvokeMember(fi.Name, BindingFlags.GetProperty, null, theobject, null);
                    //        ObjectMatch(obname, thob);
                    //    }
                    //    break;
                }
            }
        }

        public void DictionaryMatch(dynamic theDictionary)
        {
            if (theDictionary == null)
            {
                return;
            }

            DataDetails[] thedetails;

            thedetails = this.DetailsArray;

            foreach (DataDetails DataDetail in thedetails)
            {
                switch (DataDetail.DetailType)
                {

                    case "Integer":
                        try
                        {
                            Int32 tempValue = Convert.ToInt32(theDictionary[DataDetail.DetailName]);
                            DataDetail.DetailValue = tempValue.ToString();
                        }
                        catch
                        {
                        }
                        break;
                    case "Decimal":
                        try
                        {
                            Decimal tempValue = Convert.ToDecimal(theDictionary[DataDetail.DetailName]);
                            DataDetail.DetailValue = tempValue.ToString();
                        }
                        catch
                        {
                        }

                        break;
                    case "String":
                        try
                        {
                            DataDetail.DetailValue = theDictionary[DataDetail.DetailName];
                        }
                        catch
                        {
                        }
                        break;
                    case "Date":
                        try
                        {
                            DateTime tempValue = DateTime.Parse(theDictionary[DataDetail.DetailName].ToString());
                            DataDetail.DetailValue = tempValue.ToShortDateString();
                        }
                        catch
                        {
                        }
                        break;
                    case "Time":
                        try
                        {
                            DateTime tempValue = DateTime.Parse(theDictionary[DataDetail.DetailName].ToString());
                            DataDetail.DetailValue = tempValue.ToShortTimeString();
                        }
                        catch
                        {
                        }
                        break;
                    case "Date-Time":
                        try
                        {
                            DateTime tempValue = DateTime.Parse(theDictionary[DataDetail.DetailName].ToString());
                            DataDetail.DetailValue = tempValue.ToString();
                        }
                        catch
                        {
                        }
                        break;
                }
            }
        }


        public DataDetails GrabDetail(DataDetails[] detarray, string fieldname)
        {
            foreach (DataDetails dd in detarray)
            {
                if (dd.DetailName.ToUpper() == fieldname.ToUpper())
                    return dd;
            }
            return null;
        }
    }

    [Serializable]
    public class  DataDetails
    {
        protected string DetailNameField;
        protected string DetailTypeField;
        protected Int32 DetailIDField;
        //protected int InheritedIDField;
        protected DataDetails[] SubDetailsArray;
        //public List<DataObjects> objCollection { get; set; }
        protected Object DetailValueField;
        //protected string ObjectNameField;
        //protected bool CollectionField;

        //public bool Collection
        //{
        //    get
        //    {
        //        return CollectionField;
        //    }
        //    set
        //    {
        //        CollectionField = value;
        //    }
        //}
        public Object DetailValue
        {
            get
            {
                return DetailValueField;
            }
            set
            {
                DetailValueField = value;
            }
        }
        public string DetailName
        {
            get
            {
                return DetailNameField;
            }
            set
            {
                DetailNameField = value;
            }
        }
        public Int32 DetailID
        {
            get
            {
                return DetailIDField;
            }
            set
            {
                DetailIDField = value;
            }
        }
        public string DetailType
        {
            get
            {
                return DetailTypeField;
            }
            set
            {
                DetailTypeField = value;
            }
        }
        //public int InheritedID
        //{
        //    get
        //    {
        //        return InheritedIDField;
        //    }
        //    set
        //    {
        //        InheritedIDField = value;
        //    }
        //}
        public DataDetails[] SubDetails
        {
            get
            {
                return SubDetailsArray;
            }
            set
            {
                SubDetailsArray = value;
            }
        }
        //public string ObjectName
        //{
        //    get
        //    {
        //        return ObjectNameField;
        //    }
        //    set
        //    {
        //        ObjectNameField = value;
        //    }
        //}

    }

}