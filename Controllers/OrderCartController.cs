using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ScreenViewer.Models;
using System.Text.RegularExpressions;

namespace ScreenViewer.Controllers
{
    public class OrderCartController : Controller
    {
          // GET: /OrderCart/
        public ActionResult Index()     
        {
            return View();
        }
        //
        
        public ActionResult Display()
        {

           List <ItemOrdered> ItemsOrdered = SessionControl.SessionManager.GetItemsOrdered(HttpContext.Session);
           decimal taxrate;
           try
           {

               taxrate = Convert.ToDecimal(SessionControl.SessionManager.ReturnParameter(Session, "TaxRate"));
               if (taxrate > 0) taxrate = taxrate / 100;
           }
            catch
           {
               taxrate = 0;
           }
            if (ItemsOrdered == null)
            {
                return PartialView("_EmptyCartView");

            }
            List<ItemOrdered> ItemsSorted = new List<ItemOrdered>(); //ItemsOrdered.OrderBy(o=>o.Category).ThenBy(o=>o.SubCategory).ThenBy(o=>o.ItemName).ToList();
            
            decimal itemtotal = 0;
            decimal shippingtotal = 0;
            decimal taxTotal = 0;
            decimal totaltotal = 0;
            decimal priortotal = 0;
            ViewBag.PriorityDesc = "";

            foreach (ItemOrdered  item in ItemsOrdered)
            {
                itemtotal += (item.ItemPrice * item.ItemQuantity);

                if (!KeyExists(item.SetKeys,"Shipping"))
                {
                    ItemsSorted.Add(item);
                    shippingtotal += (item.ItemShipping * item.ItemQuantity);
                    taxTotal += taxrate * (item.ItemPrice + item.ItemShipping) * item.ItemQuantity;


                }
                else
                {
                    if (KeyValueEquals(item.SetKeys,"Shipping","Priority"))
                    {
                        ViewBag.PriorityDesc = (ViewBag.PriorityDesc == "") ? item.ItemName : ViewBag.PriorityDesc + ", " + item.ItemName;
                        priortotal += item.ItemShipping;
                    }
                    else
                    {
                        shippingtotal += (item.ItemShipping * item.ItemQuantity);
                    }
                }
            }
            ViewBag.PriorityAmount = priortotal;
            totaltotal = itemtotal + shippingtotal + priortotal + taxTotal;




            ViewBag.ItemTotal = itemtotal;
            ViewBag.ShippingTotal = shippingtotal;
            ViewBag.TotalTotal = totaltotal;
            ViewBag.TaxTotal = taxTotal;

           return PartialView("_OrderCartView",ItemsSorted);


        }

        public string Render(ControllerContext ContCont)
        {

            List<ItemOrdered> ItemsOrdered = SessionControl.SessionManager.GetItemsOrdered(ContCont.HttpContext.Session);
            decimal taxrate;
            try
            {

                taxrate = Convert.ToDecimal(SessionControl.SessionManager.ReturnParameter(ContCont.HttpContext.Session, "TaxRate"));
                if (taxrate > 0) taxrate = taxrate / 100;
            }
            catch
            {
                taxrate = 0;
            }
            if (ItemsOrdered == null)
            {
                return RenderHelper.RenderViewToString(ContCont, "~/Views/OrderCart/_EmptyCartView.cshtml",null, ViewData);
                //return PartialView("_EmptyCartView");

            }
            List<ItemOrdered> ItemsSorted = new List<ItemOrdered>(); //ItemsOrdered.OrderBy(o=>o.Category).ThenBy(o=>o.SubCategory).ThenBy(o=>o.ItemName).ToList();

            decimal itemtotal = 0;
            decimal shippingtotal = 0;
            decimal taxTotal = 0;
            decimal totaltotal = 0;
            decimal priortotal = 0;
            ViewBag.PriorityDesc = "";

            foreach (ItemOrdered item in ItemsOrdered)
            {
                itemtotal += (item.ItemPrice * item.ItemQuantity);

                if (!KeyExists(item.SetKeys, "Shipping"))
                {
                    ItemsSorted.Add(item);
                    shippingtotal += (item.ItemShipping * item.ItemQuantity);
                    taxTotal += taxrate * (item.ItemPrice + item.ItemShipping) * item.ItemQuantity;


                }
                else
                {
                    if (KeyValueEquals(item.SetKeys, "Shipping", "Priority"))
                    {
                        ViewBag.PriorityDesc = (ViewBag.PriorityDesc == "") ? item.ItemName : ViewBag.PriorityDesc + ", " + item.ItemName;
                        priortotal += item.ItemShipping;
                    }
                    else
                    {
                        shippingtotal += (item.ItemShipping * item.ItemQuantity);
                    }
                }
            }
            ViewBag.PriorityAmount = priortotal;
            totaltotal = itemtotal + shippingtotal + priortotal + taxTotal;




            ViewBag.ItemTotal = itemtotal;
            ViewBag.ShippingTotal = shippingtotal;
            ViewBag.TotalTotal = totaltotal;
            ViewBag.TaxTotal = taxTotal;

            //return PartialView("_OrderCartView", ItemsSorted);

            return RenderHelper.RenderViewToString(ContCont, "~/Views/OrderCart/_OrderCartView.cshtml", ItemsSorted, ViewData);

        } 
        private bool KeyExists(string KeyList,string KeyName)
        {
            if (KeyList == null) return false;

            List<string> keyPairs = Regex.Split(KeyList,",").ToList();
            foreach (string  keyPair in keyPairs)
        	{
		        if (Regex.Split(keyPair,":")[0] == KeyName)
                {
                    return true;
                }
	        }


            return false;
        }
        private bool KeyValueEquals(string KeyList, string KeyName,string keyVal)
        {
            if (KeyList == null) return false;

            List<string> keyPairs = Regex.Split(KeyList, ",").ToList();

            foreach (string keyPair in keyPairs)
            {
                if (Regex.Split(keyPair, ":")[0] == KeyName)
                {
                    if(Regex.Split(keyPair, ":")[1] == keyVal)
                    {
                        return true;
                    }
                }
            }


            return false;
        }


	}


}