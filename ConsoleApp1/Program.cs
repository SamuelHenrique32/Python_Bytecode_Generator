using Analyzer;
using System;
using System.IO;

namespace ConsoleApp1
{
    class Program
    {
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
                    Environment.Exit(0);
                    break;
                default:
                    break;

            }

        }

        public static void Lexico()
        {
            var a = new Analisador();
            Console.Clear();
            Console.WriteLine("Digite o caminho do arquivo");
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
            Console.WriteLine("Tokens Reconhecidos: ");

            a.PrintTokens();
            Console.ReadLine();
            Console.Clear();
            Init();
        }

        public static void Sintatico()
        {
            var s = new Sintatico();
            Console.Clear();
            Console.WriteLine("Digite o caminho do arquivo");
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
            Console.WriteLine("3 - Sair");
        }
    }
}
