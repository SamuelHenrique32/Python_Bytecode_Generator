using Analyzer;
using System;
using System.Collections.Generic;
using System.IO;

namespace ConsoleApp1
{
    internal class BytecodeGenerator
    {
        private string filePath { get; set; }

        public List<Token> lexicalTokens { get => lexicalTokens; set => lexicalTokens = value; }

        public BytecodeGenerator()
        {
        }

        public void generateBytecode()
        {
            var analyzer = new Analisador();

            int counter = 1;
            string line;

            using (StreamReader file = new StreamReader(filePath))
            {
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Length > 0)
                    {
                        analyzer.linha = counter;
                        analyzer.codigo = line;
                        analyzer.Analizar();
                    }
                    counter++;

                }
            }
            analyzer.lastIndentation();

            lexicalTokens = analyzer.tks;

            foreach (var t in lexicalTokens)
            {
                Console.WriteLine("bytecode");
                Console.WriteLine("Token: " + t.tipo.ToString() + "\tLexema: " + t.valor + "\t Linha: " + t.linha + "\t Coluna: " + t.coluna);
            }            
        }
    }
}