using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzer
{    public enum OperationPrecedence
    {
        TK_MUL_PRECEDENCE = 2,
        TK_DIV_PRECEDENCE = 2,
        TK_MUL_REDUCED_PRECEDENCE = 2,
        TK_DIV_REDUCED_PRECEDENCE = 2,
        TK_ADD_PRECEDENCE = 1,
        TK_SUB_PRECEDENCE = 1,
        TK_ADD_REDUCED_PRECEDENCE = 1,
        TK_SUB_REDUCED_PRECEDENCE = 1,
        TK_EQUAL_PRECEDENCE = 0,
        TK_DIFF_PRECEDENCE = 0,
        TK_LESS_THAN_PRECEDENCE = 0,
        TK_BIGGER_THAN_PRECEDENCE = 0,
        TK_ATTRIBUTION_PRECEDENCE = -1
    }
}
