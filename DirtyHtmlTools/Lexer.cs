using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirtyHtmlTools
{
	public class Lexer
	{
		public Token[] Parse (string test)
		{
			var chars = test.ToCharArray ();
			var tokens = new List<Token> ();

			int i = 0;
			var state = State.Identify;

			while (i < chars.Length) {
				int lastValid = i;
				try {

					switch (state) {
					case State.Identify:
						if (chars [i] == '<') {
							if (chars [i + 1] == '!') {
								state = State.Comment;
								break;
							}

							if (chars [i + 1] == '/') {
								state = State.TagEnd;
								break;
							}

							if (char.IsLetter (chars [i + 1])) {
								state = State.TagStart;
								break;
							}

							state = State.Unknown;
							break;
						}

						state = State.Content;
						break;

					case State.IdentifyInsideTag:
						skipWhitespace (chars, ref i);

						if (char.IsLetterOrDigit (chars [i])) {
							tokens.Add (new Token (TokenType.AttributeName, readName (chars, ref i), i));
							state = State.IdentifyInsideTag;
							break;
						}

						if (chars [i] == '=') {
							i++; // =
							tokens.Add (new Token (TokenType.AttributeEqual, "=", i));
							state = State.IdentifyInsideTag;
							break;
						}

						if (chars [i] == '\'') {
							++i; // '
							tokens.Add (new Token (TokenType.AttributeValue, readUntil (chars, ref i, '\''), i));
							++i; // '
							state = State.IdentifyInsideTag;
							break;
						}

						if (chars [i] == '"') {
							++i; // "
							tokens.Add (new Token (TokenType.AttributeValue, readUntil (chars, ref i, '"'), i));
							++i; // "
							state = State.IdentifyInsideTag;
							break;
						}

						if (chars [i] == '/' && chars [i + 1] == '>') {
							i += 2; // />
							tokens.Add (new Token (TokenType.ShortTagClose, "", i));
							state = State.Identify;
							break;
						}

						if (chars [i] == '>') {
							++i; // >
							tokens.Add (new Token (TokenType.TagClose, String.Empty, i));
							state = State.Identify;
							break;
						}

						tokens.Add (new Token (TokenType.InvalidInsideTag, chars [i].ToString (), i));
						i++; // <unknown>
						state = State.IdentifyInsideTag;
						break;
					case State.TagEnd:
						i += 2; // </

						tokens.Add (new Token (TokenType.TagEnd, readName (chars, ref i), i));
						skipWhitespace (chars, ref i);
						i++; // >;

						state = State.Identify;
						break;
					case State.Comment:
						i += 4; // <!--
						tokens.Add (new Token (TokenType.Comment, readUntilSeq (chars, ref i, "-->".ToCharArray ()), i));
						i += 3; // -->

						state = State.Identify;
						break;
					case State.TagStart:
						i++; // <
						string tagName = readName (chars, ref i);
						tokens.Add (new Token (TokenType.TagStart, tagName, i));

						state = State.IdentifyInsideTag;
						break;

					case State.Content:
						tokens.Add (new Token (TokenType.Content, readContent (chars, ref i), i));
						state = State.Identify;
						break;

					case State.Unknown:
						tokens.Add (new Token (TokenType.Incomplete, readToEnd (chars, ref i), i));
						state = State.Identify;
						break;
					}

				} catch (IndexOutOfRangeException) {
					i = lastValid;
					tokens.Add (new Token (TokenType.Incomplete, readToEnd (chars, ref i), i));
					break;
				}
			}

			return tokens.ToArray ();
		}

		private string readToEnd (char[] chars, ref int i)
		{
			StringBuilder buffer = new StringBuilder ();

			while (i < chars.Length) {
				buffer.Append (chars [i]);
				i++;
			}

			return buffer.ToString ();
		}

		private string readUntilSeq (char[] chars, ref int i, char[] stop)
		{
			StringBuilder buffer = new StringBuilder ();

			while (true) {
				bool match = true;

				foreach (char c in stop) {
					if (c == chars [i]) {
						i++;
					} else {
						match = false;
						break;
					}
				}

				if (match)
					break;
				else
					buffer.Append (chars [i]);

				i++;
			}

			i -= stop.Length;

			return buffer.ToString ();
		}

		private string readUntil (char[] chars, ref int i, char stop)
		{
			StringBuilder buffer = new StringBuilder ();

			while (chars [i] != stop) {
				buffer.Append (chars [i]);
				i++;
			}

			return buffer.ToString ();
		}

		private string readContent (char[] chars, ref int i)
		{
			StringBuilder buffer = new StringBuilder ();

			while (true) {
				if (chars [i] == '<')
					break;
				else if (chars [i] == '&') {
					string entity;
					if (tryReadEntity (chars, ref i, out entity)) {
						buffer.Append (entity);
					}
				} else {
					buffer.Append (chars [i]);
				}
				i++;
			}

			return buffer.ToString ();
		}

		private void skipWhitespace (char[] chars, ref int i)
		{
			while (char.IsWhiteSpace (chars [i]))
				i++;
		}

		private string readName (char[] chars, ref int i)
		{
			StringBuilder buffer = new StringBuilder ();

			while (char.IsLetterOrDigit (chars [i])) {
				buffer.Append (chars [i]);
				i++;
			}
			return buffer.ToString ();
		}

		private bool tryReadEntity (char[] chars, ref int i, out string ret)
		{
			StringBuilder buffer = new StringBuilder ();

			while (true) {
				buffer.Append (chars [i]);

				if (chars [i] == ';')
					break;
				i++;
			}

			ret = buffer.ToString ();
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

	public enum State
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
		public TokenType Type{ get; private set; }

		public string Value{ get; private set; }

		public int Position{ get; private set; }

		public Token (TokenType type, string value, int position)
		{
			Type = type;
			Value = value;
			Position = position;
		}

		public override string ToString ()
		{
			return String.Format ("{0}: {1}", Type, Value);
		}
	}
}
