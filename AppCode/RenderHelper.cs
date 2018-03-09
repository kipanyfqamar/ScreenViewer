using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Mvc;


namespace ScreenViewer
{
    public class RenderHelper
    {
        public static string RenderViewToString(ControllerContext context, string viewName, object model, ViewDataDictionary viewData)
        {
            try
            {
                if (string.IsNullOrEmpty(viewName))
                    viewName = context.RouteData.GetRequiredString("action");

                //ViewDataDictionary viewData = new ViewDataDictionary(model);
                viewData.Model = model;
                using (StringWriter sw = new StringWriter())
                {
                    ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(context, viewName);
                    ViewContext viewContext = new ViewContext(context, viewResult.View, viewData, new TempDataDictionary(), sw);

                    viewResult.View.Render(viewContext, sw);

                    return sw.GetStringBuilder().ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }


    }
}