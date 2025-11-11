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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runic.C
{
    public partial class AST : Runic.AST.INodeStream
    {
        internal class While : Runic.AST.Node.While, ICScope
        {
            Runic.AST.Node.Label _restartLabel;
            public Runic.AST.Node.Label RestartLabel { get { return _restartLabel; } }
            Runic.AST.Node.Label _endLabel;
            public Runic.AST.Node.Label EndLabel { get { return _endLabel; } }
            ICScope _parentScope;
#if NET6_0_OR_GREATER
            Runic.AST.Node[]? _body = null;
            public override Runic.AST.Node[]? Body { get { return _body; } }
#else
            Runic.AST.Node[] _body = null;
            public override Runic.AST.Node[] Body { get { return _body; } }
#endif
            Runic.Statement _statement;
            public Runic.Statement Statement { get { return _statement; } }
            public Runic.AST.Node GetBreakOrContinueNode() { return this; }
            public AST.Function GetFunction(Parser.Function function) { return _parentScope.GetFunction(function); }
            public Runic.AST.Node.Label GetLabel(Parser.Label label) { return _parentScope.GetLabel(label); }
            public Runic.AST.Variable GetVariable(Parser.Variable variable) { return _parentScope.GetVariable(variable); }
            public AST.Function GetParentFunction() { return _parentScope.GetParentFunction(); }
            internal While(AST parent, ICScope parentScope, Parser.While Statement) : base(parent.BuildExpression(parent, parentScope, null, Statement.Condition))
            {
                _parentScope = parentScope;
                _statement = Statement;
                _endLabel = new Runic.AST.Node.Label(parentScope.GetParentFunction());
                _restartLabel = new Runic.AST.Node.Label(parentScope.GetParentFunction());
            }
            internal While(AST parent, ICScope parentScope, Parser.UnscopedWhile Statement) : base(parent.BuildExpression(parent, parentScope, null, Statement.Condition))
            {
                _parentScope = parentScope;
                _statement = Statement;
                _endLabel = new Runic.AST.Node.Label(parentScope.GetParentFunction());
                _restartLabel = new Runic.AST.Node.Label(parentScope.GetParentFunction());
            }
            internal While(AST parent, ICScope parentScope, Parser.EmptyWhile Statement) : base(parent.BuildExpression(parent, parentScope, null, Statement.Condition))
            {
                _parentScope = parentScope;
                _statement = Statement;
                _body = new Node[0];
                _endLabel = new Runic.AST.Node.Label(parentScope.GetParentFunction());
                _restartLabel = new Runic.AST.Node.Label(parentScope.GetParentFunction());
            }
            internal void Build(AST Parent)
            {
                List<Node> body = new List<Node>();
                while (true)
                {
#if NET6_0_OR_GREATER
                    Runic.Statement? statement = Parent.ReadNextStatement();
#else
                    Runic.Statement statement = Parent.ReadNextStatement();
#endif
                    if (statement == null) { break; }
                    if (statement is Parser.Scope.ExitWhile) { break; }
#if NET6_0_OR_GREATER
                    Node? node = Parent.ReadNextNode(this, statement);
#else
                    Node node = Parent.ReadNextNode(this, statement);
#endif
                    if (node == null) { break; }
                    body.Add(node);
                }
                body.Add(_restartLabel);
                _body = body.ToArray();
            }
            internal void BuildUnscoped(AST Parent)
            {
#if NET6_0_OR_GREATER
                Runic.Statement? statement = Parent.ReadNextStatement();
                Node? body = Parent.ReadNextNode(this, statement);
#else
                Runic.Statement statement = Parent.ReadNextStatement();
                Node body = Parent.ReadNextNode(this, statement);
#endif
                if (body == null) { _body = new Node[1] { _restartLabel }; }
                else { _body = new Node[2] { body, _restartLabel }; }

            }
        }
    }
}
