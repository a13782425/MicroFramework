using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MFramework.Core
{
    /*
    * 支持运算符:
    *          +               加法
    *          -               减法
    *          *               乘法
    *          /               除法
    *          %               取余
    * 支持关键字:
    *          e               自然对数
    *          pi              圆周率
    * 支持函数:
    *          cos(v)          余弦
    */

    /// <summary>
    /// 数学计算
    /// </summary>
    public sealed partial class MathFormulaCalculate
    {
        /// <summary>
        /// 公式
        /// </summary>
        private string _formula;
        /// <summary>
        /// 公式
        /// </summary>
        public string Formula
        {
            get => _formula;
            set
            {
                _formula = value;
                if (s_tokenCacheDic.ContainsKey(value))
                {
                    _curTokens.Clear();
                    _curTokens.AddRange(s_tokenCacheDic[value]);
                }
                else
                {
                    _formulaLexer.Lexer();
                    _formulaParser.Parse();
                    s_tokenCacheDic.Add(value, _curTokens.ToList());
                }
                //string str = "";
                //foreach (var item in _curTokens)
                //{
                //    str += item.ToString() + "_";
                //}
                //UnityEngine.Debug.LogError(str);
            }
        }

        /// <summary>
        /// 当前公式的所有表征
        /// </summary>
        private List<Token> _curTokens = new List<Token>(16);
        /// <summary>
        /// 结果堆栈
        /// </summary>
        private Stack<double> _resultStack = new Stack<double>();
        /// <summary>
        /// 参数
        /// </summary>
        private Dictionary<string, double> _varDic = new Dictionary<string, double>();
        private MathFormulaLexer _formulaLexer;
        private MathFormulaParser _formulaParser;
        /// <summary>
        /// 是否在计算中
        /// </summary>
        private bool _isCalculating = false;
        /// <summary>
        /// 是否开始计算方法
        /// </summary>
        private bool _isCalculatFunc = false;
        /// <summary>
        /// 方法剩余可读取参数数量
        /// </summary>
        private int _funcParamRemainCount = 0;
        private MathFormulaCalculate()
        {
            _formulaLexer = new MathFormulaLexer(this);
            _formulaParser = new MathFormulaParser(this);
        }
        private MathFormulaCalculate(string formula) : this()
        {
            this.Formula = formula;
        }

        /// <summary>
        /// 设置变量
        /// </summary>
        /// <param name="varName"></param>
        /// <param name="varValue"></param>
        /// <returns></returns>
        public MathFormulaCalculate SetVariable(string varName, double varValue)
        {
            if (_varDic.ContainsKey(varName))
                _varDic[varName] = varValue;
            else
                _varDic.Add(varName, varValue);
            return this;
        }
        /// <summary>
        /// 删除一个变量
        /// </summary>
        /// <param name="varName"></param>
        /// <returns></returns>
        public MathFormulaCalculate RemoveVariable(string varName)
        {
            if (_varDic.ContainsKey(varName))
                _varDic.Remove(varName);
            return this;
        }
        /// <summary>
        /// 获取一个变量值
        /// </summary>
        /// <param name="varName"></param>
        /// <returns></returns>
        public double GetVariable(string varName)
        {
            if (_varDic.ContainsKey(varName))
                return _varDic[varName];
#if UNITY_EDITOR
            UnityEngine.Debug.LogError($"参数：{varName}, 没有找到, 返回0");
#endif
            return default;
        }

        /// <summary>
        /// 弹出一个值
        /// </summary>
        /// <returns></returns>
        public double PopValue()
        {
            if (_isCalculatFunc)
            {
                if (_funcParamRemainCount <= 0)
                    throw new FormulaCalculateException($"计算错误: 该方法无法获取更多的参数。");
                _funcParamRemainCount--;
            }

            if (_resultStack.Count > 0)
                return _resultStack.Pop();
            throw new FormulaCalculateException($"计算错误: 堆栈数据不如。");
        }

        /// <summary>
        /// 计算
        /// </summary>
        /// <returns>计算后的值</returns>
        public double Calculate()
        {
            if (_isCalculating)
                throw new FormulaCalculateException("公式正在计算，无法重复计算");
            _isCalculating = true;
            _resultStack.Clear();
            foreach (var item in _curTokens)
            {
                try
                {
                    m_calculate(item);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"公式: {_formula} ({item.RowNum},{item.ColNum}) -> 符号 {item.Lexeme} 错误");
                    throw ex;
                }

            }
            _isCalculatFunc = false;
            _isCalculating = false;
            // 当所有的操作符都被处理完后，栈顶应为最终结果
            if (_resultStack.Count != 1)
                throw new Exception("计算错误: 表达式不完整或者操作数不足。");

            return _resultStack.Pop();
        }

        private void m_calculate(Token item)
        {
            switch (item.TokenType)
            {
                case TokenType.Plus:
                    m_checkCount(2);
                    _resultStack.Push(s_plusOpFunc(this));
                    break;
                case TokenType.Minus:
                    m_checkCount(2);
                    _resultStack.Push(s_minusOpFunc(this));
                    break;
                case TokenType.Multiply:
                    m_checkCount(2);
                    _resultStack.Push(s_multiplyOpFunc(this));
                    break;
                case TokenType.Divide:
                    m_checkCount(2);
                    _resultStack.Push(s_divideOpFunc(this));
                    break;
                case TokenType.Modulo:
                    m_checkCount(2);
                    _resultStack.Push(s_moduloOpFunc(this));
                    break;
                case TokenType.And:
                    m_checkCount(2);
                    _resultStack.Push(s_andOpFunc(this));
                    break;
                case TokenType.Or:
                    m_checkCount(2);
                    _resultStack.Push(s_orOpFunc(this));
                    break;
                case TokenType.Equal:
                    m_checkCount(2);
                    _resultStack.Push(s_equalOpFunc(this));
                    break;
                case TokenType.NotEqual:
                    m_checkCount(2);
                    _resultStack.Push(s_notEqualOpFunc(this));
                    break;
                case TokenType.Greater:
                    m_checkCount(2);
                    _resultStack.Push(s_greaterOpFunc(this));
                    break;
                case TokenType.GreaterOrEqual:
                    m_checkCount(2);
                    _resultStack.Push(s_greaterOrEqualOpFunc(this));
                    break;
                case TokenType.Less:
                    m_checkCount(2);
                    _resultStack.Push(s_lessOpFunc(this));
                    break;
                case TokenType.LessOrEqual:
                    m_checkCount(2);
                    _resultStack.Push(s_lessOrEqualOpFunc(this));
                    break;
                case TokenType.Negative:
                    m_checkCount(1);
                    _resultStack.Push(_resultStack.Pop() * -1);
                    break;
                case TokenType.ConstName:
                    if (!s_constDic.ContainsKey(item.Lexeme))
                        throw new FormulaCalculateException($"常量 {item.Lexeme} 不被支持。");
                    _resultStack.Push(s_constDic[item.Lexeme]);
                    break;
                case TokenType.VarName:
                    if (!_varDic.ContainsKey(item.Lexeme))
                        throw new FormulaCalculateException($"变量 {item.Lexeme} 不被支持。");
                    _resultStack.Push(_varDic[item.Lexeme]);
                    break;
                case TokenType.FuncName:
                    if (!s_funcDic.ContainsKey(item.Lexeme))
                        throw new FormulaCalculateException($"方法 {item.Lexeme} 不被支持。");

                    _isCalculatFunc = true;
                    try
                    {
                        MathFunc func = s_funcDic[item.Lexeme];
                        m_checkCount(func.ParamCount);
                        _funcParamRemainCount = func.ParamCount;
                        _resultStack.Push(func.Func.Invoke(this));
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        _isCalculatFunc = false;
                    }
                    break;
                case TokenType.Number:
                    _resultStack.Push(item.Number);
                    break;
                default:
                    break;
            }
        }

        private void m_checkCount(int count)
        {
            if (_resultStack.Count < count)
                throw new FormulaCalculateException("计算错误: 操作符没有足够的操作数。");
        }
    }

    //静态方法
    partial class MathFormulaCalculate
    {
        public const double TRUE = 1;
        public const double FALSE = -1;

        /// <summary>
        /// 静态构造
        /// </summary>
        static MathFormulaCalculate()
        {
            s_constDic.Add("e", Math.E);
            s_constDic.Add("pi", Math.PI);
            AddFunc("sin", 1, s_sinFunc);
            AddFunc("cos", 1, s_cosFunc);
            AddFunc("tan", 1, s_tanFunc);
            AddFunc("asin", 1, s_asinFunc);
            AddFunc("acos", 1, s_acosFunc);
            AddFunc("atan", 1, s_atanFunc);
            AddFunc("abs", 1, s_absFunc);
            AddFunc("sqrt", 1, s_sqrtFunc);
            AddFunc("log", 2, s_logFunc);
            AddFunc("exp", 1, s_expFunc);
            AddFunc("floor", 1, s_floorFunc);
            AddFunc("ceil", 1, s_ceilFunc);
            AddFunc("pow", 2, s_powFunc);
            AddFunc("max", 2, s_maxFunc);
            AddFunc("min", 2, s_minFunc);
            AddFunc("round", 2, s_roundFunc);
            AddFunc("if", 3, s_ifFunc);
        }

        /// <summary>
        /// 所有常量
        /// </summary>
        private static Dictionary<string, double> s_constDic = new Dictionary<string, double>();
        /// <summary>
        /// 所有方法
        /// </summary>
        private static Dictionary<string, MathFunc> s_funcDic = new Dictionary<string, MathFunc>();
        /// <summary>
        /// 所有表征缓存
        /// </summary>
        private static Dictionary<string, List<Token>> s_tokenCacheDic = new Dictionary<string, List<Token>>(8);
        /// <summary>
        /// 操作token的权重
        /// </summary>
        private static Dictionary<TokenType, int> s_tokenWightDic = new Dictionary<TokenType, int>()
        {
            {TokenType.Multiply, 3},
            {TokenType.Divide, 3},
            {TokenType.Modulo, 3},
            {TokenType.Plus, 2},
            {TokenType.Minus, 2},
            {TokenType.Less, 1},
            {TokenType.LessOrEqual, 1},
            {TokenType.Greater, 1},
            {TokenType.GreaterOrEqual, 1},
            {TokenType.NotEqual, 1},
            {TokenType.Equal, 1},
            {TokenType.And, 0},
            {TokenType.Or, -1},
        };

        /// <summary>
        /// 创建一个文字数学计算
        /// </summary>
        /// <returns></returns>
        public static MathFormulaCalculate Create()
        {
            return new MathFormulaCalculate();
        }

        /// <summary>
        /// 创建一个文字数学计算
        /// </summary>
        /// <param name="formula">公式</param>
        /// <returns></returns>
        public static MathFormulaCalculate Create(string formula)
        {
            return new MathFormulaCalculate(formula);
        }

        /// <summary>
        /// 添加一个常量
        /// <para>如果添加两个名字一样的常量，新的值覆盖旧的值</para>
        /// </summary>
        /// <param name="constName">常量名</param>
        /// <param name="value">常量值</param>
        public static void AddConst(string constName, double value)
        {
            if (!s_isValidMemberName(constName))
                throw new FormulaInvalidMemberException($"常量名:{constName} 不符合规范");
            if (s_constDic.ContainsKey(constName))
                s_constDic[constName] = value;
            else
                s_constDic.Add(constName, value);
        }
        /// <summary>
        /// 移除一个常量
        /// </summary>
        /// <param name="constName">常量名</param>
        public static void RemoveConst(string constName)
        {
            if (s_constDic.ContainsKey(constName))
                s_constDic.Remove(constName);
        }
        /// <summary>
        /// 获取一个常量值
        /// <para>如果没有返回0</para>
        /// </summary>
        /// <param name="constName">常量名</param>
        public static double GetConst(string constName)
        {
            if (s_constDic.ContainsKey(constName))
                return s_constDic[constName];
#if UNITY_EDITOR
            UnityEngine.Debug.LogError($"常量：{constName}, 没有找到, 返回0");
#endif
            return default;
        }

        /// <summary>
        /// 添加一个函数
        /// <para>不支持重载</para>
        /// </summary>
        /// <param name="funcName">函数名</param>
        /// <param name="paramCount">参数个数</param>
        /// <param name="func">函数</param>
        public static void AddFunc(string funcName, int paramCount, MathFuncDelegate func)
        {
            if (func == null)
                throw new FormulaInvalidMemberException($"方法名:{funcName} 委托回调为空");
            if (!s_isValidMemberName(funcName))
                throw new FormulaInvalidMemberException($"方法名:{funcName} 不符合规范");
            if (s_funcDic.ContainsKey(funcName))
                s_funcDic.Remove(funcName);
            MathFunc mathFunc = new MathFunc(funcName, func, paramCount);
            s_funcDic.Add(funcName, mathFunc);
        }

        /// <summary>
        /// 添加一个函数
        /// <para>不支持重载</para>
        /// </summary>
        /// <param name="funcName">函数名</param>
        public static void RemoveFunc(string funcName)
        {
            if (s_funcDic.ContainsKey(funcName))
                s_funcDic.Remove(funcName);
        }

        #region 内置方法

        private static double s_sinFunc(MathFormulaCalculate calculate)
        {
            return Math.Sin(calculate.PopValue());
        }

        private static double s_cosFunc(MathFormulaCalculate calculate)
        {
            return Math.Cos(calculate.PopValue());
        }

        private static double s_tanFunc(MathFormulaCalculate calculate)
        {
            return Math.Tan(calculate.PopValue());
        }

        private static double s_asinFunc(MathFormulaCalculate calculate)
        {
            return Math.Asin(calculate.PopValue());
        }

        private static double s_acosFunc(MathFormulaCalculate calculate)
        {
            return Math.Acos(calculate.PopValue());
        }

        private static double s_atanFunc(MathFormulaCalculate calculate)
        {
            return Math.Atan(calculate.PopValue());
        }

        private static double s_absFunc(MathFormulaCalculate calculate)
        {
            return Math.Abs(calculate.PopValue());
        }

        private static double s_sqrtFunc(MathFormulaCalculate calculate)
        {
            double a = calculate.PopValue();
            if (a < 0)
                throw new FormulaCalculateException($"计算错误: Sqrt函数参数错误:{a}。");
            return Math.Sqrt(a);
        }

        private static double s_logFunc(MathFormulaCalculate calculate)
        {
            double b = calculate.PopValue();
            double a = calculate.PopValue();
            return Math.Log(a, b);
        }

        private static double s_expFunc(MathFormulaCalculate calculate)
        {
            return Math.Exp(calculate.PopValue());
        }

        private static double s_floorFunc(MathFormulaCalculate calculate)
        {
            return Math.Floor(calculate.PopValue());
        }

        private static double s_ceilFunc(MathFormulaCalculate calculate)
        {
            return Math.Ceiling(calculate.PopValue());
        }

        private static double s_powFunc(MathFormulaCalculate calculate)
        {
            double b = calculate.PopValue();
            double a = calculate.PopValue();
            return Math.Pow(a, b);
        }

        private static double s_maxFunc(MathFormulaCalculate calculate)
        {
            double b = calculate.PopValue();
            double a = calculate.PopValue();
            return Math.Max(a, b);
        }

        private static double s_minFunc(MathFormulaCalculate calculate)
        {
            double b = calculate.PopValue();
            double a = calculate.PopValue();
            return Math.Min(a, b);
        }

        private static double s_roundFunc(MathFormulaCalculate calculate)
        {
            double b = calculate.PopValue();
            double a = calculate.PopValue();
            int ib = (int)b;
            if (ib <= 0)
                return Math.Round(a);
            else
                return Math.Round(a, ib);
        }

        private static double s_ifFunc(MathFormulaCalculate calculate)
        {
            var falseVal = calculate.PopValue();
            var trueVal = calculate.PopValue();
            var condition = calculate.PopValue();
            if (condition > 0)
                return trueVal;
            else
                return falseVal;
        }

        #endregion

        #region 内置操作

        private static double s_plusOpFunc(MathFormulaCalculate calculate)
        {
            var b = calculate.PopValue();
            var a = calculate.PopValue();
            return a + b;
        }
        private static double s_minusOpFunc(MathFormulaCalculate calculate)
        {
            var b = calculate.PopValue();
            var a = calculate.PopValue();
            return a - b;
        }
        private static double s_multiplyOpFunc(MathFormulaCalculate calculate)
        {
            var b = calculate.PopValue();
            var a = calculate.PopValue();
            return a * b;
        }
        private static double s_divideOpFunc(MathFormulaCalculate calculate)
        {
            var b = calculate.PopValue();
            var a = calculate.PopValue();
            if (b == 0)
                throw new FormulaCalculateException("计算错误：除数不能为零。");
            return a / b;
        }
        private static double s_moduloOpFunc(MathFormulaCalculate calculate)
        {
            var b = calculate.PopValue();
            var a = calculate.PopValue();
            if (b == 0)
                throw new FormulaCalculateException("计算错误：除数不能为零。");
            return a % b;
        }
        private static double s_andOpFunc(MathFormulaCalculate calculate)
        {
            var b = calculate.PopValue();
            var a = calculate.PopValue();
            return a > 0 && b > 0 ? TRUE : FALSE;
        }
        private static double s_orOpFunc(MathFormulaCalculate calculate)
        {
            var b = calculate.PopValue();
            var a = calculate.PopValue();
            return a > 0 || b > 0 ? TRUE : FALSE;
        }
        private static double s_equalOpFunc(MathFormulaCalculate calculate)
        {
            var b = calculate.PopValue();
            var a = calculate.PopValue();
            return a == b ? TRUE : FALSE;
        }
        private static double s_notEqualOpFunc(MathFormulaCalculate calculate)
        {
            var b = calculate.PopValue();
            var a = calculate.PopValue();
            return a != b ? TRUE : FALSE;
        }
        private static double s_greaterOpFunc(MathFormulaCalculate calculate)
        {
            var b = calculate.PopValue();
            var a = calculate.PopValue();
            return a > b ? TRUE : FALSE;
        }
        private static double s_greaterOrEqualOpFunc(MathFormulaCalculate calculate)
        {
            var b = calculate.PopValue();
            var a = calculate.PopValue();
            return a >= b ? TRUE : FALSE;
        }
        private static double s_lessOpFunc(MathFormulaCalculate calculate)
        {
            var b = calculate.PopValue();
            var a = calculate.PopValue();
            return a < b ? TRUE : FALSE;
        }
        private static double s_lessOrEqualOpFunc(MathFormulaCalculate calculate)
        {
            var b = calculate.PopValue();
            var a = calculate.PopValue();
            return a <= b ? TRUE : FALSE;
        }
        #endregion

        /// <summary>
        /// 判断成员变量是否符合规范
        /// </summary>
        /// <param name="memberName"></param>
        /// <returns></returns>
        private static bool s_isValidMemberName(string memberName)
        {
            // String cannot be empty
            if (string.IsNullOrEmpty(memberName))
                return false;

            // The first character must be a letter or underscore
            if (!char.IsLetter(memberName[0]) && memberName[0] != '_')
                return false;

            // Subsequent characters must be letters, digits, or underscores and no spaces allowed
            for (int i = 1; i < memberName.Length; i++)
            {
                char c = memberName[i];
                if (!(char.IsLetterOrDigit(c) || c == '_'))
                    return false;
            }
            return true;
        }
    }

    //类定义
    partial class MathFormulaCalculate
    {
        /// <summary>
        /// 公式的表征类型
        /// </summary>
        enum TokenType
        {
            None = 0,
            /// <summary> 
            /// {
            /// </summary>
            LeftBrace,
            /// <summary>
            /// } 
            /// </summary>
            RightBrace,
            /// <summary>
            /// (
            /// </summary>
            LeftPar,
            /// <summary>
            /// )
            /// </summary>
            RightPar,
            /// <summary> 
            /// , 
            /// </summary>
            Comma,
            /// <summary>
            /// + 
            /// </summary>
            Plus,
            /// <summary> 
            /// -
            /// </summary>
            Minus,
            /// <summary> 
            /// * 
            /// </summary>
            Multiply,
            /// <summary> 
            /// / 
            /// </summary>
            Divide,
            /// <summary>
            /// % 模运算 
            /// </summary>
            Modulo,
            /// <summary> 
            /// && 
            /// </summary>
            And,
            /// <summary> 
            /// || 
            /// </summary>
            Or,
            /// <summary> 
            /// == 
            /// </summary>
            Equal,
            /// <summary> 
            /// !=
            /// </summary>
            NotEqual,
            /// <summary> 
            /// > 
            /// </summary>
            Greater,
            /// <summary> 
            /// >=
            /// </summary>
            GreaterOrEqual,
            /// <summary>  
            /// < 
            /// </summary>
            Less,
            /// <summary> 
            /// <=
            /// </summary>
            LessOrEqual,
            /// <summary>
            /// 负号
            /// </summary>
            Negative,
            /// <summary>
            /// 常量名
            /// </summary>
            ConstName,
            /// <summary>
            /// 变量名
            /// </summary>
            VarName,
            /// <summary>
            /// 方法名
            /// </summary>
            FuncName,
            /// <summary>
            /// 标识符 
            /// </summary>
            Identifier,
            /// <summary>
            /// 数字
            /// </summary>
            Number,
            /// <summary>
            /// 结束
            /// </summary>
            EOF,
        }

        /// <summary>
        /// 公式的表征
        /// </summary>
        readonly struct Token
        {

            public readonly TokenType TokenType;    //标记类型
            public readonly string Lexeme;          //标记值
            public readonly double Number;          //如果类型是数值，则直接转换数值
            public readonly int RowNum;             //当前行数  
            public readonly int ColNum;             //当前列数

            public Token(TokenType tokenType, string lexeme, int row = 1, int col = 1)
            {
                this.TokenType = tokenType;
                this.Lexeme = lexeme;
                this.RowNum = row;
                this.ColNum = col;
                if (tokenType == TokenType.Number)
                    double.TryParse(this.Lexeme, out Number);
                else
                    Number = 0;
            }

            public override string ToString()
            {
                return $"({RowNum},{ColNum}) -> {TokenType}({Lexeme})";
            }
        }

        private class MathFunc
        {
            public readonly int ParamCount;
            public readonly MathFuncDelegate Func;
            public readonly string FuncName;

            public MathFunc(string funcName, MathFuncDelegate func, int argCount)
            {
                ParamCount = argCount;
                Func = func;
                FuncName = funcName;
            }

        }

        private class MathFormulaLexer
        {
            private const char END_CHAR = char.MaxValue;    //结尾字符
            private MathFormulaCalculate _curCalculate;
            private List<Token> _curTokenList;
            private string _curFormula;             //当前公式
            private int _formulaIndex = 0;          //当前索引
            private int _formulaLength = 0;         //公式总长度
            private char _formulaCh = END_CHAR;     //当前解析字符
            private char _tempCh;                   //临时保存字符
            private bool _defineVar;                //是否在声明变量
            private int _beginCompareIndex = -1;    //如果有连续比较，该索引为上一个比较的值所在位置
            private int _curRow = 1;                //当前行
            private int _curCol = 0;                //当前列
            private StringBuilder _cacheBuilder = new StringBuilder();
            internal MathFormulaLexer(MathFormulaCalculate calculate)
            {
                _curCalculate = calculate;
            }
            internal void Lexer()
            {
                _curTokenList = _curCalculate._curTokens;
                _curFormula = _curCalculate._formula;
                _curTokenList.Clear();
                _formulaIndex = 0;
                _formulaCh = END_CHAR;
                _formulaLength = _curFormula.Length;
                _cacheBuilder.Length = 0;
                _defineVar = false;

                for (; _formulaIndex < _formulaLength; ++_formulaIndex)
                {
                    _curCol++;
                    _formulaCh = _curFormula[_formulaIndex];
                    switch (_formulaCh)
                    {
                        case ' ':
                        case '\t':
                            break;
                        case '\r':
                        case '\n':
                            _curRow++;
                            _curCol = 0;
                            break;
                        case '(':
                            AddToken(TokenType.LeftPar);
                            break;
                        case ')':
                            AddToken(TokenType.RightPar);
                            _beginCompareIndex = -1;
                            break;
                        case '{':
                            _defineVar = true;
                            //AddToken(TokenType.LeftBrace);
                            break;
                        case '}':
                            _defineVar = false;
                            //AddToken(TokenType.RightBrace);
                            break;
                        case ',':
                            AddToken(TokenType.Comma);
                            break;
                        case '+':
                            AddToken(TokenType.Plus);
                            break;
                        case '-':
                            ReadMinus();
                            break;
                        case '*':
                            AddToken(TokenType.Multiply);
                            break;
                        case '/':
                            AddToken(TokenType.Divide);
                            break;
                        case '%':
                            AddToken(TokenType.Modulo);
                            break;
                        case '>':
                            ReadGreater();
                            break;
                        case '<':
                            ReadLess();
                            break;
                        case '=':
                            ReadAssign();
                            break;
                        case '&':
                            ReadAnd();
                            break;
                        case '|':
                            ReadOr();
                            break;
                        case '!':
                            ReadNot();
                            break;
                        default:
                            if (char.IsDigit(_formulaCh))
                                ReadNumber();
                            else if (IsIdentifier(_formulaCh))
                                ReadIdentifier();
                            else
                                throw new FormulaLexerException($"Unexpected character [{_formulaCh}] Column:{_formulaIndex}");
                            break;
                    }
                }
                AddToken(TokenType.EOF, "");
            }

            /// <summary>
            /// 读取减号
            /// </summary>
            private void ReadMinus()
            {
                switch (PeekChar())
                {
                    case ' ':
                    case '\t':
                    case '\r':
                    case '\n':
                    case ',':
                    case '+':
                    case '-':
                    case '*':
                    case '/':
                    case '%':
                    case '>':
                    case '<':
                    case '=':
                    case '&':
                    case '|':
                    case '!':
                        break;
                    default:
                        Token token = PeekToken();
                        if (token.TokenType != TokenType.Identifier &&
                            token.TokenType != TokenType.VarName &&
                            token.TokenType != TokenType.Number &&
                            token.TokenType != TokenType.RightPar)
                        {
                            AddToken(TokenType.Negative);
                            return;
                        }
                        break;
                }
                AddToken(TokenType.Minus);
            }

            /// <summary>
            /// 读取 >
            /// </summary>
            private void ReadGreater()
            {
                _tempCh = ReadChar();
                if (_tempCh == '=')
                {
                    AddToken(TokenType.GreaterOrEqual, ">=");
                }
                else
                {
                    AddToken(TokenType.Greater, ">");
                    UndoChar();
                }
                CheckCompare();
            }
            /// <summary>
            /// 读取 <
            /// </summary>
            private void ReadLess()
            {
                _tempCh = ReadChar();
                if (_tempCh == '=')
                {
                    AddToken(TokenType.LessOrEqual, "<=");
                }
                else
                {
                    AddToken(TokenType.Less, "<");
                    UndoChar();
                }
                CheckCompare();
            }
            private void CheckCompare()
            {
                if (_beginCompareIndex == -1)
                {
                    _beginCompareIndex = this._curTokenList.Count;
                }
                else
                {
                    var token = this._curTokenList[this._curTokenList.Count - 1];
                    this._curTokenList.RemoveAt(this._curTokenList.Count - 1);
                    int end = this._curTokenList.Count;
                    this._curTokenList.Add(new Token(TokenType.And, "&&"));
                    for (int i = _beginCompareIndex; i < end; i++)
                    {
                        this._curTokenList.Add(this._curTokenList[i]);
                    }
                    this._curTokenList.Add(token);
                    _beginCompareIndex = -1;
                }
            }
            /// <summary>
            /// 读取 = 
            /// </summary>
            void ReadAssign()
            {
                _tempCh = ReadChar();
                if (_tempCh == '=')
                {
                    AddToken(TokenType.Equal, "==");
                }
                else
                {
                    throw new FormulaLexerException($"Unexpected character [{_formulaCh}] Column:{_formulaIndex}");
                }
            }
            /// <summary> 
            /// 读取 ! 
            /// </summary>
            void ReadNot()
            {
                _tempCh = ReadChar();
                if (_tempCh == '=')
                {
                    AddToken(TokenType.NotEqual, "!=");
                }
                else
                {
                    throw new FormulaLexerException($"Unexpected character [{_formulaCh}] Column:{_formulaIndex}");
                }
            }
            /// <summary> 
            /// 读取 & 
            /// </summary>
            void ReadAnd()
            {
                _tempCh = ReadChar();
                if (_tempCh == '&')
                {
                    AddToken(TokenType.And, "&&");
                    _beginCompareIndex = -1;
                }
                else
                {
                    throw new FormulaLexerException($"Unexpected character [{_formulaCh}] Column:{_formulaIndex}");
                }
            }
            /// <summary>
            /// 读取 | 
            /// </summary>
            void ReadOr()
            {
                _tempCh = ReadChar();
                if (_tempCh == '|')
                {
                    AddToken(TokenType.Or, "||");
                    _beginCompareIndex = -1;
                }
                else
                {
                    throw new FormulaLexerException($"Unexpected character [{_formulaCh}] Column:{_formulaIndex}");
                }
            }
            /// <summary>
            /// 读取数字
            /// </summary>
            private void ReadNumber()
            {
                _cacheBuilder.Append(_formulaCh);
                var hasDot = false;
                do
                {
                    _formulaCh = ReadChar();
                    if (char.IsDigit(_formulaCh))
                    {
                        _cacheBuilder.Append(_formulaCh);
                    }
                    else if (_formulaCh == '.')
                    {
                        if (hasDot)
                            throw new FormulaLexerException($"Unexpected character [{_formulaCh}] Column:{_formulaIndex}");
                        hasDot = true;
                        _cacheBuilder.Append(_formulaCh);
                    }
                    else
                    {

                        AddToken(TokenType.Number, _cacheBuilder.ToString());
                        UndoChar();
                        break;
                    }
                } while (true);
            }

            /// <summary> 
            /// 读取关键字 
            /// </summary>
            private void ReadIdentifier()
            {
                _cacheBuilder.Append(_formulaCh);
                do
                {
                    _tempCh = ReadChar();
                    if (IsIdentifier(_tempCh))
                    {
                        _cacheBuilder.Append(_tempCh);
                    }
                    else
                    {
                        UndoChar();
                        break;
                    }
                } while (true);
                string value = _cacheBuilder.ToString();
                if (_defineVar)
                    AddToken(TokenType.VarName, value);
                else
                    AddToken(TokenType.Identifier, value);
            }
            private bool IsIdentifier(char ch)
            {
                return (ch == '_' || char.IsLetterOrDigit(ch));
            }
            private void AddToken(TokenType tokenType)
            {
                AddToken(tokenType, _formulaCh.ToString());
            }
            private void AddToken(TokenType type, string lexeme)
            {
                _curTokenList.Add(new Token(type, lexeme, _curRow, _curCol));
                _cacheBuilder.Length = 0;
            }
            private char ReadChar()
            {
                ++_formulaIndex;
                if (_formulaIndex < _formulaLength)
                {
                    return _curFormula[_formulaIndex];
                }
                else if (_formulaIndex == _formulaLength)
                {
                    return END_CHAR;
                }
                throw new FormulaLexerException("End of source reached.");
            }
            private char PeekChar()
            {
                int temp = _formulaIndex + 1;
                if (temp < _formulaLength)
                {
                    return _curFormula[temp];
                }
                else if (temp == _formulaLength)
                {
                    return END_CHAR;
                }
                throw new FormulaLexerException("End of source reached.");
            }
            private void UndoChar()
            {
                if (_formulaIndex == 0)
                    throw new FormulaLexerException("Cannot undo char beyond start of source.");
                --_formulaIndex;
            }

            private Token PeekToken()
            {
                return _curTokenList.Count == 0 ? default : _curTokenList[_curTokenList.Count - 1];
            }
        }

        private class MathFormulaParser
        {

            private readonly MathFormulaCalculate _curCalculate;
            private int _tokenIndex = 0;        //当前token索引
            private int _tokenLength = 0;       //当前token长度
            private Token _curToken;        //当前token,
            private List<Token> _curTokenList;
            private Stack<int> _argCounts = new Stack<int>();  // 用于存储每个函数的参数计数
            internal MathFormulaParser(MathFormulaCalculate calculate)
            {
                _curCalculate = calculate;
            }

            public void Parse()
            {
                _curTokenList = _curCalculate._curTokens;
                var output = new List<Token>();
                var operators = new Stack<Token>();
                var negatives = new Stack<Token>();
                _argCounts.Clear();
                _tokenIndex = 0;
                _tokenLength = _curTokenList.Count;
                for (; _tokenIndex < _tokenLength; ++_tokenIndex)
                {
                    _curToken = _curTokenList[_tokenIndex];
                    switch (_curToken.TokenType)
                    {
                        case TokenType.LeftBrace:
                        case TokenType.RightBrace:
                        case TokenType.EOF:
                            break;
                        case TokenType.LeftPar:
                            operators.Push(_curToken);
                            break;
                        case TokenType.RightPar:

                            while (operators.Count > 0 && operators.Peek().TokenType != TokenType.LeftPar)
                            {
                                output.Add(operators.Pop());
                            }
                            operators.Pop();  // 弹出左括号

                            if (operators.Count > 0 && operators.Peek().TokenType == TokenType.FuncName)
                            {
                                Token token = operators.Pop();
                                if (!s_funcDic.ContainsKey(token.Lexeme))
                                    throw new FormulaParserException($"函数 {token.Lexeme} 不被支持。");
                                MathFunc mathFunc = s_funcDic[token.Lexeme];
                                // 检查该函数所期望的参数个数是否与之前记录的个数匹配
                                int expectedArgs = mathFunc.ParamCount;
                                int actualArgs = _argCounts.Pop() + 1;  // 加 1 是因为最后一个参数不由逗号分隔

                                if (expectedArgs != actualArgs)
                                {
                                    throw new FormulaParserException($"函数 {token.Lexeme} 的参数数量不正确。期望 {expectedArgs} 个参数，但得到 {actualArgs} 个。");
                                }

                                // 当我们确定参数数量正确时，把函数放进输出队列
                                output.Add(token);
                            }
                            if (operators.Count > 0 && operators.Peek().TokenType == TokenType.Negative)
                                output.Add(operators.Pop());
                            break;
                        case TokenType.Comma:
                            // 函数参数分隔符 ','
                            while (operators.Count > 0 && operators.Peek().TokenType != TokenType.LeftPar)
                            {
                                output.Add(operators.Pop());
                            }
                            // 增加当前函数的参数计数
                            if (_argCounts.Count > 0)
                            {
                                int currentCount = _argCounts.Pop();
                                _argCounts.Push(currentCount + 1);  // 增加参数计数
                            }
                            break;
                        case TokenType.Negative:
                            operators.Push(_curToken);
                            break;
                        case TokenType.Plus:
                        case TokenType.Minus:
                        case TokenType.Multiply:
                        case TokenType.Divide:
                        case TokenType.Modulo:
                        case TokenType.Equal:
                        case TokenType.NotEqual:
                        case TokenType.Less:
                        case TokenType.LessOrEqual:
                        case TokenType.Greater:
                        case TokenType.GreaterOrEqual:
                        case TokenType.And:
                        case TokenType.Or:
                            while (operators.Count > 0 && IsOperator(operators.Peek().TokenType) && GetPriority(_curToken.TokenType) <= GetPriority(operators.Peek().TokenType))
                            {
                                output.Add(operators.Pop());
                            }
                            operators.Push(_curToken);
                            break;
                        case TokenType.ConstName:
                        case TokenType.VarName:
                        case TokenType.Number:
                            output.Add(_curToken);
                            if (operators.Count > 0 && operators.Peek().TokenType == TokenType.Negative)
                                output.Add(operators.Pop());
                            //HandleChainedComparison(ref output, ref operators);  // 新增处理链式比较方法调用
                            break;
                        case TokenType.FuncName:
                            operators.Push(_curToken);
                            _argCounts.Push(0);
                            break;
                        case TokenType.Identifier:
                            if (PeekToken().TokenType == TokenType.LeftPar)
                            {
                                //方法
                                operators.Push(new Token(TokenType.FuncName, _curToken.Lexeme));
                                _argCounts.Push(0);
                            }
                            else
                            {
                                //常量
                                if (!s_constDic.ContainsKey(_curToken.Lexeme))
                                    throw new FormulaParserException($"常量 {_curToken.Lexeme} 不被支持。");
                                output.Add(new Token(TokenType.ConstName, _curToken.Lexeme));
                                if (operators.Count > 0 && operators.Peek().TokenType == TokenType.Negative)
                                    output.Add(operators.Pop());
                            }
                            break;
                        default:
                            throw new FormulaParserException($"函数 {_curToken.Lexeme} 暂不支持。");
                    }
                }

                // 把剩下的操作符弹到输出列表
                while (operators.Count > 0)
                    output.Add(operators.Pop());
                _curTokenList.Clear();
                _curTokenList.AddRange(output);
            }
            private void HandleChainedComparison(ref List<Token> output, ref Stack<Token> operators)
            {
                while (_tokenIndex + 2 < _tokenLength)
                {
                    int peekIndex = _tokenIndex + 1;
                    Token peekOperator = _curTokenList[peekIndex];
                    Token peekNext = _curTokenList[peekIndex + 1];

                    if (!IsComparisonOperator(peekOperator.TokenType))
                    {
                        break;
                    }

                    output.Add(peekNext);
                    output.Add(peekOperator);

                    // Insert logical AND between chained comparisons
                    if (_tokenIndex + 3 < _tokenLength && IsComparisonOperator(_curTokenList[peekIndex + 2].TokenType))
                    {
                        operators.Push(new Token(TokenType.And, "&&"));
                    }

                    _tokenIndex += 2;
                }
            }

            private bool IsComparisonOperator(TokenType type)
            {
                return type == TokenType.Less || type == TokenType.LessOrEqual ||
                       type == TokenType.Greater || type == TokenType.GreaterOrEqual ||
                       type == TokenType.Equal || type == TokenType.NotEqual;
            }
            private Token PeekToken()
            {
                int temp = _tokenIndex + 1;
                if (temp < _tokenLength)
                    return _curTokenList[temp];
                throw new FormulaParserException("End of source reached.");
            }

            // 检查是否为运算符
            private bool IsOperator(TokenType tokenType)
            {
                return s_tokenWightDic.ContainsKey(tokenType);
            }
            /// <summary>
            /// 获取token的优先级
            /// </summary>
            /// <param name="tokenType"></param>
            /// <returns></returns>
            private int GetPriority(TokenType tokenType)
            {
                if (s_tokenWightDic.ContainsKey(tokenType))
                    return s_tokenWightDic[tokenType];
                return -1;
            }
        }


        //词法分析异常
        public class FormulaLexerException : Exception
        {
            public FormulaLexerException(string message) : base(message) { }
        }
        //解析语法异常
        public class FormulaParserException : Exception
        {
            public FormulaParserException(string message) : base(message) { }
        }
        //计算错误
        public class FormulaCalculateException : Exception
        {
            public FormulaCalculateException(string message) : base(message) { }
        }
        /// <summary>
        /// 成员变量错误
        /// </summary>
        public class FormulaInvalidMemberException : Exception
        {
            public FormulaInvalidMemberException(string message) : base(message) { }
        }
    }
    /// <summary>
    /// 数学方法
    /// </summary>
    /// <param name="calculate"></param>
    /// <returns></returns>
    public delegate double MathFuncDelegate(MathFormulaCalculate calculate);
}
