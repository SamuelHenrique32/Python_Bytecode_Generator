using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzer
{
    public class Token
    {
        public int linha { get; set; }
        public int coluna { get; set; }
        public TipoTk tipo { get; set; }
        public string valor { get; set; }
    }
}
