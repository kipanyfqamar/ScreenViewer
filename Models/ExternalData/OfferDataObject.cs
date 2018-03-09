using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ScreenViewer.Models.ExternalData
{
    [Serializable]
    public class Offer_Class
    {
        private string OfferID_Field;
        private string OfferName_Field;
        private string OfferDesc_Field;
        private DateTime OfferStartDate_Field;


        public string OfferID
        {
            get
            { return OfferID_Field; }
            set
            { OfferID_Field = value; }
        }

        public string OfferName
        {
            get
            { return OfferName_Field; }
            set
            { OfferName_Field = value; }
        }

        public string OfferDesc
        {
            get
            { return OfferDesc_Field; }
            set
            { OfferDesc_Field = value; }
        }

        public DateTime OfferStartDate
        {
            get
            { return OfferStartDate_Field; }
            set
            { OfferStartDate_Field = value; }
        }


    }
    [Serializable]
    public class SpecialOffers_Class
    {
        public Offer_Class[] Offers {get;set;}

    }
}