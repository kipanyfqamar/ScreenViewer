using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.Web.Http.Results;

namespace ScreenViewer.Models
{
    public class ScriptCalculator
    {

        public static readonly string DELIM = string.Format("{0}{1}{2}", (Char)251, (Char)252, (Char)253);
        public static readonly string CalcStringPattern = @"\{Calc::C(\d+).*?\}";
        public static readonly string QuestionResponseStringPattern = @"\{QuestionResponse:(\d+).*?\}";

        public static string CALCDELIM = string.Format("{0}{1}{2}", (Char)251, (Char)252, (Char)253);

        public static string DateRegxStr = @"^\[([1-9]|0[1-9]|1[012])[- /]([1-9]|0[1-9]|[12][0-9]|3[01])[- /](19|2\d)\d\d]$";
        public static string DateFormatStrDefault = @"MM/dd/yyyy hh:mm tt";
        Queue<CalToken> queueOutput;
        Stack<CalToken> statckOPs;
        Stack<bool> stackWereValues;

        public string DBConnection;
        public DataObjects CalcDO;
        public QuestVal[] CalQuests;

        int levelTracking; //tracking where the func is
        Dictionary<int, int> dictionarylevelArgs; //level, stackArgsCount

        public void Parser(HttpSessionStateBase SessionBase, string expression)
        {
            queueOutput = new Queue<CalToken>();

            statckOPs = new Stack<CalToken>();

            stackWereValues = new Stack<bool>();

            levelTracking = 0;
            dictionarylevelArgs = new Dictionary<int, int>();


            string bufferStr = expression;

            bufferStr = Regex.Replace(bufferStr, @"(?<Date>\[\s*\d{1,2}\s*/\s*\d{1,2}\s*/\s*\d{4}\s*])"
                                                    , s => s.Value.Replace(" ", string.Empty));

            bufferStr = Regex.Replace(bufferStr, CalcStringPattern, new MatchEvaluator(SubstitueCalExpressionById));

            bufferStr = Regex.Replace(bufferStr, "-", "MINUS");
            bufferStr = Regex.Replace(bufferStr, @"(?<number>(}|]|\)|e|(\d+(\.\d+)?)))\s*MINUS", "${number}-"); //replace qualified 'Minus' pattern with - sign
            bufferStr = Regex.Replace(bufferStr, "MINUS", "~");

            string pattern = @"(?'LeftParan' \() |
                               (?'RightParan' \)) |                                           
                               (?'Add' \+) |
                               (?'Subtract' -) |
                               (?'Multiply' \*) | 
                               (?'Divide' /) |       
                               (?'Mod' %) |                  
                               (?'Other' [\^\r\n\t]) | 
                               (?'UnaryMinus' ~) |                                       
                               (?'Date1' \[\d{1,2}/\d{1,2}/\d{4}]) | 
                               (?'Comma' ,) |
                               (?'Text' '.*?') |
                               (?'DataObject' \{DataObject::.*?\}) |
                               (?'QuestionResponse' \{QuestionResponse:.*?\})";


            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

            string[] tokens = regex.Split(bufferStr);

            tokens = tokens.Where(t => Regex.IsMatch(t, @"[^\s\r\n\t]")).Select(t => t.Trim()).ToArray();


            for (int i = 0; i < tokens.Length; i++)
            {
                bool isNumber, isText, isDateTime, isDataObject, isQuestionResponse, isDatepart;
                double testNumber;
                DatePartType testDatepart;
                bool w; int a;
                try
                {
                    CalToken cToken = new CalToken();
                    cToken.TokenValue = tokens[i]; //store the value

                    isNumber = double.TryParse(tokens[i], out testNumber);
                    //isText = Regex.IsMatch(tokens[i], @"^'[0-9a-zA-Z\s]+'|'[0-9/]+'$");
                    isText = Regex.IsMatch(tokens[i], @"^'.*?'$");
                    isDateTime = Regex.IsMatch(tokens[i], DateRegxStr);
                    //isDataObject = Regex.IsMatch(tokens[i], @"^{DataObject::[0-9a-zA-Z.]+}$");
                    isDataObject = Regex.IsMatch(tokens[i], @"^\{DataObject::.*?\}$");
                    //isQuestionResponse = Regex.IsMatch(tokens[i], @"^{QuestionResponse:Q[0-9]+}$");
                    //isQuestionResponse = Regex.IsMatch(tokens[i], @"^\{QuestionResponse:.*?\}$");
                    isQuestionResponse = Regex.IsMatch(tokens[i], @"^\{QuestionResponse:.*?\}$", RegexOptions.IgnoreCase);
                    isDatepart = TryParseEnum<DatePartType>(tokens[i].ToLower(), out testDatepart);

                    if (isNumber)
                    {
                        cToken.TokenValue = tokens[i];
                        cToken.CalTokenType = TokenType.Number;
                        queueOutput.Enqueue(cToken);
                        if (stackWereValues.Count > 0)
                        {
                            stackWereValues.Pop();
                            stackWereValues.Push(true);
                        }
                    }
                    else if (isDateTime)
                    {
                        cToken.TokenValue = tokens[i];
                        cToken.CalTokenType = TokenType.DateTime; //surround by [ ]                        
                        queueOutput.Enqueue(cToken);
                        if (stackWereValues.Count > 0)
                        {
                            stackWereValues.Pop();
                            stackWereValues.Push(true);
                        }
                    }
                    else if (isText)
                    {
                        cToken.TokenValue = tokens[i];
                        cToken.CalTokenType = TokenType.String; //surround by ' '
                        queueOutput.Enqueue(cToken);
                        if (stackWereValues.Count > 0)
                        {
                            stackWereValues.Pop();
                            stackWereValues.Push(true);
                        }
                    }
                    else if (isDatepart)
                    {
                        cToken.TokenValue = tokens[i].ToLower();
                        cToken.CalTokenType = TokenType.Datepart;
                        queueOutput.Enqueue(cToken);
                        if (stackWereValues.Count > 0)
                        {
                            stackWereValues.Pop();
                            stackWereValues.Push(true);
                        }
                    }
                    else if (isDataObject || isQuestionResponse)
                    {
                        object obj = null;

                        if (isDataObject) { obj = GetDataObjectInfoBy(tokens[i]); }

                        if (isQuestionResponse) { obj = GetQuestionResponseInfoBy(SessionBase, GetQuestionIdBy(tokens[i])); }

                        cToken.TokenValue = obj;
                        cToken.CalTokenType = GetTokenType(obj);   //collection || number,string,date

                        queueOutput.Enqueue(cToken);

                        if (stackWereValues.Count > 0)
                        {
                            stackWereValues.Pop();
                            stackWereValues.Push(true);
                        }

                    }
                    else
                    {
                        switch (tokens[i].ToUpper())
                        {
                            case "+":
                                cToken.CalTokenType = TokenType.Plus;
                                if (statckOPs.Count > 0)
                                {
                                    CalToken opToken = statckOPs.Peek();
                                    while (IsOperatorToken(opToken.CalTokenType) && Precedence(cToken.CalTokenType) >= Precedence(opToken.CalTokenType))
                                    {
                                        //o1 is less or equal precedence, pop o2(s) out and insert o2(s) in queue
                                        queueOutput.Enqueue(statckOPs.Pop());  // 4 2 +
                                        if (statckOPs.Count > 0)
                                            opToken = statckOPs.Peek();
                                        else
                                            break;
                                    }
                                }
                                statckOPs.Push(cToken);
                                break;
                            case "-":
                                cToken.CalTokenType = TokenType.Minus;
                                if (statckOPs.Count > 0)
                                {
                                    CalToken opToken = statckOPs.Peek();
                                    while (IsOperatorToken(opToken.CalTokenType) && Precedence(cToken.CalTokenType) >= Precedence(opToken.CalTokenType))
                                    {
                                        queueOutput.Enqueue(statckOPs.Pop());

                                        if (statckOPs.Count > 0) //4 2 -
                                            opToken = statckOPs.Peek();
                                        else
                                            break;
                                    }
                                }
                                statckOPs.Push(cToken);
                                break;
                            case "*":
                                cToken.CalTokenType = TokenType.Multiply;
                                if (statckOPs.Count > 0)
                                {
                                    CalToken opToken = statckOPs.Peek();
                                    while (IsOperatorToken(opToken.CalTokenType))
                                    {
                                        if (Precedence(cToken.CalTokenType) >= Precedence(opToken.CalTokenType))
                                        {
                                            //if o1 is less precedence than o2(s), then pop o2(s) in to queue
                                            queueOutput.Enqueue(statckOPs.Pop());
                                            if (statckOPs.Count > 0)
                                            {
                                                opToken = statckOPs.Peek();
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                        else  //if o1 has higher precedence than o2, then break;
                                        {
                                            break;
                                        }
                                    }
                                }
                                statckOPs.Push(cToken);
                                break;
                            case "/":
                                cToken.CalTokenType = TokenType.Divide;

                                if (statckOPs.Count > 0)
                                {
                                    CalToken opToken = statckOPs.Peek();

                                    while (IsOperatorToken(opToken.CalTokenType))
                                    {
                                        if (Precedence(cToken.CalTokenType) >= Precedence(opToken.CalTokenType))
                                        {
                                            queueOutput.Enqueue(statckOPs.Pop());
                                            if (statckOPs.Count > 0)
                                            {
                                                opToken = statckOPs.Peek();
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                                statckOPs.Push(cToken);
                                break;
                            case "%":
                                cToken.CalTokenType = TokenType.Mod;
                                if (statckOPs.Count > 0)
                                {
                                    CalToken opToken = statckOPs.Peek();

                                    while (IsOperatorToken(opToken.CalTokenType))
                                    {
                                        if (Precedence(cToken.CalTokenType) >= Precedence(opToken.CalTokenType))
                                        {
                                            queueOutput.Enqueue(statckOPs.Pop());
                                            if (statckOPs.Count > 0)
                                            {
                                                opToken = statckOPs.Peek();
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                                statckOPs.Push(cToken);
                                break;
                            case "^":
                                cToken.CalTokenType = TokenType.Exponent;
                                if (statckOPs.Count > 0)
                                {
                                    CalToken opToken = statckOPs.Peek();
                                    while (IsOperatorToken(opToken.CalTokenType))
                                    {
                                        if (Precedence(cToken.CalTokenType) >= Precedence(opToken.CalTokenType))
                                        {
                                            queueOutput.Enqueue(statckOPs.Pop());
                                            if (statckOPs.Count > 0)
                                            {
                                                opToken = statckOPs.Peek();
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                                statckOPs.Push(cToken);
                                break;
                            case "~":
                                cToken.CalTokenType = TokenType.UnaryMinus;
                                statckOPs.Push(cToken);
                                break;
                            case "(":
                                cToken.CalTokenType = TokenType.LeftParenthesis;
                                statckOPs.Push(cToken);
                                break;
                            case ")":
                                cToken.CalTokenType = TokenType.RightParenthesis;

                                //pop until reach left leftParenthesis
                                if (statckOPs.Count > 0)
                                {
                                    CalToken opToken = statckOPs.Peek();
                                    while (opToken.CalTokenType != TokenType.LeftParenthesis)
                                    {//keep poping and queue op2(s) until reach leftParenthesis
                                        queueOutput.Enqueue(statckOPs.Pop());
                                        if (statckOPs.Count > 0)
                                        {
                                            opToken = statckOPs.Peek();
                                        }
                                        else
                                        {
                                            //if there is no open/left parenthesis, missing paranthesis exception
                                            // throw new Exception("Missing left paranthesis");
                                        }
                                    }
                                    //pop the Leftparenthese "("
                                    statckOPs.Pop();
                                }
                                //pop if it's a function Name
                                if (statckOPs.Count > 0)
                                {
                                    CalToken opToken = statckOPs.Peek();
                                    if (IsFunctionToken(opToken.CalTokenType))
                                    {
                                        opToken = statckOPs.Pop(); //pop Function Name

                                        //a = stackArgsCount.Pop();
                                        a = dictionarylevelArgs[levelTracking];

                                        w = stackWereValues.Pop();
                                        if (w) a++;
                                        opToken.NumberOfArgs = a;

                                        dictionarylevelArgs.Remove(levelTracking);
                                        levelTracking--;

                                        queueOutput.Enqueue(opToken); //Queue Function Name
                                    }
                                }
                                break;
                            case ",":
                                //pop until reach left leftParenthesis
                                if (statckOPs.Count > 0)
                                {
                                    CalToken opToken = statckOPs.Peek();

                                    while (opToken.CalTokenType != TokenType.LeftParenthesis)
                                    {//keep poping and queue op2(s) until reach leftParenthesis
                                        queueOutput.Enqueue(statckOPs.Pop());
                                        if (statckOPs.Count > 0)
                                        {
                                            opToken = statckOPs.Peek();
                                        }
                                        else
                                        {
                                            //if there is no open/left parenthesis, missing paranthesis exception
                                            // throw new Exception("Missing left paranthesis");
                                        }
                                    }
                                }


                                w = stackWereValues.Pop(); //always has value due to it's part of the func
                                if (w)
                                {
                                    //a = stackArgsCount.Pop();
                                    //a++;
                                    //stackArgsCount.Push(a);
                                    a = dictionarylevelArgs[levelTracking];
                                    a++;
                                    dictionarylevelArgs[levelTracking] = a;

                                    stackWereValues.Push(false);
                                }
                                break;

                            case "SUM":
                            case "MAX":
                            case "MIN":
                            case "COUNT":
                            case "AVERAGE":
                            case "LENGTH":
                            case "SUBSTRING":
                            case "GETMONTH":
                            case "GETDAY":
                            case "GETYEAR":
                            case "ADDMONTH":
                            case "ADDDAY":
                            case "ADDYEAR":
                            case "TOSCALAR":
                            case "TONUMBER":
                            case "TOSTRING":
                            case "TODATE":
                            case "DAYOFWEEK":
                            case "ROUND":
                            case "SQRT":
                            case "CONCAT":
                            //case "DATENOW": //no args
                            //case "DATETIMENOW": //no args
                            case "DATEDIFF":
                            case "IFBLANK":
                                cToken.CalTokenType = (TokenType)Enum.Parse(typeof(TokenType), tokens[i].ToUpper());

                                statckOPs.Push(cToken); //staight to stack

                                //stackArgsCount.Push(0); //set args info
                                levelTracking++;
                                dictionarylevelArgs.Add(levelTracking, 0);

                                if (stackWereValues.Count > 0)
                                {
                                    stackWereValues.Pop();
                                    stackWereValues.Push(true);
                                }
                                stackWereValues.Push(false);
                                break;

                            case "DATENOW": //no args
                            case "DATETIMENOW": //no args

                                cToken.TokenValue = tokens[i];
                                cToken.CalTokenType = (TokenType)Enum.Parse(typeof(TokenType), tokens[i].ToUpper());
                                queueOutput.Enqueue(cToken);
                                if (stackWereValues.Count > 0)
                                {
                                    stackWereValues.Pop();
                                    stackWereValues.Push(true);
                                }

                                break;
                            default:
                                // throw new Exception("Unable to parse " + tokens[i]);
                                break;
                        }
                    }

                }
                catch (Exception ex)
                {
                    // throw ex;
                }


            }// end for loop

            while (statckOPs.Count > 0)
            {
                CalToken opToken = statckOPs.Pop();

                if (opToken.CalTokenType == TokenType.LeftParenthesis)
                {
                    // throw new Exception("Unmatched paranthesis");
                }
                else
                {
                    queueOutput.Enqueue(opToken);
                }
            }

            string postFixExpression = string.Empty;
            //string x = string.Join(" ", queueOutput.Select(t => t.TokenValue).ToArray());

        }
        //predefined value
        public virtual object GetDataObjectInfoBy(string dataobjectName)
        {
            object obj = null;
            switch (dataobjectName)
            {
                case "{DataObject::ContactRecord.Collection.String}":
                    string[] arraystr = new string[] { "New York", "New Jersey", "Florida" };
                    obj = arraystr;
                    break;
                case "{DataObject::ContactRecord.Str}":
                    obj = "New York";
                    break;
                case "{DataObject::ContactRecord.Collection.Scalar}":
                    double[] arrayScalar = new double[] { 0.3, 10, 20, 1.7, 950, 80, 0 };
                    obj = arrayScalar;
                    break;
                case "{DataObject::ContactRecord.Scalar}":
                    obj = 2;
                    break;
            }
            if (obj == null) obj = "";
            return obj;
            //// throw new NotImplementedException();
        }
        //predefined value
        public virtual object GetQuestionResponseInfoBy(HttpSessionStateBase SessionBase, string qId)
        {
            object obj = null;
            obj = SessionControl.SessionManager.GetQuestionResponse(qId, SessionBase);

            if (obj == null) obj = "";

            return obj;
        }

        internal string GetQuestionIdBy(string questionResponseName)
        {
            string pattern = QuestionResponseStringPattern;
            string qId = Regex.Match(questionResponseName, pattern).Groups[1].ToString();
            return qId;
        }

        private string SubstitueCalExpressionById(Match m)
        {
            string pattern = CalcStringPattern;
            string cId = Regex.Match(m.ToString(), pattern).Groups[1].ToString();
            string buffer = string.Empty;

            ScriptCalculationInfo calcInfo = ScriptCalculationInfo.GetScriptCalculationBy(cId);
            if (calcInfo != null)
            {
                buffer = Regex.Replace(calcInfo.CalculationExpression, pattern, new MatchEvaluator(SubstitueCalExpressionById));
            }

            return buffer;

        }

        private int Precedence(TokenType t)
        {
            int precedence = 1000;
            switch (t)
            {
                case TokenType.Plus:
                case TokenType.Minus:
                    precedence = 5;
                    break;
                case TokenType.Multiply:
                case TokenType.Divide:
                case TokenType.Mod:
                    precedence = 4;
                    break;
                case TokenType.Exponent:
                    precedence = 3;
                    break;
                case TokenType.UnaryMinus:
                    precedence = 1;
                    break;
            }
            return precedence;
        }

        private bool IsOperatorToken(TokenType t)
        {
            bool result = false;
            switch (t)
            {
                case TokenType.Plus:
                case TokenType.Minus:
                case TokenType.Multiply:
                case TokenType.Divide:
                case TokenType.Exponent:
                case TokenType.UnaryMinus:
                case TokenType.Mod:
                    result = true;
                    break;
                default:
                    result = false;
                    break;
            }
            return result;
        }

        private bool IsFunctionToken(TokenType t)
        {
            bool result = false;
            switch (t)
            {
                case TokenType.SUM:
                case TokenType.MAX:
                case TokenType.MIN:
                case TokenType.COUNT:
                case TokenType.AVERAGE:
                case TokenType.LENGTH:
                case TokenType.SUBSTRING:
                case TokenType.ROUND:
                case TokenType.SQRT:
                case TokenType.CONCAT:

                case TokenType.GETMONTH:
                case TokenType.GETDAY:
                case TokenType.GETYEAR:
                case TokenType.ADDMONTH:
                case TokenType.ADDDAY:
                case TokenType.ADDYEAR:

                case TokenType.TOSCALAR:
                case TokenType.TONUMBER:
                case TokenType.TOSTRING:
                case TokenType.TODATE:
                case TokenType.TOSHORTDATESTRING:
                case TokenType.TOLONGDATESTRING:

                case TokenType.DATENOW:
                case TokenType.DATETIMENOW:
                case TokenType.DAYOFWEEK:
                case TokenType.DATEDIFF:
                case TokenType.IFBLANK:
                    result = true;
                    break;
            }
            return result;
        }

        private bool IsReservedType(object obj)
        {
            return obj is ReservedType;
        }

        private bool ContainReservedTypeArgs(Stack<object> result, int argsCount)
        {
            return result.Take(argsCount).Any(obj => obj is ReservedType);
        }

        public string Eval()
        {
            Stack<object> result = new Stack<object>();
            double oper1 = 0.0, oper2 = 0.0;
            DateTime operDate1, operDate2;
            CalToken cToken = null;

            for (int i = 0; i < queueOutput.Count; i++)
            {
                cToken = queueOutput.ElementAt(i) as CalToken;

                switch (cToken.CalTokenType)
                {
                    case TokenType.Number:
                        if (double.TryParse(cToken.TokenValue.ToString(), out oper1))
                        {
                            result.Push(oper1);
                        }
                        else
                        {
                            // throw new Exception("Evaluation error");
                        }
                        break;
                    case TokenType.String:
                        cToken.TokenValue = cToken.TokenValue.ToString().Trim();
                        cToken.TokenValue = cToken.TokenValue.ToString().TrimStart(new char[] { '\'' });
                        cToken.TokenValue = cToken.TokenValue.ToString().TrimEnd(new char[] { '\'' });
                        //cToken.TokenValue = cToken.TokenValue.Replace("'", string.Empty);
                        result.Push(cToken.TokenValue);
                        break;
                    case TokenType.DateTime:
                        cToken.TokenValue = cToken.TokenValue.ToString().Trim(new char[] { '[', ']' });
                        if (DateTime.TryParse(cToken.TokenValue.ToString(), out operDate1))
                        {
                            result.Push(operDate1.ToShortDateString());
                        }
                        else
                        {
                            // throw new Exception("Evaluation error");
                        }
                        break;
                    case TokenType.Datepart:
                        ReservedType tokenValue = new ReservedType { TokenValue = (DatePartType)Enum.Parse(typeof(DatePartType), cToken.TokenValue.ToString()), CalTokenType = TokenType.Datepart };
                        result.Push(tokenValue);
                        break;
                    case TokenType.Collection:
                        result.Push(cToken.TokenValue);
                        break;
                    case TokenType.Plus:
                        if (result.Count > 1 && !ContainReservedTypeArgs(result, 2))
                        {
                            try
                            {
                                var val2 = result.Pop();
                                var val1 = result.Pop();
                                if (val1 is string && val2 is string)
                                {
                                    result.Push(val1.ToString() + val2.ToString());
                                }
                                else
                                {
                                    oper2 = Convert.ToDouble(val2);//result.Pop());
                                    oper1 = Convert.ToDouble(val1);//result.Pop());
                                    result.Push(oper1 + oper2);
                                }
                            }
                            catch
                            {
                                // throw new Exception("Plus evaluation error");
                            }

                        }
                        else
                        {
                            // throw new Exception("Plus evaluation error");
                        }
                        break;
                    case TokenType.Minus:
                        if (result.Count > 1 && !ContainReservedTypeArgs(result, 2))
                        {
                            try
                            {
                                oper2 = Convert.ToDouble(result.Pop());
                                oper1 = Convert.ToDouble(result.Pop());
                                result.Push(oper1 - oper2);

                            }
                            catch
                            {
                                // throw new Exception("Minus evaluation error");
                            }
                        }
                        else
                        {
                            // throw new Exception("Minus evaluation error");
                        }
                        break;
                    case TokenType.Multiply:
                        if (result.Count > 1 && !ContainReservedTypeArgs(result, 2))
                        {
                            try
                            {
                                oper2 = Convert.ToDouble(result.Pop());
                                oper1 = Convert.ToDouble(result.Pop());
                                result.Push(oper1 * oper2);

                            }
                            catch
                            {
                                // throw new Exception("Multiply evaluation error");
                            }
                        }
                        else
                        {
                            // throw new Exception("Multiply evaluation error");
                        }
                        break;
                    case TokenType.Divide:
                        if (result.Count > 1 && !ContainReservedTypeArgs(result, 2))
                        {
                            try
                            {
                                oper2 = Convert.ToDouble(result.Pop());
                                oper1 = Convert.ToDouble(result.Pop());
                                result.Push(Decimal.Divide((decimal)oper1, (decimal)oper2));
                            }
                            catch
                            {
                                // throw new Exception("Divide evaluation error");
                            }
                        }
                        else
                        {
                            // throw new Exception("Divide evaluation error");
                        }
                        break;
                    case TokenType.Mod:
                        if (result.Count > 1 && !ContainReservedTypeArgs(result, 2))
                        {
                            try
                            {
                                oper2 = Convert.ToDouble(result.Pop());
                                oper1 = Convert.ToDouble(result.Pop());
                                result.Push(oper1 % oper2);
                            }
                            catch
                            {
                                // throw new Exception("Modulus evaluation error");
                            }
                        }
                        else
                        {
                            // throw new Exception("Modulus evaluation error");
                        }
                        break;
                    case TokenType.Exponent:
                        if (result.Count > 1 && !ContainReservedTypeArgs(result, 2))
                        {
                            try
                            {
                                oper2 = Convert.ToDouble(result.Pop());
                                oper1 = Convert.ToDouble(result.Pop());
                                result.Push(Math.Pow(oper1, oper2));
                            }
                            catch
                            {
                                // throw new Exception("Exponent evaluation error");
                            }

                        }
                        else
                        {
                            // throw new Exception("Exponent evaluation error");
                        }
                        break;
                    case TokenType.UnaryMinus:
                        if (result.Count > 0 && !ContainReservedTypeArgs(result, 1))
                        {
                            try
                            {
                                oper1 = Convert.ToDouble(result.Pop());
                                result.Push(-oper1);
                            }
                            catch
                            {
                                // throw new Exception("UnaryMinus evaluation error");
                            }
                        }
                        break;
                    case TokenType.SUM:
                        if (result.Count > 0 && !ContainReservedTypeArgs(result, cToken.NumberOfArgs))
                        {
                            try
                            {
                                result.Push(GetArgsCollection(result, cToken.NumberOfArgs).Sum(n => Convert.ToDouble(n)));
                            }
                            catch
                            {
                                // throw new Exception("SUM evaluation error");
                            }
                            //}
                        }
                        else
                        {
                            // throw new Exception("SUM evaluation error");
                        }
                        break;
                    case TokenType.MAX:
                        if (result.Count > 0 && !ContainReservedTypeArgs(result, cToken.NumberOfArgs))
                        {
                            try
                            {
                                Type t = result.Peek().GetType(); 
                                var s = GetArgsCollection(result, cToken.NumberOfArgs);
                                var stype = t.Name.Trim(new[] { '[', ']' });
                                if (s.Any(e => !e.GetType().Name.Equals(stype)))
                                {
                                    // throw new Exception("Max evaluation error, type miss matched.");
                                }

                                switch (stype)
                                {
                                    case "Double":
                                        result.Push(s.Max(d => Convert.ToDouble(d)));
                                        break;
                                    case "String":
                                        result.Push(s.OrderByDescending(str => str.ToString().Length).FirstOrDefault());
                                        break;
                                    case "DateTime":
                                        result.Push(s.Max(dt => Convert.ToDateTime(dt)));
                                        break;
                                }
                            }
                            catch
                            {
                                // throw new Exception("MAX evaluation error");
                            }

                        }
                        else
                        {
                            // throw new Exception("MAX evaluation error");
                        }
                        break;
                    case TokenType.MIN:
                        if (result.Count > 0 && !ContainReservedTypeArgs(result, cToken.NumberOfArgs))
                        {
                            try
                            {
                                Type t = result.Peek().GetType();
                                //List<object> s = new List<object>();
                                //for (int j = 0; j < cToken.NumberOfArgs; j++)
                                //{
                                //    if (result.Peek() is System.Collections.ICollection)
                                //    {
                                //        var items = result.Pop() as System.Collections.IEnumerable;
                                //        t = items.GetType().GetElementType(); //what's in the inner list
                                //        s.AddRange(items.Cast<object>());
                                //    }
                                //    else
                                //    {
                                //        s.Add(result.Pop());
                                //    }
                                //}

                                var s = GetArgsCollection(result, cToken.NumberOfArgs);
                                var stype = t.Name.Trim(new[] { '[', ']' });
                                if (s.Any(e => !e.GetType().Name.Equals(stype)))
                                {
                                    // throw new Exception("Max evaluation error, type miss matched.");
                                }

                                switch (stype)
                                {
                                    case "Double":
                                        result.Push(s.Min(d => Convert.ToDouble(d)));
                                        break;
                                    case "String":
                                        result.Push(s.OrderBy(str => str.ToString().Length).FirstOrDefault());
                                        break;
                                    case "DateTime":
                                        result.Push(s.Min(dt => Convert.ToDateTime(dt)));
                                        break;
                                }
                            }
                            catch
                            {
                                // throw new Exception("MIN evaluation error");
                            }

                        }
                        else
                        {
                            // throw new Exception("MIN evaluation error");
                        }
                        break;
                    case TokenType.COUNT:
                        if (result.Count > 0)
                        {
                            //int totalcount = 0;
                            //List<object> s = new List<object>();
                            //for (int j = 0; j < cToken.NumberOfArgs; j++)
                            //{
                            //    if (result.Peek() is System.Collections.ICollection)
                            //    {
                            //        var items = result.Pop() as System.Collections.ICollection;
                            //        totalcount += items.Count;
                            //    }
                            //    else
                            //    {
                            //        //s.Add(result.Pop());
                            //        result.Pop();
                            //        totalcount++;
                            //    }
                            //}
                            ////result.Push(cToken.NumberOfArgs);
                            //result.Push(totalcount);

                            result.Push(GetArgsCollection(result, cToken.NumberOfArgs).Count());

                        }
                        else
                        {
                            // throw new Exception("COUNT evaluation error");
                        }
                        break;
                    case TokenType.AVERAGE:
                        if (result.Count > 0 && !ContainReservedTypeArgs(result, cToken.NumberOfArgs))
                        {
                            //List<object> s = new List<object>();
                            //for (int j = 0; j < cToken.NumberOfArgs; j++)
                            //{
                            //    if (result.Peek() is System.Collections.ICollection)
                            //    {
                            //        var items = result.Pop() as System.Collections.IEnumerable;
                            //        s.AddRange(items.Cast<object>());
                            //    }
                            //    else
                            //    {
                            //        s.Add(result.Pop());
                            //    }
                            //}

                            //result.Push(s.Average(d => Convert.ToDouble(d)));

                            //if (cToken.NumberOfArgs == 1 && !(result.Peek() is System.Collections.ICollection))
                            //{                                
                            //}
                            //else
                            //{
                            result.Push(GetArgsCollection(result, cToken.NumberOfArgs).Average(d => Convert.ToDouble(d)));
                            //}
                        }
                        else
                        {
                            // throw new Exception("AVERAGE evaluation error");
                        }
                        break;
                    case TokenType.SUBSTRING:
                        if (result.Count > 2 && !ContainReservedTypeArgs(result, cToken.NumberOfArgs))
                        {
                            ////substring('xxxx',startpos,length)  
                            int newlength = Convert.ToInt32(result.Pop());
                            int startpos = Convert.ToInt32(result.Pop());

                            if (result.Peek() is System.Collections.ICollection)
                            {
                                result.Push(GetArgsCollection(result, 1).Select(str =>
                                {
                                    string t = str.ToString();
                                    if (startpos <= t.Length && newlength <= t.Length - startpos)//startposition is within string AND substring size is within range
                                    {
                                        return t.Substring(startpos, newlength);
                                    }
                                    else //argument out of bound or exceed source string length
                                    {
                                        return string.Empty;
                                    }
                                }).ToArray()
                                );
                            }
                            else
                            {
                                string str = result.Pop().ToString();
                                result.Push(str.Substring(startpos, newlength));
                            }
                        }
                        else
                        {
                            // throw new Exception("SUBSTRING evaluation error");
                        }
                        break;
                    case TokenType.LENGTH:
                        if (result.Count > 0 && !ContainReservedTypeArgs(result, cToken.NumberOfArgs))
                        {
                            string str = result.Pop().ToString();
                            result.Push(str.Length);
                        }
                        else
                        {
                            // throw new Exception("LENGTH evaluation error");
                        }
                        break;
                    case TokenType.GETDAY:
                        if (result.Count > 0 && !ContainReservedTypeArgs(result, 1))
                        {
                            try
                            {
                                result.Push(Convert.ToDateTime(result.Pop()).Day);
                            }
                            catch
                            {
                                // throw new Exception("GETDAY evaluation error");
                            }
                        }
                        else
                        {
                            // throw new Exception("GETDAY evaluation error");
                        }
                        break;
                    case TokenType.GETMONTH:
                        if (result.Count > 0 && !ContainReservedTypeArgs(result, 1))
                        {
                            try
                            {
                                result.Push(Convert.ToDateTime(result.Pop()).Month);
                            }
                            catch
                            {
                                // throw new Exception("GETMONTH evaluation error");
                            }
                        }
                        else
                        {
                            // throw new Exception("GETMONTH evaluation error");
                        }
                        break;
                    case TokenType.GETYEAR:
                        if (result.Count > 0 && !ContainReservedTypeArgs(result, 1))
                        {

                            try
                            {
                                result.Push(Convert.ToDateTime(result.Pop()).Year);
                            }
                            catch
                            {
                                // throw new Exception("GETYEAR evaluation error");
                            }

                        }
                        else
                        {
                            // throw new Exception("GETYEAR evaluation error");
                        }
                        break;
                    case TokenType.ADDMONTH:
                        if (result.Count > 1 && !ContainReservedTypeArgs(result, 2))
                        {
                            int m = Convert.ToInt32(result.Pop());

                            try
                            {
                                result.Push(Convert.ToDateTime(result.Pop()).AddMonths(m));
                            }
                            catch
                            {
                                // throw new Exception("ADDMONTH evaluation error");
                            }

                        }
                        else
                        {
                            // throw new Exception("ADDMONTH evaluation error, missing parameter.");
                        }
                        break;

                    case TokenType.ADDDAY:
                        if (result.Count > 1 && !ContainReservedTypeArgs(result, 2))
                        {
                            int d = Convert.ToInt32(result.Pop());

                            try
                            {
                                result.Push(Convert.ToDateTime(result.Pop()).AddDays(d));
                            }
                            catch
                            {
                                // throw new Exception("ADDDAY evaluation error");
                            }
                        }
                        else
                        {
                            // throw new Exception("ADDDAY evaluation error, missing parameter.");
                        }
                        break;

                    case TokenType.ADDYEAR:
                        if (result.Count > 1 && !ContainReservedTypeArgs(result, 2))
                        {
                            int y = Convert.ToInt32(result.Pop());

                            try
                            {
                                result.Push(Convert.ToDateTime(result.Pop()).AddYears(y));
                            }
                            catch
                            {
                                // throw new Exception("ADDYEAR evaluation error");
                            }

                        }
                        else
                        {
                            // throw new Exception("ADDYEAR evaluation error, missing parameter.");
                        }
                        break;

                    case TokenType.DATEDIFF:
                        if (result.Count > 2 && !ContainReservedTypeArgs(result, 2)) //first 2 are dates, last one is datepart
                        {
                            operDate2 = Convert.ToDateTime(result.Pop());
                            operDate1 = Convert.ToDateTime(result.Pop());
                            DatePartType datepart = (DatePartType)((ReservedType)result.Pop()).TokenValue;

                            switch (datepart)
                            {
                                case DatePartType.d:
                                case DatePartType.dd:
                                    result.Push(System.Data.Linq.SqlClient.SqlMethods.DateDiffDay(operDate1, operDate2));
                                    break;
                                case DatePartType.m:
                                case DatePartType.mm:
                                    result.Push(System.Data.Linq.SqlClient.SqlMethods.DateDiffMonth(operDate1, operDate2));
                                    break;
                                case DatePartType.y:
                                case DatePartType.yy:
                                    result.Push(System.Data.Linq.SqlClient.SqlMethods.DateDiffYear(operDate1, operDate2));
                                    break;
                            }
                        }
                        else
                        {
                            // throw new Exception("DATEDIFF evaluation error.");
                        }
                        break;
                    case TokenType.TOSCALAR:
                    case TokenType.TONUMBER:
                        if (result.Count > 0 && !ContainReservedTypeArgs(result, cToken.NumberOfArgs))
                        {
                            //regualr 1 arg and it's not Sequence
                            if (cToken.NumberOfArgs == 1 && !(result.Peek() is System.Collections.ICollection))
                            {
                                result.Push(Convert.ToDouble(result.Pop()));
                            }
                            else
                            {
                                result.Push(GetArgsCollection(result, cToken.NumberOfArgs).ConvertAll(str => Convert.ToDouble(str)).ToArray());
                            }

                        }
                        else
                        {
                            // throw new Exception("TOSCALAR/TONUMBER evaluation error");
                        }
                        break;
                    case TokenType.TOSTRING:
                        if (result.Count > 0 && !ContainReservedTypeArgs(result, cToken.NumberOfArgs))
                        {
                            //regualr 1 arg and it's not Sequence
                            if (cToken.NumberOfArgs == 1 && !(result.Peek() is System.Collections.ICollection))
                            {
                                if (result.Peek() is DateTime)
                                    result.Push(((DateTime)result.Pop()).ToShortDateString());
                                else
                                    result.Push(result.Pop());
                            }
                            else //multiple arg(s) making it a sequence
                            {
                                result.Push(GetArgsCollection(result, cToken.NumberOfArgs).ConvertAll(str => str.ToString()).ToArray());
                            }
                        }
                        else
                        {
                            // throw new Exception("TOSTRING evaluation error");
                        }
                        break;
                    case TokenType.TODATE:
                        if (result.Count > 0 && !ContainReservedTypeArgs(result, cToken.NumberOfArgs))
                        {
                            if (cToken.NumberOfArgs == 1 && !(result.Peek() is System.Collections.ICollection))
                            {
                                result.Push(Convert.ToDateTime(result.Pop()).ToShortDateString());
                            }
                            else
                            {
                                result.Push(GetArgsCollection(result, cToken.NumberOfArgs).ConvertAll(str => Convert.ToDateTime(str).ToShortDateString()).ToArray());
                            }
                        }
                        else
                        {
                            // throw new Exception("TODATE evaluation error");
                        }
                        break;
                    case TokenType.DAYOFWEEK:
                        if (result.Count > 0 && !ContainReservedTypeArgs(result, cToken.NumberOfArgs))
                        {
                            if (cToken.NumberOfArgs == 1 && !(result.Peek() is System.Collections.ICollection))
                            {
                                result.Push(Convert.ToDateTime(result.Pop()).DayOfWeek);
                            }
                            else
                            {
                                result.Push(GetArgsCollection(result, cToken.NumberOfArgs).Select(da => Convert.ToDateTime(da).DayOfWeek).ToArray());
                            }
                        }
                        else
                        {
                            // throw new Exception("DayOfWeek evaluation error");
                        }
                        break;
                    case TokenType.ROUND:
                        if (result.Count > 1 && !ContainReservedTypeArgs(result, cToken.NumberOfArgs))
                        {
                            int decimalPlaces = Convert.ToInt32(result.Pop());

                            if (cToken.NumberOfArgs == 2 && !(result.Peek() is System.Collections.ICollection))
                            {
                                result.Push(CommonRound(Convert.ToDouble(result.Pop()), decimalPlaces));
                            }
                            else
                            {
                                result.Push(GetArgsCollection(result, cToken.NumberOfArgs - 1).Select(num => CommonRound(Convert.ToDouble(num), decimalPlaces)).ToArray());
                            }
                        }
                        else
                        {
                            // throw new Exception("Rounding evaluation error");
                        }
                        break;
                    case TokenType.DATENOW:
                        result.Push(DateTime.Now.ToShortDateString());
                        break;
                    case TokenType.DATETIMENOW:
                        result.Push(DateTime.Now.ToString(DateFormatStrDefault));
                        break;
                    case TokenType.SQRT:
                        if (result.Count > 0 && !ContainReservedTypeArgs(result, 1))
                        {
                            try
                            {
                                result.Push(Math.Sqrt(Convert.ToDouble(result.Pop())));
                            }
                            catch
                            {
                                // throw new Exception("SQRT evaluation error");
                            }
                        }
                        else if (result.Count > 1)
                        {
                            // throw new Exception("SQRT cannot contain multiple parameters");
                        }
                        else
                        {
                            // throw new Exception("SQRT evaluation error");
                        }
                        break;
                    case TokenType.CONCAT:
                        if (result.Count > 0 && !ContainReservedTypeArgs(result, cToken.NumberOfArgs))
                        {
                            if (cToken.NumberOfArgs == 1 && !(result.Peek() is System.Collections.ICollection))
                            {
                                result.Push(result.Pop());
                            }
                            else
                            {
                                result.Push(GetArgsCollection(result, cToken.NumberOfArgs).Aggregate((a, next) => next + "" + a));
                            }
                        }
                        else
                        {
                            // throw new Exception("Concatenation evaluation error");
                        }
                        break;
                    case TokenType.IFBLANK:
                        if (result.Count > 0 && !ContainReservedTypeArgs(result, cToken.NumberOfArgs))
                        {
                            if (cToken.NumberOfArgs == 1 && !(result.Peek() is System.Collections.ICollection))
                            {
                                result.Push(string.IsNullOrEmpty(result.Peek().ToString()) ? "" : result.Pop().ToString());
                            }
                            {
                                result.Push(GetArgsCollection(result, cToken.NumberOfArgs).Reverse<object>().First(s => !string.IsNullOrEmpty(s.ToString())));
                            }
                        }
                        else
                        {
                            // throw new Exception("IFBLANK evaluation error");
                        }
                        break;
                }

            }

            if (result.Count == 1)
            {
                if (result.Peek() is System.Collections.ICollection)
                {
                    var items = result.Pop() as System.Collections.IEnumerable;
                    List<object> s = new List<object>(items.Cast<object>());
                    string x = s.Aggregate((a, next) => (next is DateTime ? ((DateTime)next).ToShortDateString() : next) + DELIM + (a is DateTime ? ((DateTime)a).ToShortDateString() : a)).ToString();
                    return x;
                }
                else
                {
                    return result.Peek() is DateTime ? ((DateTime)result.Pop()).ToShortDateString() : result.Pop().ToString();
                }
            }
            //else if (result.Count > 1)
            //{                
            //}
            else
            {
                // throw new Exception("Evaluation error");
            }

            return string.Empty;
        }
        public double CommonRound(double value, int decimals)
        {
            if (value < 0)
            {
                return Math.Round(value + 5 / Math.Pow(10, decimals + 1), decimals, MidpointRounding.AwayFromZero);
            }
            else
            {
                return Math.Round(value, decimals, MidpointRounding.AwayFromZero);
            }
        }
        public List<object> GetArgsCollection(Stack<object> result, int argsCount)
        {
            List<object> s = new List<object>();
            for (int j = 0; j < argsCount; j++)
            {
                if (result.Peek() is System.Collections.ICollection)
                {
                    var items = result.Pop() as System.Collections.IEnumerable;
                    s.AddRange(items.Cast<object>());

                }
                else
                {
                    if (!IsReservedType(result.Peek()))
                    {
                        s.Add(result.Pop());
                    }
                    else
                    {
                        // throw new ArgumentException("invalid argument");
                    }
                }
            }
            return s;
        }
        public TokenType GetTokenType(object obj)
        {
            Type t = obj.GetType();

            if (obj is System.Collections.ICollection)
            {
                return TokenType.Collection;
            }
            else
            {
                switch (t.Name)
                {
                    case "Double":
                    case "Int32":
                    case "Int16":
                    case "Int":
                        return TokenType.Number;

                    case "String":
                        return TokenType.String;

                    case "DateTime":
                        return TokenType.DateTime;
                }
            }
            return TokenType.String;
        }


        private bool TryPasteNumber(string s, out double result)
        {
            bool passed = false;
            passed = double.TryParse(s, out result);
            return passed;
        }
        private bool TryPasteDateTime(string s, out DateTime result)
        {
            bool passed = false;
            passed = DateTime.TryParse(s, out result);
            return passed;
        }
        public static bool TryParseEnum<T>(string s, out T output) where T : struct
        {
            output = default(T);

            if (!typeof(T).IsEnum)
                return false; // throw new ArgumentException("T must be an enumerated type");

            try
            {
                output = (T)Enum.Parse(typeof(T), s, true);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public class ScriptCalculationInfo
    {
        public Decimal ScriptCalculationId { get; set; }


        public string CalculationName { get; set; }


        public string CalculationDescription { get; set; }


        public string CalculationExpression { get; set; }


        public DateTime DateCreated { get; set; }


        public DateTime DateModified { get; set; }


        public int FolderId { get; set; }


        public bool Removed { get; set; }


        public int DecimalPlaces { get; set; }


        public static ScriptCalculationInfo GetScriptCalculationBy(string calcId)
        {

            ScreenViewer.API.ScriptCalculationController SCC = new API.ScriptCalculationController();
            var actionResult = SCC.GetScriptCalculation(Convert.ToDecimal(calcId));

            var result = actionResult as OkNegotiatedContentResult<Data.ScriptCalculation>;
            Data.ScriptCalculation scriptCalc = result.Content;

            ScriptCalculationInfo calc = null;

            calc = new ScriptCalculationInfo();
            calc.ScriptCalculationId = scriptCalc.ScriptCalculationID;
            calc.CalculationDescription = scriptCalc.CalculationDesc;
            calc.CalculationExpression = scriptCalc.CalculationExpression;
            calc.DecimalPlaces = 0;


            return calc;
        }
    }


    public class CalToken
    {
        public object TokenValue { get; set; }
        public TokenType CalTokenType { get; set; }
        public int NumberOfArgs { get; set; }
    }

    public enum TokenType
    {
        None,
        Plus, Minus, Multiply, Divide, Exponent, Concatenation, Mod,
        LeftParenthesis, RightParenthesis, Comma,

        SUM, MAX, MIN, COUNT, AVERAGE, LENGTH, SUBSTRING,
        ROUND, SQRT, CONCAT,

        GETMONTH, GETDAY, GETYEAR, ADDMONTH, ADDDAY, ADDYEAR,
        TOSCALAR, TONUMBER, TOSTRING, TODATE,
        TOSHORTDATESTRING, TOLONGDATESTRING,   //need to implement

        DATENOW, DATETIMENOW, DAYOFWEEK, DATEDIFF,
        IFBLANK,

        Number, Constant, UnaryMinus, DateTime, Datepart,
        String, Collection, DataObject, QuestionResponse
    }

    public enum DatePartType
    {
        yy, y, mm, m, d, dd
    }

    public class ReservedType : CalToken { }

}