using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ScreenViewer.SessionControl;
using System.Web.Http.Results;
using System.Text.RegularExpressions;

namespace ScreenViewer.Models
{

    public class itemUtilities
    {
        public void CheckItemsHaveValidKey(HttpSessionStateBase Session)
        {
            Dictionary<string, string> ItemDict = SessionManager.ReturnItemKeys(Session);

            if (ItemDict == null)
            {
                return;
            }
            List<ItemOrdered> itemsOrdered = SessionManager.GetItemsOrdered(Session);
            if (itemsOrdered == null)
            {
                return;
            }
            List<ItemOrdered> newItemsOrdered = new List<ItemOrdered>();

            SessionManager.StoreItemsOrdered(Session, newItemsOrdered);
        }
    }
}