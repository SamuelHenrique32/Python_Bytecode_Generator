using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzer
{
    public class BytecodeRegister
    {
        public int lineInGeneratedBytecode { get; set; }

        public int lineInFile { get; set; }

        public int offset { get; set; }

        public int opCode { get; set; }

        public int stackPos { get; set; }

        public String preview { get; set; }

        public int? indentationLevel = null; 
    }
}
