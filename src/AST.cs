/*
 * MIT License
 * 
 * Copyright (c) 2025 Runic Compiler Toolkit Contributors
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using Runic.AST;
using static Runic.C.Parser;
using System.Collections.Generic;

namespace Runic.C
{
    public partial class AST : Runic.AST.INodeStream
    {
        protected virtual int SizeOfLong { get { return 32; } }
        protected virtual int SizeOfInt { get { return 32; } }
        protected virtual Runic.AST.Type.Char.CharEncoding WCharEncoding { get { return Runic.AST.Type.Char.CharEncoding.UTF16; } }
        class StatementQueue : IStatementStream
        {
            IStatementStream _input;
            Queue<Statement> _queue = new Queue<Statement>();
            public StatementQueue(IStatementStream input)
            {
                _input = input;
            }
#if NET6_0_OR_GREATER
            public Statement? ReadNextStatement()
#else
            public Statement ReadNextStatement()
#endif
            {
                if (_queue.Count > 0) { return _queue.Dequeue(); }
                return _input.ReadNextStatement();
            }
#if NET6_0_OR_GREATER
            public Statement? PeekNextStatement()
#else
            public Statement PeekNextStatement()
#endif
            {
                if (_queue.Count > 0) { return _queue.Peek(); }
#if NET6_0_OR_GREATER
                Statement? statement = _input.ReadNextStatement();
#else
                Statement statement = _input.ReadNextStatement();
#endif
                if (statement == null) { return null; }
                _queue.Enqueue(statement);
                return statement;
            }
        }
        public virtual void Error_FunctionMustReturnValue(Parser.Return Return) { }
        public virtual void Error_VoidFunctionReturningValue(Parser.Return Return) { }
        public virtual void Error_ExpectedWhileAfterDo(Parser.UnscopedDo _do, Runic.Statement invalid) { }

        StatementQueue _input;
        Root _rootNode;
        public AST(IStatementStream StatementStream)
        {
            _rootNode = new Root(this);
            _input = new StatementQueue(StatementStream);
        }
#if NET6_0_OR_GREATER
        internal Statement? ReadNextStatement()
#else
        internal Statement ReadNextStatement()
#endif
        {
            return _input.ReadNextStatement();
        }

        Queue<Node> _nodeQueue = new Queue<Node>();
#if NET6_0_OR_GREATER
        internal Node? DequeuePendingNode()
#else
        internal Node DequeuePendingNode()
#endif
        {
            if (_nodeQueue.Count > 0)
            {
                return _nodeQueue.Dequeue();
            }
            return null;
        }

        static Runic.AST.Type.Void _voidType = new Runic.AST.Type.Void();

        Dictionary<string, CStruct> _structs = new Dictionary<string, CStruct>();
        internal Runic.AST.Type GetType(Parser.Type type)
        {
            switch (type)
            {
                case Parser.Type.Char @char: return Type._cchar;
                case Parser.Type.Int @int: switch (SizeOfInt) { case 64: return Type._cint64; default: return Type._cint32; }
                case Parser.Type.Long @long: switch (SizeOfInt) { case 64: return Type._clong64; default: return Type._clong32; }
                case Parser.Type.LongLong longlong: return Type._clonglong;
                case Parser.Type.Float @float: return Type._cfloat;
                case Parser.Type.Double @double: return Type._cdouble;
                case Parser.Type.Void _void: return _voidType;
                case Parser.Type.FunctionPointerType functionPointer:
                    {
                        Runic.AST.Type returnType = GetType(functionPointer.ReturnType);
                        Runic.AST.Type[] parameterTypes = new Runic.AST.Type[functionPointer.Parameters.Length];
                        for (int n = 0; n < functionPointer.Parameters.Length; n++)
                        {
                            parameterTypes[n] = GetType(functionPointer.Parameters[n].VariableType);
                        }
                        return new Runic.AST.Type.FunctionPointer(returnType, parameterTypes);
                    }
                case Parser.Type.StaticArray staticArray:
                    {
                        return new Runic.AST.Type.Pointer(GetType(staticArray.TargetType));
                    }
                    break;
                case Parser.Type.Pointer pointer:
                    {
                        Runic.AST.Type targetType = GetType(pointer.TargetType);
                        return new Runic.AST.Type.Pointer(targetType);
                    }
                case Parser.Type.StructOrUnion structOrUnion:
                    {
                        if (structOrUnion.Name == null) { return CStruct.Create(this, structOrUnion); }
                        CStruct astStruct; 
                        if (!_structs.TryGetValue(structOrUnion.Name.Value, out astStruct))
                        {
                            astStruct = CStruct.Create(this, structOrUnion);
                            _structs.Add(structOrUnion.Name.Value, astStruct);
                        }
                        return astStruct;
                    }
                    break;

            }
            return null;
        }
#if NET6_0_OR_GREATER
        internal Node? ReadNextNode(ICScope parent, Runic.Statement? initialStatement)
#else
        internal Node ReadNextNode(ICScope parent, Runic.Statement initialStatement)
#endif
        {
            if (_nodeQueue.Count > 0)
            {
                return _nodeQueue.Dequeue();
            }
#if NET6_0_OR_GREATER
            Runic.Statement? statement = initialStatement;
#else
            Runic.Statement statement = initialStatement;
#endif
            if (statement == null)
            {
                statement = _input.ReadNextStatement();
            }
            restart:;
            if (statement == null) { return null; }
            switch (statement)
            {
                case Parser.VariableDeclaration variableDeclaration:
                    Runic.AST.Variable variable = parent.GetVariable(variableDeclaration.Variable);
                    if (variableDeclaration.Initialization != null)
                    {
                        return new Runic.AST.Node.Expression.VariableAssignment(variable, BuildExpression(this, parent, variable.Type, variableDeclaration.Initialization));
                    }
                    break;
                case Parser.Expression expression: return BuildExpression(this, parent, null, expression);
                case Parser.FunctionDefinition functionDefinition:
                    {
#if NET6_0_OR_GREATER
                        Function? function = null;
#else
                        Function function = null;
#endif
                        if (_rootNode.TryGetFunction(functionDefinition.Name.Value, out function))
                        {
                            if (function == null)
                            {
                                function = new Function(this, parent, functionDefinition);
                                _rootNode.AddFunction(functionDefinition.Name.Value, function);
                            }
                            if (function.IsDeclaration)
                            {
                                function.ReplaceDeclarationByDefinition(functionDefinition);
                            }
                        }
                        else
                        {
                            function = new Function(this, parent, functionDefinition);
                            _rootNode.AddFunction(functionDefinition.Name.Value, function);
                        }
                        function.ReadBody(this);
                        return function;
                    }
                case Parser.FunctionDeclaration functionDeclaration:

                    if (_rootNode.ContainsFunction(functionDeclaration.Name.Value))
                    {
                        statement = _input.ReadNextStatement();
                        goto restart;
                    }
                    _rootNode.AddFunction(functionDeclaration.Name.Value, new Function(this, parent, functionDeclaration));
                    goto restart;
                case Parser.Switch _switch:
                    {
                        Switch nodeSwitch = new Switch(this, parent, _switch);
                        nodeSwitch.Build(this);
                        if (nodeSwitch.Body != null && nodeSwitch.Body.Length > 0)
                        {
                            for (int n = 0; n < nodeSwitch.Body.Length; n++)
                            {
                                _nodeQueue.Enqueue(nodeSwitch.Body[n]);
                            }
                            return _nodeQueue.Dequeue();
                        }
                        return new Runic.AST.Node.Empty();
                    }
                case Parser.If _if:
                    {
                        If nodeIf = new If(this, parent, _if);
                        nodeIf.Build(this);
                        // Check for an else
#if NET6_0_OR_GREATER
                        Statement? maybeElse = _input.PeekNextStatement();
#else
                        Statement maybeElse = _input.PeekNextStatement();
#endif
                        if (maybeElse == null) { return nodeIf; }
                        switch (maybeElse)
                        {
                            case Parser.Else elseBlock:
                                _input.ReadNextStatement();
                                nodeIf.BuildElse(this);
                                break;
                            case Parser.UnscopedElse unscopedElse:
                                _input.ReadNextStatement();
                                nodeIf.BuildElseUnscoped(this);
                                break;
                        }
                        return nodeIf;
                    }
                case Parser.UnscopedIf _if:
                    {
                        If nodeIf = new If(this, parent, _if);
                        nodeIf.BuildUnscoped(this);
                        // Check for an else
#if NET6_0_OR_GREATER
                        Statement? maybeElse = _input.PeekNextStatement();
#else
                        Statement maybeElse = _input.PeekNextStatement();
#endif
                        if (maybeElse == null) { return nodeIf; }
                        switch (maybeElse)
                        {
                            case Parser.Else elseBlock:
                                _input.ReadNextStatement();
                                nodeIf.BuildElse(this);
                                break;
                            case Parser.UnscopedElse unscopedElse:
                                _input.ReadNextStatement();
                                nodeIf.BuildElseUnscoped(this);
                                break;
                        }
                        return nodeIf;
                    }
                    break;
                case Parser.Else _else:
                    {
                        GenericScope nodeScope = new GenericScope(this, parent, _else);
                        nodeScope.Build(this);
                        if (nodeScope.Body != null && nodeScope.Body.Length > 0)
                        {
                            for (int n = 0; n < nodeScope.Body.Length; n++)
                            {
                                _nodeQueue.Enqueue(nodeScope.Body[n]);
                            }
                            return _nodeQueue.Dequeue();
                        }
                        else
                        {
                            return new Runic.AST.Node.Empty();
                        }
                    }
                case Parser.While _while:
                    {
                        While nodeWhile = new While(this, parent, _while);
                        nodeWhile.Build(this);
                        _nodeQueue.Enqueue(nodeWhile.EndLabel);
                        return nodeWhile;
                    }
                case Parser.UnscopedWhile _while:
                    {
                        While nodeWhile = new While(this, parent, _while);
                        nodeWhile.BuildUnscoped(this);
                        _nodeQueue.Enqueue(nodeWhile.EndLabel);
                        return nodeWhile;
                    }
                case Parser.EmptyWhile _while:
                    {
                        While nodeWhile = new While(this, parent, _while);
                        _nodeQueue.Enqueue(nodeWhile.EndLabel);
                        return nodeWhile;
                    }
                case Parser.UnscopedDo _do:
                    {
                        DoWhile nodeDoWhile = new DoWhile(this, parent, _do);
                        nodeDoWhile.BuildUnscoped(this);
                        _nodeQueue.Enqueue(nodeDoWhile.EndLabel);
                        return nodeDoWhile;
                    }
                case Parser.For _for:
                    For nodeFor = new For(this, parent, _for);
                    nodeFor.Build(this);
                    _nodeQueue.Enqueue(nodeFor.EndLabel);
                    return nodeFor;
                case Parser.DoWhile doWhile:
                    {
                        DoWhile nodeDoWhile = new DoWhile(this, parent, doWhile);
                        nodeDoWhile.Build(this);
                        _nodeQueue.Enqueue(nodeDoWhile.EndLabel);
                        return nodeDoWhile;
                    }
                case Parser.Scope.Enter scopeEnter:
                    {
                        GenericScope nodeScope = new GenericScope(this, parent, scopeEnter);
                        nodeScope.Build(this);
                        if (nodeScope.Body != null && nodeScope.Body.Length > 0)
                        {
                            for (int n = 0; n < nodeScope.Body.Length; n++)
                            {
                                _nodeQueue.Enqueue(nodeScope.Body[n]);
                            }
                            return _nodeQueue.Dequeue();
                        }
                        else
                        {
                            return new Runic.AST.Node.Empty();
                        }
                    }
                case Parser.Return _return:
                    Runic.AST.Node.Return nodeReturn;
                    if (_return.Value != null) { nodeReturn = new Runic.AST.Node.Return(BuildExpression(this, parent, null, _return.Value)); }
                    else { nodeReturn = new Runic.AST.Node.Return(null); }
                    return nodeReturn;
                case Parser.Break _break:
                    {
                        Node breakNode = parent.GetBreakOrContinueNode();
                        switch (breakNode)
                        {
                            case While whileNode: return new Runic.AST.Node.Branch(whileNode.EndLabel);
                            case DoWhile dowhileNode: return new Runic.AST.Node.Branch(dowhileNode.EndLabel);
                            case For forNode: return new Runic.AST.Node.Branch(forNode.EndLabel);
                            // This is invalid. We use it just to avoid errors further down the line
                            case Switch switchNode: return new Runic.AST.Node.Branch(switchNode.EndLabel);
                            case Switch.Case caseNode: return new Runic.AST.Node.Branch(caseNode.ParentSwitch.EndLabel);
                            case Switch.Default defaultNode: return new Runic.AST.Node.Branch(defaultNode.ParentSwitch.EndLabel);
                        }
                        return null;
                    }
                    break;
                case Parser.Continue _continue:
                    {
                        Node continueNode = parent.GetBreakOrContinueNode();
                        switch (continueNode)
                        {
                            case While whileNode: return new Runic.AST.Node.Branch(whileNode.RestartLabel);
                            case DoWhile dowhileNode: return new Runic.AST.Node.Branch(dowhileNode.RestartLabel);
                            case For forNode: return new Runic.AST.Node.Branch(forNode.RestartLabel);
                            // This is invalid. We use it just to avoid errors further down the line
                            case Switch switchNode: return new Runic.AST.Node.Branch(switchNode.EndLabel);
                            // Note: The end label use here is intentional; continue in a switch jumps to the end of the case/default
                            case Switch.Case caseNode: return new Runic.AST.Node.Branch(caseNode.EndLabel);
                            case Switch.Default defaultNode: return new Runic.AST.Node.Branch(defaultNode.EndLabel);
                        }
                        return null;
                    }
                    break;
                case Parser.Case @case:
                        return new Switch.IntermediateCase(this, parent, @case);
                case Parser.Default @default:
                    return new Switch.IntermediateDefault(this, parent, @default);
                case Parser.Goto @goto:
                    if (@goto.Label == null) { return new Runic.AST.Node.Empty(); }
                    return new Runic.AST.Node.Branch(parent.GetLabel(@goto.Label));
                case Parser.Label label:
                    return parent.GetLabel(label);
                case Parser.Empty empty:
                    return new Runic.AST.Node.Empty();
            }
            statement = _input.ReadNextStatement();
            goto restart;
        }
#if NET6_0_OR_GREATER
        internal Node? ReadNextNode(Runic.Statement? initialStatement)
#else
        internal Node ReadNextNode(Runic.Statement initialStatement)
#endif
        {
            return ReadNextNode(_rootNode, initialStatement);
        }
#if NET6_0_OR_GREATER
        public Node? ReadNextNode()
#else
        public Node ReadNextNode()
#endif
        {
            return ReadNextNode(null);
        }
    }
}
