using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzer
{
    public class Sintatico
    {

        private Analisador lex = new Analisador();
        private Erro erro = null;

        Token tk = new Token();

        public Sintatico()
        {

        }

        public int Analizar(string cod)
        {
            lex.codigo = cod;
            tk = lex.getToken();
            return PROGRAM();
        }

        public void PrintErro()
        {
            erro.PrintError();
        }

        private void setError()
        {
            erro = new Erro(lex.linha, lex.coluna);
        }
        //PROGRAM -> LSTCMD end 
        int PROGRAM()
        {
            if (LSTCMD() == 1)
            {
                if (tk.tipo == TipoTk.End)
                {// end
                    tk = lex.getToken();
                    return 1;
                }
                else { return 0; }
            }
            else { return 0; }
        }

        //LISTA DE COMANDO
        //LSTCMD -> EXPATTR | CMDIF 
        int LSTCMD()
        {
            lex.marcaPosToken(tk);
            if (EXPATTR() == 1)
            {
                lex.Peek();

                if (LSTRCMD() == 1)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }

            }
            else
            {
                tk = lex.restauraPosToken();
                lex.marcaPosToken(tk);
                if (CMDIF() == 1)
                {
                    lex.Peek();
                    if (LSTRCMD() == 1)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    tk = lex.restauraPosToken();
                    lex.marcaPosToken(tk);
                    if (CMDFOR() == 1)
                    {
                        lex.Peek();
                        if (LSTRCMD() == 1)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    else
                    {
                        tk = lex.restauraPosToken();
                        lex.marcaPosToken(tk);
                        if (CMDWHILE() == 1)
                        {
                            lex.Peek();
                            if (LSTRCMD() == 1)
                            {
                                return 1;
                            }
                            else
                            {
                                return 0;
                            }
                        }
                        else
                        {
                            tk = lex.restauraPosToken();
                            lex.marcaPosToken(tk);
                            if (CMDFUNC() == 1)
                            {
                                lex.Peek();
                                if (LSTRCMD() == 1)
                                {
                                    return 1;
                                }
                                else
                                {
                                    return 0;
                                }
                            }
                            else
                            {
                                tk = lex.restauraPosToken();
                                lex.marcaPosToken(tk);
                                if (CMDRETURN() == 1)
                                {
                                    lex.Peek();
                                    return 1;
                                }
                                else
                                {
                                    tk = lex.restauraPosToken();
                                    lex.marcaPosToken(tk);
                                    if (EXECFUNC() == 1)
                                    {
                                        lex.Peek();
                                        if (LSTRCMD() == 1)
                                        {
                                            return 1;
                                        }
                                        else
                                        {
                                            return 0;
                                        }
                                    }
                                    else
                                    {
                                        tk = lex.restauraPosToken();
                                        lex.marcaPosToken(tk);
                                        if (EXPATTRMUL() == 1)
                                        {
                                            lex.Peek();
                                            if (LSTRCMD() == 1)
                                            {
                                                return 1;
                                            }
                                            else
                                            {
                                                return 0;
                                            }
                                        }
                                        else
                                        {
                                            tk = lex.restauraPosToken();
                                            lex.marcaPosToken(tk);
                                            if (CMDCST() == 1)
                                            {
                                                lex.Peek();
                                                if (LSTRCMD() == 1)
                                                {
                                                    return 1;
                                                }
                                                else
                                                {
                                                    return 0;
                                                }
                                            }
                                            else
                                            {
                                                lex.Peek();
                                                return 0;
                                            }

                                        }
                                    }
                                }

                            }

                        }

                    }

                }

            }
        }


        //LSTRCMD -> LSTCMD | ? 
        int LSTRCMD()
        {
            if (LSTCMD() == 1)
            {
                return 1;
            }
            else { return 1; }
        }

        //EXPRESSÕES
        int OPCMP()
        {
            if (tk.tipo == TipoTk.TkMenor)
            {// <
                tk = lex.getToken();
                return 1;
            }
            else if (tk.tipo == TipoTk.TkMaior)
            {// >
                tk = lex.getToken();
                return 1;
            }
            else if (tk.tipo == TipoTk.TkMenorIgual)
            {// <=
                tk = lex.getToken();
                return 1;
            }
            else if (tk.tipo == TipoTk.TkMaiorIgual)
            {// >=
                tk = lex.getToken();
                return 1;
            }
            else if (tk.tipo == TipoTk.TkDiferente)
            {// !=
                tk = lex.getToken();
                return 1;
            }
            else if (tk.tipo == TipoTk.TkIgual)
            {// ==
                tk = lex.getToken();
                return 1;
            }
            else { setError(); return 0; }
        }

        //OPEXPP1 -> + | - 
        int OPEXPP1()
        {
            if (tk.tipo == TipoTk.TkMais)
            {// +
                tk = lex.getToken();
                return 1;
            }
            else if (tk.tipo == TipoTk.TkMenos)
            {// -
                tk = lex.getToken();
                return 1;
            }
            else { setError(); return 0; }
        }

        //OPEXPP2 -> * | / | // | % | & | | | ~ | ^ | >> | <<
        int OPEXPP2()
        {
            if (tk.tipo == TipoTk.TkMultiplicaco)
            {// *
                tk = lex.getToken();
                return 1;
            }
            else if (tk.tipo == TipoTk.TkDivisao)
            {// /
                tk = lex.getToken();
                return 1;
            }
            else if (tk.tipo == TipoTk.TkBarraBarra)
            {// //
                tk = lex.getToken();
                return 1;
            }
            else if (tk.tipo == TipoTk.TkPercent)
            {// %
                tk = lex.getToken();
                return 1;
            }
            else if (tk.tipo == TipoTk.TkBe)
            {// &
                tk = lex.getToken();
                return 1;
            }
            else if (tk.tipo == TipoTk.TkBNot)
            {// ~
                tk = lex.getToken();
                return 1;
            }
            else if (tk.tipo == TipoTk.TkXor)
            {// ^
                tk = lex.getToken();
                return 1;
            }
            else if (tk.tipo == TipoTk.TkRS)
            {// >>
                tk = lex.getToken();
                return 1;
            }
            else if (tk.tipo == TipoTk.TkLS)
            {// <<
                tk = lex.getToken();
                return 1;
            }
            else { setError(); return 0; }
        }

        //OPEXPP4 -> id | numconst 
        int OPEXPP4()
        {
            if (tk.tipo == TipoTk.TkId)
            {// id
                tk = lex.getToken();
                return 1;
            }
            else if (tk.tipo == TipoTk.TkNumconst)
            {// numconst
                tk = lex.getToken();
                return 1;
            }
            else { setError(); return 0; }
        }

        //EXPCMP -> EXPP1 EXPCMP1Linha 
        int EXPCMP()
        {
            lex.marcaPosToken(tk);
            if (EXPP1() == 1)
            {
                if (EXPCMP1Linha() == 1)
                {
                    return 1;
                }
                else { return 0; }
            }
            else
            {
                tk = lex.restauraPosToken();
                if (tk.tipo == TipoTk.TkId)
                {// id
                    tk = lex.getToken();
                    return 1;
                }
                else { setError(); return 0; }
            }
        }

        //EXPCMP1Linha -> OPCMP EXPP1 | ? 
        int EXPCMP1Linha()
        {
            if (OPCMP() == 1)
            {
                if (EXPP1() == 1)
                {
                    return 1;
                }
                else { return 0; }
            }
            else { return 1; }
        }

        //EXPP1 -> EXPP2 EXPP11Linha 
        int EXPP1()
        {
            if (EXPP2() == 1)
            {
                if (EXPP11Linha() == 1)
                {
                    return 1;
                }
                else { return 0; }
            }
            else { return 0; }
        }

        //EXPP11Linha -> OPEXPP1 EXPP1 | ? 
        int EXPP11Linha()
        {
            if (OPEXPP1() == 1)
            {
                if (EXPP1() == 1)
                {
                    return 1;
                }
                else { return 0; }
            }
            else { return 1; }
        }

        //EXPP2 -> EXPP3 EXPP21Linha 
        int EXPP2()
        {
            if (EXPP3() == 1)
            {
                if (EXPP21Linha() == 1)
                {
                    return 1;
                }
                else { return 0; }
            }
            else { return 0; }
        }

        //EXPP21Linha -> OPEXPP2 EXPP2 | ? 
        int EXPP21Linha()
        {
            if (OPEXPP2() == 1)
            {
                if (EXPP2() == 1)
                {
                    return 1;
                }
                else { return 0; }
            }
            else { return 1; }
        }

        //EXPP3 -> EXPP4 EXPP31Linha 
        int EXPP3()
        {
            if (EXPP4() == 1)
            {
                if (EXPP31Linha() == 1)
                {
                    return 1;
                }
                else { return 0; }
            }
            else { return 0; }
        }

        //EXPP31Linha -> ** EXPP3 | ? 
        int EXPP31Linha()
        {
            if (tk.tipo == TipoTk.TkAsteriscoAsterisco)
            {// **
                tk = lex.getToken();
                if (EXPP3() == 1)
                {
                    return 1;
                }
                else { return 0; }
            }
            else { setError(); return 1; }
        }

        //EXPP4 -> OPEXPP4 | ( EXPBOOL ) 
        int EXPP4()
        {
            if (OPEXPP4() == 1)
            {
                return 1;
            }
            else if (tk.tipo == TipoTk.TkAbreParenteses)
            {// (
                tk = lex.getToken();
                if (EXPBOOL() == 1)
                {
                    if (tk.tipo == TipoTk.TkFechaParenteses)
                    {// )
                        tk = lex.getToken();
                        return 1;
                    }
                    else { setError(); return 0; }
                }
                else { return 0; }
            }
            else { setError(); return 0; }
        }

        //EXPBOOL -> EXPOR 
        int EXPBOOL()
        {
            if (EXPOR() == 1)
            {
                return 1;
            }
            else { return 0; }
        }

        //EXPOR -> EXPAND EXPOR1Linha 
        int EXPOR()
        {
            if (EXPAND() == 1)
            {
                if (EXPOR1Linha() == 1)
                {
                    return 1;
                }
                else { return 0; }
            }
            else { return 0; }
        }

        //EXPOR1Linha -> or EXPOR | ? 
        int EXPOR1Linha()
        {
            if (tk.tipo == TipoTk.TkOu)
            {// or
                tk = lex.getToken();
                if (EXPOR() == 1)
                {
                    return 1;
                }
                else { return 0; }
            }
            else { return 1; }
        }

        //EXPAND -> EXPNOT EXPAND1Linha 
        int EXPAND()
        {
            if (EXPNOT() == 1)
            {
                if (EXPAND1Linha() == 1)
                {
                    return 1;
                }
                else { return 0; }
            }
            else { return 0; }
        }

        //EXPAND1Linha -> and EXPAND | ? 
        int EXPAND1Linha()
        {
            if (tk.tipo == TipoTk.TkE)
            {// and
                tk = lex.getToken();
                if (EXPAND() == 1)
                {
                    return 1;
                }
                else { return 0; }
            }
            else { return 1; }
        }

        //EXPNOT -> not EXPNOT | EXPCMP 
        int EXPNOT()
        {
            if (tk.tipo == TipoTk.TkNot)
            {// not
                tk = lex.getToken();
                if (EXPNOT() == 1)
                {
                    return 1;
                }
                else { return 0; }
            }
            else if (EXPCMP() == 1)
            {
                return 1;
            }
            else { setError(); return 0; }
        }

        //ATRIBUIÇÃO

        //OPATTR -> = | += | -= | *= | /= | %= | //= | **= | &= | |= | ^= | >>=
        int OPATTR()
        {
            if (tk.tipo == TipoTk.TkAtrib)
            {// =
                tk = lex.getToken();
                return 1;
            }
            else if (tk.tipo == TipoTk.TkMaisIgual)
            {// +=
                tk = lex.getToken();
                return 1;
            }
            else if (tk.tipo == TipoTk.TkMenosIgual)
            {// -=
                tk = lex.getToken();
                return 1;
            }
            else if (tk.tipo == TipoTk.TkMulIgual)
            {// *=
                tk = lex.getToken();
                return 1;
            }
            else if (tk.tipo == TipoTk.TkDivIgual)
            {// /=
                tk = lex.getToken();
                return 1;
            }
            else if (tk.tipo == TipoTk.TkPorcentagemIgual)
            {// %=
                tk = lex.getToken();
                return 1;
            }
            else if (tk.tipo == TipoTk.TkBarraBarraIgual)
            {// //=
                tk = lex.getToken();
                return 1;
            }
            else if (tk.tipo == TipoTk.TkAsteriscoAsteriscoIgual)
            {// **=
                tk = lex.getToken();
                return 1;
            }
            else if (tk.tipo == TipoTk.TkBeIgual)
            {// &=
                tk = lex.getToken();
                return 1;
            }
            else if (tk.tipo == TipoTk.TkBouIgual)
            {// |=
                tk = lex.getToken();
                return 1;
            }
            else if (tk.tipo == TipoTk.TkXorIgual)
            {// ^=
                tk = lex.getToken();
                return 1;
            }
            else if (tk.tipo == TipoTk.TkRSIgual)
            {// >>=
                tk = lex.getToken();
                return 1;
            }
            else if (tk.tipo == TipoTk.TkLSIgual)
            {// <<=
                tk = lex.getToken();
                return 1;
            }
            else { setError(); return 0; }
        }

        //EXPATTR -> id LSTIDS 
        int EXPATTR()
        {
            if (tk.tipo == TipoTk.TkId)
            {// id

                tk = lex.getToken();
                if (LSTIDS() == 1)
                {
                    return 1;
                }
                else { return 0; }
            }
            else
            {
                setError();
                return 0;
            }
        }

        //LSTIDS -> , id LSTIDS | LSTOPERADORES 
        int LSTIDS()
        {

            if (tk.tipo == TipoTk.TkVirgula)
            {// ,
                tk = lex.getToken();
                if (tk.tipo == TipoTk.TkId)
                {// id
                    tk = lex.getToken();
                    if (LSTIDS() == 1)
                    {
                        return 1;
                    }
                    else { return 0; }
                }
                else { setError(); return 0; }
            }
            else if (OPATTR() == 1)
            {
                if (EXPP1() == 1)
                {
                    if (LSTVAL() == 1)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else { return 0; }

            }
            else { return 0; }
        }

        //EXPATTRMUL -> id LSTMUL 
        int EXPATTRMUL()
        {
            if (tk.tipo == TipoTk.TkId)
            {// id
                tk = lex.getToken();
                if (LSTMUL() == 1)
                {
                    return 1;
                }
                else { return 0; }
            }
            else { setError(); return 0; }
        }

        //LSTMUL -> OPATTR LSTMUL1Linha | ? 
        int LSTMUL()
        {
            if (OPATTR() == 1)
            {
                if (LSTMUL1Linha() == 1)
                {
                    return 1;
                }
                else { return 0; }
            }
            else { return 1; }
        }

        //LSTMUL1Linha -> id LSTMUL | numconst 
        int LSTMUL1Linha()
        {
            if (tk.tipo == TipoTk.TkId)
            {// id
                tk = lex.getToken();
                if (LSTMUL() == 1)
                {
                    return 1;
                }
                else { return 0; }
            }
            else if (tk.tipo == TipoTk.TkNumconst)
            {// numconst
                tk = lex.getToken();
                return 1;
            }
            else { setError(); return 0; }
        }

        //VALLAST -> OPEXPP4 | charconst | EXPP1 
        int VALLAST()
        {
            if (tk.tipo == TipoTk.TkCharConst)
            {// charconst
                tk = lex.getToken();
                return 1;
            }
            else if (EXPP1() == 1)
            {
                return 1;
            }
            else { setError(); return 0; }
        }

        //LSTVAL -> , VALLAST LSTVAL1Linha 
        int LSTVAL()
        {
            if (tk.tipo == TipoTk.TkVirgula)
            {// ,
                tk = lex.getToken();
                if (VALLAST() == 1)
                {
                    if (LSTVAL() == 1)
                    {
                        return 1;
                    }
                    else { return 0; }
                }
                else { return 0; }
            }
            else { return 1; }
        }


        //COMANDOIF
        //CMDIF -> if EXPCMP : LSTCMD ELSE 
        int CMDIF()
        {
            if (tk.tipo == TipoTk.TkSe)
            {// if
                tk = lex.getToken();
                if (EXPCMP() == 1)
                {
                    if (tk.tipo == TipoTk.TkDoisPontos)
                    {// :
                        tk = lex.getToken();
                        if (tk.tipo == TipoTk.TkIdent)
                        {// ident
                            tk = lex.getToken();
                            if (LSTCMD() == 1)
                            {
                                if (tk.tipo == TipoTk.TkDesident)
                                {// desident
                                    tk = lex.getToken();
                                    if (ELSE() == 1)
                                    {
                                        return 1;
                                    }
                                    else { return 0; }
                                }
                                else { setError(); return 0; }
                            }
                            else { return 0; }
                        }
                        else { setError(); return 0; }
                    }
                    else { setError(); return 0; }
                }
                else { return 0; }
            }
            else { setError(); return 0; }
        }

        //ELSE -> else : LSTCMD | elif EXPCMP : LSTCMD ELSE | ? 
        int ELSE()
        {
            if (tk.tipo == TipoTk.TkSenao)
            {// else
                tk = lex.getToken();
                if (tk.tipo == TipoTk.TkDoisPontos)
                {// :
                    tk = lex.getToken();
                    if (tk.tipo == TipoTk.TkIdent)
                    {// ident
                        tk = lex.getToken();
                        if (LSTCMD() == 1)
                        {
                            if (tk.tipo == TipoTk.TkDesident)
                            {// desident
                                tk = lex.getToken();
                                return 1;
                            }
                            else { setError(); return 0; }
                        }
                        else { return 0; }
                    }
                    else { setError(); return 0; }
                }
                else { setError(); return 0; }
            }
            else if (tk.tipo == TipoTk.TkSenaoSe)
            {// elif
                tk = lex.getToken();
                if (EXPCMP() == 1)
                {
                    if (tk.tipo == TipoTk.TkDoisPontos)
                    {// :
                        tk = lex.getToken();
                        if (tk.tipo == TipoTk.TkIdent)
                        {// ident
                            tk = lex.getToken();
                            if (LSTCMD() == 1)
                            {
                                if (tk.tipo == TipoTk.TkDesident)
                                {// desident
                                    tk = lex.getToken();
                                    if (ELSE() == 1)
                                    {
                                        return 1;
                                    }
                                    else { return 0; }
                                }
                                else { setError(); return 0; }
                            }
                            else { return 0; }
                        }
                        else { setError(); return 0; }
                    }
                    else { setError(); return 0; }
                }
                else { return 0; }
            }
            else { return 1; }
        }

        //ARRAY
        //ARRAYVALS -> OPEXPP4 | charconst 
        int ARRAYVALS()
        {
            if (OPEXPP4() == 1)
            {
                return 1;
            }
            else if (tk.tipo == TipoTk.TkCharConst)
            {// charconst
                tk = lex.getToken();
                return 1;
            }
            else { setError(); return 0; }
        }

        //ARRAY -> [ ARRAYVALS ARRAY1Linha 
        int ARRAY()
        {
            if (tk.tipo == TipoTk.TkAbreColchete)
            {// [
                tk = lex.getToken();
                if (ARRAYVALS() == 1)
                {
                    if (ARRAY1Linha() == 1)
                    {
                        return 1;
                    }
                    else { return 0; }
                }
                else { return 0; }
            }
            else { return 0; }
        }


        //ARRAY1Linha -> ] | LSTVAL ] 
        int ARRAY1Linha()
        {
            if (tk.tipo == TipoTk.TkFechaColchete)
            {// ]
                tk = lex.getToken();
                return 1;
            }
            else if (LSTVAL() == 1)
            {
                if (tk.tipo == TipoTk.TkFechaColchete)
                {// ]
                    tk = lex.getToken();
                    return 1;
                }
                else { setError(); return 0; }
            }
            else { setError(); return 0; }
        }

        //FOR
        //VARFOR -> id | ARRAY 
        int VARFOR()
        {
            if (tk.tipo == TipoTk.TkId)
            {// id
                tk = lex.getToken();
                return 1;
            }
            else if (ARRAY() == 1)
            {
                return 1;
            }
            else if (RANGE() == 1)
            {
                return 1;
            }
            else { setError(); return 0; }
        }

        //CMDFOR -> for id in VARFOR : LSTCMD 
        int CMDFOR()
        {
            if (tk.tipo == TipoTk.TkFor)
            {// for
                tk = lex.getToken();
                if (tk.tipo == TipoTk.TkId)
                {// id
                    tk = lex.getToken();
                    if (tk.tipo == TipoTk.TkIn)
                    {// in
                        tk = lex.getToken();
                        if (VARFOR() == 1)
                        {
                            if (tk.tipo == TipoTk.TkDoisPontos)
                            {// :
                                tk = lex.getToken();
                                if (tk.tipo == TipoTk.TkIdent)
                                {// ident
                                    tk = lex.getToken();
                                    if (LSTCMD() == 1)
                                    {
                                        if (tk.tipo == TipoTk.TkDesident)
                                        {// desident
                                            tk = lex.getToken();
                                            return 1;
                                        }
                                        else { setError(); return 0; }
                                    }
                                    else { return 0; }
                                }
                                else { setError(); return 0; }
                            }
                            else { setError(); return 0; }
                        }
                        else { return 0; }
                    }
                    else { setError(); return 0; }
                }
                else { return 0; }
            }
            else { setError(); return 0; }
        }

        //WHILE
        //CMDWHILE -> while EXPCMP : LSTCMD 
        int CMDWHILE()
        {
            if (tk.tipo == TipoTk.TkEnquanto)
            {// while
                tk = lex.getToken();
                if (EXPCMP() == 1)
                {
                    if (tk.tipo == TipoTk.TkDoisPontos)
                    {// :
                        tk = lex.getToken();
                        if (tk.tipo == TipoTk.TkIdent)
                        {// ident
                            tk = lex.getToken();
                            if (LSTCMD() == 1)
                            {
                                if (tk.tipo == TipoTk.TkDesident)
                                {// desident
                                    tk = lex.getToken();
                                    return 1;
                                }
                                else { setError(); return 0; }
                            }
                            else { return 0; }
                        }
                        else { setError(); return 0; }
                    }
                    else { setError(); return 0; }
                }
                else { return 0; }
            }
            else { setError(); return 0; }
        }

        //FUNÇÃO
        //ARGS -> ( ARGS1Linha 
        int ARGS()
        {
            if (tk.tipo == TipoTk.TkAbreParenteses)
            {// (
                tk = lex.getToken();
                if (ARGS1Linha() == 1)
                {
                    return 1;
                }
                else { return 0; }
            }
            else { setError(); return 0; }
        }

        //ARGS1Linha -> ) | id ARGS2Linha 
        int ARGS1Linha()
        {
            if (tk.tipo == TipoTk.TkFechaParenteses)
            {// )
                tk = lex.getToken();
                return 1;
            }
            else if (tk.tipo == TipoTk.TkId)
            {// id
                tk = lex.getToken();
                if (ARGS2Linha() == 1)
                {
                    return 1;
                }
                else { return 0; }
            }
            else if (CMDCST() == 1)
            {
                if (ARGS2Linha() == 1)
                {
                    return 1;
                }
                else { return 0; }
            }
            else { setError(); return 0; }
        }

        //ARGS2Linha -> ) | LSTVAL ) 
        int ARGS2Linha()
        {
            if (tk.tipo == TipoTk.TkFechaParenteses)
            {// )
                tk = lex.getToken();
                return 1;
            }
            else if (LSTVAL() == 1)
            {
                if (tk.tipo == TipoTk.TkFechaParenteses)
                {// )
                    tk = lex.getToken();
                    return 1;
                }
                else { setError(); return 0; }
            }
            else { setError(); return 0; }
        }

        //CMDFUNC -> def id ARGS : LSTCMD 
        int CMDFUNC()
        {
            if (tk.tipo == TipoTk.TkDef)
            {// def
                tk = lex.getToken();
                if (tk.tipo == TipoTk.TkId)
                {// id
                    tk = lex.getToken();


                    if (ARGS() == 1)
                    {
                        if (tk.tipo == TipoTk.TkDoisPontos)
                        {// :
                            tk = lex.getToken();
                            if (tk.tipo == TipoTk.TkIdent)
                            {// ident
                                tk = lex.getToken();
                                if (LSTCMD() == 1)
                                {
                                    if (tk.tipo == TipoTk.TkDesident)
                                    {// desident
                                        tk = lex.getToken();
                                        return 1;
                                    }
                                    else { setError(); return 0; }
                                }
                                else { return 0; }
                            }
                            else { setError(); return 0; }
                        }
                        else { setError(); return 0; }
                    }
                    else { return 0; }
                }
                else { setError(); return 0; }
            }
            else { setError(); return 0; }
        }

        //RETORNO
        //CMDRETURN -> return RVAR 
        int CMDRETURN()
        {
            if (tk.tipo == TipoTk.TkReturn)
            {// return
                tk = lex.getToken();
                if (EXPP1() == 1)
                {
                    return 1;
                }
                else { return 0; }
            }
            else { setError(); return 0; }
        }

        //EXECUÇÃO DE FUNÇÃO
        //EXECFUNC -> id ARGS 
        int EXECFUNC()
        {
            if (tk.tipo == TipoTk.TkId)
            {// id
                tk = lex.getToken();
                if (ARGS() == 1)
                {
                    return 1;
                }
                else { return 0; }
            }
            else { setError(); return 0; }
        }

        //Operação de cast
        //OPCST -> int | float | double | bool | str | char 
        int OPCST()
        {
            if (tk.tipo == TipoTk.TkInt)
            {// int
                tk = lex.getToken();
                return 1;
            }
            else if (tk.tipo == TipoTk.TkFloat)
            {// float
                tk = lex.getToken();
                return 1;
            }
            else if (tk.tipo == TipoTk.TkDouble)
            {// double
                tk = lex.getToken();
                return 1;
            }
            else if (tk.tipo == TipoTk.TkBool)
            {// bool
                tk = lex.getToken();
                return 1;
            }
            else if (tk.tipo == TipoTk.TkStr)
            {// str
                tk = lex.getToken();
                return 1;
            }
            else if (tk.tipo == TipoTk.TkChar)
            {// char
                tk = lex.getToken();
                return 1;
            }
            else { setError(); return 0; }
        }


        //CMDCST -> OPCST ( id ) 
        int CMDCST()
        {
            if (OPCST() == 1)
            {
                if (tk.tipo == TipoTk.TkAbreParenteses)
                {// (
                    tk = lex.getToken();
                    if (tk.tipo == TipoTk.TkId)
                    {// id
                        tk = lex.getToken();
                        if (tk.tipo == TipoTk.TkFechaParenteses)
                        {// )
                            tk = lex.getToken();
                            return 1;
                        }
                        else { setError(); return 0; }
                    }
                    else { setError(); return 0; }
                }
                else { setError(); return 0; }
            }
            else { return 0; }
        }

        //range
        //RANGE -> range ( START stop STEP ) 
        int RANGE()
        {
            if (tk.tipo == TipoTk.TkRange)
            {// range
                tk = lex.getToken();
                if (tk.tipo == TipoTk.TkAbreParenteses)
                {// (
                    tk = lex.getToken();
                    lex.marcaPosToken(tk);
                    if (START() == 1)
                    {
                        lex.Peek();
                        if (OPEXPP4() == 1)
                        {
                            if (STEP() == 1)
                            {
                                if (tk.tipo == TipoTk.TkFechaParenteses)
                                {// )
                                    tk = lex.getToken();
                                    return 1;
                                }
                                else { setError(); return 0; }
                            }
                            else { return 0; }
                        }
                        else { return 0; }
                    }
                    else
                    {
                        tk = lex.restauraPosToken();
                        if (OPEXPP4() == 1)
                        {
                            if (STEP() == 1)
                            {
                                if (tk.tipo == TipoTk.TkFechaParenteses)
                                {// )
                                    tk = lex.getToken();
                                    return 1;
                                }
                                else { setError(); return 0; }
                            }
                            else { return 0; }
                        }
                        else { return 0; }
                    }

                }
                else { setError(); return 0; }
            }
            else { setError(); return 0; }
        }

        //START -> OPEXPP4 , | ? 
        int START()
        {
            if (OPEXPP4() == 1)
            {
                if (tk.tipo == TipoTk.TkVirgula)
                {// ,
                    tk = lex.getToken();
                    return 1;
                }
                else { setError(); return 0; }
            }
            else { return 1; }
        }

        //STEP -> , OPEXPP4 | ? 
        int STEP()
        {
            if (tk.tipo == TipoTk.TkVirgula)
            {// ,
                tk = lex.getToken();
                if (OPEXPP4() == 1)
                {
                    return 1;
                }
                else { return 0; }
            }
            else { return 1; }
        }
    }
}
