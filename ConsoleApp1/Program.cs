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
            Init();
        }

        public static void Init()
        {
            PrintMenu();
            var n = Console.ReadLine();
            var num = int.Parse(n);

            switch (num)
            {
                case 1:
                    Lexico();
                    break;

                case 2:
                    Sintatico();
                    break;

                case 3:
                    BytecodeGenerator();
                    break;

                case 4:
                    Environment.Exit(0);
                    break;

                default:
                    break;
            }
        }

        private static void BytecodeGenerator()
        {
            var bytecodeGenerator = new BytecodeGenerator();
            Console.Clear();
            Console.WriteLine("Digite o caminho do arquivo:");
            var filePath = Console.ReadLine();

            int counter = 1;
            string line;

            using (StreamReader file = new StreamReader(filePath))
            {
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Length > 0)
                    {
                        //bytecodeGenerator.codigo = line;
                    }
                    counter++;

                }
            }
        }

        public static void Lexico()
        {
            var a = new Analisador();
            Console.Clear();
            Console.WriteLine("Digite o caminho do arquivo:");
            var c = Console.ReadLine();

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

            a.PrintTokens();
            Console.ReadLine();
            Console.Clear();
            Init();
        }

        public static void Sintatico()
        {
            var s = new Sintatico();
            Console.Clear();
            Console.WriteLine("Digite o caminho do arquivo:");
            var c = Console.ReadLine();

            using (StreamReader file = new StreamReader(c))
            {
                var text = file.ReadToEnd();
                int result = s.Analizar(text);
                if (result == 1)
                {
                    Console.WriteLine("Reconhecido com sucesso");
                }
                else
                {
                    s.PrintErro();
                }

            }

            Console.ReadLine();
            Console.Clear();
            Init();

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
