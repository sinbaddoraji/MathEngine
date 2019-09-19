using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Math
{
    public class MathEngine
    {
        public readonly Dictionary<string, dynamic> Variables = new Dictionary<string, dynamic>();

        private readonly Regex R = new Regex(@"([-]?\w+(\.\w+)?)[\s+]?([\/\*\^%\+\-])[\s+]?([-]?\w+(\.\w+)?)",RegexOptions.Compiled);
        private readonly Regex Pow = new Regex(@"(.*)(([-]?\w+)?(\.)?(\w+))[\s+]?[\^][\s+]?(\w+)", RegexOptions.Compiled);
        private readonly Regex Mul = new Regex(@"([-]?\w+(\.\w+)?)[\s+]?([\*])[\s+]?([-]?\w+(\.\w+)?)", RegexOptions.Compiled);
        private readonly Regex Div = new Regex(@"([-]?\w+(\.\w+)?)[\s+]?([\/])[\s+]?([-]?\w+(\.\w+)?)", RegexOptions.Compiled);
        private readonly Regex Brackets = new Regex(@"\((?!\()[^)]+\)", RegexOptions.Compiled);
        private readonly Regex Mfuc = new Regex(@"(\w+)(\((?!\()[^)]+\))",RegexOptions.Compiled);

        public enum ReturnType { Int, Double, Float, Long }

        public enum Functions { Abs, Acos, Asin, Atan, Cos, Sin, Tan, Ceiling, Floor, Round, Truncate, Sqrt, Log, Log10, Other }

        #region Functions

        public delegate dynamic MathFuc(dynamic inp);

        public MathFuc Other;

        private void DoFunc(ref Functions mfn, ref dynamic output, ref string exp)
        {
            switch (mfn)
            {
                case Functions.Abs: output = System.Math.Abs(Convert.ToDecimal(exp)); break;
                case Functions.Acos: output = System.Math.Acos(Convert.ToDouble(exp)); break;
                case Functions.Asin: output = System.Math.Asin(Convert.ToDouble(exp)); break;
                case Functions.Atan: output = System.Math.Atan(Convert.ToDouble(exp)); break;
                case Functions.Cos: output = System.Math.Cos(Convert.ToDouble(exp)); break;
                case Functions.Sin: output = System.Math.Sin(Convert.ToDouble(exp)); break;
                case Functions.Tan: output = System.Math.Tan(Convert.ToDouble(exp)); break;
                case Functions.Ceiling: output = System.Math.Ceiling(Convert.ToDouble(exp)); break;
                case Functions.Floor: output = System.Math.Floor(Convert.ToDouble(exp)); break;
                case Functions.Round: output = System.Math.Round(Convert.ToDouble(exp)); break;
                case Functions.Truncate: output = System.Math.Truncate(Convert.ToDouble(exp)); break;
                case Functions.Sqrt: output = System.Math.Sqrt(Convert.ToDouble(exp)); break;
                case Functions.Log: output = System.Math.Log(Convert.ToDouble(exp)); break;
                case Functions.Log10: output = System.Math.Log10(Convert.ToDouble(exp)); break;
                case Functions.Other:
                    try
                    {
                        ReturnType returnType = ReturnType.Double;
                        output = Other(Get(exp,ref returnType,externalGet));
                    }
                    catch
                    {
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(mfn), mfn, null);
            }
        }

        private Functions GetFunc(string str)
        {
            if (str.Equals("abs", StringComparison.CurrentCultureIgnoreCase)) return Functions.Abs;
            if (str.Equals("acos", StringComparison.CurrentCultureIgnoreCase)) return Functions.Acos;
            if (str.Equals("asin", StringComparison.CurrentCultureIgnoreCase)) return Functions.Asin;
            if (str.Equals("atan", StringComparison.CurrentCultureIgnoreCase)) return Functions.Atan;
            if (str.Equals("cos", StringComparison.CurrentCultureIgnoreCase)) return Functions.Cos;
            if (str.Equals("sin", StringComparison.CurrentCultureIgnoreCase)) return Functions.Sin;
            if (str.Equals("tan", StringComparison.CurrentCultureIgnoreCase)) return Functions.Tan;
            if (str.Equals("ceilling", StringComparison.CurrentCultureIgnoreCase)) return Functions.Ceiling;
            if (str.Equals("floor", StringComparison.CurrentCultureIgnoreCase)) return Functions.Floor;
            if (str.Equals("round", StringComparison.CurrentCultureIgnoreCase)) return Functions.Round;
            if (str.Equals("truncate", StringComparison.CurrentCultureIgnoreCase)) return Functions.Truncate;
            if (str.Equals("sqrt", StringComparison.CurrentCultureIgnoreCase)) return Functions.Sqrt;
            if (str.Equals("log", StringComparison.CurrentCultureIgnoreCase)) return Functions.Log;
            if (str.Equals("log10", StringComparison.CurrentCultureIgnoreCase)) return Functions.Log10;
            return Functions.Other;
        }

        #endregion Functions

        #region Expression Clean up

        private void BalanceSigns(ref string expression)
        {
            var c = expression.ToCharArray();
            var emptyChar = " "[0];
            for (int i = 0; i < c.Length; i++)
            {
                if ((c[i] == '+' && c[i + 1] == '-') || (c[i] == '-' && c[i + 1] == '+'))
                {
                    c[i] = emptyChar;
                    c[i + 1] = '-';
                }
                else if (c[i] == '-' && c[i + 1] == '-')
                {
                    c[i] = emptyChar;
                    c[i + 1] = '+';
                }
            }
            expression = new string(c).Trim(' ', '+');
        }

        private void AttemptBracketRemoval(ref string expression, ref ReturnType returnType)
        {
            while (Brackets.IsMatch(expression))
            {
                MatchCollection m = Brackets.Matches(expression);
                for (int i = 0; i < m.Count; i++)
                {
                    Match item = m[i];
                    string exp = item.Value.Trim('(', ')');
                    expression = expression.Replace(item.Value, Express(ref exp, returnType).ToString());
                }
            }
        }

        private void RemoveFunctions(ref string expression, ref ReturnType returnType)
        {
            if (Mfuc.IsMatch(expression))
            {
                MatchCollection m = Mfuc.Matches(expression);
                for (int i = 0; i < m.Count; i++)
                {
                    Match item = m[i];
                    dynamic exp = item.Value;
                    GetV(ref exp, ref returnType);

                    expression = expression.Replace(item.Value, Convert.ToString(exp));
                }
            }
        }

        #endregion Expression Clean up

        #region Inteprete/Set Values

        public delegate dynamic ExternalGetV(string vName);

        public ExternalGetV externalGet;

        private string Get(dynamic val, ref ReturnType returnType, ExternalGetV externalGet)
        {
            var t = val;
            try
            {
                t = externalGet(t);
            }
            catch { }
            GetV(ref t, ref returnType);
            return Convert.ToString(t);
        }

        private void GetV(ref dynamic val, ref ReturnType returnType)
        {
            string key = Convert.ToString(val);
            if (Variables.ContainsKey(key))
            {
                val = Variables[key];
                return;
            }
            else if (Mfuc.IsMatch(key))
            {
                var g = Mfuc.Match(key).Groups;

                var mathFuncName = GetFunc(g[1].Value);
                var exp = g[2].Value;

                try
                {
                    exp = Express(ref exp, returnType);
                    DoFunc(ref mathFuncName, ref val, ref exp);
                }
                catch
                {
                    exp = Get(exp, ref returnType, externalGet);
                    DoFunc(ref mathFuncName, ref val, ref exp);
                }
            }
        }

        private void SetValues(ReturnType returnType, ref dynamic a, ref dynamic b, ref GroupCollection g)
        {
            switch (returnType)
            {
                case ReturnType.Int:
                    a = int.Parse(Get(g[1].Value, ref returnType, externalGet));
                    b = int.Parse(Get(g[4].Value, ref returnType, externalGet));
                    break;

                case ReturnType.Double:
                    a = double.Parse(Get(g[1].Value, ref returnType, externalGet));
                    b = double.Parse(Get(g[4].Value, ref returnType, externalGet));
                    break;

                case ReturnType.Float:
                    a = float.Parse(Get(g[1].Value, ref returnType, externalGet));
                    b = float.Parse(Get(g[4].Value, ref returnType, externalGet));
                    break;

                case ReturnType.Long:
                    a = long.Parse(Get(g[1].Value, ref returnType, externalGet));
                    b = long.Parse(Get(g[4].Value, ref returnType, externalGet));
                    break;
            }
        }

        #endregion Inteprete/Set Values

        #region Solve Expression

        public dynamic Express(string expression, ReturnType returnType)
        {
            string exp = expression;
            return Express(ref exp, returnType);
        }

        public dynamic Express(ref string expression, ReturnType returnType)
        {
            expression = expression.Replace(" ", "");
            BalanceSigns(ref expression);
            RemoveFunctions(ref expression, ref returnType);
            AttemptBracketRemoval(ref expression, ref returnType);

            try
            {
                foreach (Match item in new Regex(@"\w+\[\w+\]").Matches(expression))
                {
                    expression = expression.Replace(item.Value, Get(item.Value, ref returnType, externalGet));
                }
                while (Pow.IsMatch(expression))
                {
                    var m = Pow.Matches(expression)[0];
                    var item = $"{m.Groups[5].Value}^{m.Groups[6].Value}";
                    var index = expression.LastIndexOf(item, StringComparison.Ordinal);
                    expression = expression.Remove(index, item.Length).Insert(index, Convert.ToString(HandleExpression(item, returnType)));
                }
                while (Mul.IsMatch(expression) || Div.IsMatch(expression) || R.IsMatch(expression))
                {
                    MatchCollection m;

                    if (Mul.IsMatch(expression)) m = Mul.Matches(expression);
                    else if (Div.IsMatch(expression)) m = Div.Matches(expression);
                    else m = R.Matches(expression);
                    
                    var item = m[0].Value;
                    expression = expression.Replace(item, Convert.ToString(HandleExpression(item, returnType)));
                }
            }
            catch { }
            expression = Get(expression, ref returnType, externalGet).ToString();
            switch (returnType)
            {
                case ReturnType.Double: return Convert.ToDouble(expression);
                case ReturnType.Float: return Convert.ToSingle(expression);
                case ReturnType.Int: return Convert.ToInt64(expression);
                case ReturnType.Long: return long.Parse(expression);
            }

            return expression;
        }

        private dynamic HandleExpression(string expression, ReturnType returnType)
        {
            if (!R.IsMatch(expression)) return 0.0;

            dynamic a = 0.0;
            dynamic b = 0.0;

            var g = R.Match(expression).Groups;
            SetValues(returnType, ref a, ref b, ref g);

            switch (g[3].Value)
            {
                case "+":
                    return a + b;

                case "-":
                    return a - b;

                case "*":
                    return a * b;

                case "/":
                    return a / b;

                case "%":
                    return a % b;

                case "^":
                    return System.Math.Pow(double.Parse(a.ToString()), double.Parse(b.ToString()));

                case ".":
                    return expression;
            }

            return 0.0;
        }

        #endregion Solve Expression
    }
}