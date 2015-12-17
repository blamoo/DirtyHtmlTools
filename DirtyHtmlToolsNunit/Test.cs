using NUnit.Framework;
using System;
using DirtyHtmlTools;

namespace DirtyHtmlToolsNunit
{
	[TestFixture ()]
	public class Test
	{
		private Lexer L;

		public Test ()
		{
			L = new Lexer ();
		}

		private void throwOnError (Token[] tokens)
		{ 
			foreach (var item in tokens) {
				switch (item.Type) {
				case TokenType.Incomplete:
					throw new Exception (String.Format ("tag inválida em {0}", item.Position));
				case TokenType.InvalidInsideTag:
					throw new Exception (String.Format ("Atributo inválido em {0}", item.Position));
				} 
			}
		}

		[Test ()]
		public void TestMethod1 ()
		{
			string test = "<z               >aSDasdAS<b>Z\\ZZZZZ&amp;</b>asdfasdfs\naf</z><a href=\"asdfas\ndfasdf\" asdfa=\"1\" /><plau z='\"minha irmã '>sdfasdfasdfa </plau><!--asdf-->ffffffffffffffffff&teucu;wertyu\"''   <!--oirytiu <zxcvbnm> o <> eryoti--></z>";
			throwOnError (L.Parse (test));
		}
	}
}

