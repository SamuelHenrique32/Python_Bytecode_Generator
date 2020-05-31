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

        public Boolean lexicAnalyzed = false;

        public Boolean syntacticAnalyzed = false;

        public Boolean correctSyntax;

        public Boolean currentIDAlreadyInSymbolsTable = false;

        public int currentLineInFile = 0;

        public int currentLineInGeneratedBytecode = 0;

        public int currentOffset = 0;

        //--------------------------------------------------------------------------------------
        // Operations
        public Boolean operationAssignmentInProgress = false;

        public List<Operation> operationsInCurrentLine = new List<Operation>();

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

        //--------------------------------------------------------------------------------------
        // Variables to control the bytecode mounting
        public Boolean printerLoadConst = false;

        public Boolean printerSimpleAtrib = false;

        public Boolean printerFoundAnIdentifier = false;

        public Boolean printerLoadName = false;

        public Boolean printerShowOperationType = false;

        public int? printerLastExpressionResult = null;

        // Stores the most right operation index already calculed with precedence 1
        public int printerMoreToRightOperationIndexPrecedence1 = 0;

        // Stores the most right operation index already calculed with precedence 2
        public int printerMoreToRightOperationIndexPrecedence2 = 0;

        public Stack<int?> printerOperationsStack = new Stack<int?>();

        public int? printerCompElementPosition = null;

        public TipoTk printerCompElement;
        //--------------------------------------------------------------------------------------

        public BytecodeGenerator()
        {
        }

        public int getQuantityOfOperationsWithMulPrecedence()
        {
            return (multiplicationOperatorCounter + divOperatorCounter + reducedMultiplicationOperatorCounter + reducedDivOperatorCounter);
        }

        public int getQuantityOfOperationsWithAddPrecedence()
        {
            return (addOperatorCounter + subtractionOperatorCounter + reducedAddOperatorCounter + reducedSubtractionOperatorCounter);
        }

        public int getLastLineInFile()
        {
            return lexicalTokens[lexicalTokens.Count-1].linha;
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
                    if (attribuitionOperatorCounter>0)
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
            foreach(Symbol sym in symbolsTable)
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

        public void updateIdentifierValue(String identifier, int? value)
        {
            foreach(Symbol sym in symbolsTable)
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

        public void mountBytecode(int currentLine, String identifier, Operation operation, int? value, Boolean mustAddIdentifier)
        {
            int? identifierValue = 0;
            Boolean mustAddLoadConst = true;

            //--------------------------------------------------------------------------------------
            // Prepares LOAD_CONST

            int valueToVerifyInStack = 0;

            if(printerAtribuition() && printerOperationsStack.Count >= 1)
            {
                // Do nothing
            }
            else
            {
                // Verify if it's necessary to load a constant
                if (printerLoadConst || printerOperationsStack.Count <= 1)
                {
                    BytecodeRegister bytecodeRegisterCurrentToken = new BytecodeRegister();

                    printerLoadConst = false;

                    bytecodeRegisterCurrentToken.lineInGeneratedBytecode = currentLineInGeneratedBytecode++;

                    bytecodeRegisterCurrentToken.lineInFile = currentLine;

                    bytecodeRegisterCurrentToken.offset = currentOffset;

                    this.currentOffset += 2;

                    bytecodeRegisterCurrentToken.opCode = (int)OpCode.LOAD_CONST;

                    // TODO
                    bytecodeRegisterCurrentToken.stackPos = 0;

                    // If it's a simple atrib, it's necessary to load the operand 2
                    if (printerSimpleAtrib)
                    {
                        printerSimpleAtrib = false;

                        bytecodeRegisterCurrentToken.preview = "(" + operationsInCurrentLine[operationsInCurrentLine.Count - 1].operand2.ToString() + ")";

                        identifierValue = Int16.Parse(operationsInCurrentLine[operationsInCurrentLine.Count - 1].operand2);

                        handleStack(OpCode.LOAD_CONST, Int16.Parse(operationsInCurrentLine[operationsInCurrentLine.Count - 1].operand2));
                    }
                    else if (printerShowOperationType)
                    {
                        if (isIdentifier(operation.operand2))
                        {
                            valueToVerifyInStack = (int)getIdentifierValue(operation.operand2);
                        }
                        else
                        {
                            valueToVerifyInStack = Int16.Parse(operation.operand2);
                        }

                        if (!printerOperationsStack.Contains(valueToVerifyInStack))
                        {
                            bytecodeRegisterCurrentToken.preview = "(" + operation.operand2 + ")";

                            identifierValue = Int16.Parse(operation.operand2);

                            handleStack(OpCode.LOAD_CONST, Int16.Parse(operation.operand2));
                        }
                        // Load result
                        else
                        {
                            bytecodeRegisterCurrentToken.preview = "(" + printerLastExpressionResult.ToString() + ")";

                            identifierValue = printerLastExpressionResult;

                            handleStack(OpCode.LOAD_CONST, printerLastExpressionResult);
                        }
                    }
                    // Load result
                    else
                    {
                        if (mustAddIdentifier)
                        {
                            bytecodeRegisterCurrentToken.preview = "(" + identifier.ToString() + ")";

                            handleStack(OpCode.LOAD_CONST, Int16.Parse(identifier));
                        }
                        else
                        {
                            if (printerLastExpressionResult != null)
                            {
                                bytecodeRegisterCurrentToken.preview = "(" + printerLastExpressionResult.ToString() + ")";

                                identifierValue = printerLastExpressionResult;

                                handleStack(OpCode.LOAD_CONST, printerLastExpressionResult);
                            }
                            else
                            {
                                mustAddLoadConst = false;
                            }
                        }                        
                    }

                    if (mustAddLoadConst)
                    {
                        bytecodeRegisters.Add(bytecodeRegisterCurrentToken);
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

                this.currentOffset += 2;

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

                this.currentOffset += 2;

                if(operation.currentOperator == TipoTk.TkMais)
                {
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
                else if(operation.currentOperator == TipoTk.TkDivisao)
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

                this.currentOffset += 2;

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

            printerFoundAnIdentifier = false;

            printerShowOperationType = false;
        }

        public Boolean printerAtribuition()
        {
            return ((attribuitionOperatorCounter > 0) && (!printerFoundAnIdentifier) && (!printerShowOperationType));
        }

        public void generateBytecode()
        {
            currentIDAlreadyInSymbolsTable = false;

            currentLineInFile = 1;

            int j = currentLineInFile-1;

            // For each line in file
            for (int i=1; i<=getLastLineInFile(); i++)
            {
                j = 0;

                // Find the first lexical token that has the line according to current line in file
                while(lexicalTokens[j].linha != currentLineInFile)
                {
                    j++;
                }

                // New line, analyze first element in line
                if (lexicalTokens[j].linha == currentLineInFile)
                {
                    resetLineElements();

                    verifyOperatorsInCurrentLine(currentLineInFile);

                    handleLine(i);

                    currentLineInFile++;

                    continue;
                }
            }

            printGeneratedBytecode();
        }

        public String getVariableForAttribuition()
        {
            foreach(Operation op in operationsInCurrentLine)
            {
                if(op.currentOperator == TipoTk.TkAtrib)
                {
                    return op.operand1;
                }
            }

            return null;
        }

        public Boolean verifyIdentifierLoaded(String identifier)
        {
            foreach(Symbol sym in symbolsTable)
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

        public void handleArithmeticalOperations(int quantityWithOperationWithMulPrecedence, int quantityWithOperationWithAddPrecedence)
        {
            // Verify arithmetic operations with constants
            // While there are operations to analyze
            while ((quantityWithOperationWithMulPrecedence > 0) || (quantityWithOperationWithAddPrecedence > 0))
            {
                for (int i = 0; i < operationsInCurrentLine.Count; i++)
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

            // Simple atrib
            for (int i = 0; i < operationsInCurrentLine.Count; i++)
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

            // The line has an if statement
            if (ifElementCounter > 0)
            {
                verifyCompElement();

                // There is one element in the left
                if(operationsInCurrentLine[1].currentOperator == printerCompElement)
                {
                    if (isIdentifier(operationsInCurrentLine[1].operand1))
                    {
                        printerLoadName = true;

                        mountBytecode(line, operationsInCurrentLine[1].operand1, null, null, true);
                    }
                }
            }

            handleArithmeticalOperations(quantityWithOperationWithMulPrecedence, quantityWithOperationWithAddPrecedence);
            
            mountBytecode(line, null, null, null, false);
        }

        private void verifyCompElement()
        {
            for (int i = 0; i < operationsInCurrentLine.Count; i++) {
                if(operationsInCurrentLine[i].currentOperator == TipoTk.TkMaior)
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
            for (int i = 0; i < currentIndex ; i++)
            {
                if((operationsInCurrentLine[i].precedence == OperationPrecedence.TK_ADD_PRECEDENCE) && (!(operationsInCurrentLine[i].alreadyVerified)) && (operationsInCurrentLine[i].operand2Column!=operand1Column))
                {
                    operationsInCurrentLine[i].calculateNow = true;

                    return true;
                }
            }

            return false;
        }

        public Boolean arithmeticOperation(Operation operation, int index)
        {
            int? operand1 = null;
            int? operand2 = null;

            int? identifierOperand1 = null;
            int? identifierOperand2 = null;

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

                    mountBytecode(currentLineInFile, operation.operand1, null, null, false);
                }

                identifierOperand1 = getIdentifierValue(operation.operand1);
            }

            // Verify if the second operand is identifier
            if (isIdentifier(operation.operand2))
            {
                // It's necessary to load the first element
                if(printerOperationsStack.Count == 0)
                {
                    if (identifierOperand1 != null)
                    {
                        printerLoadName = true;

                        mountBytecode(currentLineInFile, operation.operand1, null, null, false);

                        printerLoadName = false;

                        operand1 = identifierOperand1;
                    }
                    else
                    {
                        printerLoadConst = true;

                        mountBytecode(currentLineInFile, operation.operand1, null, null, true);

                        printerLoadConst = false; ;

                        operand1 = Int16.Parse(operation.operand1);
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

                    mountBytecode(currentLineInFile, operation.operand2, null, null, false);
                }

                identifierOperand2 = getIdentifierValue(operation.operand2);
            }

            if (identifierOperand1 != null)
            {
                operand1 = identifierOperand1;
            }
            else
            {
                operand1 = Int16.Parse(operation.operand1);
            }

            if (identifierOperand2 != null)
            {
                operand2 = identifierOperand2;
            }
            else
            {
                operand2 = Int16.Parse(operation.operand2);
            }

            switch (operation.currentOperator)
            {
                case TipoTk.TkMais:                    

                    // There is no operator already used
                    if ((!verifyResultForAlreadyUsedElement(operation.operand1Column, 1)) && (!verifyResultForAlreadyUsedElement(operation.operand2Column, 2)))
                    {
                        operationsInCurrentLine[index].result = operand1 + operand2;
                    }
                    // Left element was already used
                    else if ((verifyResultForAlreadyUsedElement(operation.operand1Column, 1)) && (!verifyResultForAlreadyUsedElement(operation.operand2Column, 2)))
                    {
                        operand1 = getResultOfOperationWithAlreadyUsedElement(operation.operand1Column, 1);

                        operationsInCurrentLine[index].result = operand1 + operand2;

                        operationsInCurrentLine[index-1].result = operand1 + operand2;

                        printerMoreToRightOperationIndexPrecedence1 = index;
                    }
                    // Right element was already used
                    else if ((!verifyResultForAlreadyUsedElement(operation.operand1Column, 1)) && (verifyResultForAlreadyUsedElement(operation.operand2Column, 2)))
                    {
                        operand2 = getResultOfOperationWithAlreadyUsedElement(operation.operand2Column, 2);

                        operationsInCurrentLine[index].result = operand1 + operand2;

                        operationsInCurrentLine[index+1].result = operand1 + operand2;

                        printerMoreToRightOperationIndexPrecedence1 = index;
                    }
                    // Both elements was already used
                    else
                    {
                        operand1 = getResultOfOperationWithAlreadyUsedElement(operation.operand1Column, 1);
                        operand2 = getResultOfOperationWithAlreadyUsedElement(operation.operand2Column, 2);

                        operationsInCurrentLine[index].result = operand1 + operand2;
                    }

                    printerLastExpressionResult = operationsInCurrentLine[index].result;

                    operationsInCurrentLine[index].alreadyVerified = true;

                break;

                case TipoTk.TkMenos:

                    // There is no operator already used
                    if ((!verifyResultForAlreadyUsedElement(operation.operand1Column, 1)) && (!verifyResultForAlreadyUsedElement(operation.operand2Column, 2)))
                    {
                        operationsInCurrentLine[index].result = operand1 - operand2;
                    }
                    // Left element was already used
                    else if ((verifyResultForAlreadyUsedElement(operation.operand1Column, 1)) && (!verifyResultForAlreadyUsedElement(operation.operand2Column, 2)))
                    {
                        operand1 = getResultOfOperationWithAlreadyUsedElement(operation.operand1Column, 1);

                        operationsInCurrentLine[index].result = operand1 - operand2;

                        operationsInCurrentLine[index - 1].result = operand1 - operand2;

                        printerMoreToRightOperationIndexPrecedence1 = index;
                    }
                    // Right element was already used
                    else if ((!verifyResultForAlreadyUsedElement(operation.operand1Column, 1)) && (verifyResultForAlreadyUsedElement(operation.operand2Column, 2)))
                    {
                        operand2 = getResultOfOperationWithAlreadyUsedElement(operation.operand2Column, 2);

                        operationsInCurrentLine[index].result = operand1 - operand2;

                        operationsInCurrentLine[index + 1].result = operand1 - operand2;

                        printerMoreToRightOperationIndexPrecedence1 = index;
                    }
                    // Both elements was already used
                    else
                    {
                        operand1 = getResultOfOperationWithAlreadyUsedElement(operation.operand1Column, 1);
                        operand2 = getResultOfOperationWithAlreadyUsedElement(operation.operand2Column, 2);

                        operationsInCurrentLine[index].result = operand1 - operand2;
                    }

                    printerLastExpressionResult = operationsInCurrentLine[index].result;

                    operationsInCurrentLine[index].alreadyVerified = true;

                break;

                case TipoTk.TkMultiplicacao:

                    // There is no operator already used
                    if ((!verifyResultForAlreadyUsedElement(operation.operand1Column, 1)) && (!verifyResultForAlreadyUsedElement(operation.operand2Column, 2)))
                    {
                        operationsInCurrentLine[index].result = operand1 * operand2;
                    }
                    // Left element was already used
                    else if ((verifyResultForAlreadyUsedElement(operation.operand1Column, 1)) && (!verifyResultForAlreadyUsedElement(operation.operand2Column, 2)))
                    {
                        operand1 = getResultOfOperationWithAlreadyUsedElement(operation.operand1Column, 1);

                        operationsInCurrentLine[index].result = operand1 * operand2;

                        operationsInCurrentLine[index - 1].result = operand1 * operand2;

                        printerMoreToRightOperationIndexPrecedence2 = index;
                    }
                    // Right element was already used
                    else if ((!verifyResultForAlreadyUsedElement(operation.operand1Column, 1)) && (verifyResultForAlreadyUsedElement(operation.operand2Column, 2)))
                    {
                        operand2 = getResultOfOperationWithAlreadyUsedElement(operation.operand2Column, 2);

                        operationsInCurrentLine[index].result = operand1 * operand2;

                        operationsInCurrentLine[index + 1].result = operand1 * operand2;

                        printerMoreToRightOperationIndexPrecedence2 = index;
                    }
                    // Both elements was already used
                    else
                    {
                        operand1 = getResultOfOperationWithAlreadyUsedElement(operation.operand1Column, 1);
                        operand2 = getResultOfOperationWithAlreadyUsedElement(operation.operand2Column, 2);

                        operationsInCurrentLine[index].result = operand1 * operand2;
                    }

                    printerLastExpressionResult = operationsInCurrentLine[index].result;

                    operationsInCurrentLine[index].alreadyVerified = true;

                break;

                case TipoTk.TkDivisao:

                    // There is no operator already used
                    if ((!verifyResultForAlreadyUsedElement(operation.operand1Column, 1)) && (!verifyResultForAlreadyUsedElement(operation.operand2Column, 2)))
                    {
                        operationsInCurrentLine[index].result = operand1 / operand2;
                    }
                    // Left element was already used
                    else if ((verifyResultForAlreadyUsedElement(operation.operand1Column, 1)) && (!verifyResultForAlreadyUsedElement(operation.operand2Column, 2)))
                    {
                        operand1 = getResultOfOperationWithAlreadyUsedElement(operation.operand1Column, 1);

                        operationsInCurrentLine[index].result = operand1 / operand2;

                        operationsInCurrentLine[index - 1].result = operand1 / operand2;

                        printerMoreToRightOperationIndexPrecedence2 = index;
                    }
                    // Right element was already used
                    else if ((!verifyResultForAlreadyUsedElement(operation.operand1Column, 1)) && (verifyResultForAlreadyUsedElement(operation.operand2Column, 2)))
                    {
                        operand2 = getResultOfOperationWithAlreadyUsedElement(operation.operand2Column, 2);

                        operationsInCurrentLine[index].result = operand1 / operand2;

                        operationsInCurrentLine[index + 1].result = operand1 / operand2;

                        printerMoreToRightOperationIndexPrecedence2 = index;
                    }
                    // Both elements was already used
                    else
                    {
                        operand1 = getResultOfOperationWithAlreadyUsedElement(operation.operand1Column, 1);
                        operand2 = getResultOfOperationWithAlreadyUsedElement(operation.operand2Column, 2);

                        operationsInCurrentLine[index].result = operand1 / operand2;
                    }

                    printerLastExpressionResult = operationsInCurrentLine[index].result;

                    operationsInCurrentLine[index].alreadyVerified = true;

                break;

            }

            this.operationsInCurrentLine[index].calculateNow = false;

            // If a identifier was used, it's necessary to mount the operation
            if((identifierOperand1!=null) || (identifierOperand2 != null))
            {
                printerShowOperationType = true;

                printerLoadConst = false;

                mountBytecode(currentLineInFile, null, operation, printerLastExpressionResult, false);
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
            foreach(Operation op in operationsInCurrentLine)
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

        public void printGeneratedBytecode()
        {
            int lastPrintedLine = 0;

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

                if (bytecodeRegister.offset < 10)
                {
                    Console.Write(" " + bytecodeRegister.offset + "  ");
                }
                else
                {
                    Console.Write(bytecodeRegister.offset + "  ");
                }

                Console.Write(getOpCodeDescription(bytecodeRegister.opCode) + "\t\t");

                if (bytecodeRegister.opCode!=(int)OpCode.BINARY_ADD && bytecodeRegister.opCode!=(int)OpCode.BINARY_SUBTRACT && bytecodeRegister.opCode!=(int)OpCode.BINARY_MULTIPLY && bytecodeRegister.opCode!=(int)OpCode.BINARY_TRUE_DIVIDE)
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
            }

            return "";
        }

        public void verifyOperatorsInCurrentLine(int currentLine)
        {
            for(int i=0; i< lexicalTokens.Count; i++)
            {
                if(lexicalTokens[i].linha == currentLine)
                {
                    // TODO Don't call operationsInCurrentLine so many times
                    switch (lexicalTokens[i].tipo)
                    {
                        case TipoTk.TkMais:
                            addOperatorCounter++;
                            operationsInCurrentLine.Add(new Operation(lexicalTokens[i-1].valor, lexicalTokens[i+1].valor, lexicalTokens[i-1].coluna, lexicalTokens[i+1].coluna, lexicalTokens[i].tipo, OperationPrecedence.TK_ADD_PRECEDENCE));
                        break;

                        case TipoTk.TkMenos:
                            subtractionOperatorCounter++;
                            operationsInCurrentLine.Add(new Operation(lexicalTokens[i - 1].valor, lexicalTokens[i + 1].valor, lexicalTokens[i - 1].coluna, lexicalTokens[i + 1].coluna, lexicalTokens[i].tipo, OperationPrecedence.TK_SUB_PRECEDENCE));
                        break;

                        case TipoTk.TkMultiplicacao:
                            multiplicationOperatorCounter++;
                            operationsInCurrentLine.Add(new Operation(lexicalTokens[i - 1].valor, lexicalTokens[i + 1].valor, lexicalTokens[i - 1].coluna, lexicalTokens[i + 1].coluna, lexicalTokens[i].tipo, OperationPrecedence.TK_MUL_PRECEDENCE));
                        break;

                        case TipoTk.TkDivisao:
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
                        break;

                        case TipoTk.TkDiferente:
                            difOperatorCounter++;
                            operationsInCurrentLine.Add(new Operation(lexicalTokens[i - 1].valor, lexicalTokens[i + 1].valor, lexicalTokens[i - 1].coluna, lexicalTokens[i + 1].coluna, lexicalTokens[i].tipo, OperationPrecedence.TK_DIFF_PRECEDENCE));
                        break;

                        case TipoTk.TkMenor:
                            lessThanOperatorCounter++;
                            operationsInCurrentLine.Add(new Operation(lexicalTokens[i - 1].valor, lexicalTokens[i + 1].valor, lexicalTokens[i - 1].coluna, lexicalTokens[i + 1].coluna, lexicalTokens[i].tipo, OperationPrecedence.TK_LESS_THAN_PRECEDENCE));
                        break;

                        case TipoTk.TkMaior:
                            biggerThanOperatorCounter++;
                            operationsInCurrentLine.Add(new Operation(lexicalTokens[i - 1].valor, lexicalTokens[i + 1].valor, lexicalTokens[i - 1].coluna, lexicalTokens[i + 1].coluna, lexicalTokens[i].tipo, OperationPrecedence.TK_BIGGER_THAN_PRECEDENCE));
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
                            operationsInCurrentLine.Add(new Operation(null, null, lexicalTokens[i - 1].coluna, -1, lexicalTokens[i].tipo, OperationPrecedence.TK_IF_ATTRIBUTION_PRECEDENCE));
                        break;
                    }
                }
            }
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
        }
    }    
}