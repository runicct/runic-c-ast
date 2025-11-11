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
using static Runic.C.Parser.Scope;

namespace Runic.C
{
    public partial class AST : Runic.AST.INodeStream
    {
        internal class Switch : Runic.AST.Node.Scope, ICScope
        {
            internal class IntermediateCase : Runic.AST.Node
            {
                Expression _value;
                public Expression Value { get { return _value; } }
                public IntermediateCase(AST parent, ICScope parentScope, C.Parser.Case @case) : base(@case.Keyword.StartColumn, @case.Keyword.StartLine, @case.Keyword.EndColumn, @case.Keyword.EndLine, @case.Keyword.File)
                {
                    _value = parent.BuildExpression(parent, parentScope, null, @case.Constant);
                }
            }
            internal class IntermediateDefault : Runic.AST.Node
            {
                public IntermediateDefault(AST parent, ICScope parentScope, C.Parser.Default @default) : base(@default.Keyword.StartColumn, @default.Keyword.StartLine, @default.Keyword.EndColumn, @default.Keyword.EndLine, @default.Keyword.File)
                {
                }
            }
            internal class Case : Runic.AST.Node, ICScope
            {
                AST.Switch _parent;
                public AST.Switch ParentSwitch { get { return _parent; } }
                Runic.AST.Node.Switch.Case _case;
                public Runic.AST.Node.Switch.Case CaseNode { get { return _case; } }
                Runic.AST.Node.Label _endLabel;
                public Runic.AST.Node.Label EndLabel { get { return _endLabel; } }
                Node[] _body;
                public Runic.AST.Node[] Body { get { return _body; } }
                public Runic.AST.Node GetBreakOrContinueNode() { return this; }
                public AST.Function GetFunction(Parser.Function function) { return _parent.GetFunction(function); }
                public Runic.AST.Node.Label GetLabel(Parser.Label label) { return _parent.GetLabel(label); }
                public Runic.AST.Variable GetVariable(Parser.Variable variable) { return _parent.GetVariable(variable); }
                public AST.Function GetParentFunction() { return _parent.GetParentFunction(); }
                public Case(AST.Switch parent, Expression value)
                {
                    _parent = parent;
                    _case = new Runic.AST.Node.Switch.Case(value, new Label(parent.GetParentFunction()));
                    _endLabel = new Runic.AST.Node.Label(parent.GetParentFunction());
                }
                public Runic.AST.Node? Build(AST parent, out bool exitSwitch)
                {
                    exitSwitch = false;
                    List<Node> body = new List<Node>();
                    body.Add(_case.Label);
                    while (true)
                    {
                        Runic.Statement? statement = parent.ReadNextStatement();
                        if (statement == null) { break; }
                        if (statement is Parser.Scope.ExitSwitch)
                        {
                            exitSwitch = true;
                            break;
                        }
                        Node? node = parent.ReadNextNode(this, statement);
                        if (node == null) { break; }

                        switch (node)
                        {
                            case IntermediateCase _:
                            case IntermediateDefault _:
                                body.Add(_endLabel);
                                _body = body.ToArray();
                                return node;
                        }
                        body.Add(node);
                    }
                    body.Add(_endLabel);
                    _body = body.ToArray();
                    return null;
                }
            }

            internal class Default : Runic.AST.Node, ICScope
            {
                AST.Switch _parent;
                public AST.Switch ParentSwitch { get { return _parent; } }
                Runic.AST.Node.Label _defaultLabel;
                public Runic.AST.Node.Label DefaultLabel { get { return _defaultLabel; } }
                Runic.AST.Node.Label _endLabel;
                public Runic.AST.Node.Label EndLabel { get { return _endLabel; } }
                Node[] _body;
                public Runic.AST.Node[] Body { get { return _body; } }
                public Runic.AST.Node GetBreakOrContinueNode() { return this; }
                public AST.Function GetFunction(Parser.Function function) { return _parent.GetFunction(function); }
                public Runic.AST.Node.Label GetLabel(Parser.Label label) { return _parent.GetLabel(label); }
                public Runic.AST.Variable GetVariable(Parser.Variable variable) { return _parent.GetVariable(variable); }
                public AST.Function GetParentFunction() { return _parent.GetParentFunction(); }
                public Default(AST.Switch parent)
                {
                    _parent = parent;
                    _defaultLabel = new Runic.AST.Node.Label(parent.GetParentFunction());
                    _endLabel = new Runic.AST.Node.Label(parent.GetParentFunction());
                }
                public Runic.AST.Node? Build(AST parent, out bool exitSwitch)
                {
                    exitSwitch = false;
                    List<Node> body = new List<Node>();
                    body.Add(_defaultLabel);
                    while (true)
                    {
                        Runic.Statement? statement = parent.ReadNextStatement();
                        if (statement == null) { break; }
                        if (statement is Parser.Scope.ExitSwitch)
                        {
                            exitSwitch = true;
                            break; 
                        }
                        Node? node = parent.ReadNextNode(this, statement);
                        if (node == null) { break; }
                        switch (node)
                        {
                            case IntermediateCase _:
                            case IntermediateDefault _:
                                body.Add(_endLabel);
                                _body = body.ToArray();
                                return node;
                        }
                        body.Add(node);
                    }
                    body.Add(_endLabel);
                    _body = body.ToArray();
                    return null;
                }
            }

            ICScope _parentScope;
            Runic.AST.Node.Expression _value;
            Runic.AST.Node[]? _body = null;
            public Runic.AST.Node[]? Body { get { return _body; } }
            Label _default;
            Label _endLabel;
            public Label EndLabel { get { return _endLabel; } }
            Case[] _cases;
            public Runic.AST.Node GetBreakOrContinueNode() { return this; }
            public AST.Function GetFunction(Parser.Function function) { return _parentScope.GetFunction(function); }
            public Runic.AST.Node.Label GetLabel(Parser.Label label) { return _parentScope.GetLabel(label); }
            public Runic.AST.Variable GetVariable(Parser.Variable variable) { return _parentScope.GetVariable(variable); }
            public AST.Function GetParentFunction() { return _parentScope.GetParentFunction(); }
            public Switch(AST Parent, ICScope parentScope, Parser.Switch Statement)
            {
                _parentScope = parentScope;
                _default = new Runic.AST.Node.Label(parentScope.GetParentFunction());
                _endLabel = new Runic.AST.Node.Label(parentScope.GetParentFunction());
                _value = Parent.BuildExpression(Parent, parentScope, null, Statement.Value);
            }
            public void Build(AST parent)
            {
                List<Runic.AST.Node> body = new List<Runic.AST.Node>();
                List<Runic.AST.Node> defaultBody = new List<Runic.AST.Node>();
                List<Runic.AST.Node> result = new List<Runic.AST.Node>();

                List<Runic.AST.Node.Switch.Case> cases = new List<Runic.AST.Node.Switch.Case>();
                Runic.AST.Node? nextNode = null;
                C.AST.Switch.Default? @default = null;
                while (true)
                {
                    Runic.Statement? statement = parent.ReadNextStatement();
                    if (statement == null) { break; }
                    if (statement is Parser.Scope.ExitSwitch) { break; }
                    Node? node = parent.ReadNextNode(this, statement);
                    if (node == null) { break; }
                    bool exitSwitch = false;
                    while (node != null)
                    {
                        switch (node)
                        {
                            case IntermediateCase intermediateCase:
                                C.AST.Switch.Case currentCase = new C.AST.Switch.Case(this, intermediateCase.Value);
                                cases.Add(currentCase.CaseNode);
                                node = currentCase.Build(parent, out exitSwitch);
                                body.AddRange(currentCase.Body);
                                break;
                            case IntermediateDefault intermediateDefault:
                                @default = new C.AST.Switch.Default(this);
                                node = @default.Build(parent, out exitSwitch);
                                body.AddRange(@default.Body);
                                break;
                            default: node = null; break;
                        }

                    }
                    if (exitSwitch) { break; }
                }

                result.Add(new Runic.AST.Node.Switch(_value, cases.ToArray()));
                if (@default != null) { result.Add(new Runic.AST.Node.Branch(@default.DefaultLabel)); }
                else { result.Add(new Runic.AST.Node.Branch(_endLabel)); }
                result.AddRange(body);
                result.Add(_endLabel);

                _body = result.ToArray();
            }
        }
    }
}
