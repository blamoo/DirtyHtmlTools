using System;
using DirtyHtmlTools;

namespace DirtyHtmlToolsConsoleTests
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Lexer L = new Lexer ();

			var z = L.Parse ("<z               >aSDasdAS<b>Z\\ZZZZZ&amp;</b>asdfasdfs\naf</z><a href=\"asdfas\ndfasdf\" asdfa=\"1\" /><plau z='\"minha irmã '>sdfasdfasdfa </plau><!--asdf-->ffffffffffffffffff&teucu;wertyu\"''   <!--oirytiu <zxcvbnm> o <> eryoti--></z>");
			z.ToString ();
		}
	}
}
