using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DirtyHtmlTools;
using System.Collections.Generic;

namespace DirtyHtmlToolsTests
{
    [TestClass]
    public class UnitTest1
    {
        private Lexer L;
        private Parser P;

        public UnitTest1()
        {
            L = new Lexer();
            P = new Parser();
        }

        private void throwOnError(List<Token> tokens)
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

        [TestMethod]
        public void TestMethod1()
        {
            //string test = "<z               >aSDasdAS<b>Z\\ZZZZZ&amp;</b>asdfasdfs\naf</z><a href=\"asdfas\ndfasdf\" asdfa=\"1\" /><plau z='\"minha irmã '>sdfasdfasdfa </plau><!--asdf-->ffffffffffffffffff&teucu;wertyu\"''   <!--oirytiu <zxcvbnm> o <> eryoti--></z>";
            //string test = "<a a b c='d'>d<b />b<c type=\"l\">d</c></a>zzz<e>f</e>";
            //string test = "<a><b><c/></b></a>";
            string test = "<a>";
            
            var z = P.Parse(test);
        }
    }
}
