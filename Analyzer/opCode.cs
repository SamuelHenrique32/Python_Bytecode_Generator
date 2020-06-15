using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzer
{
    public enum OpCode
    {
        LOAD_CONST,
        LOAD_NAME,
        STORE_FAST,
        STORE_NAME,        
        BINARY_ADD,
        BINARY_SUBTRACT,
        BINARY_MULTIPLY,
        BINARY_TRUE_DIVIDE,
        COMPARE_OP,
        POP_JUMP_IF_FALSE,
        RETURN_VALUE,
        JUMP_FORWARD,
        SETUP_LOOP,
        JUMP_ABSOLUTE,
        POP_BLOCK,
        LOAD_GLOBAL,
        CALL_FUNCTION,
        GET_ITER,
        FOR_ITER,
        INPLACE_ADD,
        INPLACE_SUBTRACT,
        INPLACE_MULTIPLY,
        INPLACE_TRUE_DIVIDE
    }
}
