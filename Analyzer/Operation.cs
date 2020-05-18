using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzer
{
    public class Operation
    {
        private String operand1;

        private String operand2;
        
        private int currentOperator;
        public Operation(String operand1, String operand2, int currentOperator)
        {
            this.operand1 = operand1;

            this.operand2 = operand2;

            this.currentOperator = currentOperator;
        }        
    }
}
