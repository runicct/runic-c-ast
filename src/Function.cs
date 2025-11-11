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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Runic.C
{
    public partial class AST : Runic.AST.INodeStream
    {
        internal class Function : Runic.AST.Node.Function, ICScope
        {
            AST _parent;
            ICScope _parentScope;
#if NET6_0_OR_GREATER
            Runic.AST.Node[]? _body = null;
            public override Runic.AST.Node[]? Body { get { return _body; } }
#else
            Runic.AST.Node[] _body = null;
            public override Runic.AST.Node[] Body { get { return _body; } }
#endif
            string _name;
            public  string Name { get { return _name; } }
            Runic.AST.Variable.FunctionParameter[] _parameters;
            public override Runic.AST.Variable.FunctionParameter[] Parameters { get { return _parameters; } }
            Runic.AST.Type _returnType;
            public override Runic.AST.Type ReturnType { get { return _returnType; } }
            bool _isDeclaration = false;
            internal bool IsDeclaration { get { return _isDeclaration; } }
            Dictionary<ulong, Runic.AST.Node.Label> _localLabels = new Dictionary<ulong, Runic.AST.Node.Label>();

            Dictionary<ulong, Runic.AST.Node.Label> _labels = new Dictionary<ulong, Runic.AST.Node.Label>();
            public Runic.AST.Node.Label GetLabel(Parser.Label label)
            {
                Runic.AST.Node.Label result;
                if (!_labels.TryGetValue(label.Index, out result))
                {
                    result = new Label(this);
                    _labels.Add(label.Index, result);
                }
                return result;
            }
            Dictionary<ulong, Runic.AST.Variable.LocalVariable> _localVariables = new Dictionary<ulong, Runic.AST.Variable.LocalVariable>();
#if NET6_0_OR_GREATER
            public Runic.AST.Variable? GetVariable(Parser.Variable variable)
#else
            public Runic.AST.Variable GetVariable(Parser.Variable variable)
#endif
            {
                switch (variable)
                {
                    case Parser.LocalVariable local:
                        {
                            Runic.AST.Variable.LocalVariable localVariable;
                            if (!_localVariables.TryGetValue(local.Index, out localVariable))
                            {
                                localVariable = new Runic.AST.Variable.LocalVariable(this, _parent.GetType(local.VariableType));
                                _localVariables.Add(local.Index, localVariable);
                            }
                            return localVariable;
                        }
                    case Parser.GlobalVariable global: return _parentScope.GetVariable(global);
                    case Parser.FunctionParameter parameter: return _parameters[parameter.Index];
                }
                return _parentScope.GetVariable(variable);
            }
            public AST.Function GetFunction(Parser.Function func)
            {
                if (func.Name.Value != null)
                {
                    if (func.Name.Value == this.Name)
                    {
                        return this;
                    }
                }

                return _parentScope.GetFunction(func);
            }
            public Runic.AST.Node GetBreakOrContinueNode() { return this; }
            public AST.Function GetParentFunction() { return this; }
            internal void ReplaceDeclarationByDefinition(Parser.FunctionDefinition Statement)
            {
                _isDeclaration = false;
            }
            internal Function(AST parent, ICScope parentScope, Parser.FunctionDefinition statement) : base()
            {
                _parentScope = parentScope;
                _parent = parent;
                _name = statement.Name.Value;
                _parameters = new Runic.AST.Variable.FunctionParameter[statement.FunctionParameters.Length];
                _returnType = parent.GetType(statement.ReturnType);
                for (int n = 0; n < statement.FunctionParameters.Length; n++)
                {
                    _parameters[n] = new Runic.AST.Variable.FunctionParameter(this, parent.GetType(statement.FunctionParameters[n].VariableType));
                }
            }
            internal Function(AST parent, ICScope parentScope, Parser.FunctionDeclaration statement) : base()
            {
                _parentScope = parentScope;
                _parent = parent;
                _name = statement.Name.Value;
                _parameters = new Runic.AST.Variable.FunctionParameter[statement.FunctionParameters.Length];
                _returnType = parent.GetType(statement.ReturnType);
                for (int n = 0; n < statement.FunctionParameters.Length; n++)
                {
                    _parameters[n] = new Runic.AST.Variable.FunctionParameter(this, _parent.GetType(statement.FunctionParameters[n].VariableType));
                }
                _isDeclaration = true;
            }
            internal void ReadBody(AST Parent)
            {
                List<Runic.AST.Node> body = new List<Runic.AST.Node>();
                while (true)
                {
#if NET6_0_OR_GREATER
                    Runic.Statement? statement = Parent.ReadNextStatement();
#else
                    Runic.Statement statement = Parent.ReadNextStatement();
#endif
                    if (statement == null) { break; }
                    if (statement is Parser.Scope.ExitFunctionDefinition) { break; }
#if NET6_0_OR_GREATER
                    Runic.AST.Node? node = Parent.ReadNextNode(this, statement);
#else
                    Runic.AST.Node node = Parent.ReadNextNode(this, statement);
#endif
                    if (node == null) { break; }
                    do
                    {
                        body.Add(node);
                        node = Parent.DequeuePendingNode();
                    } while (node != null);
                }
                _body = body.ToArray();
            }
        }
    }
}
