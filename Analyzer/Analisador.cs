using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Analyzer
{
    public class Analisador
    {
        public Analisador()
        {
            setReservadas();
        }

        public bool isFinal = false;

        private bool isDB = false;

        private bool isString = false;

        private bool isTab = true;

        private bool isA = false;

        private Stack<int> pos = new Stack<int>();

        private List<int> tabs = new List<int>();

        private int tabCount = 0;

        public Stack<Token> last = new Stack<Token>();

        public string codigo { get; set; }

        private int atual = 0;

        private int proximo = 1;

        private char satual;

        public int linha = 1;

        public int coluna = 0;

        private Dictionary<string, TipoTk> reservadas = new Dictionary<string, TipoTk>();

        private char[] separators = { ' ', '\n', '\r' };

        private List<char> tk = new List<char>();

        public List<Token> tks = new List<Token>();

        private string[] tb = { "*", "/", ">", "<" };

        private string[] db = { "-", "+", "=", ">", "<", "&", "|", "@", "%", "/", "*" };

        private string[] atrib = { "^", "|", "&", "%", "-", "<", ">", "!", "+", "*", "/" };

        private string[] str = { "'", "\"" };
        private void Reset()
        {
            tk.Clear();
            atual = 0;
            proximo = 1;
            isFinal = false;
            isDB = false;
            isString = false;
            isTab = true;
            isA = false;
            tabCount = 0;
        }

        private void ResetSin()
        {
            tk.Clear();
            tabs.Add(tabCount);
            isFinal = false;
            isDB = false;
            isString = false;
            isTab = true;
            isA = false;
            linha = linha + 1;
            tabCount = 0;
        }

        private bool isLetNum(char c)
        {
            return c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' || c >= '0' && c <= '9' || c == '"';
        }

        private bool istNum(char c)
        {
            return c >= '0' && c <= '9';
        }

        private bool isSeparator(char c)
        {
            return separators.Contains(c);
        }

        private bool isLast(int val)
        {
            return val == codigo.Length - 1;
        }

        private bool tkIsNull()
        {
            return tk.Count == 0;
        }

        private bool isDuble(string c)
        {
            return db.Contains(c);
        }

        private bool isAtrib(string c)
        {
            return atrib.Contains(c);
        }

        private bool isTb(string c)
        {
            return tb.Contains(c);
        }

        private bool constString(string c)
        {
            return str.Contains(c);
        }

        private void Next()
        {
            atual = atual + 1;
            proximo = proximo + 1;
        }

        private Token createToken(int coluna, string value)
        {
            var t = new Token();
            t.valor = value;
            t.linha = linha;
            t.coluna = coluna;
            t.tipo = GetTipo(value);

            return t;
        }

        private Token tkToToken(int coluna)
        {
            var t = createToken(coluna, charString());
            tk.Clear();
            return t;
        }

        private Token atualToToken(int coluna)
        {
            var t = createToken(coluna, satual.ToString());
            return t;
        }

        private Token identToken()
        {
            var t = new Token();
            t.linha = linha;
            t.tipo = TipoTk.TkIdent;
            t.valor = "ident";

            return t;
        }

        private Token desidentToken()
        {
            var t = new Token();
            t.linha = linha;
            t.tipo = TipoTk.TkDesident;
            t.valor = "desident";

            return t;
        }

        private void setReservadas()
        {
            reservadas.Add(",", TipoTk.TkVirgula);
            reservadas.Add("bool", TipoTk.TkBool);
            reservadas.Add("bytes", TipoTk.TkBytes);
            reservadas.Add("NoneType", TipoTk.TkNoneType);
            reservadas.Add("str", TipoTk.TkStr);
            reservadas.Add("imaginary", TipoTk.TkImaginary);
            reservadas.Add("'", TipoTk.TkAspasSimples);
            reservadas.Add("$", TipoTk.TkCifrao);
            reservadas.Add("global", TipoTk.TkGlobal);
            reservadas.Add("import", TipoTk.TkImport);
            reservadas.Add("char", TipoTk.TkChar);
            reservadas.Add("double", TipoTk.TkDouble);
            reservadas.Add("float", TipoTk.TkFloat);
            reservadas.Add("int", TipoTk.TkInt);
            reservadas.Add("in", TipoTk.TkIn);
            reservadas.Add("is", TipoTk.TkIs);
            reservadas.Add("lambda", TipoTk.TkLambda);
            reservadas.Add("nonlocal", TipoTk.TkNonLocal);
            reservadas.Add("long", TipoTk.TkLong);
            reservadas.Add("unsigned", TipoTk.TkUnsigned);
            reservadas.Add("short", TipoTk.TkShort);
            reservadas.Add(";", TipoTk.TkPontoEVirgula);
            reservadas.Add(">", TipoTk.TkMaior);
            reservadas.Add("<", TipoTk.TkMenor);
            reservadas.Add("<=", TipoTk.TkMenorIgual);
            reservadas.Add(">=", TipoTk.TkMaiorIgual);
            reservadas.Add("==", TipoTk.TkIgual);
            reservadas.Add("=", TipoTk.TkAtrib);
            reservadas.Add("(", TipoTk.TkAbreParenteses);
            reservadas.Add(")", TipoTk.TkFechaParenteses);
            reservadas.Add("{", TipoTk.TkAbreChaves);
            reservadas.Add("}", TipoTk.TkFechaChaves);
            reservadas.Add("[", TipoTk.TkAbreColchete);
            reservadas.Add("@", TipoTk.TkArroba);
            reservadas.Add("]", TipoTk.TkFechaColchete);
            reservadas.Add("+", TipoTk.TkMais);
            reservadas.Add("**", TipoTk.TkAsteriscoAsterisco);
            reservadas.Add("continue", TipoTk.TkContinue);
            reservadas.Add("def", TipoTk.TkDef);
            reservadas.Add("del", TipoTk.TkDel);
            reservadas.Add("-", TipoTk.TkMenos);
            reservadas.Add("?", TipoTk.TkInterrogacao);
            reservadas.Add("#", TipoTk.TkHash);
            reservadas.Add("charconst", TipoTk.TkCharConst);
            reservadas.Add("numconst", TipoTk.TkNumconst);
            reservadas.Add("%", TipoTk.TkPercent);
            reservadas.Add("%=", TipoTk.TkPorcentagemIgual);
            reservadas.Add("*", TipoTk.TkMultiplicacao);
            reservadas.Add("**=", TipoTk.TkAsteriscoAsteriscoIgual);
            reservadas.Add("/", TipoTk.TkDivisao);
            reservadas.Add("//", TipoTk.TkBarraBarra);
            reservadas.Add(":", TipoTk.TkDoisPontos);
            reservadas.Add("None", TipoTk.TkNone);
            reservadas.Add(".", TipoTk.TkPonto);
            reservadas.Add("True", TipoTk.TkTrue);
            reservadas.Add("False", TipoTk.TkFalse);
            reservadas.Add("or", TipoTk.TkOu);
            reservadas.Add("pass", TipoTk.TkPass);
            reservadas.Add("rise", TipoTk.TkRise);
            reservadas.Add("and", TipoTk.TkE);
            reservadas.Add("as", TipoTk.TkAss);
            reservadas.Add("assert", TipoTk.TkAssert);
            reservadas.Add("!", TipoTk.TkNao);
            reservadas.Add("mutable", TipoTk.TkMutable);
            reservadas.Add("+=", TipoTk.TkMaisIgual);
            reservadas.Add("-=", TipoTk.TkMenosIgual);
            reservadas.Add("*=", TipoTk.TkMulIgual);
            reservadas.Add("/=", TipoTk.TkDivIgual);
            reservadas.Add("//=", TipoTk.TkBarraBarraIgual);
            reservadas.Add("++", TipoTk.TkDoisMais);
            reservadas.Add("--", TipoTk.TkDoisMenos);
            reservadas.Add("class", TipoTk.TkClass);
            reservadas.Add("if", TipoTk.TkSe);
            reservadas.Add("&=", TipoTk.TkBeIgual);
            reservadas.Add("while", TipoTk.TkEnquanto);
            reservadas.Add("return", TipoTk.TkReturn);
            reservadas.Add("record", TipoTk.TkRecord);
            reservadas.Add("break", TipoTk.TkBreak);
            reservadas.Add("else", TipoTk.TkSenao);
            reservadas.Add("elif", TipoTk.TkSenaoSe);
            reservadas.Add("from", TipoTk.TkFrom);
            reservadas.Add("except", TipoTk.TkExcept);
            reservadas.Add("finally", TipoTk.TkFinally);
            reservadas.Add("&", TipoTk.TkBe);
            reservadas.Add("|", TipoTk.TkBou);
            reservadas.Add("|=", TipoTk.TkBouIgual);
            reservadas.Add("^=", TipoTk.TkXorIgual);
            reservadas.Add("~", TipoTk.TkBNot);
            reservadas.Add("^", TipoTk.TkXor);
            reservadas.Add("not", TipoTk.TkNot);
            reservadas.Add(">>", TipoTk.TkRS);
            reservadas.Add(">>=", TipoTk.TkRSIgual);
            reservadas.Add("<<", TipoTk.TkLS);
            reservadas.Add("<<=", TipoTk.TkLSIgual);
            reservadas.Add("!=", TipoTk.TkDiferente);
            reservadas.Add("do", TipoTk.TkDo);
            reservadas.Add("for", TipoTk.TkFor);
            reservadas.Add("switch", TipoTk.TkSwitch);
            reservadas.Add("with", TipoTk.TkWith);
            reservadas.Add("yield", TipoTk.TkYield);
            reservadas.Add("case", TipoTk.TkCase);
            reservadas.Add("try", TipoTk.TkTry);
            reservadas.Add("default", TipoTk.TkDefault);
            reservadas.Add("\t", TipoTk.TkTab);
            reservadas.Add("\\", TipoTk.TkContraBarra);
            reservadas.Add("\"", TipoTk.TkAspasDuplas);
            reservadas.Add("nop", TipoTk.Default);
            reservadas.Add("_", TipoTk.TkUnderline);
            reservadas.Add("range", TipoTk.TkRange);


        }


        public void Analizar()
        {

            Reset();
            Token tk = new Token();

            while ((proximo <= codigo.Length && atual < codigo.Length) && !isFinal)
            {
                tk = getToken();
                tks.Add(tk);
            }



            tabs.Add(tabCount);
        }

        public void generateIdentation()
        {
            var last = tabs.Last();
            for (int i = last; i > 0; i--)
            {
                Console.WriteLine("DESIDENT");
            }
        }

        public Token VerificarIdentacao()
        {
            if (tabs.Count > 0 && tabCount > tabs.Last())
            {
                tabs.Add(tabCount);
                return identToken();
            }
            else if (tabs.Count > 0 && (tabCount < tabs.Last()))
            {
                tabs.Add(tabCount);
                return desidentToken();
            }
            else
            {
                tabs.Add(tabCount);
                var tkn = new Token();
                tkn.tipo = TipoTk.Default;
                return tkn;
            }
        }

        public void lastIndentation()
        {
            Token tk = new Token();
            while (tabs.Last() > 0)
            {
                tk = getToken();
                tks.Add(tk);
            }
        }

        public Token getToken()
        {
            coluna = atual;
            Token t = new Token();
            //tk.Clear();

            while (atual < codigo.Length && !isFinal)
            {

                satual = codigo[atual];

                if (satual == '\n')
                {
                    ResetSin();
                    Next();
                    satual = codigo[atual];
                }

                if (atual == codigo.Length - 1)
                {
                    isFinal = true;
                }

                if (satual == '\t' && isTab)
                {
                    tabCount++;
                    Next();
                }

                else if (constString(satual.ToString()))
                {
                    if (isTab)
                    {
                        var tkn = VerificarIdentacao();
                        isTab = false;

                        if (tkn.tipo != TipoTk.Default)
                        {
                            isFinal = false;
                            return tkn;
                        }
                    }
                    if (isString)
                    {
                        isString = false;

                        tk.Add(satual);

                        Next();

                        return tkToToken(coluna);
                    }
                    else
                    {
                        isString = true;

                        tk.Add(satual);

                        Next();
                    }
                }
                else if (isString)
                {
                    if (isTab)
                    {
                        var tkn = VerificarIdentacao();
                        isTab = false;

                        if (tkn.tipo != TipoTk.Default)
                        {
                            isFinal = false;
                            return tkn;
                        }
                    }
                    tk.Add(satual);

                    Next();

                }
                else if (isLetNum(satual))
                {
                    if (isTab)
                    {
                        var tkn = VerificarIdentacao();
                        isTab = false;

                        if (tkn.tipo != TipoTk.Default)
                        {
                            isFinal = false;
                            return tkn;
                        }
                    }
                    if (!tkIsNull() && isDB)
                    {
                        isFinal = false;
                        return tkToToken(coluna);
                    }
                    tk.Add(satual);

                    if (isLast(atual))
                    {
                        if (!tkIsNull())
                        {
                            return tkToToken(coluna);
                        }
                        else
                        {
                            return atualToToken(coluna);
                        }
                    }
                    else
                    {
                        Next();
                    }
                }
                else if (!isDB && isAtrib(satual.ToString()) && atual + 1 < codigo.Length && codigo[atual + 1].ToString() == "=")
                {
                    if (!tkIsNull())
                    {
                        return tkToToken(coluna);
                    }

                    tk.Add(satual);
                    tk.Add(codigo[atual + 1]);
                    Next();
                    Next();
                    return tkToToken(coluna);
                }
                else if (isDB && isTb(satual.ToString()) && atual + 1 < codigo.Length && codigo[atual + 1].ToString() == "=")
                {
                    tk.Add(satual);
                    tk.Add(codigo[atual + 1]);
                    Next();
                    Next();
                    return tkToToken(coluna);
                }
                else if (isSeparator(satual))
                {
                    if (isTab)
                    {
                        var tkn = VerificarIdentacao();
                        isTab = false;

                        if (tkn.tipo != TipoTk.Default)
                        {
                            isFinal = false;
                            return tkn;
                        }
                    }
                    if (!tkIsNull())
                    {
                        t = tkToToken(coluna);
                        Next();

                        if (isDB)
                        {
                            isDB = false;
                        }

                        return t;
                    }
                    else
                    {
                        Next();
                    }
                }
                else if (isDuble(satual.ToString()))
                {
                    if (!tk.Contains(satual) && !tkIsNull())
                    {

                        isFinal = false;
                        return tkToToken(coluna);
                    }
                    if (isTab)
                    {
                        var tkn = VerificarIdentacao();
                        isTab = false;
                        isFinal = false;
                        if (tkn.tipo != TipoTk.Default)
                        {
                            return tkn;
                        }
                    }
                    if (!tkIsNull() && !isDB)
                    {
                        isFinal = false;
                        return tkToToken(coluna);
                    }
                    else
                    {
                        if (isFinal)
                        {
                            return atualToToken(coluna);
                        }
                        else
                        {
                            isDB = true;
                            tk.Add(satual);
                            Next();
                        }
                    }
                }
                else
                {
                    if (isTab)
                    {
                        var tkn = VerificarIdentacao();
                        isTab = false;

                        if (tkn.tipo != TipoTk.Default)
                        {
                            isFinal = false;
                            return tkn;
                        }
                    }
                    if (!tkIsNull())
                    {
                        isFinal = false;
                        if (isDB)
                        {
                            isDB = false;
                        }
                        return tkToToken(coluna);
                    }
                    else
                    {
                        Next();
                        return atualToToken(coluna);
                    }
                }

            }

            if (tabs.Last() > 0)
            {
                if (atual == codigo.Length - 1)
                {
                    isFinal = true;
                    Next();
                }
                tabs.Add(tabs.Last() - 1);
                return desidentToken();
            }

            if (isDB && !tkIsNull())
            {
                return tkToToken(coluna);
            }



            tk.Clear();
            return new Token { tipo = TipoTk.End };

        }

        public void marcaPosToken(Token tk)
        {
            pos.Push(atual);
            last.Push(tk);
        }

        public Token restauraPosToken()
        {
            isFinal = false;
            atual = pos.Pop();
            proximo = atual + 1;
            return last.Pop();
        }

        public void Peek()
        {
            pos.Pop();
            last.Pop();
        }
        public void PrintTokens()
        {
            using (StreamWriter file = new StreamWriter("Tokens.lex"))
            {
                foreach (var t in tks)
                {
                    var line = "Token: " + t.tipo.ToString() + "\tLexema: " + t.valor + "\t Linha: " + t.linha + "\t Coluna: " + t.coluna;
                    file.WriteLine(line);
                    Console.WriteLine(line);
                }
            }
        }

        private TipoTk GetTipo(string s)
        {
            TipoTk t = TipoTk.TkId;

            if (reservadas.Where(item => item.Key == s).Select(item => item.Key).ToString() != String.Empty)
            {
                try
                {
                    t = reservadas[s];
                }
                catch
                {
                    if ((s[0].ToString().Equals("\"") && s[s.Length - 1].ToString().Equals("\"") ||
                        (s[0].ToString().Equals("'") && s[s.Length - 1].ToString().Equals("'"))))
                    {
                        t = TipoTk.TkCharConst;
                    }
                    else
                    {
                        bool isNumber = true;
                        for (int i = 0; i < s.Length; i++)
                        {
                            isNumber = istNum(s[i]);
                        }

                        if (isNumber)
                        {
                            t = TipoTk.TkNumconst;
                        }
                        else
                        {
                            t = TipoTk.TkId;
                        }
                    }

                }

            }

            return t;
        }

        private string charString()
        {
            string r = String.Empty;
            foreach (var item in tk)
            {
                r += item;
            }
            return r;
        }


    }

}