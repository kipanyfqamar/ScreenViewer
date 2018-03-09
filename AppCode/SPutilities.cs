using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Text.RegularExpressions;
using ScreenViewer.Models;
using System.Web.Http.Results;
using ScreenViewer.API;
using System.Collections;
using System.Reflection;
using ScreenViewer.Data;

namespace ScreenViewer
{
    public static class SPutilities
    {
        public static string ReplaceObjectsandQuestions(HttpSessionStateBase SessionBase, string TheText, bool displayValues)
        {
            string patt0 = @"\{Parameter::[^}]*\}";
            string patt1 = @"\{DataObject::[^}]*\}";
            string patt2 = @"\{-Question:[^}]*\}";
            string patt3 = @"{Calc::C.*?}";
            string patt4 = @"\{IMPImage:[^}]*\}";
            string patt5 = @"\{-Link:[^}]*\}";

            string newtext = TheText;
            //patt = @"DataObject";
            Regex Rg0 = new Regex(patt0);
            Regex Rg = new Regex(patt1);
            Regex Rg2 = new Regex(patt2);
            Regex Rg3 = new Regex(patt3);
            Regex Rg4 = new Regex(patt4);
            Regex Rg5 = new Regex(patt5);

            if (TheText != "")
            {
                //Match and replce for Parameter
                MatchCollection MC0 = Rg0.Matches(TheText);
                for (int i = 0; i < MC0.Count; i++)
                {
                    string paramfull = MC0[i].Value;
                    string param = Regex.Split(paramfull, "::")[1];
                    param = param.Remove(param.Length - 1);

                    string responsestr;
                    responsestr = SessionControl.SessionManager.GetProgramParameterByKey(param, SessionBase);
                    if (responsestr != string.Empty)
                    {
                        newtext = newtext.Replace(MC0[i].Value, responsestr);
                    }
                    else
                    {
                        string responsestr2;
                        responsestr2 = SessionControl.SessionManager.GetScriptParameterByKey(param, SessionBase);
                        if (responsestr2 != string.Empty)
                        {
                            newtext = newtext.Replace(MC0[i].Value, responsestr2);
                        }
                        else
                        {
                            newtext = newtext.Replace(MC0[i].Value, "(parameter not found)");
                        }
                    }
                }

                //Match and replce for DataObjects
                MatchCollection MC = Rg.Matches(TheText);
                for (int i = 0; i < MC.Count; i++)
                {
                    string datobstr = MC[i].Value;
                    int startof = datobstr.IndexOf("::") + 2;
                    string dataob = Regex.Split(datobstr, "::")[1];
                    dataob = dataob.Remove(dataob.Length - 1);
                    string dataobname = Regex.Split(dataob, "\\.")[0];
                    DataObjects ScriptDataObject = (DataObjects)SessionControl.SessionManager.GetDataObject(SessionBase, dataobname);

                    if (ScriptDataObject != null)
                    {
                        if (ScriptDataObject.IsDataObject(dataob, SessionBase))
                        {
                            //string gridstr = GenerateDataGrid(dataob);
                            //newtext = newtext.Replace(MC[i].Value, gridstr);
                        }
                        else
                        {
                            object responsestr = DataObjects.ReturnValue(dataob, SessionBase);
                            if (responsestr == null)
                            {
                                newtext = newtext.Replace(MC[i].Value, "**Value was not found - " + dataob + " **");
                            }
                            else
                            {
                                if (responsestr.ToString() == "~OBJECT~")
                                {
                                    //responsestr = GenerateDataGrid(MC[i].Value);
                                }
                                else
                                {
                                    newtext = newtext.Replace(MC[i].Value, responsestr.ToString());
                                }
                            }
                        }
                    }
                    else
                    {
                        newtext = newtext.Replace(MC[i].Value, "**Value was not found - " + dataob + " **");
                    }
                }

                //Match and replace questions
                MatchCollection MC2 = Rg2.Matches(TheText);
                for (int i = 0; i < MC2.Count; i++)
                {
                    string qval = MC2[i].Value;
                    string qid = qval.Split(new char[] { ':' })[1];
                    string responsestr;
                    if (displayValues)
                    {
                        responsestr = SessionControl.SessionManager.GetQuestionDisplayResponse(qid, SessionBase);
                    }
                    else
                    {
                        responsestr = SessionControl.SessionManager.GetQuestionResponse(qid, SessionBase);
                    }
                    if (responsestr != null)
                    {
                        newtext = newtext.Replace(MC2[i].Value, responsestr);
                    }
                    else
                    {
                        newtext = newtext.Replace(MC2[i].Value, "(No question response)");
                    }
                }

                // Image match and replace
                MatchCollection MC4 = Rg4.Matches(TheText);
                for (int i = 0; i < MC4.Count; i++)
                {
                    string ival = MC4[i].Value;
                    string iid = ival.Split(new char[] { ':' })[1];

                    API.Elements.ImageController IC = new API.Elements.ImageController();
                    var actionResult = IC.GetImage(Convert.ToInt32(iid));

                    var response = actionResult as OkNegotiatedContentResult<ScriptImage>;
                    //Assert.IsNotNull(response);
                    ScriptImage theImage = response.Content;

                    string alttext = theImage.ImageDesc;
                    string replacestr = string.Format("<img src='{0}/Image/DisplayImage/{1}\' />", System.Web.HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority), iid);
                    //string replacestr = "<img src=/Image/DisplayImage/" + iid + " />";
                    newtext = newtext.Replace(MC4[i].Value, replacestr);

                }

                // Calaculation match and replace
                MatchCollection MC3 = Rg3.Matches(TheText);
                if (MC3.Count > 0)
                {
                    ScriptCalculator IC = new ScriptCalculator();

                    for (int i = 0; i < MC3.Count; i++)
                    {
                        string str = MC3[i].ToString();
                        string pattern = ScriptCalculator.CalcStringPattern;
                        string cId = Regex.Match(str.ToString(), pattern, RegexOptions.IgnorePatternWhitespace).Groups[1].ToString();

                        try
                        {
                            ScriptCalculationInfo calcInfo = ScriptCalculationInfo.GetScriptCalculationBy(cId);

                            string calText = calcInfo.CalculationExpression;

                            //
                            MatchCollection MCInner = Rg.Matches(calText);
                            for (int k = 0; k < MCInner.Count; k++)
                            {
                                string datobstr = MCInner[k].Value;
                                string dataob = Regex.Split(datobstr, "::")[1];
                                dataob = dataob.Remove(dataob.Length - 1);
                                string dataobname = Regex.Split(dataob, "\\.")[0];
                                DataObjects ScriptDataObject = (DataObjects)SessionControl.SessionManager.GetDataObject(SessionBase, dataobname);

                                if (ScriptDataObject != null)
                                {
                                    if (ScriptDataObject.IsDataObject(dataob, SessionBase))
                                    {
                                        //string gridstr = GenerateDataGrid(dataob);
                                        //calText = calText.Replace(MCInner[k].Value, gridstr);
                                    }
                                    else
                                    {
                                        object responsestr = DataObjects.ReturnValue(dataob, SessionBase);
                                        if (responsestr == null)
                                        {
                                            calText = calText.Replace(MCInner[k].Value, "''");//"**Value was not found - " + dataob + " **");
                                        }
                                        else
                                        {

                                            calText = calText.Replace(MCInner[k].Value, "'" + responsestr.ToString() + "'");

                                        }
                                    }
                                }
                            }

                            IC.Parser(SessionBase, calText);
                            string outputstring = IC.Eval();

                            newtext = newtext.Replace(MC3[i].Value, outputstring);

                        }
                        catch
                        {
                            newtext = newtext.Replace(MC3[i].Value, "(Invalid Calculation)");
                        }
                    }
                }

                //Match and replace link
                MatchCollection MC5 = Rg5.Matches(TheText);
                for (int i = 0; i < MC5.Count; i++)
                {
                    try
                    {
                        string qval = MC5[i].Value;
                        string linkid = qval.Split(new char[] { ':' })[1];

                        API.Elements.LinkController linkController = new API.Elements.LinkController();
                        var actionResult = linkController.GetScriptLink(Convert.ToDecimal(linkid));

                        var response = actionResult as OkNegotiatedContentResult<ScriptLink>;
                        //Assert.IsNotNull(response);
                        ScriptLink theLink = response.Content;
                        string linkHtml = string.Empty;


                        string target = theLink.LinkNewWindow.Equals(true) ? "_blank" : "_self";

                        if (theLink.LinkType == "Web" || theLink.LinkType == "Document")
                        {
                            //linkHtml = string.Format("<a href=\"{0}\" title=\"{1}\" target = \"{2}\">{3}</a>", theLink.LinkURL, theLink.LinkDisplay, target, theLink.LinkDisplay);
                            linkHtml = string.Format("<a href='{0}' title=\"{1}\" target = \"{2}\">{3}</a>", theLink.LinkURL, theLink.LinkDisplay, target, theLink.LinkDisplay);
                        }

                        if (theLink.LinkType == "Section")
                        {
                            linkHtml = string.Format("<a href=\"#\" id=\"section_{0}\" onclick=\"clickSection('{1}')\" title=\"{2}\">{3}</a>", theLink.LinkTypeID, theLink.LinkTypeID, theLink.LinkDisplay, theLink.LinkDisplay);
                        }

                        if (theLink.LinkType == "Workflow")
                        {
                            linkHtml = string.Format("<a href=\"/{0}/Display/{1}\" title=\"{2}\">{3}</a>", theLink.LinkType, theLink.LinkTypeID, theLink.LinkDisplay, theLink.LinkDisplay);
                        }


                        newtext = newtext.Replace(MC5[i].Value, linkHtml);
                    }
                    catch
                    {
                        newtext = newtext.Replace(MC5[i].Value, "(Link not found)");
                    }
                }
            }

            return newtext;
        }
        public static string ReplaceObjectsandQuestions_blanks(HttpSessionStateBase SessionBase, string TheText, bool displayValues)
        {
            string patt0 = @"\{Parameter::[^}]*\}";
            string patt1 = @"\{DataObject::[^}]*\}";
            string patt2 = @"\{-Question:[^}]*\}";
            string patt3 = @"{Calc::C.*?}";
            string patt4 = @"\{IMPImage:[^}]*\}";
            string patt5 = @"\{-Link:[^}]*\}";

            string newtext = TheText;
            //patt = @"DataObject";
            Regex Rg0 = new Regex(patt0);
            Regex Rg = new Regex(patt1);
            Regex Rg2 = new Regex(patt2);
            Regex Rg3 = new Regex(patt3);
            Regex Rg4 = new Regex(patt4);
            Regex Rg5 = new Regex(patt5);

            if (TheText != "")
            {
                //Match and replce for Parameter
                MatchCollection MC0 = Rg0.Matches(TheText);
                for (int i = 0; i < MC0.Count; i++)
                {
                    string paramfull = MC0[i].Value;
                    string param = Regex.Split(paramfull, "::")[1];
                    param = param.Remove(param.Length - 1);

                    string responsestr;
                    responsestr = SessionControl.SessionManager.GetProgramParameterByKey(param, SessionBase);
                    if (responsestr != string.Empty)
                    {
                        newtext = newtext.Replace(MC0[i].Value, responsestr);
                    }
                    else
                    {
                        string responsestr2;
                        responsestr2 = SessionControl.SessionManager.GetScriptParameterByKey(param, SessionBase);
                        if (responsestr2 != string.Empty)
                        {
                            newtext = newtext.Replace(MC0[i].Value, responsestr2);
                        }
                        else
                        {
                            newtext = newtext.Replace(MC0[i].Value, "");
                        }
                    }
                }

                //Match and replce for DataObjects
                MatchCollection MC = Rg.Matches(TheText);
                for (int i = 0; i < MC.Count; i++)
                {
                    string datobstr = MC[i].Value;
                    int startof = datobstr.IndexOf("::") + 2;
                    string dataob = Regex.Split(datobstr, "::")[1];
                    dataob = dataob.Remove(dataob.Length - 1);
                    string dataobname = Regex.Split(dataob, "\\.")[0];
                    DataObjects ScriptDataObject = (DataObjects)SessionControl.SessionManager.GetDataObject(SessionBase, dataobname);

                    if (ScriptDataObject != null)
                    {
                        if (ScriptDataObject.IsDataObject(dataob, SessionBase))
                        {
                            //string gridstr = GenerateDataGrid(dataob);
                            //newtext = newtext.Replace(MC[i].Value, gridstr);
                        }
                        else
                        {
                            object responsestr = DataObjects.ReturnValue(dataob, SessionBase);
                            if (responsestr == null)
                            {
                                newtext = newtext.Replace(MC[i].Value, "");
                            }
                            else
                            {
                                if (responsestr.ToString() == "~OBJECT~")
                                {
                                    //responsestr = GenerateDataGrid(MC[i].Value);
                                }
                                else
                                {
                                    newtext = newtext.Replace(MC[i].Value, responsestr.ToString());
                                }
                            }
                        }
                    }
                    else
                    {
                        newtext = newtext.Replace(MC[i].Value, "");
                    }
                }

                //Match and replace questions
                MatchCollection MC2 = Rg2.Matches(TheText);
                for (int i = 0; i < MC2.Count; i++)
                {
                    string qval = MC2[i].Value;
                    string qid = qval.Split(new char[] { ':' })[1];
                    string responsestr;
                    if (displayValues)
                    {
                        responsestr = SessionControl.SessionManager.GetQuestionDisplayResponse(qid, SessionBase);
                    }
                    else
                    {
                        responsestr = SessionControl.SessionManager.GetQuestionResponse(qid, SessionBase);
                    }
                    if (responsestr != null)
                    {
                        newtext = newtext.Replace(MC2[i].Value, responsestr);
                    }
                    else
                    {
                        newtext = newtext.Replace(MC2[i].Value, "");
                    }
                }

                // Image match and replace
                //MatchCollection MC4 = Rg4.Matches(TheText);
                //for (int i = 0; i < MC4.Count; i++)
                //{
                //    string ival = MC4[i].Value;
                //    string iid = ival.Split(new char[] { ':' })[1];

                //    API.Elements.ImageController IC = new API.Elements.ImageController();
                //    var actionResult = IC.GetImage(Convert.ToInt32(iid));

                //    var response = actionResult as OkNegotiatedContentResult<ScriptImage>;
                //    //Assert.IsNotNull(response);
                //    ScriptImage theImage = response.Content;

                //    string alttext = theImage.ImageDesc;
                //    string replacestr = string.Format("<img src=\"{0}/Image/DisplayImage/{1}\" />", System.Web.HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority), iid);
                //    //string replacestr = "<img src=/Image/DisplayImage/" + iid + " />";
                //    newtext = newtext.Replace(MC4[i].Value, replacestr);

                //}

                // Calaculation match and replace
                MatchCollection MC3 = Rg3.Matches(TheText);
                if (MC3.Count > 0)
                {
                    ScriptCalculator IC = new ScriptCalculator();

                    for (int i = 0; i < MC3.Count; i++)
                    {
                        string str = MC3[i].ToString();
                        string pattern = ScriptCalculator.CalcStringPattern;
                        string cId = Regex.Match(str.ToString(), pattern, RegexOptions.IgnorePatternWhitespace).Groups[1].ToString();

                        try
                        {
                            ScriptCalculationInfo calcInfo = ScriptCalculationInfo.GetScriptCalculationBy(cId);

                            string calText = calcInfo.CalculationExpression;

                            //
                            MatchCollection MCInner = Rg.Matches(calText);
                            for (int k = 0; k < MCInner.Count; k++)
                            {
                                string datobstr = MCInner[k].Value;
                                string dataob = Regex.Split(datobstr, "::")[1];
                                dataob = dataob.Remove(dataob.Length - 1);
                                string dataobname = Regex.Split(dataob, "\\.")[0];
                                DataObjects ScriptDataObject = (DataObjects)SessionControl.SessionManager.GetDataObject(SessionBase, dataobname);

                                if (ScriptDataObject != null)
                                {
                                    if (ScriptDataObject.IsDataObject(dataob, SessionBase))
                                    {
                                        //string gridstr = GenerateDataGrid(dataob);
                                        //calText = calText.Replace(MCInner[k].Value, gridstr);
                                    }
                                    else
                                    {
                                        object responsestr = DataObjects.ReturnValue(dataob, SessionBase);
                                        if (responsestr == null)
                                        {
                                            calText = calText.Replace(MCInner[k].Value, "''");//"**Value was not found - " + dataob + " **");
                                        }
                                        else
                                        {

                                            calText = calText.Replace(MCInner[k].Value, "'" + responsestr.ToString() + "'");

                                        }
                                    }
                                }
                            }

                            IC.Parser(SessionBase, calText);
                            string outputstring = IC.Eval();

                            newtext = newtext.Replace(MC3[i].Value, outputstring);

                        }
                        catch
                        {
                            newtext = newtext.Replace(MC3[i].Value, "");
                        }
                    }
                }

                //Match and replace link
                //MatchCollection MC5 = Rg5.Matches(TheText);
                //for (int i = 0; i < MC5.Count; i++)
                //{
                //    try
                //    {
                //        string qval = MC5[i].Value;
                //        string linkid = qval.Split(new char[] { ':' })[1];

                //        API.Elements.LinkController linkController = new API.Elements.LinkController();
                //        var actionResult = linkController.GetScriptLink(Convert.ToDecimal(linkid));

                //        var response = actionResult as OkNegotiatedContentResult<ScriptLink>;
                //        //Assert.IsNotNull(response);
                //        ScriptLink theLink = response.Content;
                //        string linkHtml = string.Empty;


                //        string target = theLink.LinkNewWindow.Equals(true) ? "_blank" : "_self";

                //        if (theLink.LinkType == "Web" || theLink.LinkType == "Document")
                //        {
                //            //linkHtml = string.Format("<a href=\"{0}\" title=\"{1}\" target = \"{2}\">{3}</a>", theLink.LinkURL, theLink.LinkDisplay, target, theLink.LinkDisplay);
                //            linkHtml = string.Format("<a href='{0}' title=\"{1}\" target = \"{2}\">{3}</a>", theLink.LinkURL, theLink.LinkDisplay, target, theLink.LinkDisplay);
                //        }

                //        if (theLink.LinkType == "Section")
                //        {
                //            linkHtml = string.Format("<a href=\"#\" id=\"section_{0}\" onclick=\"clickSection('{1}')\" title=\"{2}\">{3}</a>", theLink.LinkTypeID, theLink.LinkTypeID, theLink.LinkDisplay, theLink.LinkDisplay);
                //        }

                //        if (theLink.LinkType == "Workflow")
                //        {
                //            linkHtml = string.Format("<a href=\"/{0}/Display/{1}\" title=\"{2}\">{3}</a>", theLink.LinkType, theLink.LinkTypeID, theLink.LinkDisplay, theLink.LinkDisplay);
                //        }


                //        newtext = newtext.Replace(MC5[i].Value, linkHtml);
                //    }
                //    catch
                //    {
                //        newtext = newtext.Replace(MC5[i].Value, "(Link not found)");
                //    }
                //}
            }

            return newtext;
        }

        public static List<string> GeneratePickData(string DataObject, string valCol, string dispCol, HttpSessionStateBase SessionBase)
        {
            //DataObject = "SpecialOffers.Offers";

            List<string> columnnames = new List<string>();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            string dataobname = Regex.Split(DataObject, "\\.")[0];
            string dataobcol = Regex.Split(DataObject, "\\.")[1];
            List<string> returnValues = new List<string>();
            DataObjects ScriptDataObject = (DataObjects)SessionControl.SessionManager.GetDataObject(SessionBase, dataobname);


            List<DataObjects> qObjects = DataObjects.ReturnObjects(DataObject, SessionBase);
            int i = 0;

            foreach (DataObjects theDO in qObjects)
            {

                string val = "";
                string disp = "";

                foreach (var item in theDO.Details)
                {
                    if (item.DetailName == valCol)
                    {
                        val = item.DetailValue.ToString();

                    }
                    if (item.DetailName == dispCol)
                    {
                        disp = item.DetailValue.ToString();


                    }
                }
                returnValues.Add(val + "||" + disp);
            }
            return returnValues;
        }

        public static List<string> GeneratePickData1(string DataObject, string valCol, string dispCol, HttpSessionStateBase SessionBase)
        {
            //DataObject = "SpecialOffers.Offers";

            List<string> columnnames = new List<string>();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            string dataobname = Regex.Split(DataObject, "\\.")[0];
            List<string> returnValues = new List<string>();
            DataObjects ScriptDataObject = (DataObjects)SessionControl.SessionManager.GetDataObject(SessionBase, dataobname);
            
            foreach (DataDetails detail in ScriptDataObject.Details)
            {
                string val = "";
                string disp = "";
                foreach (DataDetails item in detail.SubDetails)
                {
                    if (item.DetailName == valCol)
                    {
                        val = item.DetailValue.ToString();

                    }
                    if (item.DetailName == dispCol)
                    {
                        disp = item.DetailValue.ToString();


                    }
                }
                returnValues.Add(val + "||" + disp);
            }
            return returnValues;
        }
        public static DataTable GeneratePickDataCollection(string DataObject, string valCol, string dispCol, HttpSessionStateBase SessionBase)
        {
            DataTable mydt = new DataTable();

            List<string> dispCols = Regex.Split(dispCol, "~").ToList();


                DataColumn DC = new DataColumn("KeyColumn");
                mydt.Columns.Add(DC);

            foreach (string displaycol in dispCols)
            {
                DataColumn dc1 = new DataColumn(displaycol);
                mydt.Columns.Add(dc1);
            }

            string dataobname = Regex.Split(DataObject, "\\.")[0];
            //List<string> returnValues = new List<string>();

            DataObjects ScriptDataObject = (DataObjects)SessionControl.SessionManager.GetDataObject(SessionBase, dataobname);


            foreach (DataDetails detail in ScriptDataObject.Details)
            {
                DataRow dr = mydt.NewRow();

                string val = "";
                string disp = "";
                foreach (DataDetails item in detail.SubDetails)
                {
                    if (item.DetailName == valCol)
                    {
                        if (item.DetailValue != null)
                        {
                            val = item.DetailValue.ToString();
                            dr["KeyColumn"] = val;
                        }

                    }
                    if (dispCols.Contains(item.DetailName))
                    {
                        if (item.DetailValue != null)
                        {
                            disp = item.DetailValue.ToString();
                            dr[item.DetailName] = disp;
                        }
                    }
                }

                mydt.Rows.Add(dr);
            }
            return mydt;
        }

        public static DataTable GetSelectGridCols(string valCol, string dispCol)
        {

            DataTable mydt = new DataTable();

            List<string> dispCols = Regex.Split(dispCol, "~").ToList();

            DataColumn DC = new DataColumn("KeyColumn");
            mydt.Columns.Add(DC);

            foreach (string displaycol in dispCols)
            {
                DataColumn dc1 = new DataColumn(displaycol);
                mydt.Columns.Add(dc1);
            }


            return mydt;

        }


        public static bool KeyExists(string KeyList, string KeyName)
        {
            if (KeyList == null) return false;

            List<string> keyPairs = Regex.Split(KeyList, ",").ToList();
            foreach (string keyPair in keyPairs)
            {
                if (Regex.Split(keyPair, ":")[0] == KeyName)
                {
                    return true;
                }
            }


            return false;
        }
        public static bool KeyValueEquals(string KeyList, string KeyName, string keyVal)
        {
            if (KeyList == null) return false;

            List<string> keyPairs = Regex.Split(KeyList, ",").ToList();
            bool keyfound = false;

            foreach (string keyPair in keyPairs)
            {
                if (Regex.Split(keyPair, ":")[0] == KeyName)
                {
                    keyfound = true;
                    if (Regex.Split(keyPair, ":")[1] == keyVal)
                    {
                        return true;
                    }
                }
            }

            if (!keyfound) return true;

            return false;
        }
        public static string GenerateGridData(string DataObject, HttpSessionStateBase SessionBase)
        {
            //DataObject = "SpecialOffers.Offers";
            List<string> columnnames = new List<string>();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            //string dataobname = Regex.Split(DataObject, "\\.")[0];
            //string dataobcol = Regex.Split(DataObject, "\\.")[1];

            DataObjects ScriptDataObject = (DataObjects)SessionControl.SessionManager.GetDataObject(SessionBase, "Lead");


            object[] qChoices = (object[])DataObjects.ReturnValue(DataObject, SessionBase);
            int i = 0;

            foreach (object choiceob in qChoices)
            {

                if (i == 0)
                {
                    SB.AppendLine("<thead>");
                    SB.AppendLine("<tr>");
                    Type objtype = choiceob.GetType();
                    PropertyInfo[] FIS = objtype.GetProperties();

                    foreach (PropertyInfo fi in FIS)
                    {
                        if (fi.PropertyType.Name == "String" || fi.PropertyType.Name == "Int32" || fi.PropertyType.Name == "Decimal" || fi.PropertyType.Name == "DateTime")
                        {
                            SB.AppendLine("<th>" + fi.Name + "</th>");
                            columnnames.Add(fi.Name);
                            // DT.Columns.Add(fi.Name, fi.PropertyType);
                        }
                    }
                    SB.AppendLine("</tr>");
                    SB.AppendLine("</thead>");
                    SB.AppendLine("<tbody>");

                }
                Type objtype2 = choiceob.GetType();
                PropertyInfo[] FIS2 = objtype2.GetProperties();
                string stringval;
                Int32 intval;
                Decimal decval;
                DateTime datetimecol;
                SB.AppendLine("<TR>");
                foreach (PropertyInfo fi in FIS2)
                {

                    for (int u = 0; u < columnnames.Count(); u++)
                    {
                        if (fi.Name == columnnames[u])
                        {

                            switch (fi.PropertyType.Name)
                            {
                                case "String":
                                    stringval = (string)objtype2.InvokeMember(fi.Name, BindingFlags.GetProperty, null, choiceob, null);
                                    SB.AppendLine("<TD>" + stringval + "</TD>");
                                    //DR[u] = stringval;
                                    break;

                                case "Int32":
                                    intval = (Int32)objtype2.InvokeMember(fi.Name, BindingFlags.GetProperty, null, choiceob, null);
                                    SB.AppendLine("<TD>" + intval.ToString() + "</TD>");
                                    //DR[u] = intval;
                                    break;

                                case "Decimal":
                                    decval = (Decimal)objtype2.InvokeMember(fi.Name, BindingFlags.GetProperty, null, choiceob, null);
                                    SB.AppendLine("<TD>" + decval.ToString() + "</TD>");
                                    //DR[u] = decval;
                                    break;

                                case "DateTime":
                                    datetimecol = (DateTime)objtype2.InvokeMember(fi.Name, BindingFlags.GetProperty, null, choiceob, null);
                                    SB.AppendLine("<TD>" + datetimecol.ToString() + "</TD>");
                                    //DR[u] = datetimecol;
                                    break;

                            }

                        }


                    }


                }
                SB.AppendLine("</TR>");
                //DT.Rows.Add(DR);
                i++;
            }
            SB.AppendLine("</tbody>");
            return SB.ToString();

        }

        public static string GenerateGridData2(string DataObject, HttpSessionStateBase SessionBase)
        {
            //DataObject = "SpecialOffers.Offers";
            List<string> columnnames = new List<string>();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            //string dataobname = Regex.Split(DataObject, "\\.")[0];
            //string dataobcol = Regex.Split(DataObject, "\\.")[1];

            // DataObjects ScriptDataObject = (DataObjects)SessionControl.SessionManager.GetDataObject(SessionBase, dataobname);


            List<DataObjects> qOb = DataObjects.ReturnObjects(DataObject, SessionBase);

            int i = 0;

            foreach (DataObjects choiceob in qOb)
            {

                if (i == 0)
                {


                    SB.AppendLine("<thead>");
                    SB.AppendLine("<tr>");

                    foreach (DataDetails ddet in choiceob.Details)
                    {

                        SB.AppendLine("<th>" + ddet.DetailName + "</th>");
                        columnnames.Add(ddet.DetailName);
                    }

                    SB.AppendLine("</tr>");
                    SB.AppendLine("</thead>");
                    SB.AppendLine("<tbody>");

                }
                string stringval;
                Int32 intval;
                Decimal decval;
                DateTime datetimecol;
                SB.AppendLine("<TR>");

                foreach (DataDetails ddet2 in choiceob.Details)
                {
                    switch (ddet2.DetailType)
                    {
                        case "String":
                            stringval = ddet2.DetailValue == null ? "" : ddet2.DetailValue.ToString();
                            SB.AppendLine("<TD>" + stringval + "</TD>");
                            //DR[u] = stringval;
                            break;

                        case "Integer":
                            intval = Convert.ToInt32(ddet2.DetailValue.ToString());
                            SB.AppendLine("<TD>" + intval.ToString() + "</TD>");
                            //DR[u] = intval;
                            break;

                        case "Decimal":
                            decval = Convert.ToDecimal(ddet2.DetailValue.ToString());
                            SB.AppendLine("<TD>" + decval.ToString() + "</TD>");
                            //DR[u] = decval;
                            break;

                        case "DateTime":
                        case "Date":
                        case "Time":
                            datetimecol = DateTime.Parse(ddet2.DetailValue.ToString());
                            SB.AppendLine("<TD>" + datetimecol.ToString() + "</TD>");
                            //DR[u] = datetimecol;
                            break;



                    }



                }


                    SB.AppendLine("</TR>");
                    //DT.Rows.Add(DR);
                    i++;




            }
            SB.AppendLine("</tbody>");
            return SB.ToString();



        }

        public static string GenerateGridData3(string DataObject, HttpSessionStateBase SessionBase)
        {
            List<string> columnnames = new List<string>();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            DataObjects ScriptDataObject = (DataObjects)SessionControl.SessionManager.GetDataObject(SessionBase, DataObject);
            int i = 0;

            foreach (DataDetails detail in ScriptDataObject.Details)
            {
                if (i == 0)
                {
                    SB.AppendLine("<thead>");
                    SB.AppendLine("<tr>");

                    foreach (DataDetails ddet in detail.SubDetails)
                    {

                        SB.AppendLine("<th>" + ddet.DetailName + "</th>");
                        columnnames.Add(ddet.DetailName);
                    }

                    SB.AppendLine("</tr>");
                    SB.AppendLine("</thead>");
                    SB.AppendLine("<tbody>");

                }

                string stringval;
                Int32 intval;
                Decimal decval;
                DateTime datetimecol;
                SB.AppendLine("<TR>");

                foreach (DataDetails ddet2 in detail.SubDetails)
                {
                    switch (ddet2.DetailType)
                    {
                        case "String":
                            stringval = ddet2.DetailValue == null ? "" : ddet2.DetailValue.ToString();
                            SB.AppendLine("<TD>" + stringval + "</TD>");
                            //DR[u] = stringval;
                            break;

                        case "Integer":
                            intval = Convert.ToInt32(ddet2.DetailValue.ToString());
                            SB.AppendLine("<TD>" + intval.ToString() + "</TD>");
                            //DR[u] = intval;
                            break;

                        case "Decimal":
                            decval = Convert.ToDecimal(ddet2.DetailValue.ToString());
                            SB.AppendLine("<TD>" + decval.ToString() + "</TD>");
                            //DR[u] = decval;
                            break;

                        case "DateTime":
                        case "Date":
                        case "Time":
                            datetimecol = DateTime.Parse(ddet2.DetailValue.ToString());
                            SB.AppendLine("<TD>" + datetimecol.ToString() + "</TD>");
                            //DR[u] = datetimecol;
                            break;
                    }
                }

                SB.AppendLine("</TR>");
                i++;
            }
            SB.AppendLine("</tbody>");
            return SB.ToString();
        }

        public static string GetVariableValue(HttpSessionStateBase Session, int variableId)
        {
            string variableValue = string.Empty;
            ClauseEvaluator CE = new ClauseEvaluator(Session);
            VariableController variableController = new VariableController();
            var variableresult = variableController.GetScriptVariable(variableId);
            var variableresponse = variableresult as OkNegotiatedContentResult<ScriptVariable>;
            ScriptVariable scriptVariable = (ScriptVariable)variableresponse.Content;

            foreach (var items in scriptVariable.ScriptVariableDetails)
            {
                bool isValue = CE.EvaluateClause(items.ScriptClauseID.ToString(), items.ShowIf);

                if (isValue)
                {
                    if (items.Value.StartsWith("{"))
                        variableValue = ReplaceObjectsandQuestions(Session, items.Value, false);
                    else
                        variableValue = items.Value;

                    break;
                }
            }

            return variableValue;
        }
    }
    public struct ClauseExpressionResult
    {
        public String ReturnString;
        public decimal ReturnNumber;
        public DateTime ReturnDateTime;
        public Boolean IsDate;
        public Boolean IsNumber;
        public Boolean IsList;
        public String[] ReturnStringList;
        public decimal[] ReturnNumberList;
        public DateTime[] ReturnDateList;
        public Boolean Unknown;
        public string OrigDataType;
    }
 
    public class ClauseEvaluator
    {
        public HttpSessionStateBase theSession;

        public ClauseEvaluator(HttpSessionStateBase callerSession)
        {
            theSession = callerSession;

        }



        public  bool EvaluateClause(string ClauseID, Int32 ShowIF)
        {
            Int32 clauseResult = EvaluateClause(ClauseID);
            return ShouldContinue(ShowIF,clauseResult);
        }


        public  int EvaluateClause(string ClauseID)
        {
            ClauseController CC = new ClauseController();
            var clauseresult = CC.GetClause(Convert.ToInt32(ClauseID));
            var clauseresponse = clauseresult as OkNegotiatedContentResult<Clause>;
            Clause currentClause = (Clause)clauseresponse.Content;

            return GetClauseResult(currentClause);
        }


        protected int GetClauseResult(Clause TheClause)
        {
            ArrayList resultset = new ArrayList();

            int result = 0;
            bool canttell;

            if (TheClause == null)
            {
                return 2;
            }

            foreach (Clause claws in TheClause.SubClause)
            {
                if (claws.SubClause != null)
                {
                    result = GetClauseResult(claws);
                }
                else
                {
                    if (claws.LSide.EType == Clause.ElementTypeLeft.Clause)
                    {
                        bool trueorfalse = EvaluateClause(claws.LSide.CID, claws.LSide.Sif);

                        if (trueorfalse == true)
                        {
                            result = 1;
                        }
                        else
                        {
                            result = 0;
                        }

                    }
                    else
                    {
                        if (claws.ClauseComplete == true)
                        {
                            result = CheckExpression(claws);
                        }
                        else
                        {
                            result = 2;
                        }

                    }
                }
                resultset.Add(result);
            }
            switch (TheClause.CTyp)
            {
                case Clause.ClauseType.AND:
                    canttell = false;
                    foreach (Int32 reslt in resultset)
                    {
                        if (reslt == 0)
                        {
                            return 0;
                        }
                        else if (reslt == 2)
                        {
                            canttell = true;
                        }
                    }
                    if (canttell == true)
                    {
                        return 2;
                    }
                    else
                    {
                        return 1;
                    }
                case Clause.ClauseType.NAND:
                    canttell = false;
                    bool foundtrue = false;
                    foreach (Int32 reslt in resultset)
                    {
                        if (reslt == 1)
                        {
                            if (foundtrue == true)
                            {
                                return 0;
                            }
                            else if (canttell == true)
                            {
                                return 2;
                            }
                            else
                            {
                                foundtrue = true;
                            }
                        }
                        else if (reslt == 2)
                        {
                            if (foundtrue == true || canttell == true)
                            {
                                return 2;
                            }
                            else
                            {
                                canttell = true;
                            }


                        }
                    }
                    return 1;
                case Clause.ClauseType.NOR:
                    canttell = false;
                    foreach (Int32 reslt in resultset)
                    {
                        if (reslt == 1)
                        {
                            return 0;
                        }
                        else if (reslt == 2)
                        {
                            canttell = true;
                        }
                    }
                    if (canttell == true)
                    {
                        return 2;
                    }
                    else
                    {
                        return 1;
                    }
                case Clause.ClauseType.OR:
                    canttell = false;
                    foreach (Int32 reslt in resultset)
                    {
                        if (reslt == 1)
                        {
                            return 1;
                        }
                        else if (reslt == 2)
                        {
                            canttell = true;
                        }
                    }
                    if (canttell == true)
                    {
                        return 2;
                    }
                    else
                    {
                        return 0;
                    }
            }


            return 0;
        }

        protected int CheckExpression(Clause checkclause)
        {
            // Item Clause
            if (checkclause.LSide.EType == Clause.ElementTypeLeft.Item)
            {

                return CheckItemExpression(checkclause);
            }
            //ItemQuantity Clause
            else if (checkclause.LSide.EType == Clause.ElementTypeLeft.ItemQuantity)
            {
                return CheckItemQuantityExpression(checkclause);
            }
            //CalculationClause
            else if (checkclause.LSide.EType == Clause.ElementTypeLeft.Calculation || checkclause.RSide.EType == Clause.ElementTypeRight.Calculation)
            {
                string lsidecalc = null;
                string rsidecalc = null;

                ScriptCalculator IC = new ScriptCalculator();

                if (checkclause.LSide.EType == Clause.ElementTypeLeft.Calculation)
                {
                    ScriptCalculationInfo calcInfo = ScriptCalculationInfo.GetScriptCalculationBy(checkclause.LSide.CalcID);
                    IC.Parser(theSession, calcInfo.CalculationExpression);
                    lsidecalc = IC.Eval();



                }
                if (checkclause.RSide.EType == Clause.ElementTypeRight.Calculation)
                {
                    ScriptCalculationInfo calcInfo = ScriptCalculationInfo.GetScriptCalculationBy(checkclause.LSide.CalcID);
                    IC.Parser(theSession, calcInfo.CalculationExpression);
                    lsidecalc = IC.Eval();


                }
                return CheckCalcExpression(checkclause, lsidecalc, rsidecalc);

            }
            // All other clauses
            else
            {
                return CheckCalcExpression(checkclause,"","");
            }


        }

        public int CheckCalcExpression(Clause checkclause, string lsidecalc, string rsidecalc)
        {

            DateTime leftdate = DateTime.Now;

            DateTime Rightdate = DateTime.Now;

            LExpr LE = checkclause.LSide;


            ClauseExpressionResult RightResult = new ClauseExpressionResult();
            ClauseExpressionResult LeftResult = new ClauseExpressionResult();

            if (checkclause.LSide.EType == Clause.ElementTypeLeft.Calculation && checkclause.RSide.EType == Clause.ElementTypeRight.Calculation)
            {
                LeftResult = new ClauseExpressionResult();
                if (lsidecalc.Contains(ScriptCalculator.DELIM))
                {
                    LeftResult.IsList = true;
                    LeftResult.ReturnStringList = Regex.Split(lsidecalc, ScriptCalculator.DELIM);
                    LeftResult.OrigDataType = "String";

                    ArrayList numbersadd = new ArrayList();
                    try
                    {
                        foreach (string teststring in LeftResult.ReturnStringList)
                        {
                            decimal td = Convert.ToDecimal(teststring);
                            numbersadd.Add(td);
                        }

                        LeftResult.IsNumber = true;
                        LeftResult.ReturnNumberList = new decimal[numbersadd.Count];
                        LeftResult.OrigDataType = "Decimal";
                        numbersadd.CopyTo(LeftResult.ReturnNumberList);
                    }
                    catch
                    {
                        LeftResult.IsNumber = false;

                    }
                    ArrayList datestoadd = new ArrayList();
                    try
                    {
                        foreach (string teststring in LeftResult.ReturnStringList)
                        {
                            DateTime td = DateTime.Parse(teststring);
                            datestoadd.Add(td);

                        }
                        LeftResult.ReturnDateList = new DateTime[datestoadd.Count];
                        datestoadd.CopyTo(LeftResult.ReturnDateList);
                        LeftResult.IsDate = true;
                        LeftResult.OrigDataType = "Date";
                    }
                    catch
                    {
                        LeftResult.IsDate = false;
                    }



                }
                else
                {
                    LeftResult.IsList = false;
                    LeftResult.ReturnString = lsidecalc;
                    LeftResult.OrigDataType = "String";

                    try
                    {
                        decimal td = Convert.ToDecimal(LeftResult.ReturnString);

                        LeftResult.IsNumber = true;
                        LeftResult.OrigDataType = "Decimal";

                    }
                    catch
                    {
                        LeftResult.IsNumber = false;
                    }
                    try
                    {
                        DateTime td = DateTime.Parse(LeftResult.ReturnString);

                        LeftResult.IsDate = true;
                        LeftResult.OrigDataType = "Date";
                    }
                    catch
                    {
                        LeftResult.IsDate = false;
                    }
                }

                RightResult = new ClauseExpressionResult();
                if (rsidecalc.Contains(ScriptCalculator.DELIM))
                {
                    RightResult.IsList = true;
                    RightResult.ReturnStringList = Regex.Split(rsidecalc, ScriptCalculator.DELIM);
                    RightResult.OrigDataType = "String";
                    ArrayList numbersadd2 = new ArrayList();
                    try
                    {
                        foreach (string teststring in RightResult.ReturnStringList)
                        {
                            decimal td = Convert.ToDecimal(teststring);
                            numbersadd2.Add(td);
                        }

                        RightResult.IsNumber = true;
                        RightResult.ReturnNumberList = new decimal[numbersadd2.Count];
                        RightResult.OrigDataType = "Decimal";
                        numbersadd2.CopyTo(RightResult.ReturnNumberList);
                    }
                    catch
                    {
                        RightResult.IsNumber = false;
                    }
                    ArrayList datestoadd = new ArrayList();
                    try
                    {
                        foreach (string teststring in RightResult.ReturnStringList)
                        {
                            DateTime td = DateTime.Parse(teststring);
                            datestoadd.Add(td);

                        }
                        RightResult.ReturnDateList = new DateTime[datestoadd.Count];
                        datestoadd.CopyTo(RightResult.ReturnDateList);
                        RightResult.IsDate = true;
                        RightResult.OrigDataType = "Date";
                    }
                    catch
                    {
                        RightResult.IsDate = false;
                    }



                }
                else
                {

                    RightResult.IsList = false;
                    RightResult.ReturnString = rsidecalc;
                    RightResult.OrigDataType = "String";

                    try
                    {
                        decimal td = Convert.ToDecimal(RightResult.ReturnString);
                        RightResult.OrigDataType = "Decimal";

                        RightResult.IsNumber = true;
                    }
                    catch
                    {
                        RightResult.IsNumber = false;
                    }
                    try
                    {
                        DateTime td = DateTime.Parse(RightResult.ReturnString);
                        RightResult.OrigDataType = "Date";
                        RightResult.IsDate = true;
                    }
                    catch
                    {
                        RightResult.IsDate = false;
                    }

                }
                if (RightResult.OrigDataType != LeftResult.OrigDataType)
                {
                    RightResult.OrigDataType = "String";
                    LeftResult.OrigDataType = "String";
                }

            }
            else
            {

                if (checkclause.LSide.EType == Clause.ElementTypeLeft.Calculation)
                {
                    LeftResult = new ClauseExpressionResult();
                    if (lsidecalc.Contains(ScriptCalculator.DELIM))
                    {
                        LeftResult.IsList = true;
                        LeftResult.ReturnStringList = Regex.Split(lsidecalc, ScriptCalculator.DELIM);
                        LeftResult.OrigDataType = checkclause.RSide.FTyp;

                        ArrayList numbersadd = new ArrayList();
                        try
                        {
                            foreach (string teststring in LeftResult.ReturnStringList)
                            {
                                decimal td = Convert.ToDecimal(teststring);
                                numbersadd.Add(td);
                            }

                            LeftResult.IsNumber = true;
                            LeftResult.ReturnNumberList = new decimal[numbersadd.Count];

                            numbersadd.CopyTo(LeftResult.ReturnNumberList);
                        }
                        catch
                        {
                            LeftResult.IsNumber = false;
                        }
                        ArrayList datestoadd = new ArrayList();
                        try
                        {
                            foreach (string teststring in LeftResult.ReturnStringList)
                            {
                                DateTime td = DateTime.Parse(teststring);
                                datestoadd.Add(td);

                            }
                            LeftResult.ReturnDateList = new DateTime[datestoadd.Count];
                            datestoadd.CopyTo(LeftResult.ReturnDateList);
                            LeftResult.IsDate = true;
                        }
                        catch
                        {
                            LeftResult.IsDate = false;
                        }



                    }
                    else
                    {
                        LeftResult.IsList = false;
                        LeftResult.ReturnString = lsidecalc;
                        try
                        {
                            decimal td = Convert.ToDecimal(LeftResult.ReturnString);
                            LeftResult.ReturnNumber = td;
                            LeftResult.IsNumber = true;
                        }
                        catch
                        {
                            LeftResult.IsNumber = false;
                        }
                        try
                        {
                            DateTime td = DateTime.Parse(LeftResult.ReturnString);

                            LeftResult.IsDate = true;
                        }
                        catch
                        {
                            LeftResult.IsDate = false;
                        }
                    }

                }
                else
                {
                    try
                    {

                        LeftResult = ReturnExpressionResult(checkclause.LSide);
                        if (LeftResult.Unknown)
                        {
                            return 2;
                        }
                        LeftResult.OrigDataType = checkclause.LSide.FTyp;

                    }
                    catch
                    {
                        return 2;
                    }

                }


                if (checkclause.RSide.EType == Clause.ElementTypeRight.Calculation)
                {
                    RightResult = new ClauseExpressionResult();
                    if (rsidecalc.Contains(ScriptCalculator.DELIM))
                    {
                        RightResult.IsList = true;
                        RightResult.ReturnStringList = Regex.Split(rsidecalc, ScriptCalculator.DELIM);
                        RightResult.OrigDataType = checkclause.LSide.FTyp;
                        ArrayList numbersadd2 = new ArrayList();
                        try
                        {
                            foreach (string teststring in RightResult.ReturnStringList)
                            {
                                decimal td = Convert.ToDecimal(teststring);
                                numbersadd2.Add(td);
                            }

                            RightResult.IsNumber = true;
                            RightResult.ReturnNumberList = new decimal[numbersadd2.Count];
                            numbersadd2.CopyTo(RightResult.ReturnNumberList);
                        }
                        catch
                        {
                            RightResult.IsNumber = false;
                        }
                        ArrayList datestoadd = new ArrayList();
                        try
                        {
                            foreach (string teststring in RightResult.ReturnStringList)
                            {
                                DateTime td = DateTime.Parse(teststring);
                                datestoadd.Add(td);

                            }
                            RightResult.ReturnDateList = new DateTime[datestoadd.Count];
                            datestoadd.CopyTo(RightResult.ReturnDateList);
                            RightResult.IsDate = true;
                        }
                        catch
                        {
                            RightResult.IsDate = false;
                        }



                    }
                    else
                    {

                        RightResult.IsList = false;
                        RightResult.ReturnString = rsidecalc;
                        try
                        {
                            decimal td = Convert.ToDecimal(RightResult.ReturnString);

                            RightResult.IsNumber = true;
                        }
                        catch
                        {
                            RightResult.IsNumber = false;
                        }
                        try
                        {
                            DateTime td = DateTime.Parse(RightResult.ReturnString);

                            RightResult.IsDate = true;
                        }
                        catch
                        {
                            RightResult.IsDate = false;
                        }

                    }
                }
                else
                {

                    try
                    {

                        RightResult = ReturnExpressionResult(checkclause.RSide);
                        if (RightResult.Unknown)
                        {
                            return 2;
                        }
                        RightResult.OrigDataType = checkclause.RSide.FTyp;

                    }
                    catch
                    {
                        return 2;
                    }
                }
            }

            return returnExpressionResult(LeftResult, RightResult, checkclause, checkclause.mcase);


        }

        public int CheckItemExpression(Clause checkclause)
        {
            List<ItemOrdered> IO = SessionControl.SessionManager.GetItemsOrdered(theSession);

            bool OrderedAll = true;
            bool OrderedNone = true;
            Int32 NumberOrdered = 0;
            bool founditem = false;

            try
            {
                switch (checkclause.RSide.EType)
                {
                    case Clause.ElementTypeRight.Dobj:
                        break;
                    case Clause.ElementTypeRight.Question:
                        break;
                    case Clause.ElementTypeRight.Item:
                        break;



                }
                List<ScriptItem> TheItemList = GetClauseItems(checkclause.RSide.MVal);
                foreach (ScriptItem iteml in TheItemList)
                {
                    foreach (ItemOrdered Iord in IO)
                    {
                        if (Iord.ItemCode == iteml.ItemCode)
                        {
                            founditem = true;
                            if (Iord.ItemQuantity > 0)
                            {
                                OrderedNone = false;
                                NumberOrdered += 1;
                                break;
                            }
                            else
                            {
                                OrderedAll = false;
                            }
                            break;
                        }
                    }
                    if (founditem == false)
                    {
                        OrderedAll = false;
                    }
                }


                switch (checkclause.Cmp)
                {
                    case Clause.CmpType.Includes_All:
                        if (OrderedAll)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }

                    case Clause.CmpType.Includes_Any:
                        if (OrderedNone)
                        {
                            return 0;
                        }
                        else
                        {
                            return 1;
                        }

                    case Clause.CmpType.Includes_None:
                        if (OrderedNone)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }

                    case Clause.CmpType.Includes_X:
                        if (NumberOrdered == checkclause.x)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }

                    case Clause.CmpType.Includes_X_or_Less:
                        if (NumberOrdered <= checkclause.x)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    case Clause.CmpType.Includes_X_or_More:
                        if (NumberOrdered >= checkclause.x)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }

                }

            }
            catch
            {
                return 2;
            }


            return 0;
        }
        public List<ScriptItem> GetClauseItems(string ItemList)
        {

            string[] IndItems = System.Text.RegularExpressions.Regex.Split(ItemList, ",");

            List<ScriptItem> ScriptItems = new List<ScriptItem>();
            API.ItemController itemC = new API.ItemController();

            foreach (string indItem in IndItems)
            {


                string[] itemparts = indItem.Split(new char[] { '\\' });
                switch (itemparts.Length)
                {
                    case 1:  //Category Selected

                        ScriptItems.AddRange(itemC.GetActiveScriptItems(itemparts[0]));

                        break;
                    case 2:
                        ScriptItems.AddRange(itemC.GetActiveScriptItems(itemparts[0], itemparts[1]));

                        break;
                    case 3:    //Item Selected

                        string[] itempieces = System.Text.RegularExpressions.Regex.Split(itemparts[2], "::");
                        ScriptItems.Add(itemC.GetScriptItem(itemparts[0], itemparts[1], itempieces[0]));

                        break;


                    default:

                        break;
                }


            }

            return ScriptItems;


        }

        public int CheckItemQuantityExpression(Clause checkclause)
        {

            List<ItemOrdered> IO = SessionControl.SessionManager.GetItemsOrdered(theSession);

            
            Int32 Leftquant = 0;
            Int32 Rightquant = 0;
            bool nothingOrdered = false;

            object oresp2;
            if (IO == null)
            {
                nothingOrdered = true;
            }
            else if (IO.Count < 1)
            {
                nothingOrdered = true;
            }
            else if (IO[0] == null)
            {
                nothingOrdered = true;
            }



            try
            {
                if (nothingOrdered)
                {
                    Leftquant = 0;
                }
                else
                {
                    List<ScriptItem> TheItemList = GetClauseItems(checkclause.LSide.MVal);
                    foreach (ScriptItem iteml in TheItemList)
                    {
                        foreach (ItemOrdered Iord in IO)
                        {
                            if (Iord.ItemCode == iteml.ItemCode)
                            {
                                Leftquant += Iord.ItemQuantity;
                                break;
                            }
                        }
                    }
                }
                RExpr RE = checkclause.RSide;

                switch (RE.EType)
                {
                    case Clause.ElementTypeRight.Dobj:
                        ClauseExpressionResult  CER = ReturnDOResult(RE.Dobj, "Number", "");
                        oresp2 = CER.ReturnNumber;
                        if (oresp2 == null || oresp2 == "")
                        {
                            return 2;
                        }
                        try
                        {
                            Rightquant = Convert.ToInt32(oresp2);
                        }
                        catch
                        {
                            return 2;
                        }
                        break;
                    case Clause.ElementTypeRight.Question:
                        ClauseExpressionResult CERQ = ReturnQResult(RE.Qid.ToString(), "Number");
                        oresp2 = CERQ.ReturnNumber;
                        if (oresp2 == null || oresp2 == "")
                        {
                            return 2;
                        }
                        try
                        {
                            Rightquant = Convert.ToInt32(oresp2);
                        }
                        catch
                        {
                            return 2;
                        }
                        break;
                    case Clause.ElementTypeRight.ItemQuantity:


                        if (nothingOrdered)
                        {
                            Rightquant = 0;
                        }
                        else
                        {

                            List<ScriptItem> TheItemListR = GetClauseItems(checkclause.RSide.MVal);
                            foreach (ScriptItem itemr in TheItemListR)
                            {
                                foreach (ItemOrdered Iord in IO)
                                {
                                    if (Iord.ItemCode == itemr.ItemCode)
                                    {
                                        Rightquant += Iord.ItemQuantity;
                                        break;
                                    }
                                }
                            }

                        }
                        break;
                    case Clause.ElementTypeRight.EnterValue:
                        try
                        {
                            Rightquant = Convert.ToInt32(checkclause.RSide.NumberValue);
                        }
                        catch
                        {
                            return 2;
                        }
                        break;

                }
                bool equalsit = false;
                bool greaterthanit = false;
                bool lessthanit = false;

                if (Leftquant == Rightquant)
                {
                    equalsit = true;
                }
                else if (Leftquant > Rightquant)
                {
                    greaterthanit = true;
                }
                else if (Leftquant < Rightquant)
                {
                    lessthanit = true;
                }

                switch (checkclause.Cmp)
                {
                    case Clause.CmpType.Equals:
                        if (equalsit == true)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    case Clause.CmpType.Greater_than:
                        if (greaterthanit == true)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    case Clause.CmpType.Greater_than_or_Equals:
                        if (greaterthanit == true || equalsit == true)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    case Clause.CmpType.Less_than:
                        if (lessthanit == true)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    case Clause.CmpType.Less_than_or_Equals:
                        if (lessthanit == true || equalsit == true)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    case Clause.CmpType.Not_Equal_too:
                        if (equalsit == false)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                }


            }
            catch
            {
                return 2;
            }

            return 2;
        }


        private ClauseExpressionResult ReturnQResult(string Qid, string dtype)
        {
            ClauseExpressionResult CER = new ClauseExpressionResult();

            string oresp = SessionControl.SessionManager.GetQuestionResponse(Qid, theSession);
            
            if (oresp == null)
            {
                CER.Unknown = true;
                return CER;
            }
            switch (dtype)
            {
                case "String":

                    string retstring = (string)oresp;

                    if (retstring.Contains("~"))
                    {
                        CER.ReturnStringList = Regex.Split(retstring, "~");
                        CER.IsList = true;
                        ArrayList numbersadd = new ArrayList();
                        try
                        {
                            foreach (string teststring in CER.ReturnStringList)
                            {
                                decimal td = Convert.ToDecimal(teststring);
                                numbersadd.Add(td);
                            }

                            CER.IsNumber = true;
                            numbersadd.CopyTo(CER.ReturnNumberList);
                        }
                        catch
                        {
                            CER.IsNumber = false;
                        }
                        ArrayList datestoadd = new ArrayList();
                        try
                        {
                            foreach (string teststring in CER.ReturnStringList)
                            {
                                DateTime td = DateTime.Parse(teststring);
                                datestoadd.Add(td);

                            }
                            datestoadd.CopyTo(CER.ReturnDateList);
                            CER.IsDate = true;
                        }
                        catch
                        {
                            CER.IsDate = false;
                        }

                    }
                    else
                    {
                        CER.ReturnString = retstring;
                        try
                        {
                            decimal td = Convert.ToDecimal(CER.ReturnString);

                            CER.IsNumber = true;
                        }
                        catch
                        {
                            CER.IsNumber = false;
                        }
                        try
                        {
                            DateTime td = DateTime.Parse(CER.ReturnString);

                            CER.IsDate = true;
                        }
                        catch
                        {
                            CER.IsDate = false;
                        }


                    }
                    break;
                case "Number":
                    CER.ReturnNumber = Convert.ToDecimal(oresp);

                    break;
                case "Date":
                case "Time":
                case "DateTime":
                    CER.ReturnDateTime = DateTime.Parse(oresp);
                    break;
                default:
                    CER.Unknown = true;
                    break;

            }

            return CER;

        }

        private ClauseExpressionResult ReturnDOResult(string DODetName, string dtype, string delim)
        {
            DataDetails DD = null;
            ClauseExpressionResult CER = new ClauseExpressionResult();

            string dataobname = Regex.Split(DODetName, "\\.")[0];

            if (dataobname.StartsWith("Parameter::"))
            {
                string param = Regex.Split(DODetName, "::")[1];

                string responsestr = SessionControl.SessionManager.GetProgramParameterByKey(param, theSession);
                if (string.IsNullOrEmpty(responsestr)) responsestr = SessionControl.SessionManager.GetScriptParameterByKey(param, theSession);

                if (string.IsNullOrEmpty(responsestr))
                {
                    CER.Unknown = true;
                    return CER;
                }
                else
                {
                    switch (dtype)
                    {
                        case "String":
                            CER.ReturnString = responsestr;
                            if (delim != "" && delim != null)
                            {
                                Regex r = new Regex(delim);
                                CER.ReturnStringList = r.Split(CER.ReturnString);
                                CER.IsList = true;
                            }
                            break;
                        case "Number":
                        case "Decimal":
                        case "Integer":
                            CER.IsNumber = true;
                            CER.ReturnNumber = Convert.ToDecimal(DD.DetailValue);
                            break;
                        case "DateTime":
                        case "Date":
                        case "Time":
                            CER.IsDate = true;
                            CER.ReturnDateTime = (DateTime)DD.DetailValue;
                            break;
                        default:
                            CER.Unknown = true;
                            return CER;
                    }
                }
            }
            else
            {
                DataObjects ScriptDataObject = (DataObjects)SessionControl.SessionManager.GetDataObject(theSession, dataobname);

                DD = ScriptDataObject.ReturnObjectDetail(DODetName, ScriptDataObject.Details);
                if (DD == null)
                {
                    CER.Unknown = true;
                    return CER;
                }

                if (DD.DetailValue == null)
                {
                    CER.Unknown = true;
                    return CER;
                }

                //if (DD.Collection)
                //{
                //    if (DD.DetailType != "Object")
                //    {
                //        CER.IsList = true;
                //        switch (dtype)
                //        {
                //            case "String":
                //                CER.ReturnStringList = (string[])DD.DetailValue;
                //                ArrayList numbersadd = new ArrayList();
                //                try
                //                {
                //                    foreach (string teststring in CER.ReturnStringList)
                //                    {
                //                        decimal td = Convert.ToDecimal(teststring);
                //                        numbersadd.Add(td);
                //                    }

                //                    CER.IsNumber = true;
                //                    CER.ReturnNumberList = new decimal[numbersadd.Count];
                //                    numbersadd.CopyTo(CER.ReturnNumberList);
                //                }
                //                catch
                //                {
                //                    CER.IsNumber = false;
                //                }
                //                ArrayList datestoadd = new ArrayList();
                //                try
                //                {
                //                    foreach (string teststring in CER.ReturnStringList)
                //                    {
                //                        DateTime td = DateTime.Parse(teststring);
                //                        datestoadd.Add(td);

                //                    }
                //                    CER.ReturnDateList = new DateTime[datestoadd.Count];
                //                    datestoadd.CopyTo(CER.ReturnDateList);
                //                    CER.IsDate = true;
                //                }
                //                catch
                //                {
                //                    CER.IsDate = false;
                //                }
                //                return CER;
                //            case "Number":
                //            case "Decimal":
                //                CER.ReturnNumberList = (Decimal[])DD.DetailValue;
                //                break;
                //            case "Integer":
                //                Int32[] intlist = (Int32[])DD.DetailValue;
                //                CER.ReturnNumberList = new Decimal[intlist.Length];
                //                for (int i = 0; i < intlist.Length; i++)
                //                {
                //                    CER.ReturnNumberList[i] = Convert.ToDecimal(intlist[i]);
                //                }
                //                break;
                //            case "DateTime":
                //            case "Date":
                //            case "Time":
                //                CER.ReturnDateList = (DateTime[])DD.DetailValue;
                //                break;
                //            default:
                //                CER.Unknown = true;
                //                return CER;
                //        }
                //    }
                //    else
                //    {
                //        CER.Unknown = true;
                //        return CER;
                //    }
                //}
                //else
                //{
                switch (dtype)
                {
                    case "String":
                        CER.ReturnString = DD.DetailValue.ToString();
                        if (delim != "" && delim != null)
                        {

                            Regex r = new Regex(delim);

                            CER.ReturnStringList = r.Split(CER.ReturnString);
                            CER.IsList = true;
                        }
                        break;
                    case "Number":
                    case "Decimal":
                    case "Integer":
                        CER.IsNumber = true;
                        CER.ReturnNumber = Convert.ToDecimal(DD.DetailValue);
                        break;
                    case "DateTime":
                    case "Date":
                    case "Time":
                        CER.IsDate = true;
                        CER.ReturnDateTime = (DateTime)DD.DetailValue;
                        break;
                    default:
                        CER.Unknown = true;
                        return CER;
                }
                //}

            }
            return CER;
        }

        private ClauseExpressionResult ReturnExpressionResult(LExpr LE)
        {

            ClauseExpressionResult CER = new ClauseExpressionResult();
            CER.IsList = false;

            switch (LE.EType)
            {
                case Clause.ElementTypeLeft.Question:
                    CER = ReturnQResult(LE.Qid.ToString(), LE.FTyp);
                    return CER;
                case Clause.ElementTypeLeft.DateTime:
                    CER.ReturnDateTime = System.DateTime.Now;
                    if (LE.Hpl > 0 || LE.Hpl < 0)
                    {
                        CER.ReturnDateTime.AddHours(LE.Hpl);
                    }
                    if (LE.Dpl > 0 || LE.Dpl < 0)
                    {
                        CER.ReturnDateTime.AddDays(LE.Hpl);
                    }
                    CER.IsDate = true;
                    CER.ReturnString = CER.ReturnDateTime.ToString();

                    return CER;
                case Clause.ElementTypeLeft.Dobj:

                    CER = ReturnDOResult(LE.Dobj, LE.FTyp, LE.MVal);
                    return CER;

            }

            return CER;
        }

        private ClauseExpressionResult ReturnExpressionResult(RExpr RE)
        {
            ClauseExpressionResult CER = new ClauseExpressionResult();


            switch (RE.EType)
            {
                case Clause.ElementTypeRight.Dobj:
                    CER = ReturnDOResult(RE.Dobj, RE.FTyp, RE.MVal);
                    return CER;
                case Clause.ElementTypeRight.Question:
                    CER = ReturnQResult(RE.Qid.ToString(), RE.FTyp);
                    return CER;

                case Clause.ElementTypeRight.DateValue:
                case Clause.ElementTypeRight.DTVal:
                case Clause.ElementTypeRight.TimeValue:
                    CER.ReturnDateTime = RE.DTVal;
                    CER.IsDate = true;
                    return CER;
                case Clause.ElementTypeRight.SelectValue:
                case Clause.ElementTypeRight.EnterValue:
                    string[] responsearray2 = RE.MVal.Split(new char[] { '~' });

                    if (responsearray2.Count() > 1)
                    {
                        CER.IsList = true;


                        switch (RE.FTyp)
                        {
                            case "String":
                                CER.ReturnStringList = responsearray2;
                                try
                                {
                                    ArrayList stringcon = new ArrayList();
                                    foreach (string respstr in responsearray2)
                                    {
                                        Decimal rightnum = Convert.ToDecimal(respstr);
                                        stringcon.Add(rightnum);
                                    }
                                    CER.ReturnNumberList = new Decimal[stringcon.Count];
                                    stringcon.CopyTo(CER.ReturnNumberList);
                                    CER.IsNumber = true;
                                }
                                catch
                                {
                                    CER.IsNumber = false;
                                }
                                ArrayList datestoadd = new ArrayList();
                                try
                                {
                                    foreach (string teststring in CER.ReturnStringList)
                                    {
                                        DateTime td = DateTime.Parse(teststring);
                                        datestoadd.Add(td);

                                    }
                                    CER.ReturnDateList = new DateTime[datestoadd.Count];
                                    datestoadd.CopyTo(CER.ReturnDateList);
                                    CER.IsDate = true;
                                }
                                catch
                                {
                                    CER.IsDate = false;
                                }
                                break;
                            case "Number":
                                ArrayList stringcon2 = new ArrayList();
                                foreach (string respstr in responsearray2)
                                {
                                    Decimal rightnum = Convert.ToDecimal(respstr);
                                    stringcon2.Add(rightnum);
                                }
                                CER.ReturnNumberList = new Decimal[stringcon2.Count];
                                stringcon2.CopyTo(CER.ReturnNumberList);
                                break;
                            default:
                                CER.Unknown = true;
                                return CER;
                        }
                        break;
                    }
                    else
                    {

                        switch (RE.FTyp)
                        {
                            case "String":
                                CER.ReturnString = responsearray2[0];
                                try
                                {
                                    CER.ReturnNumber = Convert.ToDecimal(CER.ReturnString);
                                    CER.IsNumber = true;
                                }
                                catch
                                {
                                    CER.IsNumber = false;
                                }
                                try
                                {
                                    CER.ReturnDateTime = DateTime.Parse(CER.ReturnString);
                                    CER.IsDate = true;
                                }
                                catch
                                {
                                    CER.IsDate = false;
                                }
                                break;
                            case "Number":
                                CER.ReturnNumber = Convert.ToDecimal(responsearray2[0]);
                                CER.IsNumber = true;
                                break;
                            default:
                                CER.Unknown = true;
                                return CER;
                        }
                    }
                    break;
            }
            return CER;
        }
        public static int returnExpressionResult(ClauseExpressionResult LSide, ClauseExpressionResult RSide, Clause checkclause, bool MatchCase)
        {
            Clause.CmpType CompareType = checkclause.Cmp;

            if (LSide.IsList || RSide.IsList || CompareType.ToString().StartsWith("Include"))
            {
                Int32 includecount = 0;
                Int32 LengthAll = 0;
                if (!CompareType.ToString().StartsWith("Include"))
                {
                    if (CompareType.ToString() != "Equals_Any" && CompareType.ToString() != "Not_Equals_Any" && CompareType.ToString() != "Contains")
                    return 2;
                }

                if (LSide.OrigDataType != RSide.OrigDataType)
                {
                    if (LSide.IsDate && RSide.IsDate)
                    {
                        if (!LSide.IsList)
                        {
                            LSide.ReturnDateList = new DateTime[1];
                            LSide.ReturnDateList[0] = LSide.ReturnDateTime;
                        }
                        if (!RSide.IsList)
                        {
                            RSide.ReturnDateList = new DateTime[1];
                            RSide.ReturnDateList[0] = RSide.ReturnDateTime;
                        }
                        LengthAll = LSide.ReturnDateList.Length;
                        includecount = ReturnInCount(LSide.ReturnDateList, RSide.ReturnDateList, "DateTime");
                    }
                    else if (LSide.IsNumber && RSide.IsNumber)
                    {
                        if (!LSide.IsList)
                        {
                            LSide.ReturnNumberList = new Decimal[1];
                            LSide.ReturnNumberList[0] = LSide.ReturnNumber;
                        }
                        if (!RSide.IsList)
                        {
                            RSide.ReturnNumberList = new Decimal[1];
                            RSide.ReturnNumberList[0] = RSide.ReturnNumber;
                        }
                        LengthAll = LSide.ReturnNumberList.Length;

                        includecount = ReturnInCount(LSide.ReturnNumberList, RSide.ReturnNumberList);
                    }
                    else
                    {
                        if (!LSide.IsList)
                        {
                            LSide.ReturnStringList = new String[1];
                            LSide.ReturnStringList[0] = LSide.ReturnString;
                        }
                        if (!RSide.IsList)
                        {
                            RSide.ReturnStringList = new String[1];
                            RSide.ReturnStringList[0] = RSide.ReturnString;
                        }
                        LengthAll = LSide.ReturnStringList.Length;
                        includecount = ReturnInCount(LSide.ReturnStringList, RSide.ReturnStringList);

                    }

                }
                else
                {
                    switch (LSide.OrigDataType)
                    {

                        case "String":
                            if (!LSide.IsList)
                            {
                                LSide.ReturnStringList = new String[1];
                                LSide.ReturnStringList[0] = LSide.ReturnString;
                            }
                            if (!RSide.IsList)
                            {
                                RSide.ReturnStringList = new String[1];
                                RSide.ReturnStringList[0] = RSide.ReturnString;
                            }
                            LengthAll = LSide.ReturnStringList.Length;
                            includecount = ReturnInCount(LSide.ReturnStringList, RSide.ReturnStringList);

                            break;
                        case "Number":
                        case "Decimal":
                        case "Integer":
                            if (!LSide.IsList)
                            {
                                LSide.ReturnNumberList = new Decimal[1];
                                LSide.ReturnNumberList[0] = LSide.ReturnNumber;
                            }
                            if (!RSide.IsList)
                            {
                                RSide.ReturnNumberList = new Decimal[1];
                                RSide.ReturnNumberList[0] = RSide.ReturnNumber;
                            }
                            LengthAll = LSide.ReturnNumberList.Length;
                            includecount = ReturnInCount(LSide.ReturnNumberList, RSide.ReturnNumberList);
                            break;
                        case "Date":
                        case "Time":
                        case "DateTime":
                            if (LSide.IsDate && RSide.IsDate)
                            {
                                if (!LSide.IsList)
                                {
                                    LSide.ReturnDateList = new DateTime[1];
                                    LSide.ReturnDateList[0] = LSide.ReturnDateTime;
                                }
                                if (!RSide.IsList)
                                {
                                    RSide.ReturnDateList = new DateTime[1];
                                    RSide.ReturnDateList[0] = RSide.ReturnDateTime;
                                }
                                LengthAll = LSide.ReturnDateList.Length;
                                includecount = ReturnInCount(LSide.ReturnDateList, RSide.ReturnDateList, LSide.OrigDataType);
                            }
                            break;
                        default:
                            return 2;
                    }



                }
                int incount = includecount;
                switch (checkclause.Cmp)
                {
                    case Clause.CmpType.Includes_X:
                        if (checkclause.x == incount)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }

                    case Clause.CmpType.Includes_All:
                        if (LengthAll == incount)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    case Clause.CmpType.Includes_Any:
                        if (incount > 0)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    case Clause.CmpType.Includes_None:
                        if (incount == 0)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }

                    case Clause.CmpType.Includes_X_or_Less:
                        if (checkclause.x == incount || checkclause.x > incount)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }

                    case Clause.CmpType.Includes_X_or_More:
                        if (checkclause.x == incount || checkclause.x < incount)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    case Clause.CmpType.Equals_Any:
                        if (incount > 0)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    case Clause.CmpType.Not_Equals_Any:
                        if (incount == 0)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    case Clause.CmpType.Contains:
                        if (incount > 0)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                }


            }
            else
            {


                if (LSide.OrigDataType != RSide.OrigDataType)
                {
                    if (LSide.IsDate && RSide.IsDate)
                    {
                        LSide.OrigDataType = "DateTime";
                    }
                    else if (LSide.IsNumber && RSide.IsNumber)
                    {
                        LSide.OrigDataType = "Decimal";
                    }
                    else
                    {
                        LSide.OrigDataType = "String";
                    }

                }

                bool equalsit = false;
                bool greaterthanit = false;
                bool lessthanit = false;

                switch (LSide.OrigDataType)
                {
                    case "String":

                        int result = LSide.ReturnString.CompareTo(RSide.ReturnString);
                        if (result == 0)
                        {
                            equalsit = true;
                        }
                        else if (result < 0)
                        {
                            lessthanit = true;
                        }
                        else
                        {
                            greaterthanit = true;
                        }
                        break;
                    case "Number":
                    case "Integer":
                    case "Decimal":
                        if (LSide.ReturnNumber == RSide.ReturnNumber)
                        {
                            equalsit = true;
                        }
                        else if (LSide.ReturnNumber < RSide.ReturnNumber)
                        {
                            lessthanit = true;
                        }
                        else
                        {
                            greaterthanit = true;
                        }
                        break;
                    case "Date":
                        if (LSide.ReturnDateTime.Date == RSide.ReturnDateTime.Date)
                        {
                            equalsit = true;
                        }
                        else if (LSide.ReturnDateTime.Date < RSide.ReturnDateTime.Date)
                        {
                            lessthanit = true;
                        }
                        else
                        {
                            greaterthanit = true;
                        }
                        break;
                    case "Time":
                        if (LSide.ReturnDateTime.TimeOfDay == RSide.ReturnDateTime.TimeOfDay)
                        {
                            equalsit = true;
                        }
                        else if (LSide.ReturnDateTime.TimeOfDay < RSide.ReturnDateTime.TimeOfDay)
                        {
                            lessthanit = true;
                        }
                        else
                        {
                            greaterthanit = true;
                        }
                        break;
                    case "DateTime":
                         if (RSide.OrigDataType == "Time")
                        {
                            if (LSide.ReturnDateTime.TimeOfDay == RSide.ReturnDateTime.TimeOfDay)
                            {
                                equalsit = true;
                            }
                            else if (LSide.ReturnDateTime.TimeOfDay < RSide.ReturnDateTime.TimeOfDay)
                            {
                                lessthanit = true;
                            }
                            else
                            {
                                greaterthanit = true;
                            }
                        }
                         else if(RSide.OrigDataType == "Date")
                            if (LSide.ReturnDateTime.Date == RSide.ReturnDateTime.Date)
                            {
                                equalsit = true;
                            }
                            else if (LSide.ReturnDateTime.Date < RSide.ReturnDateTime.Date)
                            {
                                lessthanit = true;
                            }
                            else
                            {
                                greaterthanit = true;
                            }

                        else
                        if (LSide.ReturnDateTime == RSide.ReturnDateTime)
                        {
                            equalsit = true;
                        }
                        else if (LSide.ReturnDateTime < RSide.ReturnDateTime)
                        {
                            lessthanit = true;
                        }
                        else
                        {
                            greaterthanit = true;
                        }
                        break;
                    default:
                        return 2;
                }
                switch (checkclause.Cmp)
                {
                    case Clause.CmpType.Equals:
                        if (equalsit == true)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    case Clause.CmpType.Greater_than:
                        if (greaterthanit == true)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    case Clause.CmpType.Greater_than_or_Equals:
                        if (greaterthanit == true || equalsit == true)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    case Clause.CmpType.Less_than:
                        if (lessthanit == true)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    case Clause.CmpType.Less_than_or_Equals:
                        if (lessthanit == true || equalsit == true)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    case Clause.CmpType.Not_Equal_too:
                        if (equalsit == false)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    case Clause.CmpType.Starts_with:
                        if (LSide.OrigDataType != "String")
                        {
                            return 2;
                        }

                        if (LSide.ReturnString.Substring(0, RSide.ReturnString.Length) == RSide.ReturnString)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    case Clause.CmpType.Contains:
                        if (LSide.OrigDataType != "String")
                        {
                            return 2;
                        }

                        if (LSide.ReturnString.IndexOf(RSide.ReturnString) >= 0)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                }
            }

            return 2;
        }

        public static Int32 ReturnInCount(string[] MainString, string[] SearchString)
        {
            Int32 incount = 0;
            foreach (string mainone in MainString)
            {
                foreach (string searchone in SearchString)
                {
                    if (mainone == searchone)
                    {
                        incount++;
                        break;
                    }
                }
            }
            return incount;

        }
        public static Int32 ReturnInCount(Decimal[] MainDecimal, Decimal[] SearchDecimal)
        {
            Int32 incount = 0;
            foreach (Decimal mainone in MainDecimal)
            {
                foreach (Decimal searchone in SearchDecimal)
                {
                    if (mainone == searchone)
                    {
                        incount++;
                        break;
                    }
                }
            }
            return incount;

        }

        public static Int32 ReturnInCount(DateTime[] MainDate, DateTime[] SearchDate, String DType)
        {
            Int32 incount = 0;
            foreach (DateTime mainone in MainDate)
            {
                foreach (DateTime searchone in SearchDate)
                {

                    if (DType == "Date")
                    {
                        if (mainone.Date == searchone.Date)
                        {
                            incount++;
                            break;
                        }
                    }

                    else if (DType == "Time")
                    {
                        if (mainone.TimeOfDay == searchone.TimeOfDay)
                        {
                            incount++;
                            break;
                        }

                    }
                    else if (DType == "DateTime")
                    {
                        if (mainone == searchone)
                        {
                            incount++;
                            break;
                        }
                    }

                }


            }
            return incount;

        }

        public static bool ShouldContinue(Int32 showif, Int32 result)
        {
            bool continueon = false;

            switch (showif)
            {
                case 1:
                    if (result == 1)
                    {
                        continueon = true;
                    }
                    else
                    {
                        continueon = false;
                    }
                    break;
                case 2:
                    if (result == 0)
                    {
                        continueon = true;
                    }
                    else
                    {
                        continueon = false;
                    }
                    break;

                case 4:
                    if (result == 2)
                    {
                        continueon = true;
                    }
                    else
                    {
                        continueon = false;
                    }

                    break;

                case 5:
                    if (result == 1 || result == 2)
                    {
                        continueon = true;
                    }
                    else
                    {
                        continueon = false;
                    }

                    break;
                case 6:
                    if (result == 0 || result == 2)
                    {
                        continueon = true;
                    }
                    else
                    {
                        continueon = false;
                    }

                    break;


            }
            return continueon;

        }

    }

    public class LeadInformation
    {
        public string LeadName { get; set; }
        public string LeadAddress { get; set; }
        public string LeadPhone { get; set; }
    }
}