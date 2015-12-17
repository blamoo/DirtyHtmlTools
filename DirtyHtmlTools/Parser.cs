using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DirtyHtmlTools
{
    public enum ParserState
    {
        Content,
        Tag,
    }

    public class Parser
    {
        public Element[] Parse(string data)
        {
            Lexer lexer = new Lexer();
            return Parse(lexer.Parse(data));
        }

        public Element[] Parse(Token[] tokens)
        {
            Tag root = new Tag() { Name = "<ROOT>" };
            Element current = root;

            Stack<Element> stack = new Stack<Element>();
            stack.Push(current);
            ParserState state = ParserState.Content;

            int i = 0;

            while (i < tokens.Length)
            {
                switch (state)
                {
                    case ParserState.Content:
                        switch (tokens[i].Type)
                        {
                            case TokenType.Content:
                                ((Tag)current).Children.Add(new Content() { Value = tokens[i].Value });
                                i++;
                                break;
                            case TokenType.TagStart:
                                current = new Tag() { Name = tokens[i].Value };
                                stack.Push(current);
                                i++;
                                state = ParserState.Tag;
                                break;
                            case TokenType.TagEnd:
                                if (stack.Peek() == root)
                                    throw new ParseException("wrong close tag");

                                Element finished = stack.Pop();
                                current = stack.Peek();
                                ((Tag)current).Children.Add(finished);
                                i++;
                                state = ParserState.Content;
                                break;
                            case TokenType.Comment:
                                i++;
                                state = ParserState.Content;
                                break;
                            default:
                                throw new ParseException("unexpected token in content");
                        }
                        break;
                    case ParserState.Tag:
                        bool isShortTag = readInsideTag(tokens, ref i, ((Tag)current));

                        if (isShortTag)
                        {
                            Element finished = stack.Pop();
                            current = stack.Peek();
                            ((Tag)current).Children.Add(finished);
                            state = ParserState.Content;
                            break;
                        }
                        else
                        {
                            state = ParserState.Content;
                            break;
                        }
                }

            }
            if (stack.Count != 1)
                throw new ParseException("unexpected eof");

            return ((Tag)current).Children.ToArray();
        }

        private bool readInsideTag(Token[] tokens, ref int i, Tag current)
        {
            while (true)
            {
                if (tokens[i].Type == TokenType.TagClose)
                {
                    i++;
                    return false;
                }
                if (tokens[i].Type == TokenType.ShortTagClose)
                {
                    i++;
                    return true;
                }

                if (tokens[i].Type == TokenType.AttributeName)
                {
                    if (tokens[i + 1].Type == TokenType.AttributeEqual)
                    {
                        if (tokens[i + 2].Type == TokenType.AttributeValue)
                        {
                            current.Attributes.Add(tokens[i].Value, tokens[i + 2].Value);
                            i += 3;
                        }
                        else
                        {
                            throw new ParseException("unexpected stuff after equal sign");
                        }
                    }
                    else
                    {
                        current.Attributes.Add(tokens[i].Value, string.Empty);
                        i++;
                    }

                    continue;
                }

                throw new ParseException("unexpected thing inside tag");
            }
        }
    }

    public class ParseException : Exception
    {
        public ParseException(string message)
            : base(message)
        {
        }
    }

    public class Element
    {
        public ElementType Type { get; protected set; }
    }

    public class Content : Element
    {
        public string Value;

        public Content()
        {
            Type = ElementType.Content;
        }

        public override string ToString()
        {
            return String.Format("C: {0}", Value);
        }
    }

    public class Tag : Element
    {
        public string Name;
        public List<Element> Children = new List<Element>();
        public Dictionary<string, string> Attributes = new Dictionary<string, string>();

        public Tag()
        {
            Type = ElementType.Tag;
        }

        public override string ToString()
        {
            return String.Format("T: {0}", Name);
        }
    }

    public enum ElementType
    {
        Tag,
        Content,
    }
}
