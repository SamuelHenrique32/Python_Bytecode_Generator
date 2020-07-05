using Analyzer;
using System;
using System.IO;

namespace ConsoleApp1
{
    class Program
    {
        BytecodeGenerator bytecodeGenerator = new BytecodeGenerator();

        static void Main(string[] args)
        {
            BytecodeGenerator bytecodeGenerator = new BytecodeGenerator();

            Init(bytecodeGenerator);
        }

        public static void Init(BytecodeGenerator bytecodeGenerator)
        {
            PrintMenu();
            var n = Console.ReadLine();
            var num = int.Parse(n);

            switch (num)
            {
                case 1:
                    Lexico(bytecodeGenerator, false, null);
                    break;

                case 2:
                    Sintatico(bytecodeGenerator, false, null);
                    break;

                case 3:
                    BytecodeGenerator(bytecodeGenerator);
                    break;

                case 4:
                    Environment.Exit(0);
                    break;

                default:
                    break;
            }
        }

        private static void BytecodeGenerator(BytecodeGenerator bytecodeGenerator)
        {
            Console.Clear();

            if(!String.IsNullOrEmpty(bytecodeGenerator.filePath))
            {
                Console.WriteLine("Utilizando último caminho de arquivo informado\n\n");
            }
            else
            {
                Console.WriteLine("Digite o caminho do arquivo:");
                bytecodeGenerator.filePath = Console.ReadLine();
            }

            // If lexyc was not executed yet
            if(!bytecodeGenerator.lexicAnalyzed)
            {
                Lexico(bytecodeGenerator, true, bytecodeGenerator.filePath);
            }

            if(!bytecodeGenerator.syntacticAnalyzed)
            {
                Sintatico(bytecodeGenerator, true, bytecodeGenerator.filePath);
            }

            if(bytecodeGenerator.correctSyntax)
            {
                bytecodeGenerator.generateBytecode();
            }
            else
            {
                Console.WriteLine("\nErro de sintaxe foi identificado, bytecode não gerado");
            }

            Console.WriteLine("\n\nPressione tecla");
            Console.ReadLine();
            Console.Clear();
            Init(bytecodeGenerator);
        }

        public static void Lexico(BytecodeGenerator bytecodeGenerator, Boolean isCalledFromBytecodeGenerator, String filePath)
        {
            var a = new Analisador();
            var c = "";

            Console.Clear();

            if (String.IsNullOrEmpty(filePath))
            {
                Console.WriteLine("Digite o caminho do arquivo:");

                c = Console.ReadLine();

                bytecodeGenerator.filePath = c;
            }
            else
            {
                c = filePath;
            }

            int counter = 1;
            string line;

            using (StreamReader file = new StreamReader(c))
            {
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Length > 0)
                    {
                        a.linha = counter;
                        a.codigo = line;
                        a.Analizar();
                    }
                    counter++;
                }
            }
            a.lastIndentation();

            bytecodeGenerator.lexicalTokens = a.tks;

            bytecodeGenerator.lexicAnalyzed = true;

            if (!isCalledFromBytecodeGenerator)
            {
                a.PrintTokens();
                Console.WriteLine("\nPressione tecla");
                Console.ReadLine();
                Console.Clear();
                Init(bytecodeGenerator);
            }            
        }

        public static void Sintatico(BytecodeGenerator bytecodeGenerator, Boolean isCalledFromBytecodeGenerator, String filePath)
        {
            var s = new Sintatico();
            var c = "";

            Console.Clear();

            if (String.IsNullOrEmpty(filePath))
            {
                Console.WriteLine("Digite o caminho do arquivo:");

                c = Console.ReadLine();

                bytecodeGenerator.filePath = c;
            }
            else
            {
                c = filePath;
            }

            using (StreamReader file = new StreamReader(c))
            {
                var text = file.ReadToEnd();
                int result = s.Analizar(text);
                if (result == 1)
                {
                    Console.WriteLine("\nReconhecido com sucesso");

                    bytecodeGenerator.correctSyntax = true;
                }
                else
                {
                    s.PrintErro();

                    bytecodeGenerator.correctSyntax = false;
                }
            }

            bytecodeGenerator.syntacticAnalyzed = true;

            if(!isCalledFromBytecodeGenerator)
            {
                Console.WriteLine("\nPressione tecla");
                Console.ReadLine();
                Console.Clear();
                Init(bytecodeGenerator);
            }
        }

        public static void PrintMenu()
        {
            Console.WriteLine("1 - Léxico");
            Console.WriteLine("2 - Sintático");
            Console.WriteLine("3 - Bytecode");
            Console.WriteLine("4 - Sair");
            Console.Write("Opcao Escolhida: ");
        }
    }
}