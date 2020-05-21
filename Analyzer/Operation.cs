using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzer
{
    public class Operation
    {
        public String operand1;

        public int operand1Column;

        public String operand2;

        public int operand2Column;

        public TipoTk currentOperator;

        public OperationPrecedence precedence;

        // Nullable
        public int? result = null;

        public Operation(String operand1, String operand2, int operand1Column, int operand2Column, TipoTk currentOperator, OperationPrecedence precedence)
        {
            this.operand1 = operand1;

            this.operand2 = operand2;

            this.operand1Column = operand1Column;

            this.operand2Column = operand2Column;

            this.currentOperator = currentOperator;

            this.precedence = precedence;
        }        
    }
}
