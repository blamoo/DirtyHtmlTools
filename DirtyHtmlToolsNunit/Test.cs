using NUnit.Framework;
using System;
using DirtyHtmlTools;

namespace DirtyHtmlToolsNunit
{
    [TestFixture]
    public class Test
    {
        private Lexer L;
        private Parser P;

        public Test()
        {
            L = new Lexer();
            P = new Parser();
        }

        private void throwOnError(Token[] tokens)
        { 
            foreach (var item in tokens)
            {
                switch (item.Type)
                {
                    case TokenType.Incomplete:
                        throw new Exception(String.Format("tag inválida em {0}", item.Position));
                    case TokenType.InvalidInsideTag:
                        throw new Exception(String.Format("Atributo inválido em {0}", item.Position));
                } 
            }
        }

        [Test]
        public void TestMethod1()
        {
            string test = "<z               >aSDasdAS<b>Z\\ZZZZZ&amp;</b>asdfasdfs\naf</z><a href=\"asdfas\ndfasdf\" asdfa=\"1\" /><plau z='\"minha irmã '>sdfasdfasdfa </plau><!--asdf-->ffffffffffffffffff&teucu;wertyu\"''   <!--oirytiu <zxcvbnm> o <> eryoti-->";
             
            throwOnError(L.Parse(test));
        }

        [Test]
        public void TestMethod2()
        {
            string test = "<a a b c='d'>d<b />b<c type=\"l\">d</c></a>zzz<e>f</e>";

            P.Parse(test);
        }
    }
}

