using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DirtyHtmlTools
{
    public class Lexer
    {
        public Token[] Parse(string test)
        {
            var chars = test.ToCharArray();
            var tokens = new List<Token>();

            int i = 0;
            var state = LexerState.Identify;

            while (i < chars.Length)
            {
                int lastValid = i;
                try
                {

                    switch (state)
                    {
                        case LexerState.Identify:
                            if (chars[i] == '<')
                            {
                                if (chars[i + 1] == '!')
                                {
                                    state = LexerState.Comment;
                                    break;
                                }

                                if (chars[i + 1] == '/')
                                {
                                    state = LexerState.TagEnd;
                                    break;
                                }

                                if (char.IsLetter(chars[i + 1]))
                                {
                                    state = LexerState.TagStart;
                                    break;
                                }

                                state = LexerState.Unknown;
                                break;
                            }

                            state = LexerState.Content;
                            break;

                        case LexerState.IdentifyInsideTag:
                            {
                                SkipWhitespace(chars, ref i);
                                int start = i;

                                if (char.IsLetterOrDigit(chars[i]))
                                {
                                    tokens.Add(new Token(TokenType.AttributeName, ReadName(chars, ref i), start));
                                    state = LexerState.IdentifyInsideTag;
                                    break;
                                }

                                if (chars[i] == '=')
                                {
                                    i++; // =
                                    tokens.Add(new Token(TokenType.AttributeEqual, "=", start));
                                    state = LexerState.IdentifyInsideTag;
                                    break;
                                }

                                if (chars[i] == '\'')
                                {
                                    ++i; // '
                                    string tmp = HttpUtility.HtmlDecode(ReadUntil(chars, ref i, '\''));
                                    tokens.Add(new Token(TokenType.AttributeValue, tmp, start));
                                    ++i; // '
                                    state = LexerState.IdentifyInsideTag;
                                    break;
                                }

                                if (chars[i] == '"')
                                {
                                    ++i; // "
                                    string tmp = HttpUtility.HtmlDecode(ReadUntil(chars, ref i, '"'));
                                    tokens.Add(new Token(TokenType.AttributeValue, tmp, start));
                                    ++i; // "
                                    state = LexerState.IdentifyInsideTag;
                                    break;
                                }

                                if (chars[i] == '/' && chars[i + 1] == '>')
                                {
                                    i += 2; // />
                                    tokens.Add(new Token(TokenType.ShortTagClose, "", start));
                                    state = LexerState.Identify;
                                    break;
                                }

                                if (chars[i] == '>')
                                {
                                    ++i; // >
                                    tokens.Add(new Token(TokenType.TagClose, String.Empty, start));
                                    state = LexerState.Identify;
                                    break;
                                }

                                tokens.Add(new Token(TokenType.InvalidInsideTag, chars[i].ToString(), start));
                                i++; // <unknown>
                                state = LexerState.IdentifyInsideTag;
                                break;
                            }
                        case LexerState.TagEnd:
                            {
                                int start = i;
                                i += 2; // </

                                tokens.Add(new Token(TokenType.TagEnd, ReadName(chars, ref i), start));
                                SkipWhitespace(chars, ref i);
                                i++; // >;

                                state = LexerState.Identify;
                                break;
                            }
                        case LexerState.Comment:
                            {
                                int start = i;
                                i += 4; // <!--
                                tokens.Add(new Token(TokenType.Comment, ReadUntilSeq(chars, ref i, "-->".ToCharArray()), start));
                                i += 3; // -->

                                state = LexerState.Identify;
                                break;
                            }
                        case LexerState.TagStart:
                            {
                                int start = i;
                                i++; // <
                                string tagName = ReadName(chars, ref i);
                                tokens.Add(new Token(TokenType.TagStart, tagName, start));

                                state = LexerState.IdentifyInsideTag;
                                break;
                            }

                        case LexerState.Content:
                            {
                                int start = i;
                                tokens.Add(new Token(TokenType.Content, ReadContent(chars, ref i), start));
                                state = LexerState.Identify;
                                break;
                            }

                        case LexerState.Unknown:
                            {
                                int start = i;
                                tokens.Add(new Token(TokenType.Incomplete, ReadToEnd(chars, ref i), start));
                                state = LexerState.Identify;
                                break;
                            }
                    }

                }
                catch (IndexOutOfRangeException)
                {
                    i = lastValid;
                    tokens.Add(new Token(TokenType.Incomplete, ReadToEnd(chars, ref i), i));
                    break;
                }
            }

            return tokens.ToArray();
        }

        private string ReadToEnd(char[] chars, ref int i)
        {
            StringBuilder buffer = new StringBuilder();

            while (i < chars.Length)
            {
                buffer.Append(chars[i]);
                i++;
            }

            return buffer.ToString();
        }

        private string ReadUntilSeq(char[] chars, ref int i, char[] stop)
        {
            StringBuilder buffer = new StringBuilder();

            while (true)
            {
                bool match = true;

                foreach (char c in stop)
                {
                    if (c == chars[i])
                    {
                        i++;
                    }
                    else
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                    break;
                else
                    buffer.Append(chars[i]);

                i++;
            }

            i -= stop.Length;

            return buffer.ToString();
        }

        private string ReadUntil(char[] chars, ref int i, char stop)
        {
            StringBuilder buffer = new StringBuilder();

            while (chars[i] != stop)
            {
                buffer.Append(chars[i]);
                i++;
            }

            return buffer.ToString();
        }

        private string ReadContent(char[] chars, ref int i)
        {
            StringBuilder buffer = new StringBuilder();

            while (true)
            {
                if (chars[i] == '<')
                    break;
                else if (chars[i] == '&')
                {
                    string entity;
                    if (TryReadEntity(chars, ref i, out entity))
                    {
                        buffer.Append(entity);
                    }
                }
                else
                {
                    buffer.Append(chars[i]);
                }
                i++;
            }

            return buffer.ToString();
        }

        private void SkipWhitespace(char[] chars, ref int i)
        {
            while (char.IsWhiteSpace(chars[i]))
                i++;
        }

        private string ReadName(char[] chars, ref int i)
        {
            StringBuilder buffer = new StringBuilder();

            while (
                char.IsLetterOrDigit(chars[i]) ||
                chars[i] == ':' ||
                chars[i] == '-' ||
                chars[i] == '_' ||
                chars[i] == '.')
            {
                buffer.Append(chars[i]);
                i++;
            }
            return buffer.ToString();
        }

        private bool TryReadEntity(char[] chars, ref int i, out string ret)
        {
            StringBuilder buffer = new StringBuilder();

            while (true)
            {
                buffer.Append(chars[i]);

                if (chars[i] == ';')
                    break;
                i++;
            }

            ret = HttpUtility.HtmlDecode(buffer.ToString());
            return true;
        }
    }

    public enum TokenType
    {
        TagStart,
        TagEnd,
        TagClose,
        ShortTagClose,
        AttributeName,
        AttributeEqual,
        AttributeValue,
        Content,
        Comment,
        Incomplete,
        InvalidInsideTag,
    }

    public enum LexerState
    {
        Identify,
        Comment,
        TagEnd,
        TagStart,
        Content,
        Unknown,
        IdentifyInsideTag,
    }

    public struct Token
    {
        public TokenType Type { get; private set; }

        public string Value { get; private set; }

        public int Position { get; private set; }

        public Token(TokenType type, string value, int position)
        {
            Type = type;
            Value = value;
            Position = position;
        }

        public override string ToString()
        {
            return String.Format("{0}: {1}", Type, Value);
        }
    }
}
