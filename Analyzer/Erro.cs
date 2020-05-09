using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzer
{
    public class Erro
    {
        public int linha { get; set; }
        public int coluna { get; set; }

        public Erro(int linha, int coluna)
        {
            this.linha = linha;
            this.coluna = coluna;
        }

        public void PrintError()
        {
            Console.WriteLine("Ocorreu um erro Linha: " + linha.ToString() + " Coluna: " + coluna.ToString());
        }
    }
}
