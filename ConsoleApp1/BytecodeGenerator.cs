using Analyzer;
using System;
using System.Collections.Generic;
using System.IO;

namespace ConsoleApp1
{
    internal class BytecodeGenerator
    {
        public string filePath { get; set; }

        public List<Token> lexicalTokens = new List<Token>();

        public Boolean lexicAnalyzed = false;

        public Boolean syntacticAnalyzed = false;

        public Boolean correctSyntax;

        public BytecodeGenerator()
        {
        }

        public void generateBytecode()
        {
            foreach (var t in lexicalTokens)
            {
                //Console.WriteLine("Token: " + t.tipo.ToString() + "\tLexema: " + t.valor + "\t Linha: " + t.linha + "\t Coluna: " + t.coluna);
            }            
        }
    }
}