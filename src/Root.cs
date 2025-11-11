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
        internal class Root : Runic.AST.Node.Scope, ICScope
        {
            Dictionary<string, AST.Function> _functions = new Dictionary<string, AST.Function>();
            public bool TryGetFunction(string name, out AST.Function function) { return _functions.TryGetValue(name, out function); }
            public void AddFunction(string name, AST.Function function) { _functions.Add(name, function); }
            public bool ContainsFunction(string name) { return _functions.ContainsKey(name); }
            Dictionary<string, Runic.AST.Variable.GlobalVariable> _globalVariable = new Dictionary<string, Runic.AST.Variable.GlobalVariable>();
            public Runic.AST.Node GetBreakOrContinueNode() { return new Node.Empty(); }
            public AST.Function GetFunction(Parser.Function func)
            {
#if NET6_0_OR_GREATER
                AST.Function? function;
#else
                AST.Function function;
#endif
                if (func.Name.Value == null) { return null; }
                if (!_functions.TryGetValue(func.Name.Value, out function))
                {
                    throw new Exception("Function '" + func.Name.Value + "' not found.");
                }
                return function;
            }
            uint _nextGlobalVariableIndex = 0;
            Dictionary<ulong, Runic.AST.Variable.GlobalVariable> _globalVariables = new Dictionary<ulong, Runic.AST.Variable.GlobalVariable>();
#if NET6_0_OR_GREATER
            public Runic.AST.Variable? GetVariable(Parser.Variable variable)
#else
            public Runic.AST.Variable GetVariable(Parser.Variable variable)
#endif
            {
                switch (variable)
                {
                    case Parser.GlobalVariable global:
                        {
                            Runic.AST.Variable.GlobalVariable globalVariable;

                            if (_globalVariables.TryGetValue(global.Index, out globalVariable))
                            {

                                globalVariable = new Runic.AST.Variable.GlobalVariable(_nextGlobalVariableIndex, _parent.GetType(global.VariableType));
                                _nextGlobalVariableIndex++;
                            }
                            return globalVariable;
                        }
                }
                return null;
            }
#if NET6_0_OR_GREATER
            public Runic.AST.Node.Label? GetLabel(Parser.Label label)
#else
            public Runic.AST.Node.Label GetLabel(Parser.Label label)
#endif
            {
                return null;
            }
            public AST.Function GetParentFunction() { return null; }
            AST _parent;
            public Root(AST parent)
            {
                _parent = parent;
            }
        }
    }
}
