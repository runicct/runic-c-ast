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
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Runic.AST.Node.Expression;
using static Runic.C.Parser;
using static Runic.C.Parser.Expression;
using static Runic.C.Parser.Expression.SizeOf;

namespace Runic.C
{
    public partial class AST : Runic.AST.INodeStream
    {
        enum BinaryOperator
        {
            Add,
            Subtract,
            Multiply,
            Divide,
            And,
            Or,
            Xor,
            LogicalAnd,
            LogicalOr,
            LowerThan,
            GreaterThan,
            LowerOrEqual,
            GreaterOrEqual,
            Equal,
            NotEqual
        }
#if NET6_0_OR_GREATER
        Runic.AST.Node.Expression BuildOperator(BinaryOperator operatorType, AST ast, ICScope parent, Runic.AST.Type? type, Parser.Expression left, Token op, Parser.Expression right)
#else
        Runic.AST.Node.Expression BuildOperator(BinaryOperator operatorType, AST ast, ICScope parent, Runic.AST.Type type, Parser.Expression left, Token op, Parser.Expression right)
#endif
        {
            Runic.AST.Node.Expression leftExpr = BuildExpression(ast, parent, type, left);
            Runic.AST.Node.Expression rightExpr = BuildExpression(ast, parent, leftExpr.Type, right);
            if (leftExpr.Type != rightExpr.Type)
            {
                switch (leftExpr.Type)
                {
                    case Runic.AST.Type.Integer leftInt:
                        switch (rightExpr.Type)
                        {
                            case Runic.AST.Type.Integer rightInt:
                                if (leftInt.Bits < rightInt.Bits) { leftExpr = new Runic.AST.Node.Expression.Cast(leftExpr, rightInt); }
                                else if (rightInt.Bits < leftInt.Bits) { rightExpr = new Runic.AST.Node.Expression.Cast(rightExpr, leftInt); }
                                break;
                            case Runic.AST.Type.FloatingPoint rightFlt:
                                leftExpr = new Runic.AST.Node.Expression.Cast(leftExpr, rightFlt);
                                break;
                            case Runic.AST.Type.Pointer rightPtr:
                                leftExpr = new Runic.AST.Node.Expression.Cast(leftExpr, new Runic.AST.Type.NativeInteger(leftInt.Signed));
                                break;
                        }
                        break;
                    case Runic.AST.Type.FloatingPoint leftFlt:
                        switch (rightExpr.Type)
                        {
                            case Runic.AST.Type.Pointer rightPtr:
                                leftExpr = new Runic.AST.Node.Expression.Cast(leftExpr, new Runic.AST.Type.NativeInteger(true));
                                break;
                            default:
                                rightExpr = new Runic.AST.Node.Expression.Cast(rightExpr, leftFlt);
                                break;
                        }
                        break;
                    case Runic.AST.Type.Pointer leftPtr:
                        {
                            switch (rightExpr.Type)
                            {
                                case Runic.AST.Type.Integer rightInt:
                                    rightExpr = new Runic.AST.Node.Expression.Cast(rightExpr, new Runic.AST.Type.NativeInteger(rightInt.Signed));
                                    break;
                                case Runic.AST.Type.FloatingPoint rightFlt:
                                    rightExpr = new Runic.AST.Node.Expression.Cast(rightExpr, new Runic.AST.Type.NativeInteger(true));
                                    break;
                            }
                        }
                        break;
                }
            }
            switch (operatorType)
            {
                case BinaryOperator.Add: return new Runic.AST.Node.Expression.Add(op.StartLine, op.StartColumn, op.EndLine, op.EndColumn, op.File, leftExpr, rightExpr);
                case BinaryOperator.Subtract: return new Runic.AST.Node.Expression.Sub(op.StartLine, op.StartColumn, op.EndLine, op.EndColumn, op.File, leftExpr, rightExpr);
                case BinaryOperator.Multiply: return new Runic.AST.Node.Expression.Mul(op.StartLine, op.StartColumn, op.EndLine, op.EndColumn, op.File, leftExpr, rightExpr);
                case BinaryOperator.Divide: return new Runic.AST.Node.Expression.Div(op.StartLine, op.StartColumn, op.EndLine, op.EndColumn, op.File, leftExpr, rightExpr);
                case BinaryOperator.And: return new Runic.AST.Node.Expression.And(op.StartLine, op.StartColumn, op.EndLine, op.EndColumn, op.File, leftExpr, rightExpr);
                case BinaryOperator.Or: return new Runic.AST.Node.Expression.Or(op.StartLine, op.StartColumn, op.EndLine, op.EndColumn, op.File, leftExpr, rightExpr);
                case BinaryOperator.Xor: return new Runic.AST.Node.Expression.Xor(op.StartLine, op.StartColumn, op.EndLine, op.EndColumn, op.File, leftExpr, rightExpr);
                case BinaryOperator.LogicalAnd: return new Runic.AST.Node.Expression.LogicalAnd(op.StartLine, op.StartColumn, op.EndLine, op.EndColumn, op.File, leftExpr, rightExpr);
                case BinaryOperator.LogicalOr: return new Runic.AST.Node.Expression.LogicalOr(op.StartLine, op.StartColumn, op.EndLine, op.EndColumn, op.File, leftExpr, rightExpr);
                case BinaryOperator.LowerThan: return new Runic.AST.Node.Expression.Comparison(op.StartLine, op.StartColumn, op.EndLine, op.EndColumn, op.File, Runic.AST.Node.Expression.Comparison.ComparisonOperation.LowerThan, leftExpr, rightExpr);
                case BinaryOperator.GreaterThan: return new Runic.AST.Node.Expression.Comparison(op.StartLine, op.StartColumn, op.EndLine, op.EndColumn, op.File, Runic.AST.Node.Expression.Comparison.ComparisonOperation.GreaterThan, leftExpr, rightExpr);
                case BinaryOperator.LowerOrEqual: return new Runic.AST.Node.Expression.Comparison(op.StartLine, op.StartColumn, op.EndLine, op.EndColumn, op.File, Runic.AST.Node.Expression.Comparison.ComparisonOperation.LowerOrEqual, leftExpr, rightExpr);
                case BinaryOperator.GreaterOrEqual: return new Runic.AST.Node.Expression.Comparison(op.StartLine, op.StartColumn, op.EndLine, op.EndColumn, op.File, Runic.AST.Node.Expression.Comparison.ComparisonOperation.GreaterOrEqual, leftExpr, rightExpr);
                case BinaryOperator.Equal: return new Runic.AST.Node.Expression.Comparison(op.StartLine, op.StartColumn, op.EndLine, op.EndColumn, op.File, Runic.AST.Node.Expression.Comparison.ComparisonOperation.Equal, leftExpr, rightExpr);
                case BinaryOperator.NotEqual: return new Runic.AST.Node.Expression.Comparison(op.StartLine, op.StartColumn, op.EndLine, op.EndColumn, op.File, Runic.AST.Node.Expression.Comparison.ComparisonOperation.NotEqual, leftExpr, rightExpr);
            }
            throw new Exception("Invalid binary operator " + operatorType.ToString());
        }
#if NET6_0_OR_GREATER
        Runic.AST.Node.Expression BuildExpression(AST ast, ICScope parent, Runic.AST.Type? type, Parser.Expression expression)
#else
        Runic.AST.Node.Expression BuildExpression(AST ast, ICScope parent, Runic.AST.Type type, Parser.Expression expression)
#endif
        {
            if (expression == null) { return null; }
            switch (expression)
            {
                case Parser.Expression.Constant constant: return BuildConstant(ast, null, constant);
                case Parser.MemberUse memberUse:
                    {
                        Runic.AST.Type.StructOrUnion.Field[] fields = new Runic.AST.Type.StructOrUnion.Field[memberUse.Fields.Length];
                        Runic.AST.Variable variable = parent.GetVariable(memberUse.Variable);
#if NET6_0_OR_GREATER
                        Runic.AST.Type.StructOrUnion? previousType = variable.Type as Runic.AST.Type.StructOrUnion;
                        Runic.AST.Type.Pointer? pointerType;
#else
                        Runic.AST.Type.StructOrUnion previousType = variable.Type as Runic.AST.Type.StructOrUnion;
                        Runic.AST.Type.Pointer pointerType;
#endif
                        if (previousType == null)
                        {
                            // This means we are accessing a member via a pointer dereference (-> operator)
                           pointerType = variable.Type as Runic.AST.Type.Pointer;
                            if (pointerType != null) { previousType = pointerType.TargetType as Runic.AST.Type.StructOrUnion; }
                        }

                        for (int n = 0; n < memberUse.Fields.Length; n++)
                        {
                            if (previousType == null)
                            {
                                fields[n] = null;
                            }
                            else
                            {
                                fields[n] = previousType.Fields[memberUse.Fields[n].Index];
                                previousType = fields[n].Type as Runic.AST.Type.StructOrUnion;
                                if (previousType == null)
                                {
                                    // This means we are accessing a member via a pointer dereference (-> operator)
                                    pointerType = variable.Type as Runic.AST.Type.Pointer;
                                    if (pointerType != null) { previousType = pointerType.TargetType as Runic.AST.Type.StructOrUnion; }
                                }
                            }
                        }
                        return new Runic.AST.Node.Expression.MemberUse(variable, fields);
                    }
                case Parser.VariableUse variableUse:
                    switch (variableUse.Variable)
                    {
                        case Parser.Expression.Nullptr nullptr: return new Runic.AST.Node.Expression.Constant.Null();
                        default: return new Runic.AST.Node.Expression.VariableUse(parent.GetVariable(variableUse.Variable));
                    }
                case Parser.Expression.Assignment.MemberAssignment assignment:
                    {
                        Runic.AST.Type.StructOrUnion.Field[] fields = new Runic.AST.Type.StructOrUnion.Field[assignment.Fields.Length];
                        Runic.AST.Variable target = parent.GetVariable(assignment.Target);
#if NET6_0_OR_GREATER
                        Runic.AST.Type.StructOrUnion? previousType = target.Type as Runic.AST.Type.StructOrUnion;
#else
                        Runic.AST.Type.StructOrUnion previousType = target.Type as Runic.AST.Type.StructOrUnion;
#endif
                        for (int n = 0; n < assignment.Fields.Length; n++)
                        {
                            if (previousType == null)
                            {
                                fields[n] = null;
                            }
                            else
                            {
                                fields[n] = previousType.Fields[assignment.Fields[n].Index];
                                previousType = fields[n].Type as Runic.AST.Type.StructOrUnion;
                            }
                        }
                        return new Runic.AST.Node.Expression.MemberUse(assignment.Operator.StartLine, assignment.Operator.StartColumn, assignment.Operator.EndLine, assignment.Operator.EndColumn, assignment.Operator.File, target, fields);
                    }
                case Parser.Expression.Assignment.IndexingAssignment assignment:
                    {
                        Runic.AST.Node.Expression target = BuildExpression(ast, parent, null, assignment.Target);
                        return new Runic.AST.Node.Expression.IndexingAssignment(assignment.Operator.StartLine, assignment.Operator.StartColumn, assignment.Operator.EndLine, assignment.Operator.EndColumn, assignment.Operator.File, target, BuildExpression(ast, parent, null, assignment.Index), BuildExpression(ast, parent, target.Type, assignment.Value));
                    }
                case Parser.Expression.Assignment.VariableAssignment assignment:
                    {
                        Runic.AST.Variable target = parent.GetVariable(assignment.Target);
                        return new Runic.AST.Node.Expression.VariableAssignment(assignment.Operator.StartLine, assignment.Operator.StartColumn, assignment.Operator.EndLine, assignment.Operator.EndColumn, assignment.Operator.File, target, BuildExpression(ast, parent, target.Type, assignment.Value));
                    }
                case Parser.Expression.Assignment.DereferenceAssignment derefAssignment:
                    {
                        Runic.AST.Node.Expression target = BuildExpression(ast, parent, null, derefAssignment.Target);
                        Runic.AST.Node.Expression value = BuildExpression(ast, parent, target.Type, derefAssignment.Value);
                        return new Runic.AST.Node.Expression.DereferenceAssignment(derefAssignment.Operator.StartLine, derefAssignment.Operator.StartColumn, derefAssignment.Operator.EndLine, derefAssignment.Operator.EndColumn, derefAssignment.Operator.File, target, value);
                    }
                case Parser.Expression.Cast Cast: return new Runic.AST.Node.Expression.Cast(BuildExpression(ast, parent, null, Cast.Value), GetType(Cast.TargetType));
                case Parser.Expression.Call call:
                    {
                        Runic.AST.Node.Expression[] parameters = new Runic.AST.Node.Expression[call.Parameters.Length];
                        for (int n = 0; n < call.Parameters.Length; n++)
                        {
                            parameters[n] = BuildExpression(ast, parent, ast.GetType(call.Function.FunctionParameters[n].VariableType), call.Parameters[n]);
                        }
                        Runic.AST.Node.Function function = parent.GetFunction(call.Function);
                        return new Runic.AST.Node.Expression.Call(function, parameters);
                    }
                case Parser.Expression.IndirectCall indirectCall:
                    {
                        Runic.AST.Node.Expression function = BuildExpression(ast, parent, type, indirectCall.Function);
#if NET6_0_OR_GREATER
                        Runic.AST.Type.FunctionPointer? functionPointer = function.Type as Runic.AST.Type.FunctionPointer;
#else
                        Runic.AST.Type.FunctionPointer functionPointer = function.Type as Runic.AST.Type.FunctionPointer;
#endif
                        Runic.AST.Node.Expression[] parameters = new Runic.AST.Node.Expression[indirectCall.Parameters.Length];
                        for (int n = 0; n < indirectCall.Parameters.Length; n++)
                        {
                            parameters[n] = BuildExpression(ast, parent, null, indirectCall.Parameters[n]);
                        }
                        return new Runic.AST.Node.Expression.IndirectCall(function, parameters);
                    }
                case Parser.Expression.Not not: return new Runic.AST.Node.Expression.Not(not.Operator.StartLine, not.Operator.StartColumn, not.Operator.EndLine, not.Operator.EndColumn, not.Operator.File, BuildExpression(ast, parent, type, not.Value));
                case Parser.Expression.Add add: return BuildOperator(BinaryOperator.Add, ast, parent, type, add.Left, add.Operator, add.Right);
                case Parser.Expression.Sub sub: return BuildOperator(BinaryOperator.Subtract, ast, parent, type, sub.Left, sub.Operator, sub.Right);
                case Parser.Expression.Mul mul: return BuildOperator(BinaryOperator.Multiply, ast, parent, type, mul.Left, mul.Operator, mul.Right);
                case Parser.Expression.Div div: return BuildOperator(BinaryOperator.Divide, ast, parent, type, div.Left, div.Operator, div.Right);
                case Parser.Expression.And and: return BuildOperator(BinaryOperator.And, ast, parent, type, and.Left, and.Operator, and.Right);
                case Parser.Expression.Or or: return BuildOperator(BinaryOperator.Or, ast, parent, type, or.Left, or.Operator, or.Right);
                case Parser.Expression.Xor xor: return BuildOperator(BinaryOperator.Xor, ast, parent, type, xor.Left, xor.Operator, xor.Right);
                case Parser.Expression.LogicalAnd land: return BuildOperator(BinaryOperator.LogicalAnd, ast, parent, type, land.Left, land.Operator, land.Right);
                case Parser.Expression.LogicalOr lor: return BuildOperator(BinaryOperator.LogicalOr, ast, parent, type, lor.Left, lor.Operator, lor.Right);
                case Parser.Expression.Increment.Postfix.Variable postincr: return new Runic.AST.Node.Expression.Increment.Postfix.Variable(postincr.Operator.StartLine, postincr.Operator.StartColumn, postincr.Operator.EndLine, postincr.Operator.EndColumn, postincr.Operator.File, parent.GetVariable(postincr.Target));
                case Parser.Expression.Increment.Prefix.Variable preincr: return new Runic.AST.Node.Expression.Increment.Prefix.Variable(preincr.Operator.StartLine, preincr.Operator.StartColumn, preincr.Operator.EndLine, preincr.Operator.EndColumn, preincr.Operator.File, parent.GetVariable(preincr.Target));
                case Parser.Expression.Increment.Postfix.Dereference postincr: return new Runic.AST.Node.Expression.Increment.Postfix.Dereference(postincr.Operator.StartLine, postincr.Operator.StartColumn, postincr.Operator.EndLine, postincr.Operator.EndColumn, postincr.Operator.File, BuildExpression(ast, parent, type, postincr.Address));
                case Parser.Expression.Increment.Prefix.Dereference preincr: return new Runic.AST.Node.Expression.Increment.Prefix.Dereference(preincr.Operator.StartLine, preincr.Operator.StartColumn, preincr.Operator.EndLine, preincr.Operator.EndColumn, preincr.Operator.File, BuildExpression(ast, parent, type, preincr.Address));
                case Parser.Expression.Comparison cmp:
                    {
                        BinaryOperator op;
                        switch (cmp.Operation)
                        {
                            case Parser.Expression.Comparison.ComparisonOperation.LowerThan: op = BinaryOperator.LowerThan; break;
                            case Parser.Expression.Comparison.ComparisonOperation.GreaterThan: op = BinaryOperator.GreaterThan; break;
                            case Parser.Expression.Comparison.ComparisonOperation.LowerOrEqual: op = BinaryOperator.LowerOrEqual; break;
                            case Parser.Expression.Comparison.ComparisonOperation.GreaterOrEqual: op = BinaryOperator.GreaterOrEqual; break;
                            case Parser.Expression.Comparison.ComparisonOperation.Equal: op = BinaryOperator.Equal; break;
                            case Parser.Expression.Comparison.ComparisonOperation.NotEqual: op = BinaryOperator.NotEqual; break;
                            default: throw new Exception("Invalid comparison operation " + cmp.Operation.ToString());
                        }
                        return BuildOperator(op, ast, parent, type, cmp.Left, cmp.Operator, cmp.Right);
                    }
                case Parser.Expression.Dereference deref: return new Runic.AST.Node.Expression.Dereference(BuildExpression(ast, parent, type, deref.Address));
                case Parser.Expression.MemberReference memberReference:
                    {
                        Runic.AST.Variable variable = parent.GetVariable(memberReference.Variable);
                        Runic.AST.Type.StructOrUnion.Field[] fields = new Runic.AST.Type.StructOrUnion.Field[memberReference.Fields.Length];
#if NET6_0_OR_GREATER
                        Runic.AST.Type.StructOrUnion? previousType = variable.Type as Runic.AST.Type.StructOrUnion;
#else
                        Runic.AST.Type.StructOrUnion previousType = variable.Type as Runic.AST.Type.StructOrUnion;
#endif

                        for (int n = 0; n < memberReference.Fields.Length; n++)
                        {
                            if (previousType == null)
                            {
                                fields[n] = null;
                            }
                            else
                            {
                                fields[n] = previousType.Fields[memberReference.Fields[n].Index];
                                previousType = fields[n].Type as Runic.AST.Type.StructOrUnion;
                            }
                        }
                        return new Runic.AST.Node.Expression.MemberReference(memberReference.Operator.StartLine, memberReference.Operator.StartColumn, memberReference.Operator.EndLine, memberReference.Operator.EndColumn, memberReference.Operator.File, variable, fields);
                    }
                case Parser.Expression.IndexingReference indexingReference: return new Runic.AST.Node.Expression.IndexingReference(BuildExpression(ast, parent, null, indexingReference.Target), BuildExpression(ast, parent, null, indexingReference.Index));
                case Parser.Expression.VariableReference variableReference: return new Runic.AST.Node.Expression.VariableReference(parent.GetVariable(variableReference.Variable));
                case Parser.Expression.FunctionReference functionReference: return new Runic.AST.Node.Expression.FunctionReference(parent.GetFunction(functionReference.Function));
                case Parser.Expression.Indexing indexing: return new Runic.AST.Node.Expression.Indexing(BuildExpression(ast, parent, null, indexing.Target), BuildExpression(ast, parent, null, indexing.Index));
                case Parser.Expression.EnumMemberUse enumMemberUser: return BuildExpression(ast, parent, type, enumMemberUser.Member.Value);
                case Parser.Expression.CompoundLiteralsStruct compoundLiteralsStruct: return BuildCompoundLiterals(parent, compoundLiteralsStruct);
                case Parser.Expression.ArrayInitializer arrayInitializer: return BuildArrayInitializer(parent, arrayInitializer);
                case Parser.Expression.Comma comma: return new Runic.AST.Node.Expression.Sequence(comma.Operator.StartLine, comma.Operator.StartColumn, comma.Operator.EndLine, comma.Operator.EndColumn, comma.Operator.File, BuildExpression(ast, parent, null, comma.Left), BuildExpression(ast, parent, type, comma.Left));
                case Parser.Expression.SizeOf.SizeOfExpression sizeOfExpression: return new Runic.AST.Node.Expression.SizeOf(BuildExpression(ast, parent, null, sizeOfExpression).Type);
                case Parser.Expression.SizeOf.SizeOfType sizeOfType: return new Runic.AST.Node.Expression.SizeOf(GetType(sizeOfType.TargetType));
            }
             throw new Exception("Invalid object" );
        }
    }
}
