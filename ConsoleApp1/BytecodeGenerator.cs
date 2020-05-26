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

        //--------------------------------------------------------------------------------------
        // Variables to control the bytecode mounting
        public Boolean printerLoadConst = false;

        public Boolean printerSimpleAtrib = false;

        public Boolean foundAnIdentifier = false;

        public int? printerLastExpressionResult = null;

        // Stores the most right operation index already calculed with precedence 1
        public int printerMoreToRightOperationIndexPrecedence1 = 0;

        // Stores the most right operation index already calculed with precedence 2
        public int printerMoreToRightOperationIndexPrecedence2 = 0;
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

        public void handleTkId(Token tk, int currentPos)
        {
            Token tkProx = getNextToken(currentPos);

            switch (tkProx.tipo)
            {
                case TipoTk.TkAtrib:

                    operationAssignmentInProgress = true;

                   

                    //--------------------------------------------------------------------------------------
                    // Prepares STORE_FAST
                    BytecodeRegister bytecodeRegisterNextToken = new BytecodeRegister();

                    bytecodeRegisterNextToken.preview = ("(" + tk.valor + ")");

                    bytecodeRegisters.Add(bytecodeRegisterNextToken);
                    //--------------------------------------------------------------------------------------

                    break;
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

        public void mountBytecode(int currentLine)
        {
            int? identifierValue = 0;
            //--------------------------------------------------------------------------------------
            // Prepares LOAD_CONST

            // Verify if it's necessary to load a constant
            if (printerLoadConst)
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
                }
                // Load result
                else
                {
                    bytecodeRegisterCurrentToken.preview = "(" + printerLastExpressionResult.ToString() + ")";

                    identifierValue = printerLastExpressionResult;
                }

                bytecodeRegisters.Add(bytecodeRegisterCurrentToken);                
            }
            //--------------------------------------------------------------------------------------

            //--------------------------------------------------------------------------------------
            // Prepares attribuition

            // If there is a attribuition
            if ((attribuitionOperatorCounter > 0) && (!foundAnIdentifier))
            {
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
            }
            //--------------------------------------------------------------------------------------

            foundAnIdentifier = false;
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

        public void handleLine(int line)
        { 
            int quantityWithOperationWithMulPrecedence = getQuantityOfOperationsWithMulPrecedence();
            int quantityWithOperationWithAddPrecedence = getQuantityOfOperationsWithAddPrecedence();

            // Verify arithmetic operations with constants
            // While there are operations to analyze
            while((quantityWithOperationWithMulPrecedence>0) || (quantityWithOperationWithAddPrecedence>0))
            {
                for (int i = 0; i < operationsInCurrentLine.Count; i++)
                {
                    // Precedence 2
                    if (((operationsInCurrentLine[i].precedence == OperationPrecedence.TK_MUL_PRECEDENCE) && (quantityWithOperationWithMulPrecedence != 0) && (i>printerMoreToRightOperationIndexPrecedence2) && (!(operationsInCurrentLine[i].alreadyVerified))) || ((operationsInCurrentLine[i].calculateNow) && (operationsInCurrentLine[i].precedence == OperationPrecedence.TK_MUL_PRECEDENCE)))
                    {
                        if(arithmeticOperation(operationsInCurrentLine[i], i))
                        {
                            // It's necessary to show LOAD_CONST
                            printerLoadConst = true;

                            // Decrement because one operation was analyzed
                            quantityWithOperationWithMulPrecedence--;
                        }

                        // Return to the left
                        break;
                    }

                    // Precedence 1, just analyze if all level 2 precedencse was already analized
                    if (((operationsInCurrentLine[i].precedence == OperationPrecedence.TK_ADD_PRECEDENCE) && (quantityWithOperationWithMulPrecedence == 0) && (i>printerMoreToRightOperationIndexPrecedence1) && (!(operationsInCurrentLine[i].alreadyVerified))) || ((operationsInCurrentLine[i].calculateNow) && (operationsInCurrentLine[i].precedence == OperationPrecedence.TK_ADD_PRECEDENCE)))
                    {
                        if(arithmeticOperation(operationsInCurrentLine[i], i))
                        {
                            // It's necessary to show LOAD_CONST
                            printerLoadConst = true;

                            // Decrement because one operation was analyzed
                            quantityWithOperationWithAddPrecedence--;
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
                if ((operationsInCurrentLine[i].precedence == OperationPrecedence.TK_ATTRIBUTION_PRECEDENCE) && (i == operationsInCurrentLine.Count-1))
                {
                    // It's necessary to show LOAD_CONST
                    printerLoadConst = true;

                    printerSimpleAtrib = true;

                    break;
                }
            }

            mountBytecode(line);
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

                foundAnIdentifier = true;

                mountBytecode(currentLineInFile);

                identifierOperand1 = getIdentifierValue(operation.operand1);
            }

            // Verify if the second operand is identifier
            if (isIdentifier(operation.operand2))
            {
                // Verify if all the operations to the left was already calculated
                if (verifyLeftOperationsNotCalculated(index, operation.operand1Column, operation.operand2Column))
                {
                    return false;
                }

                foundAnIdentifier = true;

                mountBytecode(currentLineInFile);

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

                Console.Write(bytecodeRegister.offset + "  ");

                Console.Write(getOpCodeDescription(bytecodeRegister.opCode) + "\t\t");

                // TODO
                Console.Write(bytecodeRegister.stackPos + "  ");

                Console.WriteLine(bytecodeRegister.preview);
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
                    return "STORE_FAST";
                break;

                case 2:
                    return "STORE_NAME";
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
        }
    }    
}