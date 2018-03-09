using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ScreenViewer.Data;
using ScreenViewer.Models.ExternalData;

namespace ScreenViewer.API.ExternalData
{
    public class DataObjectManager
    {
        //---This class is a template for handling the loading of DataObjects. 
        //---The Project controller will request DataObjects to be loaded. These requests
        //---should be wrapped in an API Controller

        //private DemoCRMEntities db = new DemoCRMEntities();

        //public Object CreateDataObject(string DataObjectName,string attributes)
        //{

        //    switch (DataObjectName)
        //    {
        //        case "LeadRecord":

        //    string leadID = GetAttribute(attributes, "LeadRecordID");
        //    LeadRecord leadrecord = db.LeadRecords.Find(System.Convert.ToInt32(leadID));
        //    return leadrecord == null ? null : leadrecord;


        //        case "Offers" :


        //            break;

                
        //        default:
        //            break;
        //    }

        //    return null;


        //}

        //public SpecialOffers_Class GetOffers()
        //{
        //    SpecialOffers_Class TheOffers = new SpecialOffers_Class();
        //    //List<Offer_Class> OFC = new List<Offer_Class>();
        //    Offer_Class[] OFC;
        //    API.ExternalData.OfferController OC = new OfferController();
            
        //    IQueryable<object> OffersReturned = OC.GetOffers();


        //    int counter = 0;

        //    OFC = new Offer_Class[OffersReturned.Count()];

        //    foreach (var offeritem  in OffersReturned)
	       // { 
        //        Offer TheOfferItem = (Offer)offeritem;

        //        Offer_Class TheOffer = new Offer_Class();
        //        TheOffer.OfferID = TheOfferItem.OfferID.ToString();
        //        TheOffer.OfferName = TheOfferItem.OfferName;
        //        TheOffer.OfferDesc = TheOfferItem.OfferDesc;
        //        TheOffer.OfferStartDate = (DateTime)TheOfferItem.OfferStartDate;

        //        OFC[counter] = TheOffer;
        //        counter++;
	       // }

        //    TheOffers.Offers = OFC;
        //    return TheOffers;


        //}


        public static string GetAttribute(string attributes,string ReturnAttribute)
        {
            List<string> attribs = System.Text.RegularExpressions.Regex.Split(attributes,";").ToList();

            foreach (string  attrib in attribs)
            {
                string AttributeName = System.Text.RegularExpressions.Regex.Split(attrib, ":")[0];
                string Value = System.Text.RegularExpressions.Regex.Split(attrib, ":")[1];
                if (AttributeName == ReturnAttribute)
                {
                    return Value;
                }

            }

            return "";

        }




    }
}