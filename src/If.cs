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
        internal class If : Runic.AST.Node.If, ICScope
        {
            ICScope _parentScope;
#if NET6_0_OR_GREATER
            Node[]? _body = null;
            public override Node[]? Body { get { return _body; } }
            Node[]? _elseBody = null;
            public override Node[]? ElseBody { get { return _elseBody; } }
#else
            Node[] _body = null;
            public override Node[] Body { get { return _body; } }
            Node[] _elseBody = null;
            public override Node[] ElseBody { get { return _elseBody; } }
#endif
            public Runic.AST.Node GetBreakOrContinueNode() { return _parentScope.GetBreakOrContinueNode(); }
            public AST.Function GetFunction(Parser.Function function) { return _parentScope.GetFunction(function); }
            public Runic.AST.Node.Label GetLabel(Parser.Label label) { return _parentScope.GetLabel(label); }
            public Runic.AST.Variable GetVariable(Parser.Variable variable) { return _parentScope.GetVariable(variable); }
            public AST.Function GetParentFunction() { return _parentScope.GetParentFunction(); }
            internal If(AST parent, ICScope parentScope, Parser.If @if) : base(parent.BuildExpression(parent, parentScope, null, @if.Condition))
            {
                _parentScope = parentScope;
            }
            internal If(AST parent, ICScope parentScope, Parser.UnscopedIf @if) : base(parent.BuildExpression(parent, parentScope, null, @if.Condition))
            {
                _parentScope = parentScope;
            }
            internal void Build(AST parent)
            {
                List<Node> body = new List<Node>();
                while (true)
                {
#if NET6_0_OR_GREATER
                    Runic.Statement? statement = parent.ReadNextStatement();
#else
                    Runic.Statement statement = parent.ReadNextStatement();
#endif
                    if (statement == null) { break; }
                    if (statement is Parser.Scope.ExitIf) { break; }
#if NET6_0_OR_GREATER
                    Node? node = parent.ReadNextNode(this, statement);
#else
                    Node node = parent.ReadNextNode(this, statement);
#endif
                    if (node == null) { break; }
                    body.Add(node);
                }
                _body = body.ToArray();
            }
            internal void BuildUnscoped(AST parent)
            {
#if NET6_0_OR_GREATER
                Runic.Statement? statement = parent.ReadNextStatement();
                Node? body = parent.ReadNextNode(this, statement);
#else
                Runic.Statement statement = parent.ReadNextStatement();
                Node body = parent.ReadNextNode(this, statement);
#endif
                if (body == null) { _body = new Node[0]; }
                else { _body = new Node[1] { body }; }
            }

            internal void BuildElse(AST parent)
            {
                List<Node> body = new List<Node>();
                while (true)
                {
#if NET6_0_OR_GREATER
                    Runic.Statement? statement = parent.ReadNextStatement();
#else
                    Runic.Statement statement = parent.ReadNextStatement();
#endif
                    if (statement == null) { break; }
                    if (statement is Parser.Scope.ExitElse) { break; }
#if NET6_0_OR_GREATER
                    Node? node = parent.ReadNextNode(this, statement);
#else
                    Node node = parent.ReadNextNode(this, statement);
#endif
                    if (node == null) { break; }
                    body.Add(node);
                }
                _elseBody = body.ToArray();
            }
            internal void BuildElseUnscoped(AST parent)
            {
#if NET6_0_OR_GREATER
                Runic.Statement? statement = parent.ReadNextStatement();
                Node? body = parent.ReadNextNode(this, statement);
#else
                Runic.Statement statement = parent.ReadNextStatement();
                Node body = parent.ReadNextNode(this, statement);
#endif
                if (body == null) { _elseBody = new Node[0]; }
                else { _elseBody = new Node[1] { body }; }
            }
        }
    }
}
