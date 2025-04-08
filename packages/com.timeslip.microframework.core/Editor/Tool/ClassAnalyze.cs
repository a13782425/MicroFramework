using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFramework.Core.Editor
{
    /// <summary>
    /// 类分析器
    /// </summary>
    internal static class ClassAnalyze
    {
        /// <summary>
        /// 分析代码并构建语法树
        /// </summary>
        /// <param name="code">代码</param>
        /// <returns></returns>
        public static SyntaxTree Analyze(string code)
        {
            var tokenizer = new Tokenizer(code);
            var tokens = tokenizer.Tokenize();
            var parser = new Parser(tokens);
            return parser.Parse();
        }

        // Token 类型
        public enum TokenType
        {
            Keyword,    // 关键字（如 namespace、class、interface、struct、enum）
            Identifier, // 标识符（如类名、命名空间名）
            Symbol,     // 符号（如 {、}）
            EOF         // 文件结束
        }

        // 关键字枚举
        public enum Keyword
        {
            Namespace,
            Class,
            Interface,
            Struct,
            Enum,
            Partial,
            Abstract,
            Static
        }

        // Token 结构
        public class Token
        {
            public TokenType Type { get; set; }
            public string Value { get; set; }
            public Keyword? Keyword { get; set; } // 如果是关键字，存储对应的枚举值

            public Token(TokenType type, string value, Keyword? keyword = null)
            {
                Type = type;
                Value = value;
                Keyword = keyword;
            }

            public override string ToString()
            {
                return $"{Type}: {Value}";
            }
        }

        // 词法分析器
        public class Tokenizer
        {
            private readonly string _code;
            private int _position;

            public Tokenizer(string code)
            {
                _code = code;
                _position = 0;
            }

            // 将代码拆分为 Token 流
            public List<Token> Tokenize()
            {
                var tokens = new List<Token>();

                while (_position < _code.Length)
                {
                    char currentChar = _code[_position];

                    // 跳过空白字符
                    if (char.IsWhiteSpace(currentChar))
                    {
                        _position++;
                        continue;
                    }

                    // 处理关键字和标识符
                    if (char.IsLetter(currentChar))
                    {
                        string word = ReadWord();
                        if (TryGetKeyword(word, out var keyword))
                        {
                            tokens.Add(new Token(TokenType.Keyword, word, keyword));
                        }
                        else
                        {
                            tokens.Add(new Token(TokenType.Identifier, word));
                        }
                        continue;
                    }

                    // 处理符号
                    if (currentChar == '{' || currentChar == '}')
                    {
                        tokens.Add(new Token(TokenType.Symbol, currentChar.ToString()));
                        _position++;
                        continue;
                    }

                    // 其他字符（如分号、逗号等）
                    _position++;
                }

                // 添加文件结束标记
                tokens.Add(new Token(TokenType.EOF, string.Empty));
                return tokens;
            }

            // 读取一个单词（关键字或标识符）
            private string ReadWord()
            {
                int start = _position;
                while (_position < _code.Length && char.IsLetterOrDigit(_code[_position]))
                {
                    _position++;
                }
                return _code.Substring(start, _position - start);
            }

            // 判断是否为关键字，并返回对应的枚举值
            private bool TryGetKeyword(string word, out Keyword keyword)
            {
                switch (word)
                {
                    case "namespace":
                        keyword = Keyword.Namespace;
                        return true;
                    case "class":
                        keyword = Keyword.Class;
                        return true;
                    case "interface":
                        keyword = Keyword.Interface;
                        return true;
                    case "struct":
                        keyword = Keyword.Struct;
                        return true;
                    case "enum":
                        keyword = Keyword.Enum;
                        return true;
                    case "partial":
                        keyword = Keyword.Partial;
                        return true;
                    case "abstract":
                        keyword = Keyword.Abstract;
                        return true;
                    case "static":
                        keyword = Keyword.Static;
                        return true;
                    default:
                        keyword = default;
                        return false;
                }
            }
        }

        // 类型种类枚举
        public enum TypeKind
        {
            Class,
            Interface,
            Struct,
            Enum
        }

        // 语法分析器
        public class Parser
        {
            private readonly List<Token> _tokens;
            private int _position;

            public Parser(List<Token> tokens)
            {
                _tokens = tokens;
                _position = 0;
            }

            // 解析 Token 流并构建语法树
            public SyntaxTree Parse()
            {
                var syntaxTree = new SyntaxTree();
                var namespaceStack = new Stack<NamespaceNode>();
                var typeStack = new Stack<TypeNode>();
                var currentNamespace = new NamespaceNode { Name = string.Empty };
                syntaxTree.Namespaces.Add(currentNamespace);
                namespaceStack.Push(currentNamespace);

                while (_position < _tokens.Count)
                {
                    var token = _tokens[_position];

                    // 检测命名空间
                    if (token.Type == TokenType.Keyword && token.Keyword == Keyword.Namespace)
                    {
                        _position++;
                        var namespaceName = _tokens[_position].Value;
                        currentNamespace = new NamespaceNode { Name = namespaceName };
                        syntaxTree.Namespaces.Add(currentNamespace);
                        namespaceStack.Push(currentNamespace);
                        _position++;
                        continue;
                    }

                    // 检测类型定义（类、接口、结构体、枚举）
                    if (token.Type == TokenType.Keyword && (token.Keyword == Keyword.Class || token.Keyword == Keyword.Interface || token.Keyword == Keyword.Struct || token.Keyword == Keyword.Enum))
                    {
                        _position++;
                        var typeNode = ParseType(token.Keyword.Value);
                        if (typeStack.Count > 0)
                        {
                            typeStack.Peek().NestedTypes.Add(typeNode); // 嵌套类型
                        }
                        else
                        {
                            currentNamespace.Types.Add(typeNode); // 顶级类型
                        }
                        typeStack.Push(typeNode);
                        continue;
                    }

                    // 检测符号
                    if (token.Type == TokenType.Symbol)
                    {
                        if (token.Value == "{")
                        {
                            _position++;
                            continue;
                        }
                        if (token.Value == "}")
                        {
                            _position++;
                            if (namespaceStack.Count > 1)
                            {
                                namespaceStack.Pop(); // 退出当前命名空间
                                currentNamespace = namespaceStack.Peek();
                            }
                            if (typeStack.Count > 0)
                            {
                                typeStack.Pop(); // 退出当前类型
                            }
                            continue;
                        }
                    }

                    _position++;
                }

                return syntaxTree;
            }

            // 解析类型定义
            private TypeNode ParseType(Keyword typeKind)
            {
                var typeNode = new TypeNode { TypeKind = (TypeKind)typeKind };

                // 提取类型名
                typeNode.Name = _tokens[_position].Value;
                _position++;

                // 检测修饰符
                while (_position < _tokens.Count && _tokens[_position].Type == TokenType.Keyword)
                {
                    var modifier = _tokens[_position].Keyword;
                    if (modifier == Keyword.Partial)
                    {
                        typeNode.IsPartial = true;
                    }
                    else if (modifier == Keyword.Abstract)
                    {
                        typeNode.IsAbstract = true;
                    }
                    else if (modifier == Keyword.Static)
                    {
                        typeNode.IsStatic = true;
                    }
                    _position++;
                }

                return typeNode;
            }
        }

        // 语法树
        public class SyntaxTree
        {
            public List<NamespaceNode> Namespaces { get; } = new List<NamespaceNode>();
        }

        // 命名空间节点
        public class NamespaceNode
        {
            public string Name { get; set; }
            public List<TypeNode> Types { get; } = new List<TypeNode>();
        }

        // 类型节点
        public class TypeNode
        {
            public string Name { get; set; }
            public TypeKind TypeKind { get; set; } // 类型种类（类、接口、结构体、枚举）
            public bool IsPartial { get; set; }
            public bool IsAbstract { get; set; }
            public bool IsStatic { get; set; }
            public bool IsGeneric { get; set; }
            public List<TypeNode> NestedTypes { get; } = new List<TypeNode>();
        }
    }
}
