using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;
using System.Text;
using System.IO;

namespace ScreenViewer.Models
{
    public class ClauseModel : Controller
    {
        //
        // GET: /ClauseModel/
        public ActionResult Index()
        {
            return View();
        }
	}

    [Serializable]
    public class Clause : ICloneable
    {
        public enum ElementTypeLeft { Question, Dobj, DateTime, Clause, Item, ItemQuantity, Calculation }
        public enum ElementTypeRight { Question, Dobj, SelectValue, EnterValue, DateValue, TimeValue, DTVal, Item, ItemQuantity, Calculation }

        public enum ClauseType
        {
            AND, OR, NAND, NOR, NONE
        }
        public enum CmpType : int
        {
            Equals = 1,
            Not_Equal_too = 2,
            Greater_than_or_Equals = 3,
            Greater_than = 4,
            Less_than_or_Equals = 5,
            Less_than = 6,
            Starts_with = 7,
            Contains = 8,
            X_Days_Difference = 50,
            X_Hours_Difference = 51,
            Includes_All = 100,
            Includes_None = 101,
            Includes_Any = 102,
            Includes_X = 103,
            Includes_X_or_More = 104,
            Includes_X_or_Less = 105,
            Equals_Any = 110,
            Not_Equals_Any = 111,
            None = 0
        }



        public Clause()
        {
            this.LSideObject = new LExpr();
            this.RSideObject = new RExpr();
            this.CmpField = CmpType.None;
        }

        //Clause should contain either subclauses
        private Clause[] SubClauseCollection;
        private String LOpFIeld;
        private String ROpField;
        private LExpr LSideObject;
        private RExpr RSideObject;
        private string XField;
        private CmpType CmpField;
        private string ClauseTextField;
        private bool matchcasefield;
        private Int32 xfield;
        private string clauseobjectfield;

        public string clauseobject
        {
            get
            {
                return clauseobjectfield;
            }
            set
            {
                clauseobjectfield = value;
            }

        }
        public object Clone() // ICloneable implementation    
        {
            Clause mc = this.MemberwiseClone() as Clause;
            return mc;
        }
        public Clause[] SubClause
        {
            get
            {
                return SubClauseCollection;
            }
            set
            {
                SubClauseCollection = value;
            }
        }

        public bool mcase
        {
            get
            {
                return matchcasefield;
            }
            set
            {
                matchcasefield = value;
            }

        }
        public Int32 x
        {
            get
            {
                return xfield;
            }
            set
            {
                xfield = value;
            }

        }
        public string X
        {
            get
            {
                return XField;
            }
            set
            {
                XField = value;
            }
        }
        public string ClauseText
        {
            get
            {
                return ClauseTextField;
            }
            set
            {
                ClauseTextField = value;
            }
        }
        public string LOp
        {
            get
            {
                return LOpFIeld;
            }
            set
            {
                LOpFIeld = value;
            }
        }
        public string ROp
        {
            get
            {
                return ROpField;
            }
            set
            {
                ROpField = value;
            }
        }
        public LExpr LSide
        {
            get
            {
                return LSideObject;
            }
            set
            {
                LSideObject = value;
            }
        }
        public RExpr RSide
        {
            get
            {
                return RSideObject;
            }
            set
            {
                RSideObject = value;
            }
        }
        public CmpType Cmp
        {
            get
            {
                return CmpField;
            }
            set
            {
                CmpField = value;
            }

        }

        public bool ClauseComplete
        {
            get
            {
                if (CmpField == CmpType.None)
                {
                    return false;
                }
                if (LOpFIeld == "" || LOpFIeld == null)
                {
                    return false;
                }
                if (ROpField == "" || ROpField == null)
                {
                    return false;
                }
                return true;
            }
        }
        public static Clause CreateClauseFromXML(string xml)
        {
            Clause clause = null;
            try
            {
                StringReader xmlSR = new StringReader(xml);

                XmlSerializer s = new XmlSerializer(typeof(Clause));
                clause = (Clause)(s.Deserialize(xmlSR));
            }
            catch { }

            return clause;
        }

        //private Clause[] subClauses = null;
        public Clause[] subClauses = null;
        //private string comparison;
        //public string comparison;
        //private CTyp CTyp = CTyp.NONE;

        public ClauseType CTyp = ClauseType.NONE;


        // Create a clause with the specified comparison (no ANDs/ORs; could start with NOT).
        public Clause(string comparison)
        {
            string trimComp = comparison.Trim();
        }

        // Create a clause of the specified type, with the specified subClauses
        public Clause(ClauseType inType, Clause[] inSubClauses)
        {
            this.CTyp = inType;
            this.subClauses = inSubClauses;
        }

    }
    [Serializable]
    public class LExpr : ICloneable
    {
        private Clause.ElementTypeLeft ETypeField;
        private Int32 QidField;
        private string DobjField;
        private string FTypField;
        private Int32 DplField;
        private Int32 HplField;
        private string MValField;
        private string MVSField;
        private string CIDField;
        private int SifField;
        private string CalcIDField;

        public string CalcID
        {
            get
            {
                return CalcIDField;
            }
            set
            {
                CalcIDField = value;
            }
        }

        public string CID
        {
            get
            {
                return CIDField;
            }
            set
            {
                CIDField = value;
            }
        }

        public int Sif
        {
            get
            {
                return SifField;
            }
            set
            {
                SifField = value;
            }
        }

        public object Clone() // ICloneable implementation    
        {
            LExpr mc = this.MemberwiseClone() as LExpr;
            return mc;
        }
        public string MVS
        {
            get
            {
                return MVSField;
            }
            set
            {
                MVSField = value;
            }
        }

        public Clause.ElementTypeLeft EType
        {
            get
            {
                return ETypeField;
            }
            set
            {
                ETypeField = value;
            }
        }
        public string FTyp
        {
            get
            {
                return FTypField;
            }
            set
            {
                FTypField = value;
            }
        }
        public string MVal
        {
            get
            {
                return MValField;
            }
            set
            {
                MValField = value;
            }
        }
        public Int32 Qid
        {
            get
            {
                return QidField;
            }
            set
            {
                QidField = value;
            }
        }
        public string Dobj
        {
            get
            {
                return DobjField;
            }
            set
            {
                DobjField = value;
            }
        }
        public Int32 Dpl
        {
            get
            {
                return DplField;
            }
            set
            {
                DplField = value;
            }
        }
        public Int32 Hpl
        {
            get
            {
                return HplField;
            }
            set
            {
                HplField = value;
            }
        }
    }

    [Serializable]
    public class RExpr : ICloneable
    {
        private Clause.ElementTypeRight ETypeField;
        private Int32 QidField;
        private string DobjField;
        private DateTime DTValField;
        private string StringValueField;
        private string FTypField;
        private String NumberValueField;
        private string MValField;
        private string MVSField;
        private string CalcIDField;

        public string CalcID
        {
            get
            {
                return CalcIDField;
            }
            set
            {
                CalcIDField = value;
            }
        }
        public object Clone() // ICloneable implementation    
        {
            RExpr mc = this.MemberwiseClone() as RExpr;
            return mc;
        }
        public string FTyp
        {
            get
            {
                return FTypField;
            }
            set
            {
                FTypField = value;
            }
        }

        public string MVal
        {
            get
            {
                return MValField;
            }
            set
            {
                MValField = value;
            }
        }

        public string MVS
        {
            get
            {
                return MVSField;
            }
            set
            {
                MVSField = value;
            }
        }

        public Clause.ElementTypeRight EType
        {
            get
            {
                return ETypeField;
            }
            set
            {
                ETypeField = value;
            }
        }

        public Int32 Qid
        {
            get
            {
                return QidField;
            }
            set
            {
                QidField = value;
            }
        }
        public DateTime DTVal
        {
            get
            {
                return DTValField;
            }
            set
            {
                DTValField = value;
            }
        }
        public string StringValue
        {
            get
            {
                return StringValueField;
            }
            set
            {
                StringValueField = value;
            }
        }
        public string NumberValue
        {
            get
            {
                return NumberValueField;
            }
            set
            {
                NumberValueField = value;
            }
        }
        public string Dobj
        {
            get
            {
                return DobjField;
            }
            set
            {
                DobjField = value;
            }
        }
    }
    [Serializable]
    public class ScriptClause
    {
        private string ClauseNameField;
        private int ClauseIDField;
        private int FolderIDField;
        private String ClauseTextField;
        private Clause ClauseObject;


        public string ClauseName
        {
            get
            {
                return ClauseNameField;

            }
            set
            {
                ClauseNameField = value;
            }
        }
        public int ClauseID
        {
            get
            {
                return ClauseIDField;

            }
            set
            {
                ClauseIDField = value;
            }
        }
        public int FolderID
        {
            get
            {
                return FolderIDField;

            }
            set
            {
                FolderIDField = value;
            }
        }
        public string ClauseText
        {
            get
            {
                return ClauseTextField;

            }
            set
            {
                ClauseTextField = value;

                XmlSerializer XS = new XmlSerializer(typeof(Clause));
                StringReader SR = new StringReader(value);
                ClauseObject = (Clause)XS.Deserialize(SR);
            }
        }
        public Clause MainClause
        {
            get
            {
                return ClauseObject;
            }
            set
            {
                ClauseObject = value;

                XmlSerializer XS = new XmlSerializer(typeof(Clause));
                StringWriter sw = new StringWriter();
                XS.Serialize(sw, ClauseObject);
                ClauseTextField = sw.ToString();
            }
        }

        public bool SaveClause()
        {
            return true;
        }
        public bool UpdateClause()
        {
            return true;
        }
    }

}