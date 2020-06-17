using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzer
{
    public class IndentationLevel
    {
        public TipoTk tipoTk;

        // 1=ident 0=desident
        public Boolean type;

        public int? initialLine = null;

        public int? finalLine = null;
    }
}
