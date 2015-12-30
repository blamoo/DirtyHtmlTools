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

        private Token[] ThrowOnError(Token[] tokens)
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

            return tokens;
        }

        [Test]
        public void TestLexer()
        {
            string test = "<z               >aSDasdAS<b>Z\\ZZZZZ&amp;</b>asdfasdfs\naf</z><a href=\"asdfas\ndfasdf\" asdfa=\"1\" /><plau z='\"minha irmã '>sdfasdfasdfa </plau><!--asdf-->ffffffffffffffffff&teucu;wertyu\"''   <!--oirytiu <zxcvbnm> o <> eryoti-->";
            Token[] tmp = ThrowOnError(L.Parse(test));
            tmp.ToString();
        }

        [Test]
        public void TestParser()
        {
            Element[] tmp;
            tmp = P.Parse("<a:b:c/><d_e f.g-h=\"&amp;\"/>");
            tmp = P.Parse("<a><b><c/></b></a>");
            tmp = P.Parse("<a a b c='d'>d<b />b<c type=\"l\">d</c></a>zzz<e>f</e>");
            tmp = P.Parse("<z               >aSDasdAS<b>Z\\ZZZZZ&amp;</b>asdfasdfs\naf</z><a href=\"asdfas\ndfasdf\" asdfa=\"1\" /><plau z='\"minha irmã '>sdfasdfasdfa </plau><!--asdf-->ffffffffffffffffff&tesgj;wertyu\"''   <!--oirytiu <zxcvbnm> o <> eryoti-->");
            tmp = P.Parse("<p><b>Área: </b>Serviços</p><p><b>Sexo: </b>Masculino</p><p><b>Descrição: </b>Atendente / Estoquista\\r\\n\\r\\nvaga para o Sexo Masculino\\r\\n\\r\\nApartir dos 18 anos at&eacute; 30 anos\\r\\n\\r\\nCom Experiencia na fun&ccedil;&atilde;o\\r\\n\\r\\nInteressados pela vaga devem ir pessoalmente&nbsp;das 10h. &aacute;s 12h. ou das 16h. &aacute;s 17h. Na Pra&ccedil;a Coronel Raphael de Moura Campos, 11 (Questa Tattoo)&nbsp;Falar com Pex&atilde;o\\r\\n\\r\\nPor favor fale que viu a vaga pela SoluTudo\\r\\n</p><p><b>Última atualização: </b>08/09/2014 - 15:24:45</p><p><b>Validade: </b>24/12/2015</p>");
            tmp.ToString();
        }

    }
}
