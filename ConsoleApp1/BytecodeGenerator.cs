using Analyzer;
using ConsoleApp1;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using System.Threading;

namespace Analyzer
{
    internal class BytecodeGenerator
    {
        public string filePath { get; set; }

        public List<Token> lexicalTokens = new List<Token>();

        // Stores the variables
        //public HashSet<Symbol> symbolsTable = new HashSet<Symbol>(); 
        public List<Symbol> symbolsTable = new List<Symbol>();

        public List<BytecodeRegister> bytecodeRegisters = new List<BytecodeRegister>();

        public List<LineType> lineTypes = new List<LineType>();

        public Boolean lexicAnalyzed = false;

        public Boolean syntacticAnalyzed = false;

        public Boolean correctSyntax;

        public Boolean currentIDAlreadyInSymbolsTable = false;

        public int currentLineInFile = 0;

        public int currentLineInGeneratedBytecode = 0;

        public int currentOffset = 0;

        public int lineinFileGlobalToAddLoadName = 0;

        //--------------------------------------------------------------------------------------
        // Operations
        public Boolean operationAssignmentInProgress = false;

        public List<Operation> operationsInCurrentLine = new List<Operation>();

        public int operationRelationalPosInCurrentLine = 0;

        //--------------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------
        // Arithmetical operations
        int? arithmeticalOperand1 = null;
        int? arithmeticalOperand2 = null;

        int? arithmeticalIdentifierOperand1 = null;
        int? arithmeticalIdentifierOperand2 = null;

        //--------------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------
        // Elements in line counter
        public int addOperatorCounter = 0;

        public int subtractionOperatorCounter = 0;

        public int multiplicationOperatorCounter = 0;

        public int divOperatorCounter = 0;

        public int reducedAddOperatorCounter = 0;

        public int reducedSubtractionOperatorCounter = 0;

        public int reducedMultiplicationOperatorCounter = 0;

        public int reducedDivOperatorCounter = 0;

        public int equalOperatorCounter = 0;

        public int difOperatorCounter = 0;

        public int lessThanOperatorCounter = 0;

        public int biggerThanOperatorCounter = 0;

        public int attribuitionOperatorCounter = 0;

        public int idElementsCounter = 0;

        public int ifElementCounter = 0;

        public int desidentElementCounter = 0;

        public int identElementCounter = 0;

        public int elseElementCounter = 0;

        public int elseIfElementCounter = 0;

        public int whileElementCounter = 0;

        public int rangeElementCouter = 0;

        //--------------------------------------------------------------------------------------
        // Elements in line counter for if statements
        public int addOperatorCounterLeft = 0;

        public int addOperatorCounterRight = 0;

        public int subtractionOperatorCounterLeft = 0;

        public int subtractionOperatorCounterRight = 0;

        public int multiplicationOperatorCounterLeft = 0;

        public int multiplicationOperatorCounterRight = 0;

        public int divOperatorCounterLeft = 0;

        public int divOperatorCounterRight = 0;

        //--------------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------
        // Variables to control the bytecode mounting
        public Boolean printerLoadConst = false;

        public Boolean printerSimpleAtrib = false;

        public Boolean printerFoundAnIdentifier = false;

        public Boolean printerLoadName = false;

        public Boolean printerShowOperationType = false;

        public Boolean printerShowCompareOp = false;

        public Boolean printerPopJumpIfFalse = false;

        public int? printerLastExpressionResult = null;

        // Stores the most right operation index already calculed with precedence 1
        public int printerMoreToRightOperationIndexPrecedence1 = 0;

        // Stores the most right operation index already calculed with precedence 2
        public int printerMoreToRightOperationIndexPrecedence2 = 0;

        public Stack<int?> printerOperationsStack = new Stack<int?>();

        public int? printerCompElementPosition = null;

        public TipoTk printerCompElement;

        public int? printerLastCompLine = null;

        public int printerCurrentOffset = 0;

        public int printerCurrentIfIdentationLevel = 0;

        public Stack<IdentDesidentLevel> printerIdentationRegisters = new Stack<IdentDesidentLevel>();

        public Stack<IdentDesidentLevel> printerDesidentRegisters = new Stack<IdentDesidentLevel>();

        public IdentDesidentLevel printerCurrentIdentationLevel = null;

        public IdentDesidentLevel printerCurrentDesidentLevel = null;

        public int printerLastLineWithElse = -1;

        public int printerLastLineWithElseIf = -1;

        public int printerOffsetIndexToInsertInPopJumpIfFalse = -1;

        public Boolean printerWaitingForElseDesident = false;

        public Boolean printerWaitingOffsetForJumpForward = false;

        public int printerIndexToGetOffsetForJumpForward = -1;

        public Boolean printerWaitingForElseAfterElseIf = false;

        public int printerLineInByteCodeRegisterWithElseDesident = -1;

        public Boolean printerCountLinesInElseIf = false;
        
        public int printerQuantityOfLinesInElseIf = 0;

        public Boolean printerWhileInProgress = false;

        public Boolean printerForInProgress = false;

        public String printerForTopLimit = null;

        public Boolean printerFoundOpenParenthesis = false;

        public String printerVariableToRange = null;
        //--------------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------
        // Lynes type
        public Boolean lyneTypeIfIdent = false;

        public Boolean lyneTypeElseIfIdent = false;

        public Boolean lyneTypeElseIdent = false;

        public Boolean lyneTypeWhileIdent = false;

        public Boolean lyneTypeForIdent = false;

        public Boolean lyneTypeLastLineHasIf = false;

        public Boolean lyneTypeLastLineHasElseIf = false;

        public Boolean lyneTypeLastLineHasElse = false;

        public Boolean lyneTypeLastLineHasWhile = false;

        public Boolean lyneTypeLastLineHasFor = false;

        public Boolean lyneTypeAddEnable = true;

        //--------------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------
        // Indentatation
        public List<IndentationLevel> ifIndentationLevel = new List<IndentationLevel>();

        public List<IndentationLevel> elseIfIndentationLevel = new List<IndentationLevel>();

        public List<IndentationLevel> elseIndentationLevel = new List<IndentationLevel>();        

        public List<IndentationLevel> forIndentationLevel = new List<IndentationLevel>();

        public List<IndentationLevel> whileIndentationLevel = new List<IndentationLevel>();

        //--------------------------------------------------------------------------------------

        public BytecodeGenerator()
        {
        }

        public void handleAddOfIdentationLevels(TipoTk tipoTk, int line, Boolean type)
        {
            IndentationLevel indentationLevel = new IndentationLevel();

            switch (tipoTk)
            {
                case TipoTk.TkSe:
                    if (type)
                    {
                        indentationLevel.tipoTk = TipoTk.TkSe;
                        indentationLevel.initialLine = line;
                        ifIndentationLevel.Add(indentationLevel);
                    }
                    else
                    {
                        updateIndentationLevel(tipoTk, line);
                    }
                    break;

                case TipoTk.TkSenaoSe:
                    if (type)
                    {
                        indentationLevel.tipoTk = TipoTk.TkSenaoSe;
                        indentationLevel.initialLine = line;
                        elseIfIndentationLevel.Add(indentationLevel);
                    }
                    else
                    {
                        updateIndentationLevel(tipoTk, line);
                    }
                    break;

                case TipoTk.TkSenao:
                    if (type)
                    {
                        indentationLevel.tipoTk = TipoTk.TkSenao;
                        indentationLevel.initialLine = line;
                        elseIndentationLevel.Add(indentationLevel);
                    }
                    else
                    {
                        updateIndentationLevel(tipoTk, line);
                    }
                    break;

                case TipoTk.TkFor:
                    if (type)
                    {
                        indentationLevel.tipoTk = TipoTk.TkFor;
                        indentationLevel.initialLine = line;
                        forIndentationLevel.Add(indentationLevel);
                    }
                    else
                    {
                        updateIndentationLevel(tipoTk, line);
                    }
                    break;

                case TipoTk.TkEnquanto:
                    if (type)
                    {
                        indentationLevel.tipoTk = TipoTk.TkEnquanto;
                        indentationLevel.initialLine = line;
                        whileIndentationLevel.Add(indentationLevel);
                    }
                    else
                    {
                        updateIndentationLevel(tipoTk, line);
                    }
                    break;
            }
        }

        public void updateIndentationLevel(TipoTk tipoTk, int line)
        {
            switch (tipoTk)
            {
                case TipoTk.TkSe:

                    for(int i=ifIndentationLevel.Count-1; i>0; i--)
                    {
                        if (ifIndentationLevel[i].finalLine == null)
                        {
                            ifIndentationLevel[i].finalLine = line;

                            break;
                        }
                    }

                    break;

                case TipoTk.TkSenaoSe:

                    for (int i = elseIfIndentationLevel.Count - 1; i > 0; i--)
                    {
                        if (elseIfIndentationLevel[i].finalLine == null)
                        {
                            elseIfIndentationLevel[i].finalLine = line;

                            break;
                        }
                    }

                    break;

                case TipoTk.TkSenao:

                    for (int i = elseIndentationLevel.Count - 1; i > 0; i--)
                    {
                        if (elseIndentationLevel[i].finalLine == null)
                        {
                            elseIndentationLevel[i].finalLine = line;

                            break;
                        }
                    }

                    break;

                case TipoTk.TkFor:

                    for (int i = forIndentationLevel.Count - 1; i > 0; i--)
                    {
                        if (forIndentationLevel[i].finalLine == null)
                        {
                            forIndentationLevel[i].finalLine = line;

                            break;
                        }
                    }

                    break;

                case TipoTk.TkEnquanto:

                    for (int i = whileIndentationLevel.Count - 1; i > 0; i--)
                    {
                        if (whileIndentationLevel[i].finalLine == null)
                        {
                            whileIndentationLevel[i].finalLine = line;

                            break;
                        }
                    }

                    break;
            }
        }

        public int getQuantityOfOperationsWithMulPrecedence()
        {
            //return (multiplicationOperatorCounter + divOperatorCounter + reducedMultiplicationOperatorCounter + reducedDivOperatorCounter);
            return (multiplicationOperatorCounter + divOperatorCounter);
        }

        public int getQuantityOfOperationsWithAddPrecedence()
        {
            //return (addOperatorCounter + subtractionOperatorCounter + reducedAddOperatorCounter + reducedSubtractionOperatorCounter);
            return (addOperatorCounter + subtractionOperatorCounter);
        }

        public int getQuantityOfOperationsWithMulPrecedenceIfStatement(Boolean leftRightOption)
        {
            // Right
            if (leftRightOption)
            {
                return (multiplicationOperatorCounterRight + divOperatorCounterRight);
            }
            // Left
            else
            {
                return (multiplicationOperatorCounterLeft + divOperatorCounterLeft);
            }
        }

        public int getQuantityOfOperationsWithAddPrecedenceIfStatement(Boolean leftRightOption)
        {
            // Right
            if (leftRightOption)
            {
                return (addOperatorCounterRight + subtractionOperatorCounterRight);
            }
            // Left
            else
            {
                return (addOperatorCounterLeft + subtractionOperatorCounterLeft);
            }
        }

        public int getLastLineInFile()
        {
            return lexicalTokens[lexicalTokens.Count - 1].linha;
        }

        public Boolean verifyIfSymbolsExistsInTable(Token token)
        {
            for (int i = 0; i < symbolsTable.Count; i++)
            {
                if (symbolsTable[i].identifier.ToString().Equals(token.valor))
                {
                    return true;
                }
            }

            return false;
        }

        public void updateSymbolsTable(Token token)
        {
            for (int i = 0; i < symbolsTable.Count; i++)
            {
                // Add line and column of the current element and update value
                if (symbolsTable[i].identifier.ToString().Equals(token.valor))
                {
                    symbolsTable[i].lines.Add(token.linha);

                    symbolsTable[i].columns.Add(token.coluna);

                    break;
                }
            }
        }

        public void verifyTkId(Token token)
        {
            // Add first element of the table
            if (symbolsTable.Count == 0)
            {
                //Console.WriteLine("\nAdding first symbol\n");

                // Creates symbol
                Symbol symbolToAdd = new Symbol();
                symbolToAdd.identifier = token.valor;
                symbolToAdd.lines.Add(token.linha);
                symbolToAdd.columns.Add(token.coluna);

                // Add to table of symbols
                symbolsTable.Add(symbolToAdd);

                //symbolToString(symbolToAdd);

                currentIDAlreadyInSymbolsTable = false;

                // Add to generated bytecode
            }
            else
            {
                // Verify if already exists in symbolsTable
                if (verifyIfSymbolsExistsInTable(token))
                {
                    //Console.WriteLine("Symbol '" + token.valor + "' already exists in symbols table");

                    updateSymbolsTable(token);

                    currentIDAlreadyInSymbolsTable = true;
                }
                else
                {
                    // If there is an attribution and this id is not in the symbols table there is a semantic error
                    if (attribuitionOperatorCounter > 0)
                    {
                        Console.WriteLine("\nErro semântico, a variável '" + token.valor + "' não está definida\n");

                        Program.Init(this);

                    }

                    //Console.WriteLine("Adding symbol '" + token.valor + "'\n");

                    // Creates symbol
                    Symbol symbolToAdd = new Symbol();
                    symbolToAdd.identifier = token.valor;
                    symbolToAdd.lines.Add(token.linha);
                    symbolToAdd.columns.Add(token.coluna);

                    // Add to table of symbols
                    symbolsTable.Add(symbolToAdd);

                    currentIDAlreadyInSymbolsTable = false;
                }
            }
        }

        public int? getIdentifierValue(String identifier)
        {
            foreach (Symbol sym in symbolsTable)
            {
                if (sym.identifier.Equals(identifier))
                {
                    if (sym.value != null)
                    {
                        return sym.value;
                    }
                }
            }

            return null;
        }

        public Boolean getLastStackPosition(int? value)
        {
            if (printerOperationsStack.Count == 0)
            {
                return false;
            }

            if (printerOperationsStack.Peek() != value)
            {
                return false;
            }

            return true;

        }

        public void updateIdentifierValue(String identifier, int? value)
        {
            foreach (Symbol sym in symbolsTable)
            {
                if (sym.identifier.Equals(identifier))
                {
                    sym.value = value;

                    break;
                }
            }
        }

        public void symbolToString(Symbol sym)
        {
            Console.WriteLine("-----------------------------");

            Console.WriteLine("Symbol: " + sym.identifier + "\n");

            Console.WriteLine("Lines:");

            foreach (int line in sym.lines)
            {
                Console.WriteLine(line + "\n");
            }

            Console.WriteLine("Columns:");

            foreach (int column in sym.columns)
            {
                Console.WriteLine(column + "");
            }

            Console.WriteLine("-----------------------------\n");
        }

        public Token getNextToken(int currentPos)
        {
            return lexicalTokens[currentPos + 1];
        }

        public void mountBytecode(int currentLine, String identifier, Operation operation, int? value, Boolean mustAddIdentifier, Boolean restrictedOpCode)
        {
            int? identifierValue = 0;
            int? valueToAddInStack = 0;
            Boolean mustAddLoadConst = true;

            //--------------------------------------------------------------------------------------
            // Prepares LOAD_CONST

            int? valueToVerifyInStack = 0;

            if ((!printerSimpleAtrib) && ((printerAtribuition() && printerOperationsStack.Count >= 1) || (printerLoadName && restrictedOpCode)))
            {
                // Do nothing
            }
            else
            {
                if ((operation != null) && (!isIdentifier(operation.operand1)) && printerShowOperationType && printerOperationsStack.Count <= 1)
                {
                    valueToVerifyInStack = Int16.Parse(operation.operand1);

                    if (!printerOperationsStack.Contains(valueToVerifyInStack))
                    {
                        addSimpleLoadConst(valueToVerifyInStack, currentLine);
                    }
                }


                if ((operation != null) && (!isIdentifier(operation.operand2)))
                {
                    valueToVerifyInStack = Int16.Parse(operation.operand2);

                    if (!printerOperationsStack.Contains(valueToVerifyInStack))
                    {
                        printerLoadConst = true;
                    }
                }

                // Verify if it's necessary to load a constant
                if ((currentLine != printerLastCompLine) && (printerLoadConst || printerOperationsStack.Count <= 1))
                {
                    BytecodeRegister bytecodeRegisterCurrentToken = new BytecodeRegister();

                    printerLoadConst = false;

                    bytecodeRegisterCurrentToken.lineInGeneratedBytecode = currentLineInGeneratedBytecode++;

                    bytecodeRegisterCurrentToken.lineInFile = currentLine;

                    bytecodeRegisterCurrentToken.offset = currentOffset;

                    //this.currentOffset += 2;

                    bytecodeRegisterCurrentToken.opCode = (int)OpCode.LOAD_CONST;

                    // TODO
                    bytecodeRegisterCurrentToken.stackPos = 0;

                    // If it's a simple atrib, it's necessary to load the operand 2
                    if (printerSimpleAtrib)
                    {
                        printerSimpleAtrib = false;

                        bytecodeRegisterCurrentToken.preview = "(" + operationsInCurrentLine[searchForSimpleAtribIndex()].operand2.ToString() + ")";

                        valueToAddInStack = Int16.Parse(operationsInCurrentLine[searchForSimpleAtribIndex()].operand2);

                        identifierValue = Int16.Parse(operationsInCurrentLine[searchForSimpleAtribIndex()].operand2);

                        //handleStack(OpCode.LOAD_CONST, Int16.Parse(operationsInCurrentLine[operationsInCurrentLine.Count - 1].operand2));
                    }
                    else if (printerShowOperationType)
                    {
                        if (isIdentifier(operation.operand1))
                        {
                            /*valueToVerifyInStack = (int)getIdentifierValue(operation.operand1);

                            mustAddLoadConst = false;

                            printerLoadName = true;

                            identifier = operation.operand1;*/
                        }
                        else
                        {
                            valueToVerifyInStack = Int16.Parse(operation.operand1);

                            // Verify if the operand 1 is in stack
                            if (!printerOperationsStack.Contains(valueToVerifyInStack))
                            {
                                //addSimpleLoadConst(valueToVerifyInStack, currentLine);
                            }
                        }

                        if (isIdentifier(operation.operand2))
                        {
                            valueToVerifyInStack = (int)getIdentifierValue(operation.operand2);

                            mustAddLoadConst = false;

                            printerLoadName = true;

                            identifier = operation.operand2;
                        }
                        else
                        {
                            valueToVerifyInStack = Int16.Parse(operation.operand2);
                        }

                        if (!printerOperationsStack.Contains(valueToVerifyInStack))
                        {
                            bytecodeRegisterCurrentToken.preview = "(" + valueToVerifyInStack + ")";

                            valueToAddInStack = valueToVerifyInStack;

                            //identifierValue = Int16.Parse(operation.operand2);
                            identifierValue = valueToVerifyInStack;

                            //handleStack(OpCode.LOAD_CONST, Int16.Parse(operation.operand2));
                            //handleStack(OpCode.LOAD_CONST, identifierValue);
                        }
                        // Load result
                        else
                        {
                            bytecodeRegisterCurrentToken.preview = "(" + printerLastExpressionResult.ToString() + ")";

                            valueToAddInStack = (int)printerLastExpressionResult;

                            identifierValue = printerLastExpressionResult;

                            //handleStack(OpCode.LOAD_CONST, printerLastExpressionResult);
                        }
                    }
                    // Load result
                    else
                    {
                        if (mustAddIdentifier)
                        {
                            bytecodeRegisterCurrentToken.preview = "(" + identifier.ToString() + ")";

                            //printerLastExpressionResult = getIdentifierValue(identifier);

                            valueToAddInStack = Int16.Parse(identifier);

                            //handleStack(OpCode.LOAD_CONST, Int16.Parse(identifier));
                        }
                        else
                        {
                            if (printerLastExpressionResult != null)
                            {
                                bytecodeRegisterCurrentToken.preview = "(" + printerLastExpressionResult.ToString() + ")";

                                valueToAddInStack = printerLastExpressionResult;

                                identifierValue = printerLastExpressionResult;

                                //handleStack(OpCode.LOAD_CONST, printerLastExpressionResult);
                            }
                            else
                            {
                                mustAddLoadConst = false;
                            }
                        }
                    }

                    if (mustAddLoadConst && !printerShowCompareOp)
                    {
                        if (operation != null && identifier == operation.operand1 && printerLastExpressionResult.ToString() != identifier)
                        {
                            // For now, do nothing
                        }
                        else
                        {
                            bytecodeRegisters.Add(bytecodeRegisterCurrentToken);

                            handleStack(OpCode.LOAD_CONST, valueToAddInStack);
                        }

                        //this.currentOffset += getOpCodeOffsetSize(OpCode.LOAD_CONST);
                    }
                }
            }
            //--------------------------------------------------------------------------------------

            //--------------------------------------------------------------------------------------
            // Prepares LOAD_NAME
            if (printerLoadName && !mustAddIdentifier)
            {
                handleStack(OpCode.LOAD_NAME, getIdentifierValue(identifier));

                printerLoadName = false;

                BytecodeRegister bytecodeRegisterCurrentToken = new BytecodeRegister();

                bytecodeRegisterCurrentToken.lineInGeneratedBytecode = currentLineInGeneratedBytecode++;

                bytecodeRegisterCurrentToken.lineInFile = currentLine;

                bytecodeRegisterCurrentToken.offset = currentOffset;

                //this.currentOffset += getOpCodeOffsetSize(OpCode.LOAD_NAME);

                bytecodeRegisterCurrentToken.opCode = (int)OpCode.LOAD_NAME;

                // TODO
                bytecodeRegisterCurrentToken.stackPos = 0;

                bytecodeRegisterCurrentToken.preview = "(" + identifier + ")";

                bytecodeRegisters.Add(bytecodeRegisterCurrentToken);

            }
            //--------------------------------------------------------------------------------------

            //--------------------------------------------------------------------------------------
            // Prepares to show operation type
            if (printerShowOperationType && !mustAddIdentifier)
            {
                BytecodeRegister bytecodeRegisterForAttribuition = new BytecodeRegister();

                bytecodeRegisterForAttribuition.lineInGeneratedBytecode = currentLineInGeneratedBytecode++;

                bytecodeRegisterForAttribuition.lineInFile = currentLine;

                bytecodeRegisterForAttribuition.offset = currentOffset;

                //this.currentOffset += getOpCodeOffsetSize(OpCode.BINARY_ADD);

                if (operation.currentOperator == TipoTk.TkMais)
                {
                    // Verify if the first operand (if not identifier) is in the stack
                    if (!isIdentifier(operation.operand1))
                    {
                        valueToVerifyInStack = Int16.Parse(operation.operand1);
                    }

                    // Verify if the second operand (if identifier) is in the stack
                    if (isIdentifier(operation.operand2)) {

                        valueToVerifyInStack = getIdentifierValue(operation.operand2);

                        if (!getLastStackPosition(valueToVerifyInStack))
                        {
                            addSimpleLoadName(valueToVerifyInStack, currentLine, operation.operand2);
                        }
                    }

                    handleStack(OpCode.BINARY_ADD, value);

                    bytecodeRegisterForAttribuition.opCode = (int)OpCode.BINARY_ADD;
                }
                else if (operation.currentOperator == TipoTk.TkMenos)
                {
                    handleStack(OpCode.BINARY_SUBTRACT, value);

                    bytecodeRegisterForAttribuition.opCode = (int)OpCode.BINARY_SUBTRACT;
                }
                else if (operation.currentOperator == TipoTk.TkMultiplicacao)
                {
                    handleStack(OpCode.BINARY_MULTIPLY, value);

                    bytecodeRegisterForAttribuition.opCode = (int)OpCode.BINARY_MULTIPLY;
                }
                else if (operation.currentOperator == TipoTk.TkDivisao)
                {
                    handleStack(OpCode.BINARY_TRUE_DIVIDE, value);

                    bytecodeRegisterForAttribuition.opCode = (int)OpCode.BINARY_TRUE_DIVIDE;
                }

                bytecodeRegisters.Add(bytecodeRegisterForAttribuition);
            }

            //--------------------------------------------------------------------------------------

            //--------------------------------------------------------------------------------------
            // Prepares attribuition

            // If there is a attribuition
            if (printerAtribuition() && !mustAddIdentifier)
            {
                handleStack(OpCode.STORE_NAME, value);

                BytecodeRegister bytecodeRegisterForAttribuition = new BytecodeRegister();

                bytecodeRegisterForAttribuition.lineInGeneratedBytecode = currentLineInGeneratedBytecode++;

                bytecodeRegisterForAttribuition.lineInFile = currentLine;

                bytecodeRegisterForAttribuition.offset = currentOffset;

                //this.currentOffset += getOpCodeOffsetSize(OpCode.STORE_NAME);

                bytecodeRegisterForAttribuition.opCode = (int)OpCode.STORE_NAME;

                // TODO
                bytecodeRegisterForAttribuition.stackPos = 0;

                bytecodeRegisterForAttribuition.preview = "(" + getVariableForAttribuition() + ")";

                // Update symbols table
                updateIdentifierValue(getVariableForAttribuition(), identifierValue);

                bytecodeRegisters.Add(bytecodeRegisterForAttribuition);

                printerLoadConst = false;
            }
            //--------------------------------------------------------------------------------------

            //--------------------------------------------------------------------------------------
            // Prepares comparison

            if (printerShowCompareOp)
            {
                printerLastCompLine = currentLine;

                handleStack(OpCode.COMPARE_OP, null);

                BytecodeRegister bytecodeRegisterForAttribuition = new BytecodeRegister();

                bytecodeRegisterForAttribuition.lineInGeneratedBytecode = currentLineInGeneratedBytecode++;

                bytecodeRegisterForAttribuition.lineInFile = currentLine;

                bytecodeRegisterForAttribuition.offset = currentOffset;

                //this.currentOffset += getOpCodeOffsetSize(OpCode.COMPARE_OP);

                bytecodeRegisterForAttribuition.opCode = (int)OpCode.COMPARE_OP;

                // TODO
                bytecodeRegisterForAttribuition.stackPos = 0;

                if (printerCompElement == TipoTk.TkMaior)
                {
                    bytecodeRegisterForAttribuition.preview = "(>)";
                }
                else if (printerCompElement == TipoTk.TkMenor)
                {
                    bytecodeRegisterForAttribuition.preview = "(<)";
                }
                else if (printerCompElement == TipoTk.TkIgual)
                {
                    bytecodeRegisterForAttribuition.preview = "(==)";
                }
                else if (printerCompElement == TipoTk.TkDiferente)
                {
                    bytecodeRegisterForAttribuition.preview = "(!=)";
                }

                bytecodeRegisters.Add(bytecodeRegisterForAttribuition);

                printerShowCompareOp = false;
            }
            //--------------------------------------------------------------------------------------

            //--------------------------------------------------------------------------------------
            // Prepares POP_JUMP_IF_FALSE
            if (printerPopJumpIfFalse)
            {
                BytecodeRegister bytecodeRegisterForJumpIfFalse = new BytecodeRegister();

                bytecodeRegisterForJumpIfFalse.lineInGeneratedBytecode = currentLineInGeneratedBytecode;

                bytecodeRegisterForJumpIfFalse.lineInFile = currentLine;

                bytecodeRegisterForJumpIfFalse.offset = currentOffset;

                //this.currentOffset += getOpCodeOffsetSize(OpCode.POP_JUMP_IF_FALSE);

                handleStack(OpCode.POP_JUMP_IF_FALSE, null);

                bytecodeRegisterForJumpIfFalse.opCode = (int)OpCode.POP_JUMP_IF_FALSE;

                // TODO
                bytecodeRegisterForJumpIfFalse.stackPos = 0;

                bytecodeRegisters.Add(bytecodeRegisterForJumpIfFalse);

                printerPopJumpIfFalse = false;

                // Pop jump if false line
                printerCurrentIdentationLevel.bytecodeRegistersLine = currentLineInGeneratedBytecode;

                printerIdentationRegisters.Push(printerCurrentIdentationLevel);

                currentLineInGeneratedBytecode++;
            }

            //--------------------------------------------------------------------------------------

            printerFoundAnIdentifier = false;

            printerShowOperationType = false;
        }

        public int searchForSimpleAtribIndex()
        {
            for (int i = 0; i < operationsInCurrentLine.Count; i++)
            {
                if ((operationsInCurrentLine[i].currentOperator == TipoTk.TkAtrib) ||
                    (operationsInCurrentLine[i].currentOperator == TipoTk.TkMaisIgual) ||
                    (operationsInCurrentLine[i].currentOperator == TipoTk.TkMenosIgual) ||
                    (operationsInCurrentLine[i].currentOperator == TipoTk.TkMulIgual) ||
                    (operationsInCurrentLine[i].currentOperator == TipoTk.TkDivIgual))
                {
                    return i;
                }
            }

            return -1;
        }

        public void addSimpleLoadConst(int? value, int currentLine)
        {
            BytecodeRegister bytecodeRegisterCurrentToken = new BytecodeRegister();

            bytecodeRegisterCurrentToken.lineInGeneratedBytecode = currentLineInGeneratedBytecode++;

            bytecodeRegisterCurrentToken.lineInFile = currentLine;

            bytecodeRegisterCurrentToken.offset = currentOffset;

            bytecodeRegisterCurrentToken.opCode = (int)OpCode.LOAD_CONST;

            // TODO
            bytecodeRegisterCurrentToken.stackPos = 0;

            bytecodeRegisterCurrentToken.preview = "(" + value.ToString() + ")";

            handleStack(OpCode.LOAD_CONST, value);

            bytecodeRegisters.Add(bytecodeRegisterCurrentToken);

            //this.currentOffset += getOpCodeOffsetSize(OpCode.LOAD_CONST);
        }

        public void addSimpleLoadName(int? value, int currentLine, String identifier)
        {
            handleStack(OpCode.LOAD_NAME, getIdentifierValue(identifier));

            //printerLoadName = false;

            BytecodeRegister bytecodeRegisterCurrentToken = new BytecodeRegister();

            bytecodeRegisterCurrentToken.lineInGeneratedBytecode = currentLineInGeneratedBytecode++;

            bytecodeRegisterCurrentToken.lineInFile = currentLine;

            bytecodeRegisterCurrentToken.offset = currentOffset;

            //this.currentOffset += getOpCodeOffsetSize(OpCode.LOAD_NAME);

            bytecodeRegisterCurrentToken.opCode = (int)OpCode.LOAD_NAME;

            // TODO
            bytecodeRegisterCurrentToken.stackPos = 0;

            bytecodeRegisterCurrentToken.preview = "(" + identifier + ")";

            bytecodeRegisters.Add(bytecodeRegisterCurrentToken);
        }

        public Boolean printerAtribuition()
        {
            return ((attribuitionOperatorCounter > 0) && (!printerFoundAnIdentifier) && (!printerShowOperationType));
        }

        public int getOpCodeOffsetSize(OpCode opCode)
        {
            int opcodeToNum = (Int16)opCode;

            switch (opcodeToNum)
            {
                case 0:
                    return 3;
                    break;

                case 1:
                    return 3;
                    break;

                case 2:
                    return 3;
                    break;

                case 3:
                    return 3;
                    break;

                case 4:
                    return 1;
                    break;

                case 5:
                    return 1;
                    break;

                case 6:
                    return 1;
                    break;

                case 7:
                    return 1;
                    break;

                case 8:
                    return 3;
                    break;

                case 9:
                    return 3;
                    break;

                case 10:
                    return 1;
                    break;

                case 11:
                    return 3;
                    break;

                case 12:
                    return 3;
                    break;

                case 13:
                    return 3;
                    break;

                case 14:
                    return 1;
                    break;

                case 15:
                    return 3;
                    break;

                case 16:
                    return 3;
                    break;

                case 17:
                    return 1;
                    break;

                case 18:
                    return 3;
                    break;

                case 19:
                    return 1;
                    break;

                case 20:
                    return 1;
                    break;

                case 21:
                    return 1;
                    break;

                case 22:
                    return 1;
                    break;
            }

            return -1;
        }

        public void generateBytecode()
        {
            currentIDAlreadyInSymbolsTable = false;

            currentLineInFile = 1;

            int j = currentLineInFile - 1;

            Boolean mustEnterInHandleLine = true;

            // For each line in file
            for (int i = 1; i <= getLastLineInFile(); i++)
            {
                lineinFileGlobalToAddLoadName = i;

                mustEnterInHandleLine = true;

                j = 0;

                lyneTypeAddEnable = true;

                // Find the first lexical token that has the line according to current line in file
                while (lexicalTokens[j].linha != currentLineInFile)
                {
                    j++;
                }

                // New line, analyze first element in line
                if (lexicalTokens[j].linha == currentLineInFile)
                {
                    resetLineElements();

                    if (printerCountLinesInElseIf)
                    {
                        printerQuantityOfLinesInElseIf++;
                    }

                    verifyOperatorsInCurrentLine(currentLineInFile);

                    if (desidentElementCounter > 0)
                    {
                        printerCurrentDesidentLevel.bytecodeRegistersLine = bytecodeRegisters.Count - 1;

                        handleDesident(i);
                    }

                    if (printerWhileInProgress && (desidentElementCounter > 0))
                    {
                        printerWhileInProgress = false;

                        addEndWhileRegisters(i-1);
                    }

                    if (rangeElementCouter > 0)
                    {
                        addForRangeInitialRegisters(i);

                        if(operationsInCurrentLine.Count == 1)
                        {
                            mustEnterInHandleLine = false;

                            if (isIdentifier(printerForTopLimit))
                            {
                                addSimpleLoadName(null, i, printerForTopLimit);
                            }
                            else
                            {
                                addSimpleLoadConst(Int16.Parse(printerForTopLimit), i);
                            }

                            addForRangeIntermediateRegisters(i);
                        }
                    }

                    if (elseElementCounter == 0)
                    {
                        if (whileElementCounter > 0)
                        {
                            addSetupLoop(i);

                            //printerWhileInProgress = true;
                        }

                        if (printerForInProgress && (desidentElementCounter > 0) && printerAnyReducedOperationInCurrentLine())
                        {
                            printerForInProgress = false;

                            addForRangeFinalRegisters(i-1);
                        }

                        verifyLoadForReducedOperations(i);

                        if (mustEnterInHandleLine)
                        {
                            handleLine(i);

                            if (rangeElementCouter > 0)
                            {
                                addForRangeIntermediateRegisters(i);
                            }
                        }

                        addLineType();

                        if (printerWhileInProgress && (desidentElementCounter>0))
                        {
                            printerWhileInProgress = false;

                            addEndWhileRegisters(i);
                        }

                        if (printerForInProgress && (desidentElementCounter > 0))
                        {
                            printerForInProgress = false;

                            addForRangeFinalRegisters(i);
                        }

                        if (printerWaitingOffsetForJumpForward)
                        {
                            printerWaitingOffsetForJumpForward = false;

                            if (printerWaitingForElseAfterElseIf)
                            {
                                printerIndexToGetOffsetForJumpForward = bytecodeRegisters.Count;
                            }
                            else
                            {
                                printerIndexToGetOffsetForJumpForward = bytecodeRegisters.Count + 1;
                            }                            
                        }                        
                    }

                    if (whileElementCounter > 0)
                    {
                        printerWhileInProgress = true;
                    }

                    if (rangeElementCouter > 0)
                    {
                        printerForInProgress = true;
                    }

                    verifyReduceOperationsFinalRegisters();

                    currentLineInFile++;

                    continue;
                }
            }

            printGeneratedBytecode();
        }
        public Boolean printerAnyReducedOperationInCurrentLine()
        {
            if(reducedAddOperatorCounter>0 || reducedSubtractionOperatorCounter>0 || reducedMultiplicationOperatorCounter>0 || reducedDivOperatorCounter > 0)
            {
                return true;
            }

            return false;
        }

        public void verifyReduceOperationsFinalRegisters()
        {
            int operand1Index = 0;

            if(operationsInCurrentLine[0].currentOperator == TipoTk.TkDesident)
            {
                operand1Index = 1;
            }

            BytecodeRegister bytecodeRegisterCurrentToken = new BytecodeRegister();

            bytecodeRegisterCurrentToken.lineInGeneratedBytecode = currentLineInGeneratedBytecode++;

            bytecodeRegisterCurrentToken.lineInFile = bytecodeRegisters[bytecodeRegisters.Count - 1].lineInFile;

            if (reducedAddOperatorCounter>0)
            {
                bytecodeRegisterCurrentToken.opCode = (int)OpCode.INPLACE_ADD;

                bytecodeRegisters.Add(bytecodeRegisterCurrentToken);

                addSimpleStoreNameForReducedOperations(operationsInCurrentLine[operand1Index].operand1, null, OpCode.INPLACE_ADD);
            }
            else if (reducedSubtractionOperatorCounter>0)
            {
                bytecodeRegisterCurrentToken.opCode = (int)OpCode.INPLACE_SUBTRACT;

                bytecodeRegisters.Add(bytecodeRegisterCurrentToken);

                addSimpleStoreNameForReducedOperations(operationsInCurrentLine[operand1Index].operand1, null, OpCode.INPLACE_SUBTRACT);
            }
            else if (reducedMultiplicationOperatorCounter>0)
            {
                bytecodeRegisterCurrentToken.opCode = (int)OpCode.INPLACE_MULTIPLY;

                bytecodeRegisters.Add(bytecodeRegisterCurrentToken);

                addSimpleStoreNameForReducedOperations(operationsInCurrentLine[operand1Index].operand1, null, OpCode.INPLACE_MULTIPLY);
            }
            else if (reducedDivOperatorCounter>0)
            {
                bytecodeRegisterCurrentToken.opCode = (int)OpCode.INPLACE_TRUE_DIVIDE;

                bytecodeRegisters.Add(bytecodeRegisterCurrentToken);

                addSimpleStoreNameForReducedOperations(operationsInCurrentLine[operand1Index].operand1, null, OpCode.INPLACE_TRUE_DIVIDE);
            }
        }

        public Boolean verifySimpleReducedOperation()
        {
            int counter = 0;

            for(int i=0; i<operationsInCurrentLine.Count; i++)
            {
                if(operationsInCurrentLine[i].currentOperator != TipoTk.TkDesident)
                {
                    counter++;
                }
            }

            if(counter == 1)
            {
                return true;
            }

            return false;
        }

        public void verifyLoadForReducedOperations(int line)
        {
            if (reducedAddOperatorCounter > 0 || reducedSubtractionOperatorCounter > 0 || reducedMultiplicationOperatorCounter > 0 || reducedDivOperatorCounter > 0)
            {
                if(operationsInCurrentLine[0].currentOperator != TipoTk.TkDesident)
                {
                    addSimpleLoadName(null, line, operationsInCurrentLine[0].operand1);
                }
                else
                {
                    addSimpleLoadName(null, line, operationsInCurrentLine[1].operand1);
                }
            }
        }

        public void addForRangeFinalRegisters(int line)
        {
            // Adds JUMP_ABSOLUTE
            BytecodeRegister bytecodeRegisterCurrentToken = new BytecodeRegister();

            bytecodeRegisterCurrentToken.lineInGeneratedBytecode = currentLineInGeneratedBytecode++;

            bytecodeRegisterCurrentToken.lineInFile = line;

            bytecodeRegisterCurrentToken.offset = currentOffset;

            bytecodeRegisterCurrentToken.opCode = (int)OpCode.JUMP_ABSOLUTE;

            // TODO
            bytecodeRegisterCurrentToken.stackPos = 0;

            bytecodeRegisters.Add(bytecodeRegisterCurrentToken);

            // Adds POP_BLOCK
            bytecodeRegisterCurrentToken = null;

            bytecodeRegisterCurrentToken = new BytecodeRegister();

            bytecodeRegisterCurrentToken.lineInGeneratedBytecode = currentLineInGeneratedBytecode++;

            bytecodeRegisterCurrentToken.lineInFile = line;

            bytecodeRegisterCurrentToken.opCode = (int)OpCode.POP_BLOCK;

            bytecodeRegisters.Add(bytecodeRegisterCurrentToken);
        }

        public void addForRangeIntermediateRegisters(int line)
        {
            // Call function
            BytecodeRegister bytecodeRegisterCurrentToken = new BytecodeRegister();

            bytecodeRegisterCurrentToken.lineInGeneratedBytecode = currentLineInGeneratedBytecode++;

            bytecodeRegisterCurrentToken.lineInFile = line;

            bytecodeRegisterCurrentToken.opCode = (int)OpCode.CALL_FUNCTION;

            // TODO
            bytecodeRegisterCurrentToken.stackPos = 0;

            bytecodeRegisterCurrentToken.preview = "(1 positional, 0 keyword pair)";

            bytecodeRegisters.Add(bytecodeRegisterCurrentToken);

            // Get iter
            bytecodeRegisterCurrentToken = null;

            bytecodeRegisterCurrentToken = new BytecodeRegister();

            bytecodeRegisterCurrentToken.lineInGeneratedBytecode = currentLineInGeneratedBytecode++;

            bytecodeRegisterCurrentToken.lineInFile = line;

            bytecodeRegisterCurrentToken.opCode = (int)OpCode.GET_ITER;

            bytecodeRegisters.Add(bytecodeRegisterCurrentToken);

            // For iter
            bytecodeRegisterCurrentToken = null;

            bytecodeRegisterCurrentToken = new BytecodeRegister();

            bytecodeRegisterCurrentToken.lineInGeneratedBytecode = currentLineInGeneratedBytecode++;

            bytecodeRegisterCurrentToken.lineInFile = line;

            bytecodeRegisterCurrentToken.opCode = (int)OpCode.FOR_ITER;

            // TODO
            bytecodeRegisterCurrentToken.stackPos = 0;

            bytecodeRegisters.Add(bytecodeRegisterCurrentToken);

            // Store name
            bytecodeRegisterCurrentToken = null;

            bytecodeRegisterCurrentToken = new BytecodeRegister();

            bytecodeRegisterCurrentToken.lineInGeneratedBytecode = currentLineInGeneratedBytecode++;

            bytecodeRegisterCurrentToken.lineInFile = line;

            bytecodeRegisterCurrentToken.opCode = (int)OpCode.STORE_NAME;

            // TODO
            bytecodeRegisterCurrentToken.stackPos = 0;

            bytecodeRegisterCurrentToken.preview = "(" + printerVariableToRange + ")";

            // Update symbols table
            //updateIdentifierValue(printerVariableToRange, identifierValue);

            bytecodeRegisters.Add(bytecodeRegisterCurrentToken);
        }

        public void addForRangeInitialRegisters(int currentLine)
        {
            addSetupLoop(currentLine);

            // Adds LOAD_GLOBAL
            BytecodeRegister bytecodeRegisterCurrentToken = new BytecodeRegister();

            bytecodeRegisterCurrentToken.lineInGeneratedBytecode = currentLineInGeneratedBytecode++;

            bytecodeRegisterCurrentToken.lineInFile = currentLineInFile;

            bytecodeRegisterCurrentToken.offset = currentOffset;

            bytecodeRegisterCurrentToken.opCode = (int)OpCode.LOAD_GLOBAL;

            // TODO
            bytecodeRegisterCurrentToken.stackPos = 0;

            bytecodeRegisterCurrentToken.preview = "(range)";

            bytecodeRegisters.Add(bytecodeRegisterCurrentToken);
        }

        public void addLineType()
        {
            if((desidentElementCounter == 1) && (identElementCounter == 1))
            {
                lyneTypeAddEnable = true;
            }

            if(rangeElementCouter == 1)
            {
                lyneTypeAddEnable = true;
            }

            if((reducedAddOperatorCounter > 0) ||
               (reducedSubtractionOperatorCounter > 0) ||
               (reducedMultiplicationOperatorCounter > 0) ||
               (reducedDivOperatorCounter > 0))
            {
                lyneTypeAddEnable = true;
            }

            //lyneTypeAddEnable = true;

            if (lyneTypeAddEnable)
            {
                if (lyneTypeIfIdent)
                {
                    lineTypes.Add(LineType.IfStatement);
                }
                else if (lyneTypeElseIfIdent)
                {
                    lineTypes.Add(LineType.ElseIfStatement);
                }
                else if(lyneTypeElseIdent)
                {
                    lineTypes.Add(LineType.ElseStatement);
                }
                else if (lyneTypeWhileIdent)
                {
                    lineTypes.Add(LineType.WhileStatement);
                }
                else if(rangeElementCouter>0 || lyneTypeForIdent)
                {
                    lyneTypeLastLineHasFor = true;

                    lineTypes.Add(LineType.ForStatement);
                }
                else
                {
                    lineTypes.Add(LineType.Expression);
                }
            }
        }

        public void addEndWhileRegisters(int currentLine)
        {
            // Adds JUMP_ABSOLUTE
            BytecodeRegister bytecodeRegisterCurrentToken = new BytecodeRegister();

            bytecodeRegisterCurrentToken.lineInGeneratedBytecode = currentLineInGeneratedBytecode++;

            bytecodeRegisterCurrentToken.lineInFile = currentLine;

            bytecodeRegisterCurrentToken.offset = currentOffset;

            bytecodeRegisterCurrentToken.opCode = (int)OpCode.JUMP_ABSOLUTE;

            // TODO
            bytecodeRegisterCurrentToken.stackPos = 0;

            bytecodeRegisters.Add(bytecodeRegisterCurrentToken);

            // Adds POP_BLOCK
            bytecodeRegisterCurrentToken = null;

            bytecodeRegisterCurrentToken = new BytecodeRegister();

            bytecodeRegisterCurrentToken.lineInGeneratedBytecode = currentLineInGeneratedBytecode++;

            bytecodeRegisterCurrentToken.lineInFile = currentLine;

            bytecodeRegisterCurrentToken.opCode = (int)OpCode.POP_BLOCK;

            bytecodeRegisters.Add(bytecodeRegisterCurrentToken);
        }

        public void addSetupLoop(int currentLineInFile)
        {
            // Adds SETUP_LOOP
            BytecodeRegister bytecodeRegisterCurrentToken = new BytecodeRegister();

            bytecodeRegisterCurrentToken.lineInGeneratedBytecode = currentLineInGeneratedBytecode++;

            bytecodeRegisterCurrentToken.lineInFile = currentLineInFile;

            bytecodeRegisterCurrentToken.offset = currentOffset;

            bytecodeRegisterCurrentToken.opCode = (int)OpCode.SETUP_LOOP;

            // TODO
            bytecodeRegisterCurrentToken.stackPos = 0;

            bytecodeRegisters.Add(bytecodeRegisterCurrentToken);
        }

        // Handle desident for IF
        public void handleDesident(int currentLineInFile)
        {
            IdentDesidentLevel identationLevel = printerIdentationRegisters.Peek();

            // If it's an if desident
            //if (identationLevel.tokenType == TipoTk.TkSe && elseIfElementCounter == 0)
            if (identationLevel.tokenType == TipoTk.TkSe)
            {
                printerCurrentIfIdentationLevel--;

                addSimpleJumpForward(currentLineInFile);

                printerIdentationRegisters.Pop();
            }
        }

        public int? handleOperandInStoreNameForReducedOperations(int? value1, int? value2, OpCode opCode)
        {
            switch (opCode)
            {
                case OpCode.INPLACE_ADD:
                    return (value1 + value2);
                    break;

                case OpCode.INPLACE_SUBTRACT:
                    return (value1 - value2);
                    break;

                case OpCode.INPLACE_MULTIPLY:
                    return (value1 * value2);
                    break;

                case OpCode.INPLACE_TRUE_DIVIDE:
                    return (value1 / value2);
                    break;
            }

            return null;
        }

        public void addSimpleStoreNameForReducedOperations(String identifier, int? value, OpCode opCode)
        {
            BytecodeRegister bytecodeRegisterCurrentToken = new BytecodeRegister();

            bytecodeRegisterCurrentToken.lineInGeneratedBytecode = currentLineInGeneratedBytecode++;

            bytecodeRegisterCurrentToken.lineInFile = lineinFileGlobalToAddLoadName;

            bytecodeRegisterCurrentToken.opCode = (int)OpCode.STORE_NAME;

            // TODO
            bytecodeRegisterCurrentToken.stackPos = 0;

            bytecodeRegisterCurrentToken.preview = "(" + identifier + ")";

            // Update symbols table
            if (!verifySimpleReducedOperation())
            {
                updateIdentifierValue(identifier, handleOperandInStoreNameForReducedOperations(getIdentifierValue(identifier), printerLastExpressionResult, opCode));
            }
            else
            {
                if(operationsInCurrentLine[0].currentOperator == TipoTk.TkDesident)
                {
                    updateIdentifierValue(identifier, handleOperandInStoreNameForReducedOperations(getIdentifierValue(identifier), Int16.Parse(operationsInCurrentLine[1].operand2), opCode));
                }
                else
                {
                    updateIdentifierValue(identifier, handleOperandInStoreNameForReducedOperations(getIdentifierValue(identifier), Int16.Parse(operationsInCurrentLine[0].operand2), opCode));
                }
            }

            bytecodeRegisters.Add(bytecodeRegisterCurrentToken);
        }

        public void addSimpleJumpForward(int currentLineInFile)
        {
            // Adds JUMP_FORWARD
            BytecodeRegister bytecodeRegisterCurrentToken = new BytecodeRegister();

            bytecodeRegisterCurrentToken.lineInGeneratedBytecode = currentLineInGeneratedBytecode++;

            bytecodeRegisterCurrentToken.lineInFile = currentLineInFile - 1;

            bytecodeRegisterCurrentToken.offset = currentOffset;

            bytecodeRegisterCurrentToken.opCode = (int)OpCode.JUMP_FORWARD;

            // TODO
            bytecodeRegisterCurrentToken.stackPos = 0;

            //bytecodeRegisterCurrentToken.preview = "(to " + currentOffset + getOpCodeOffsetSize(OpCode.JUMP_FORWARD) + ")";

            bytecodeRegisters.Add(bytecodeRegisterCurrentToken);
        }

        public String getVariableForAttribuition()
        {
            foreach (Operation op in operationsInCurrentLine)
            {
                if (op.currentOperator == TipoTk.TkAtrib)
                {
                    return op.operand1;
                }
            }

            return null;
        }

        public Boolean verifyIdentifierLoaded(String identifier)
        {
            foreach (Symbol sym in symbolsTable)
            {
                if (sym.identifier.Equals(identifier))
                {
                    if (sym.isLoaded)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void setIdentifierLoaded(String identifier, Boolean valueToSet)
        {
            foreach (Symbol sym in symbolsTable)
            {
                if (sym.identifier.Equals(identifier))
                {
                    sym.isLoaded = valueToSet;
                }
            }
        }

        public void handleArithmeticalOperations(int quantityWithOperationWithMulPrecedence, int quantityWithOperationWithAddPrecedence, int bottomLimit, int topLimit)
        {
            // Verify arithmetic operations with constants
            // While there are operations to analyze
            while ((quantityWithOperationWithMulPrecedence > 0) || (quantityWithOperationWithAddPrecedence > 0))
            {
                for (int i = bottomLimit; i < topLimit; i++)
                {
                    // Precedence 2
                    if (((operationsInCurrentLine[i].precedence == OperationPrecedence.TK_MUL_PRECEDENCE) && (quantityWithOperationWithMulPrecedence != 0) && (i > printerMoreToRightOperationIndexPrecedence2) && (!(operationsInCurrentLine[i].alreadyVerified))) || ((operationsInCurrentLine[i].calculateNow) && (operationsInCurrentLine[i].precedence == OperationPrecedence.TK_MUL_PRECEDENCE)))
                    {
                        // It's necessary to show LOAD_CONST
                        printerLoadConst = true;

                        if (arithmeticOperation(operationsInCurrentLine[i], i))
                        {
                            // Decrement because one operation was analyzed
                            quantityWithOperationWithMulPrecedence--;
                        }
                        else
                        {
                            printerLoadConst = false;
                        }

                        // Return to the left
                        break;
                    }

                    // Precedence 1, just analyze if all level 2 precedencse was already analized
                    if (((operationsInCurrentLine[i].precedence == OperationPrecedence.TK_ADD_PRECEDENCE) && (quantityWithOperationWithMulPrecedence == 0) && (i > printerMoreToRightOperationIndexPrecedence1) && (!(operationsInCurrentLine[i].alreadyVerified))) || ((operationsInCurrentLine[i].calculateNow) && (operationsInCurrentLine[i].precedence == OperationPrecedence.TK_ADD_PRECEDENCE)))
                    {
                        // It's necessary to show LOAD_CONST
                        printerLoadConst = true;

                        if (arithmeticOperation(operationsInCurrentLine[i], i))
                        {
                            // Decrement because one operation was analyzed
                            quantityWithOperationWithAddPrecedence--;
                        }
                        else
                        {
                            printerLoadConst = false;
                        }

                        // Return to the left
                        break;
                    }
                }
            }

            if(rangeElementCouter>0 && operationsInCurrentLine.Count == 1)
            {

            }

            // Simple atrib
            for (int i = bottomLimit; i < topLimit; i++)
            {
                // Precedence -1 and it's the last operation in this line
                if ((operationsInCurrentLine[i].precedence == OperationPrecedence.TK_ATTRIBUTION_PRECEDENCE) && (i == operationsInCurrentLine.Count - 1))
                {
                    // It's necessary to show LOAD_CONST
                    printerLoadConst = true;

                    printerSimpleAtrib = true;

                    break;
                }
            }
        }

        public void handleLine(int line)
        {
            int quantityWithOperationWithMulPrecedence = getQuantityOfOperationsWithMulPrecedence();
            int quantityWithOperationWithAddPrecedence = getQuantityOfOperationsWithAddPrecedence();
            printerMoreToRightOperationIndexPrecedence1 = 0;
            printerMoreToRightOperationIndexPrecedence2 = 0;

            // The line has an if statement
            if (ifElementCounter > 0)
            {
                verifyCompElement();

                // There is one element in the left
                if (((desidentElementCounter == 0) && (operationsInCurrentLine[1].currentOperator == printerCompElement)) || ((desidentElementCounter == 1) && (operationsInCurrentLine[2].currentOperator == printerCompElement)))
                {
                    // It's an identifier
                    if (isIdentifier(operationsInCurrentLine[1].operand1))
                    {
                        printerLoadName = true;

                        if (desidentElementCounter == 0)
                        {
                            mountBytecode(line, operationsInCurrentLine[1].operand1, null, null, false, true);
                        }
                        else if (desidentElementCounter == 1)
                        {
                            mountBytecode(line, operationsInCurrentLine[2].operand1, null, null, false, true);
                        }

                        printerLoadName = false;
                    }
                    // It's a const
                    else
                    {
                        printerLoadConst = true;

                        mountBytecode(line, operationsInCurrentLine[1].operand1, null, null, true, true);

                        printerLoadConst = false;
                    }
                }
                else
                {
                    // Expression in the left
                    int quantityWithOperationWithMulPrecedenceIfStatement = getQuantityOfOperationsWithMulPrecedenceIfStatement(false);
                    int quantityWithOperationWithAddPrecedenceIfStatement = getQuantityOfOperationsWithAddPrecedenceIfStatement(false);

                    handleArithmeticalOperations(quantityWithOperationWithMulPrecedenceIfStatement, quantityWithOperationWithAddPrecedenceIfStatement, 1, operationRelationalPosInCurrentLine);

                    // If the operands aren't null, the bytecode element was already added
                    if (arithmeticalIdentifierOperand1 == null && arithmeticalIdentifierOperand2 == null)
                    {
                        mountBytecode(line, null, null, null, false, true);
                    }
                }

                // There is one element in the right
                if (operationsInCurrentLine[operationsInCurrentLine.Count - 1].currentOperator == printerCompElement)
                {
                    // It's an identifier
                    if (isIdentifier(operationsInCurrentLine[operationsInCurrentLine.Count - 1].operand2))
                    {
                        printerLoadName = true;

                        mountBytecode(line, operationsInCurrentLine[operationsInCurrentLine.Count - 1].operand2, null, null, false, true);

                        printerLoadName = false;
                    }
                    // It's a const
                    else
                    {
                        printerLoadConst = true;

                        mountBytecode(line, operationsInCurrentLine[operationsInCurrentLine.Count - 1].operand2, null, null, true, true);

                        printerLoadConst = false;
                    }
                }
                else
                {
                    // Expression in the right
                    int quantityWithOperationWithMulPrecedenceIfStatement = getQuantityOfOperationsWithMulPrecedenceIfStatement(true);
                    int quantityWithOperationWithAddPrecedenceIfStatement = getQuantityOfOperationsWithAddPrecedenceIfStatement(true);

                    handleArithmeticalOperations(quantityWithOperationWithMulPrecedenceIfStatement, quantityWithOperationWithAddPrecedenceIfStatement, operationRelationalPosInCurrentLine + 1, operationsInCurrentLine.Count);

                    // If the operands aren't null, the bytecode element was already added
                    if (arithmeticalIdentifierOperand1 == null && arithmeticalIdentifierOperand2 == null)
                    {
                        mountBytecode(line, null, null, null, false, true);
                    }
                }

                printerShowCompareOp = true;

                mountBytecode(line, null, null, null, false, true);

                printerPopJumpIfFalse = true;

                mountBytecode(line, null, null, null, false, true);
            }
            else if (whileElementCounter > 0)
            {
                verifyCompElement();

                int index = -1;

                if(operationsInCurrentLine[0].currentOperator == printerCompElement)
                {
                    index = 0;
                }
                else
                {
                    index = 1;
                }

                // There is one element in the left
                if (((desidentElementCounter == 0) && (operationsInCurrentLine[index].currentOperator == printerCompElement)) || ((desidentElementCounter == 1) && (operationsInCurrentLine[index].currentOperator == printerCompElement)))
                {
                    // It's an identifier
                    if (isIdentifier(operationsInCurrentLine[index].operand1))
                    {
                        printerLoadName = true;

                        if (desidentElementCounter == 0)
                        {
                            mountBytecode(line, operationsInCurrentLine[index].operand1, null, null, false, true);
                        }
                        else if (desidentElementCounter == 1)
                        {
                            mountBytecode(line, operationsInCurrentLine[index].operand1, null, null, false, true);
                        }

                        printerLoadName = false;
                    }
                    // It's a const
                    else
                    {
                        printerLoadConst = true;

                        mountBytecode(line, operationsInCurrentLine[index].operand1, null, null, true, true);

                        printerLoadConst = false;
                    }
                }
                else
                {
                    // Expression in the left
                    int quantityWithOperationWithMulPrecedenceIfStatement = getQuantityOfOperationsWithMulPrecedenceIfStatement(false);
                    int quantityWithOperationWithAddPrecedenceIfStatement = getQuantityOfOperationsWithAddPrecedenceIfStatement(false);

                    handleArithmeticalOperations(quantityWithOperationWithMulPrecedenceIfStatement, quantityWithOperationWithAddPrecedenceIfStatement, 1, operationRelationalPosInCurrentLine);

                    // If the operands aren't null, the bytecode element was already added
                    if (arithmeticalIdentifierOperand1 == null && arithmeticalIdentifierOperand2 == null)
                    {
                        mountBytecode(line, null, null, null, false, true);
                    }
                }

                // There is one element in the right
                if (operationsInCurrentLine[operationsInCurrentLine.Count - 1].currentOperator == printerCompElement)
                {
                    // It's an identifier
                    if (isIdentifier(operationsInCurrentLine[operationsInCurrentLine.Count - 1].operand2))
                    {
                        printerLoadName = true;

                        mountBytecode(line, operationsInCurrentLine[operationsInCurrentLine.Count - 1].operand2, null, null, false, true);

                        printerLoadName = false;
                    }
                    // It's a const
                    else
                    {
                        printerLoadConst = true;

                        mountBytecode(line, operationsInCurrentLine[operationsInCurrentLine.Count - 1].operand2, null, null, true, true);

                        printerLoadConst = false;
                    }
                }
                else
                {
                    // Expression in the right
                    int quantityWithOperationWithMulPrecedenceIfStatement = getQuantityOfOperationsWithMulPrecedenceIfStatement(true);
                    int quantityWithOperationWithAddPrecedenceIfStatement = getQuantityOfOperationsWithAddPrecedenceIfStatement(true);

                    handleArithmeticalOperations(quantityWithOperationWithMulPrecedenceIfStatement, quantityWithOperationWithAddPrecedenceIfStatement, operationRelationalPosInCurrentLine + 1, operationsInCurrentLine.Count);

                    // If the operands aren't null, the bytecode element was already added
                    if (arithmeticalIdentifierOperand1 == null && arithmeticalIdentifierOperand2 == null)
                    {
                        mountBytecode(line, null, null, null, false, true);
                    }
                }

                printerShowCompareOp = true;

                mountBytecode(line, null, null, null, false, true);

                printerPopJumpIfFalse = true;

                mountBytecode(line, null, null, null, false, true);
            }
            // Common arithmetical operation
            else
            {
                handleArithmeticalOperations(quantityWithOperationWithMulPrecedence, quantityWithOperationWithAddPrecedence, 0, operationsInCurrentLine.Count);
            }

            if (rangeElementCouter == 0)
            {
                mountBytecode(line, null, null, null, false, true);
            }            
        }

        private void verifyCompElement()
        {
            for (int i = 0; i < operationsInCurrentLine.Count; i++) {
                if (operationsInCurrentLine[i].currentOperator == TipoTk.TkMaior)
                {
                    printerCompElement = TipoTk.TkMaior;
                    break;
                }
                else if (operationsInCurrentLine[i].currentOperator == TipoTk.TkMenor)
                {
                    printerCompElement = TipoTk.TkMenor;
                    break;
                }
                else if (operationsInCurrentLine[i].currentOperator == TipoTk.TkIgual)
                {
                    printerCompElement = TipoTk.TkIgual;
                    break;
                }
                else if (operationsInCurrentLine[i].currentOperator == TipoTk.TkDiferente)
                {
                    printerCompElement = TipoTk.TkDiferente;
                    break;
                }
            }
        }

        // When found a identifier, it's necessary to verify if all left operations with precedence 1 was already done
        public Boolean verifyLeftOperationsNotCalculated(int currentIndex, int operand1Column, int operand2Column)
        {
            for (int i = 0; i < currentIndex; i++)
            {
                if ((operationsInCurrentLine[i].precedence == OperationPrecedence.TK_ADD_PRECEDENCE) && (!(operationsInCurrentLine[i].alreadyVerified)) && (operationsInCurrentLine[i].operand2Column != operand1Column))
                {
                    operationsInCurrentLine[i].calculateNow = true;

                    return true;
                }
            }

            return false;
        }

        public Boolean arithmeticOperation(Operation operation, int index)
        {
            arithmeticalOperand1 = null;
            arithmeticalOperand2 = null;

            arithmeticalIdentifierOperand1 = null;
            arithmeticalIdentifierOperand2 = null;

            // Verify if the first operand is identifier
            if (isIdentifier(operation.operand1))
            {
                // Verify if all the operations to the left was already calculated
                if (verifyLeftOperationsNotCalculated(index, operation.operand1Column, operation.operand2Column))
                {
                    return false;
                }

                printerFoundAnIdentifier = true;

                if (!verifyIdentifierLoaded(operation.operand1))
                {
                    setIdentifierLoaded(operation.operand1, true);

                    printerLoadName = true;

                    mountBytecode(currentLineInFile, operation.operand1, operation, null, false, false);
                }

                arithmeticalIdentifierOperand1 = getIdentifierValue(operation.operand1);
            }

            // Verify if the second operand is identifier
            if (isIdentifier(operation.operand2))
            {
                // It's necessary to load the first element
                if (printerOperationsStack.Count == 0)
                {
                    if (arithmeticalIdentifierOperand1 != null)
                    {
                        printerLoadName = true;

                        mountBytecode(currentLineInFile, operation.operand1, null, null, false, false);

                        printerLoadName = false;

                        arithmeticalOperand1 = arithmeticalIdentifierOperand1;
                    }
                    else
                    {
                        printerLoadConst = true;

                        mountBytecode(currentLineInFile, operation.operand1, null, null, true, false);

                        printerLoadConst = false; ;

                        arithmeticalOperand1 = Int16.Parse(operation.operand1);
                    }
                }

                // Verify if all the operations to the left was already calculated
                if (verifyLeftOperationsNotCalculated(index, operation.operand1Column, operation.operand2Column))
                {
                    return false;
                }

                printerFoundAnIdentifier = true;

                if (!verifyIdentifierLoaded(operation.operand2))
                {
                    setIdentifierLoaded(operation.operand2, true);

                    printerLoadName = true;

                    mountBytecode(currentLineInFile, operation.operand2, null, null, false, true);
                }

                arithmeticalIdentifierOperand2 = getIdentifierValue(operation.operand2);
            }

            if (arithmeticalIdentifierOperand1 != null)
            {
                arithmeticalOperand1 = arithmeticalIdentifierOperand1;
            }
            else
            {
                arithmeticalOperand1 = Int16.Parse(operation.operand1);
            }

            if (arithmeticalIdentifierOperand2 != null)
            {
                arithmeticalOperand2 = arithmeticalIdentifierOperand2;
            }
            else
            {
                arithmeticalOperand2 = Int16.Parse(operation.operand2);
            }

            switch (operation.currentOperator)
            {
                case TipoTk.TkMais:

                    // There is no operator already used
                    if ((!verifyResultForAlreadyUsedElement(operation.operand1Column, 1)) && (!verifyResultForAlreadyUsedElement(operation.operand2Column, 2)))
                    {
                        operationsInCurrentLine[index].result = arithmeticalOperand1 + arithmeticalOperand2;
                    }
                    // Left element was already used
                    else if ((verifyResultForAlreadyUsedElement(operation.operand1Column, 1)) && (!verifyResultForAlreadyUsedElement(operation.operand2Column, 2)))
                    {
                        arithmeticalOperand1 = getResultOfOperationWithAlreadyUsedElement(operation.operand1Column, 1);

                        operationsInCurrentLine[index].result = arithmeticalOperand1 + arithmeticalOperand2;

                        operationsInCurrentLine[index - 1].result = arithmeticalOperand1 + arithmeticalOperand2;

                        printerMoreToRightOperationIndexPrecedence1 = index;
                    }
                    // Right element was already used
                    else if ((!verifyResultForAlreadyUsedElement(operation.operand1Column, 1)) && (verifyResultForAlreadyUsedElement(operation.operand2Column, 2)))
                    {
                        arithmeticalOperand2 = getResultOfOperationWithAlreadyUsedElement(operation.operand2Column, 2);

                        operationsInCurrentLine[index].result = arithmeticalOperand1 + arithmeticalOperand2;

                        operationsInCurrentLine[index + 1].result = arithmeticalOperand1 + arithmeticalOperand2;

                        printerMoreToRightOperationIndexPrecedence1 = index;
                    }
                    // Both elements was already used
                    else
                    {
                        arithmeticalOperand1 = getResultOfOperationWithAlreadyUsedElement(operation.operand1Column, 1);
                        arithmeticalOperand2 = getResultOfOperationWithAlreadyUsedElement(operation.operand2Column, 2);

                        operationsInCurrentLine[index].result = arithmeticalOperand1 + arithmeticalOperand2;
                    }

                    printerLastExpressionResult = operationsInCurrentLine[index].result;

                    operationsInCurrentLine[index].alreadyVerified = true;

                    break;

                case TipoTk.TkMenos:

                    // There is no operator already used
                    if ((!verifyResultForAlreadyUsedElement(operation.operand1Column, 1)) && (!verifyResultForAlreadyUsedElement(operation.operand2Column, 2)))
                    {
                        operationsInCurrentLine[index].result = arithmeticalOperand1 - arithmeticalOperand2;
                    }
                    // Left element was already used
                    else if ((verifyResultForAlreadyUsedElement(operation.operand1Column, 1)) && (!verifyResultForAlreadyUsedElement(operation.operand2Column, 2)))
                    {
                        arithmeticalOperand1 = getResultOfOperationWithAlreadyUsedElement(operation.operand1Column, 1);

                        operationsInCurrentLine[index].result = arithmeticalOperand1 - arithmeticalOperand2;

                        operationsInCurrentLine[index - 1].result = arithmeticalOperand1 - arithmeticalOperand2;

                        printerMoreToRightOperationIndexPrecedence1 = index;
                    }
                    // Right element was already used
                    else if ((!verifyResultForAlreadyUsedElement(operation.operand1Column, 1)) && (verifyResultForAlreadyUsedElement(operation.operand2Column, 2)))
                    {
                        arithmeticalOperand2 = getResultOfOperationWithAlreadyUsedElement(operation.operand2Column, 2);

                        operationsInCurrentLine[index].result = arithmeticalOperand1 - arithmeticalOperand2;

                        operationsInCurrentLine[index + 1].result = arithmeticalOperand1 - arithmeticalOperand2;

                        printerMoreToRightOperationIndexPrecedence1 = index;
                    }
                    // Both elements was already used
                    else
                    {
                        arithmeticalOperand1 = getResultOfOperationWithAlreadyUsedElement(operation.operand1Column, 1);
                        arithmeticalOperand2 = getResultOfOperationWithAlreadyUsedElement(operation.operand2Column, 2);

                        operationsInCurrentLine[index].result = arithmeticalOperand1 - arithmeticalOperand2;
                    }

                    printerLastExpressionResult = operationsInCurrentLine[index].result;

                    operationsInCurrentLine[index].alreadyVerified = true;

                    break;

                case TipoTk.TkMultiplicacao:

                    // There is no operator already used
                    if ((!verifyResultForAlreadyUsedElement(operation.operand1Column, 1)) && (!verifyResultForAlreadyUsedElement(operation.operand2Column, 2)))
                    {
                        operationsInCurrentLine[index].result = arithmeticalOperand1 * arithmeticalOperand2;
                    }
                    // Left element was already used
                    else if ((verifyResultForAlreadyUsedElement(operation.operand1Column, 1)) && (!verifyResultForAlreadyUsedElement(operation.operand2Column, 2)))
                    {
                        arithmeticalOperand1 = getResultOfOperationWithAlreadyUsedElement(operation.operand1Column, 1);

                        operationsInCurrentLine[index].result = arithmeticalOperand1 * arithmeticalOperand2;

                        operationsInCurrentLine[index - 1].result = arithmeticalOperand1 * arithmeticalOperand2;

                        printerMoreToRightOperationIndexPrecedence2 = index;
                    }
                    // Right element was already used
                    else if ((!verifyResultForAlreadyUsedElement(operation.operand1Column, 1)) && (verifyResultForAlreadyUsedElement(operation.operand2Column, 2)))
                    {
                        arithmeticalOperand2 = getResultOfOperationWithAlreadyUsedElement(operation.operand2Column, 2);

                        operationsInCurrentLine[index].result = arithmeticalOperand1 * arithmeticalOperand2;

                        operationsInCurrentLine[index + 1].result = arithmeticalOperand1 * arithmeticalOperand2;

                        printerMoreToRightOperationIndexPrecedence2 = index;
                    }
                    // Both elements was already used
                    else
                    {
                        arithmeticalOperand1 = getResultOfOperationWithAlreadyUsedElement(operation.operand1Column, 1);
                        arithmeticalOperand2 = getResultOfOperationWithAlreadyUsedElement(operation.operand2Column, 2);

                        operationsInCurrentLine[index].result = arithmeticalOperand1 * arithmeticalOperand2;
                    }

                    printerLastExpressionResult = operationsInCurrentLine[index].result;

                    operationsInCurrentLine[index].alreadyVerified = true;

                    break;

                case TipoTk.TkDivisao:

                    // There is no operator already used
                    if ((!verifyResultForAlreadyUsedElement(operation.operand1Column, 1)) && (!verifyResultForAlreadyUsedElement(operation.operand2Column, 2)))
                    {
                        operationsInCurrentLine[index].result = arithmeticalOperand1 / arithmeticalOperand2;
                    }
                    // Left element was already used
                    else if ((verifyResultForAlreadyUsedElement(operation.operand1Column, 1)) && (!verifyResultForAlreadyUsedElement(operation.operand2Column, 2)))
                    {
                        arithmeticalOperand1 = getResultOfOperationWithAlreadyUsedElement(operation.operand1Column, 1);

                        operationsInCurrentLine[index].result = arithmeticalOperand1 / arithmeticalOperand2;

                        operationsInCurrentLine[index - 1].result = arithmeticalOperand1 / arithmeticalOperand2;

                        printerMoreToRightOperationIndexPrecedence2 = index;
                    }
                    // Right element was already used
                    else if ((!verifyResultForAlreadyUsedElement(operation.operand1Column, 1)) && (verifyResultForAlreadyUsedElement(operation.operand2Column, 2)))
                    {
                        arithmeticalOperand2 = getResultOfOperationWithAlreadyUsedElement(operation.operand2Column, 2);

                        operationsInCurrentLine[index].result = arithmeticalOperand1 / arithmeticalOperand2;

                        operationsInCurrentLine[index + 1].result = arithmeticalOperand1 / arithmeticalOperand2;

                        printerMoreToRightOperationIndexPrecedence2 = index;
                    }
                    // Both elements was already used
                    else
                    {
                        arithmeticalOperand1 = getResultOfOperationWithAlreadyUsedElement(operation.operand1Column, 1);
                        arithmeticalOperand2 = getResultOfOperationWithAlreadyUsedElement(operation.operand2Column, 2);

                        operationsInCurrentLine[index].result = arithmeticalOperand1 / arithmeticalOperand2;
                    }

                    printerLastExpressionResult = operationsInCurrentLine[index].result;

                    operationsInCurrentLine[index].alreadyVerified = true;

                    break;

            }

            this.operationsInCurrentLine[index].calculateNow = false;

            // If a identifier was used, it's necessary to mount the operation
            if ((arithmeticalIdentifierOperand1 != null) || (arithmeticalIdentifierOperand2 != null))
            {
                if (arithmeticalIdentifierOperand1 != null && printerVariableToRange!=null)
                {
                    if (!getLastStackPosition(arithmeticalIdentifierOperand1))
                    {
                        addSimpleLoadName(arithmeticalIdentifierOperand1, lineinFileGlobalToAddLoadName, operation.operand1);
                    }
                }

                printerShowOperationType = true;

                printerLoadConst = false;

                mountBytecode(currentLineInFile, null, operation, printerLastExpressionResult, false, false);
            }

            // If reached here, result is true
            return true;
        }

        public Boolean isIdentifier(String operand)
        {
            int result = 0;

            // Try to convert, if it isn't possible, return true indicating that the operand is an identifier
            try
            {
                result = Int16.Parse(operand);

                return false;
            }
            catch (Exception e)
            {
                return true;
            }
        }

        public int? getResultOfOperationWithAlreadyUsedElement(int elementColumn, int operandIndicator)
        {
            foreach (Operation op in operationsInCurrentLine)
            {
                if ((op.operand1Column == elementColumn) || (op.operand2Column == elementColumn))
                {
                    if (op.result != null)
                    {
                        return op.result;
                    }
                }
            }

            return 0;
        }

        public Boolean verifyResultForAlreadyUsedElement(int elementColumn, int operandIndicator)
        {
            foreach (Operation op in operationsInCurrentLine)
            {

                if ((op.operand1Column == elementColumn) || (op.operand2Column == elementColumn))
                {
                    // There is a operation with this element thas was already calculated
                    if (op.result != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void handleIfStatements()
        {
            int lineOfJumpForwardInFile = 0;

            for (int i = 0; i < bytecodeRegisters.Count - 1; i++)
            {
                if (bytecodeRegisters[i].opCode == (int)OpCode.JUMP_FORWARD)
                {
                    lineOfJumpForwardInFile = bytecodeRegisters[i].lineInFile;

                    for (int j = i + 1; j < bytecodeRegisters.Count - 1; j++)
                    {
                        if (bytecodeRegisters[j].lineInFile == lineOfJumpForwardInFile + 1)
                        {
                            bytecodeRegisters[i].preview = "(to " + bytecodeRegisters[j].offset + ")";

                            handlePopJumpIfFalse(bytecodeRegisters[j].offset, i);

                            break;
                        }
                    }
                }
            }
        }

        public void handlePopJumpIfFalse(int offsetValue, int currentLineInBytecode)
        {
            for (int i = currentLineInBytecode; i >= 0; i--)
            {
                if (bytecodeRegisters[i].opCode == (int)OpCode.POP_JUMP_IF_FALSE)
                {
                    bytecodeRegisters[i].stackPos = offsetValue;

                    break;
                }
            }
        }

        public void insertStackPosInPopJumpIfFalse()
        {
            foreach (BytecodeRegister bcr in bytecodeRegisters)
            {
                if (bcr.opCode == (int)OpCode.POP_JUMP_IF_FALSE)
                {
                    bcr.stackPos = bytecodeRegisters[printerOffsetIndexToInsertInPopJumpIfFalse].offset;

                    break;
                }
            }
        }

        public void insertOffsetInJumpForwardPreview()
        {
            for (int i = printerIndexToGetOffsetForJumpForward; i >= 0; i--)
            {
                if (bytecodeRegisters[i].opCode == (int)OpCode.JUMP_FORWARD)
                {
                    bytecodeRegisters[i].preview = "(to " + bytecodeRegisters[printerIndexToGetOffsetForJumpForward].offset + ")";
                }
            }
        }

        public void handleElseIf()
        {
            for (int i = 0; i < bytecodeRegisters.Count; i++)
            {
                if (bytecodeRegisters[i].lineInFile > printerLastLineWithElse)
                {

                }
            }
        }

        public void printGeneratedBytecode()
        {
            addFinalByteCodeRegisters();

            int lastPrintedLine = 0;

            handleGeneratedBytecodeOffset();

            handleIfStatements();

            handleElse();

            handleGeneratedBytecodeOffset();

            if (printerOffsetIndexToInsertInPopJumpIfFalse != -1)
            {
                insertStackPosInPopJumpIfFalse();
            }

            if (printerIndexToGetOffsetForJumpForward != -1)
            {
                insertOffsetInJumpForwardPreview();
            }

            if (printerLastLineWithElseIf != -1)
            {
                handleElseIf();
            }

            if (printerQuantityOfLinesInElseIf != 0)
            {
                insertStackPosInPopJumpIfFalseOfElseIf();
            }

            handleJumpForward();

            handleSetupLoop();

            handleJumpAbsolute();

            popJumpIfFalseCorrection();

            handleForIter();

            Console.WriteLine("\nBytecode Gerado:");

            foreach (BytecodeRegister bytecodeRegister in bytecodeRegisters)
            {
                if (bytecodeRegister.lineInFile != lastPrintedLine)
                {
                    Console.Write("\n" + bytecodeRegister.lineInFile + "\t");

                    lastPrintedLine = bytecodeRegister.lineInFile;
                }
                else
                {
                    Console.Write("\t");
                }

                if (bytecodeRegister.offset >= 100)
                {
                    Console.Write(bytecodeRegister.offset + " ");
                }
                else if (bytecodeRegister.offset < 10)
                {
                    Console.Write(" " + bytecodeRegister.offset + "  ");
                }
                else
                {
                    Console.Write(bytecodeRegister.offset + "  ");
                }

                if (bytecodeRegister.opCode == (int)OpCode.POP_JUMP_IF_FALSE)
                {
                    Console.Write(getOpCodeDescription(bytecodeRegister.opCode) + "\t");
                }
                else if (bytecodeRegister.opCode == (int)OpCode.RETURN_VALUE)
                {
                    Console.Write(getOpCodeDescription(bytecodeRegister.opCode));

                    continue;
                }
                else if ((bytecodeRegister.opCode == (int)OpCode.INPLACE_ADD) || (bytecodeRegister.opCode == (int)OpCode.INPLACE_SUBTRACT) || (bytecodeRegister.opCode == (int)OpCode.INPLACE_MULTIPLY) || (bytecodeRegister.opCode == (int)OpCode.INPLACE_TRUE_DIVIDE))
                {
                    Console.WriteLine(getOpCodeDescription(bytecodeRegister.opCode));

                    continue;
                }
                else if (bytecodeRegister.opCode == (int)OpCode.JUMP_FORWARD)
                {
                    Console.Write(getOpCodeDescription(bytecodeRegister.opCode) + "\t");
                }
                else if(bytecodeRegister.opCode == (int)OpCode.JUMP_ABSOLUTE)
                {
                    Console.Write(getOpCodeDescription(bytecodeRegister.opCode) + "\t");
                }
                else if(bytecodeRegister.opCode == (int)OpCode.POP_BLOCK)
                {
                    Console.Write(getOpCodeDescription(bytecodeRegister.opCode) + "\n");

                    continue;
                }
                else if (bytecodeRegister.opCode == (int)OpCode.GET_ITER)
                {
                    Console.Write(getOpCodeDescription(bytecodeRegister.opCode) + "\n");

                    continue;
                }
                else if (bytecodeRegister.opCode == (int)OpCode.CALL_FUNCTION)
                {
                    Console.Write(getOpCodeDescription(bytecodeRegister.opCode) + "\t");
                }
                else
                {
                    Console.Write(getOpCodeDescription(bytecodeRegister.opCode) + "\t\t");
                }


                if (bytecodeRegister.opCode != (int)OpCode.BINARY_ADD && bytecodeRegister.opCode != (int)OpCode.BINARY_SUBTRACT && bytecodeRegister.opCode != (int)OpCode.BINARY_MULTIPLY && bytecodeRegister.opCode != (int)OpCode.BINARY_TRUE_DIVIDE)
                {
                    // TODO
                    Console.Write(bytecodeRegister.stackPos + "  ");
                }
                else
                {
                    Console.Write("   ");
                }

                Console.WriteLine(bytecodeRegister.preview);
            }
        }

        public void handleForIter()
        {
            Boolean next = false;

            for(int i=0; i<bytecodeRegisters.Count; i++)
            {
                next = false;

                if(bytecodeRegisters[i].opCode == (int)OpCode.FOR_ITER)
                {
                    for(int j=i; j<bytecodeRegisters.Count; j++)
                    {
                        if(bytecodeRegisters[j].opCode == (int)OpCode.POP_BLOCK)
                        {
                            bytecodeRegisters[i].preview = "(to " + bytecodeRegisters[j].offset + ")";

                            next = true;

                            break;
                        }
                    }
                }

                if (next)
                {
                    continue;
                }
            }
        }

        public void popJumpIfFalseCorrection()
        {
            Boolean next = false;

            for (int i = 0; i < bytecodeRegisters.Count - 1; i++)
            {
                next = false;

                if (bytecodeRegisters[i].opCode == (int)OpCode.POP_JUMP_IF_FALSE)
                {
                    int lineToGetOffset = getValidLineToPopJumpIfFalse(bytecodeRegisters[i].lineInFile);

                    if(lineToGetOffset == -1)
                    {
                        for (int j = i; j < bytecodeRegisters.Count - 1; j++)
                        {
                            if(bytecodeRegisters[j].opCode == (int)OpCode.POP_BLOCK)
                            {
                                bytecodeRegisters[i].stackPos = bytecodeRegisters[j].offset;

                                next = true;

                                break;
                            }
                        }

                        if (next)
                        {
                            continue;
                        }
                    }

                    for (int j = i; j < bytecodeRegisters.Count - 1; j++)
                    {
                        if (bytecodeRegisters[j].lineInFile == lineToGetOffset)
                        {
                            bytecodeRegisters[i].stackPos = bytecodeRegisters[j].offset;

                            next = true;

                            break;
                        }
                    }

                    if (next)
                    {
                        continue;
                    }
                }
            }
        }

        public int getValidLineToPopJumpIfFalse(int line)
        {
            int value = 0;

            if(lineTypes[line] == LineType.IfStatement)
            {
                value = (getValidLineToPopJumpIfFalseIfStatement(line));
            }
            else if(lineTypes[line] == LineType.ElseIfStatement)
            {
                value = (getValidLineToPopJumpIfFalseElseIfStatement(line));
            }
            else if(lineTypes[line] == LineType.WhileStatement)
            {
                value = -1;
            }

            if((value != -1) && (lineTypes[value] == LineType.ElseStatement) && (lineTypes[value-2] != LineType.ElseStatement))
            {
                value++;
            }            

            return value;
        }

        public int getValidLineToPopJumpIfFalseIfStatement(int line)
        {
            for (int i = line; i <= bytecodeRegisters[bytecodeRegisters.Count-1].lineInFile-1; i++)
            {
                if (lineTypes[i] != LineType.IfStatement)
                {
                    return i + 1;
                }
            }

            return -1;
        }

        public int getValidLineToPopJumpIfFalseElseIfStatement(int line)
        {
            for (int i = line; i <= bytecodeRegisters[bytecodeRegisters.Count-1].lineInFile-1; i++)
            {
                if (lineTypes[i] != LineType.ElseIfStatement)
                {
                    return i + 1;
                }
            }

            return -1;
        }

        public void handleJumpAbsolute()
        {
            Boolean next = false;

            for (int i = 0; i < bytecodeRegisters.Count - 1; i++)
            {
                next = false;

                if ((bytecodeRegisters[i].opCode == (int)OpCode.SETUP_LOOP) || (bytecodeRegisters[i].opCode == (int)OpCode.FOR_ITER))
                {
                    for (int j = i + 1; j < bytecodeRegisters.Count - 1; j++)
                    {
                        if(bytecodeRegisters[j].opCode == (int)OpCode.JUMP_ABSOLUTE)
                        {
                            if(bytecodeRegisters[i].opCode == (int)OpCode.SETUP_LOOP)
                            {
                                bytecodeRegisters[j].stackPos = bytecodeRegisters[i + 1].offset;
                            }
                            else
                            {
                                bytecodeRegisters[j].stackPos = bytecodeRegisters[i].offset;
                            }                            

                            next = true;

                            break;
                        }
                    }

                    if (next)
                    {
                        continue;
                    }
                }
            }
        }

        public void handleSetupLoop()
        {
            Boolean next = false;

            for (int i = 0; i < bytecodeRegisters.Count - 1; i++)
            {
                next = false;

                if (bytecodeRegisters[i].opCode == (int)OpCode.SETUP_LOOP)
                {
                    for (int j = i; j < bytecodeRegisters.Count - 1; j++)
                    {
                        if (bytecodeRegisters[j].opCode == (int)OpCode.POP_BLOCK)
                        {
                            j++;

                            bytecodeRegisters[i].preview = "(to " + bytecodeRegisters[j].offset + ")";

                            next = true;

                            break;
                        }
                    }

                    if (next)
                    {
                        continue;
                    }
                }
            }
        }

        public void handleJumpForward()
        {
            Boolean next = false;

            for(int i=0; i<bytecodeRegisters.Count-1; i++)
            {
                next = false;

                if (bytecodeRegisters[i].opCode == (int)OpCode.JUMP_FORWARD)
                {
                    int lineToGetOffset = getValidLineToJumpForward(bytecodeRegisters[i].lineInFile);

                    for(int j=i; j < bytecodeRegisters.Count - 1; j++)
                    {
                        if (bytecodeRegisters[j].lineInFile == lineToGetOffset)
                        {
                            bytecodeRegisters[i].preview = "(to " + bytecodeRegisters[j].offset + ")";

                            next = true;

                            break;
                        }
                    }

                    if (next)
                    {
                        continue;
                    }
                }
            }
        }

        public int getValidLineToJumpForward(int line)
        {
            for(int i=line; i<=bytecodeRegisters[bytecodeRegisters.Count-1].lineInFile; i++)
            {
                if((lineTypes[i]!=LineType.ElseIfStatement) && (lineTypes[i] != LineType.ElseStatement))
                {
                    return i+1;
                }
            }

            return -1;
        }

        public void insertStackPosInPopJumpIfFalseOfElseIf()
        {
            Boolean internalReturnValue = false;

            for(int i=bytecodeRegisters.Count-1; i>=0; i--)
            {
                if((bytecodeRegisters[i].opCode == (int)OpCode.POP_JUMP_IF_FALSE) && (bytecodeRegisters[i].stackPos == 0))
                {
                    for(int j=i; j<bytecodeRegisters.Count; j++)
                    {
                        if(bytecodeRegisters[j].lineInFile >= bytecodeRegisters[i].lineInFile + printerQuantityOfLinesInElseIf)
                        {
                            bytecodeRegisters[i].stackPos = bytecodeRegisters[j].offset;

                            internalReturnValue = true;

                            break;
                        }
                    }
                }

                if (internalReturnValue)
                {
                    break;
                }
            }
        }

        public void handleElse()
        {
            if (printerLastLineWithElse != -1)
            {
                if (searchJumpForward() == -1)
                {
                    // It's necessary to add a jump forward
                    addJumpForwardWithElseStatement();
                }
            }
        }

        public void addJumpForwardWithElseStatement()
        {
            for (int i = 0; i < bytecodeRegisters.Count; i++)
            {
                if ((bytecodeRegisters[i].lineInFile == printerLastLineWithElse - 1) && (bytecodeRegisters[i + 1].lineInFile == printerLastLineWithElse + 1))
                {
                    // Adds JUMP_FORWARD
                    BytecodeRegister bytecodeRegisterCurrentToken = new BytecodeRegister();

                    bytecodeRegisterCurrentToken.lineInGeneratedBytecode = i + 1;

                    currentLineInGeneratedBytecode++;

                    bytecodeRegisterCurrentToken.lineInFile = printerLastLineWithElse - 1;


                    bytecodeRegisterCurrentToken.opCode = (int)OpCode.JUMP_FORWARD;

                    // TODO
                    bytecodeRegisterCurrentToken.stackPos = 0;

                    bytecodeRegisterCurrentToken.preview = "(to " + currentOffset + getOpCodeOffsetSize(OpCode.JUMP_FORWARD) + ")";

                    bytecodeRegisters.Insert(i + 1, bytecodeRegisterCurrentToken);

                    printerOffsetIndexToInsertInPopJumpIfFalse = i + 2;

                    break;
                }
            }
        }

        public int searchJumpForward()
        {
            for (int i = 0; i < bytecodeRegisters.Count; i++)
            {
                if (bytecodeRegisters[i].opCode == (int)OpCode.JUMP_FORWARD)
                {
                    return i;
                }
            }

            return -1;
        }

        public void addFinalByteCodeRegisters()
        {
            // Adds LOAD_CONST
            BytecodeRegister bytecodeRegisterCurrentToken = new BytecodeRegister();

            bytecodeRegisterCurrentToken.lineInGeneratedBytecode = currentLineInGeneratedBytecode++;

            bytecodeRegisterCurrentToken.lineInFile = bytecodeRegisters[bytecodeRegisters.Count - 1].lineInFile;

            bytecodeRegisterCurrentToken.opCode = (int)OpCode.LOAD_CONST;

            // TODO
            bytecodeRegisterCurrentToken.stackPos = 0;

            bytecodeRegisterCurrentToken.preview = "(None)";

            bytecodeRegisters.Add(bytecodeRegisterCurrentToken);

            // Adds RETURN_VALUE
            bytecodeRegisterCurrentToken = null;

            bytecodeRegisterCurrentToken = new BytecodeRegister();

            bytecodeRegisterCurrentToken.lineInGeneratedBytecode = currentLineInGeneratedBytecode++;

            bytecodeRegisterCurrentToken.lineInFile = bytecodeRegisters[bytecodeRegisters.Count - 1].lineInFile;

            bytecodeRegisterCurrentToken.opCode = (int)OpCode.RETURN_VALUE;

            bytecodeRegisters.Add(bytecodeRegisterCurrentToken);
        }

        public void handleGeneratedBytecodeOffset()
        {
            printerCurrentOffset = 0;

            foreach (BytecodeRegister bytecodeRegister in bytecodeRegisters)
            {
                bytecodeRegister.offset = printerCurrentOffset;

                printerCurrentOffset += getOpCodeOffsetSize((Analyzer.OpCode)bytecodeRegister.opCode);
            }
        }

        public void handleStack(OpCode opCode, int? value)
        {
            switch (opCode)
            {
                case OpCode.LOAD_CONST:
                    printerOperationsStack.Push(value);
                    break;

                case OpCode.LOAD_NAME:
                    printerOperationsStack.Push(value);
                    break;

                case OpCode.STORE_FAST:
                    printerOperationsStack.Pop();
                    break;

                case OpCode.STORE_NAME:
                    printerOperationsStack.Pop();
                    break;

                case OpCode.BINARY_ADD:
                case OpCode.BINARY_SUBTRACT:
                case OpCode.BINARY_MULTIPLY:
                case OpCode.BINARY_TRUE_DIVIDE:
                    printerOperationsStack.Pop();
                    printerOperationsStack.Pop();
                    printerOperationsStack.Push(value);
                    break;

                case OpCode.COMPARE_OP:
                    printerOperationsStack.Pop();
                    printerOperationsStack.Pop();
                    break;

                case OpCode.POP_JUMP_IF_FALSE:
                    break;

                case OpCode.RETURN_VALUE:
                    break;

                case OpCode.JUMP_FORWARD:
                    break;

                case OpCode.SETUP_LOOP:
                    break;

                case OpCode.JUMP_ABSOLUTE:
                    break;

                case OpCode.POP_BLOCK:
                    break;

                case OpCode.LOAD_GLOBAL:
                    break;

                case OpCode.CALL_FUNCTION:
                    break;

                case OpCode.GET_ITER:
                    break;

                case OpCode.FOR_ITER:
                    break;

                case OpCode.INPLACE_ADD:
                    break;

                case OpCode.INPLACE_SUBTRACT:
                    break;

                case OpCode.INPLACE_MULTIPLY:
                    break;

                case OpCode.INPLACE_TRUE_DIVIDE:
                    break;
            }
        }

        public String getOpCodeDescription(int opCode)
        {
            switch (opCode)
            {
                case 0:
                    return "LOAD_CONST";
                    break;

                case 1:
                    return "LOAD_NAME";
                    break;

                case 2:
                    return "STORE_FAST";
                    break;

                case 3:
                    return "STORE_NAME";
                    break;

                case 4:
                    return "BINARY_ADD";
                    break;

                case 5:
                    return "BINARY_SUBTRACT";
                    break;

                case 6:
                    return "BINARY_MULTIPLY";
                    break;

                case 7:
                    return "BINARY_TRUE_DIVIDE";
                    break;

                case 8:
                    return "COMPARE_OP";
                    break;

                case 9:
                    return "POP_JUMP_IF_FALSE";
                    break;

                case 10:
                    return "RETURN_VALUE";
                    break;

                case 11:
                    return "JUMP_FORWARD";
                    break;

                case 12:
                    return "SETUP_LOOP";
                    break;

                case 13:
                    return "JUMP_ABSOLUTE";
                    break;

                case 14:
                    return "POP_BLOCK";
                    break;

                case 15:
                    return "LOAD_GLOBAL";
                    break;

                case 16:
                    return "CALL_FUNCTION";
                    break;

                case 17:
                    return "GET_ITER";
                    break;

                case 18:
                    return "FOR_ITER";
                    break;

                case 19:
                    return "INPLACE_ADD";
                    break;

                case 20:
                    return "INPLACE_SUBTRACT";
                    break;

                case 21:
                    return "INPLACE_MULTIPLY";
                    break;

                case 22:
                    return "INPLACE_TRUE_DIVIDE";
                    break;

            }

            return "";
        }

        public void verifyOperatorsInCurrentLine(int currentLine)
        {
            for (int i = 0; i < lexicalTokens.Count; i++)
            {
                if (lexicalTokens[i].linha == currentLine)
                {
                    if((rangeElementCouter>0) && (printerFoundOpenParenthesis) && (printerForTopLimit==null))
                    {
                        printerForTopLimit = lexicalTokens[i].valor;
                    }

                    switch (lexicalTokens[i].tipo)
                    {
                        case TipoTk.TkMais:

                            if (ifElementCounter > 0)
                            {
                                if ((equalOperatorCounter > 0) || (difOperatorCounter > 0) || (lessThanOperatorCounter > 0) || (biggerThanOperatorCounter > 0))
                                {
                                    addOperatorCounterRight++;
                                }
                                else
                                {
                                    addOperatorCounterLeft++;
                                }
                            }

                            addOperatorCounter++;

                            operationsInCurrentLine.Add(new Operation(lexicalTokens[i - 1].valor, lexicalTokens[i + 1].valor, lexicalTokens[i - 1].coluna, lexicalTokens[i + 1].coluna, lexicalTokens[i].tipo, OperationPrecedence.TK_ADD_PRECEDENCE));

                            break;

                        case TipoTk.TkMenos:

                            if (ifElementCounter > 0)
                            {
                                if ((equalOperatorCounter > 0) || (difOperatorCounter > 0) || (lessThanOperatorCounter > 0) || (biggerThanOperatorCounter > 0))
                                {
                                    subtractionOperatorCounterRight++;
                                }
                                else
                                {
                                    subtractionOperatorCounterLeft++;
                                }
                            }

                            subtractionOperatorCounter++;

                            operationsInCurrentLine.Add(new Operation(lexicalTokens[i - 1].valor, lexicalTokens[i + 1].valor, lexicalTokens[i - 1].coluna, lexicalTokens[i + 1].coluna, lexicalTokens[i].tipo, OperationPrecedence.TK_SUB_PRECEDENCE));

                            break;

                        case TipoTk.TkMultiplicacao:

                            if (ifElementCounter > 0)
                            {
                                if ((equalOperatorCounter > 0) || (difOperatorCounter > 0) || (lessThanOperatorCounter > 0) || (biggerThanOperatorCounter > 0))
                                {
                                    multiplicationOperatorCounterRight++;
                                }
                                else
                                {
                                    multiplicationOperatorCounterLeft++;
                                }
                            }

                            multiplicationOperatorCounter++;

                            operationsInCurrentLine.Add(new Operation(lexicalTokens[i - 1].valor, lexicalTokens[i + 1].valor, lexicalTokens[i - 1].coluna, lexicalTokens[i + 1].coluna, lexicalTokens[i].tipo, OperationPrecedence.TK_MUL_PRECEDENCE));

                            break;

                        case TipoTk.TkDivisao:

                            if (ifElementCounter > 0)
                            {
                                if ((equalOperatorCounter > 0) || (difOperatorCounter > 0) || (lessThanOperatorCounter > 0) || (biggerThanOperatorCounter > 0))
                                {
                                    divOperatorCounterRight++;
                                }
                                else
                                {
                                    divOperatorCounterLeft++;
                                }
                            }

                            divOperatorCounter++;

                            operationsInCurrentLine.Add(new Operation(lexicalTokens[i - 1].valor, lexicalTokens[i + 1].valor, lexicalTokens[i - 1].coluna, lexicalTokens[i + 1].coluna, lexicalTokens[i].tipo, OperationPrecedence.TK_DIV_PRECEDENCE));

                            break;

                        case TipoTk.TkMaisIgual:
                            reducedAddOperatorCounter++;
                            operationsInCurrentLine.Add(new Operation(lexicalTokens[i - 1].valor, lexicalTokens[i + 1].valor, lexicalTokens[i - 1].coluna, lexicalTokens[i + 1].coluna, lexicalTokens[i].tipo, OperationPrecedence.TK_ADD_REDUCED_PRECEDENCE));
                            break;

                        case TipoTk.TkMenosIgual:
                            reducedSubtractionOperatorCounter++;
                            operationsInCurrentLine.Add(new Operation(lexicalTokens[i - 1].valor, lexicalTokens[i + 1].valor, lexicalTokens[i - 1].coluna, lexicalTokens[i + 1].coluna, lexicalTokens[i].tipo, OperationPrecedence.TK_SUB_REDUCED_PRECEDENCE));
                            break;

                        case TipoTk.TkMulIgual:
                            reducedMultiplicationOperatorCounter++;
                            operationsInCurrentLine.Add(new Operation(lexicalTokens[i - 1].valor, lexicalTokens[i + 1].valor, lexicalTokens[i - 1].coluna, lexicalTokens[i + 1].coluna, lexicalTokens[i].tipo, OperationPrecedence.TK_MUL_REDUCED_PRECEDENCE));
                            break;

                        case TipoTk.TkDivIgual:
                            reducedDivOperatorCounter++;
                            operationsInCurrentLine.Add(new Operation(lexicalTokens[i - 1].valor, lexicalTokens[i + 1].valor, lexicalTokens[i - 1].coluna, lexicalTokens[i + 1].coluna, lexicalTokens[i].tipo, OperationPrecedence.TK_DIV_REDUCED_PRECEDENCE));
                            break;

                        case TipoTk.TkIgual:
                            equalOperatorCounter++;
                            operationsInCurrentLine.Add(new Operation(lexicalTokens[i - 1].valor, lexicalTokens[i + 1].valor, lexicalTokens[i - 1].coluna, lexicalTokens[i + 1].coluna, lexicalTokens[i].tipo, OperationPrecedence.TK_EQUAL_PRECEDENCE));

                            operationRelationalPosInCurrentLine = operationsInCurrentLine.Count - 1;

                            break;

                        case TipoTk.TkDiferente:
                            difOperatorCounter++;
                            operationsInCurrentLine.Add(new Operation(lexicalTokens[i - 1].valor, lexicalTokens[i + 1].valor, lexicalTokens[i - 1].coluna, lexicalTokens[i + 1].coluna, lexicalTokens[i].tipo, OperationPrecedence.TK_DIFF_PRECEDENCE));

                            operationRelationalPosInCurrentLine = operationsInCurrentLine.Count - 1;

                            break;

                        case TipoTk.TkMenor:
                            lessThanOperatorCounter++;
                            operationsInCurrentLine.Add(new Operation(lexicalTokens[i - 1].valor, lexicalTokens[i + 1].valor, lexicalTokens[i - 1].coluna, lexicalTokens[i + 1].coluna, lexicalTokens[i].tipo, OperationPrecedence.TK_LESS_THAN_PRECEDENCE));

                            operationRelationalPosInCurrentLine = operationsInCurrentLine.Count - 1;

                            break;

                        case TipoTk.TkMaior:
                            biggerThanOperatorCounter++;
                            operationsInCurrentLine.Add(new Operation(lexicalTokens[i - 1].valor, lexicalTokens[i + 1].valor, lexicalTokens[i - 1].coluna, lexicalTokens[i + 1].coluna, lexicalTokens[i].tipo, OperationPrecedence.TK_BIGGER_THAN_PRECEDENCE));

                            operationRelationalPosInCurrentLine = operationsInCurrentLine.Count - 1;

                            break;

                        case TipoTk.TkAtrib:
                            attribuitionOperatorCounter++;
                            operationsInCurrentLine.Add(new Operation(lexicalTokens[i - 1].valor, lexicalTokens[i + 1].valor, lexicalTokens[i - 1].coluna, lexicalTokens[i + 1].coluna, lexicalTokens[i].tipo, OperationPrecedence.TK_ATTRIBUTION_PRECEDENCE));
                            break;

                        case TipoTk.TkId:
                            idElementsCounter++;
                            verifyTkId(lexicalTokens[i]);
                            break;

                        case TipoTk.TkSe:
                            ifElementCounter++;
                            printerCurrentIfIdentationLevel++;
                            operationsInCurrentLine.Add(new Operation(null, null, lexicalTokens[i - 1].coluna, -1, lexicalTokens[i].tipo, OperationPrecedence.TK_IF_ATTRIBUTION_PRECEDENCE));

                            printerCurrentIdentationLevel = null;
                            printerCurrentIdentationLevel = new IdentDesidentLevel(currentLine, TipoTk.TkSe);

                            lineTypeReset();

                            lyneTypeLastLineHasIf = true;

                            lineTypes.Add(LineType.IfStatement);

                            lyneTypeAddEnable = false;

                            handleAddOfIdentationLevels(TipoTk.TkSe, currentLine, true);

                            break;

                        case TipoTk.TkSenao:
                            elseElementCounter++;
                            printerCurrentIdentationLevel = null;
                            printerCurrentIdentationLevel = new IdentDesidentLevel(currentLine, TipoTk.TkSenao);
                            printerIdentationRegisters.Push(printerCurrentIdentationLevel);
                            printerLastLineWithElse = currentLine;
                            printerWaitingForElseDesident = true;

                            if (printerLastLineWithElseIf != -1)
                            {
                                addSimpleJumpForward(currentLine);

                                printerWaitingForElseAfterElseIf = true;
                            }

                            printerCountLinesInElseIf = false;

                            lineTypeReset();

                            lyneTypeLastLineHasElse = true;

                            lineTypes.Add(LineType.ElseStatement);

                            lyneTypeAddEnable = false;

                            handleAddOfIdentationLevels(TipoTk.TkSenao, currentLine, true);

                            break;

                        case TipoTk.TkSenaoSe:
                            ifElementCounter++;
                            elseIfElementCounter++;
                            printerCurrentIfIdentationLevel++;
                            operationsInCurrentLine.Add(new Operation(null, null, lexicalTokens[i - 1].coluna, -1, lexicalTokens[i].tipo, OperationPrecedence.TK_IF_ATTRIBUTION_PRECEDENCE));
                            printerLastLineWithElseIf = currentLine;
                            printerCurrentIdentationLevel = null;
                            printerCurrentIdentationLevel = new IdentDesidentLevel(currentLine, TipoTk.TkSe);
                            printerCountLinesInElseIf = true;
                            lineTypeReset();
                            lyneTypeLastLineHasElseIf = true;
                            lineTypes.Add(LineType.ElseIfStatement);
                            lyneTypeAddEnable = false;
                            handleAddOfIdentationLevels(TipoTk.TkSenaoSe, currentLine, true);
                            break;

                        case TipoTk.TkDesident:
                            desidentElementCounter++;
                            operationsInCurrentLine.Add(new Operation(null, null, -1, -1, lexicalTokens[i].tipo, OperationPrecedence.TK_DESIDENT_PRECEDENCE));

                            printerCurrentDesidentLevel = null;
                            printerCurrentDesidentLevel = new IdentDesidentLevel(currentLine, TipoTk.TkDesident);

                            if (printerWaitingForElseDesident)
                            {
                                printerWaitingForElseDesident = false;
                                printerWaitingOffsetForJumpForward = true;
                            }

                            lyneTypeAddEnable = false;

                            if (lyneTypeForIdent)
                            {
                                lyneTypeForIdent = false;
                            }

                            break;

                        case TipoTk.TkIdent:

                            identElementCounter++;

                            if (lyneTypeLastLineHasIf)
                            {
                                lyneTypeIfIdent = true;
                            }
                            else if (lyneTypeLastLineHasElseIf)
                            {
                                lyneTypeElseIfIdent = true;
                            }
                            else if (lyneTypeLastLineHasElse)
                            {
                                lyneTypeElseIdent = true;
                            }
                            else if (lyneTypeLastLineHasWhile)
                            {
                                lyneTypeWhileIdent = true;
                            }
                            else if (lyneTypeLastLineHasFor)
                            {
                                lyneTypeForIdent = true;
                            }

                            printerLineInByteCodeRegisterWithElseDesident = -1;

                            if(printerWaitingForElseAfterElseIf){
                                //printerWaitingForElseAfterElseIf = false;
                            }

                        break;

                        case TipoTk.TkEnquanto:

                            whileElementCounter++;

                            lineTypeReset();

                            lyneTypeLastLineHasWhile = true;

                            //printerWhileInProgress = true;

                            lineTypes.Add(LineType.WhileStatement);

                            lyneTypeAddEnable = false;

                            handleAddOfIdentationLevels(TipoTk.TkEnquanto, currentLine, true);

                            break;

                        case TipoTk.TkRange:
                            rangeElementCouter++;
                            lineTypeReset();
                            lyneTypeLastLineHasFor = true;
                            printerVariableToRange = lexicalTokens[i - 2].valor;
                            handleAddOfIdentationLevels(TipoTk.TkFor, currentLine, true);
                            break;
                    }

                    if (lexicalTokens[i].tipo == TipoTk.TkAbreParenteses)
                    {
                        printerFoundOpenParenthesis = true;
                    }
                }
            }
        }

        public void lineTypeReset()
        {
            lyneTypeIfIdent = false;

            lyneTypeElseIfIdent = false;

            lyneTypeElseIdent = false;

            lyneTypeWhileIdent = false;

            lyneTypeForIdent = false;

            lyneTypeLastLineHasIf = false;

            lyneTypeLastLineHasElseIf = false;

            lyneTypeLastLineHasElse = false;

            lyneTypeLastLineHasWhile = false;

            lyneTypeLastLineHasFor = false;
        }

        public void resetLineElements()
        {
            addOperatorCounter = 0;

            subtractionOperatorCounter = 0;

            multiplicationOperatorCounter = 0;

            divOperatorCounter = 0;

            reducedAddOperatorCounter = 0;

            reducedSubtractionOperatorCounter = 0;

            reducedMultiplicationOperatorCounter = 0;

            reducedDivOperatorCounter = 0;

            equalOperatorCounter = 0;

            difOperatorCounter = 0;

            lessThanOperatorCounter = 0;

            biggerThanOperatorCounter = 0;

            attribuitionOperatorCounter = 0;

            operationsInCurrentLine.Clear();

            idElementsCounter = 0;

            ifElementCounter = 0;

            addOperatorCounterLeft = 0;

            addOperatorCounterRight = 0;

            subtractionOperatorCounterLeft = 0;

            subtractionOperatorCounterRight = 0;

            multiplicationOperatorCounterLeft = 0;

            multiplicationOperatorCounterRight = 0;

            divOperatorCounterLeft = 0;

            divOperatorCounterRight = 0;

            operationRelationalPosInCurrentLine = 0;

            desidentElementCounter = 0;

            identElementCounter = 0;

            elseElementCounter = 0;

            elseIfElementCounter = 0;

            whileElementCounter = 0;

            rangeElementCouter = 0;

            printerFoundOpenParenthesis = false;
        }
    }    
}