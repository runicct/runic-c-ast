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
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Runic.C
{
    public partial class AST : Runic.AST.INodeStream
    {
        Runic.AST.Node.Expression.Constant BuildCharConstant(AST parent, Runic.AST.Type? hint, Parser.Expression.Constant constant)
        {
            Runic.AST.Type.CharEncoding encoding = Runic.AST.Type.CharEncoding.UTF8;
            string literal = LiteralParser.ParseLiteralChar(parent.WCharEncoding, constant.Tokens, out encoding);
            if (hint == null) { hint = new Runic.AST.Type.Char(encoding); }
            switch (hint)
            {
                case Runic.AST.Type.Char chr:
                    {
                        if (literal == "") 
                        {
                            switch (chr.Encoding)
                            {
                                case Runic.AST.Type.CharEncoding.UTF16: return new Runic.AST.Node.Expression.Constant.Char.UTF16('\0');
                                case Runic.AST.Type.CharEncoding.UTF32: return new Runic.AST.Node.Expression.Constant.Char.UTF32('\0');
                                default: return new Runic.AST.Node.Expression.Constant.Char.UTF8('\0');
                            }
                        }
                        if (chr.Encoding == encoding)
                        {
                            switch (encoding)
                            {
                                case Runic.AST.Type.CharEncoding.UTF16: return new Runic.AST.Node.Expression.Constant.Char.UTF16(literal[literal.Length - 1]);
                                case Runic.AST.Type.CharEncoding.UTF32: return new Runic.AST.Node.Expression.Constant.Char.UTF32(literal[literal.Length - 1]);
                                default: return new Runic.AST.Node.Expression.Constant.Char.UTF8(literal[literal.Length - 1]);
                            }
                        }
                    }
                    break;
            }
            byte[] data;
            switch (encoding)
            {
                case Runic.AST.Type.CharEncoding.UTF16: data = System.Text.Encoding.Unicode.GetBytes(literal); break;
                case Runic.AST.Type.CharEncoding.UTF32: data = System.Text.Encoding.UTF32.GetBytes(literal); break;
                default: data = System.Text.Encoding.UTF8.GetBytes(literal); break;
            }

            return null;
        }

        Runic.AST.Node.Expression.Constant BuildStringConstant(AST parent, Parser.Expression.Constant constant)
        {
            Runic.AST.Type.CharEncoding encoding = Runic.AST.Type.CharEncoding.UTF8;
            string literal = LiteralParser.ParseLiteralString(parent.WCharEncoding, constant.Tokens, out encoding);
            switch (encoding)
            {
                case Runic.AST.Type.CharEncoding.UTF16:
                    return new Runic.AST.Node.Expression.Constant.String.UTF16(literal);
                case Runic.AST.Type.CharEncoding.UTF32:
                    return new Runic.AST.Node.Expression.Constant.String.UTF32(literal);
                default:
                    return new Runic.AST.Node.Expression.Constant.String.UTF8(literal);
            }
        }
        Runic.AST.Node.Expression.Constant BuildConstant(AST parent, Runic.AST.Type type, Parser.Expression.Constant constant)
        {
            if (constant.Tokens == null || constant.Tokens.Length == 0)
            {
                return new Runic.AST.Node.Expression.Constant.Null();
            }
            string? firstTokenValue = constant.Tokens[0].Value;
            if (firstTokenValue != null && firstTokenValue.Length > 0)
            {
                if (firstTokenValue[0] == '\'') { return BuildCharConstant(parent, type, constant); }
                if (firstTokenValue[0] == '\"') { return BuildStringConstant(parent, constant); }
                if (firstTokenValue[0] == 'L')
                {
                    if (firstTokenValue.Length > 1)
                    {
                        if (firstTokenValue[1] == '\'') { return BuildCharConstant(parent, type, constant); }
                        if (firstTokenValue[1] == '\"') { return BuildStringConstant(parent, constant); }
                    }
                }
            }

            Token token = constant.Tokens[0];

            bool unsigned, isLong, isLongLong, isFloat, isDouble, isLongDouble, negative, exponantNegative;
            System.Numerics.BigInteger integralPart, decimalPart, decimalPartFraction, exponantPart;
            LiteralParser.ParseNumerical(constant.Tokens[0], out _, out _, out _, out unsigned, out isLong, out isLongLong, out isFloat, out isDouble, out isLongDouble, out negative, out integralPart, out decimalPart, out decimalPartFraction, out exponantPart, out exponantNegative);
            if (unsigned)
            {
                if (isLong) { return new Runic.AST.Node.Expression.Constant.I32(token.StartLine, token.StartColumn, token.EndLine, token.EndColumn, token.File, (int)(integralPart)); }
                else if (isLongLong) { return new Runic.AST.Node.Expression.Constant.I64(token.StartLine, token.StartColumn, token.EndLine, token.EndColumn, token.File, (long)(integralPart)); }
                else { return new Runic.AST.Node.Expression.Constant.I32(token.StartLine, token.StartColumn, token.EndLine, token.EndColumn, token.File, (int)(integralPart)); }
            }
            else
            {
                if (isFloat || isDouble || isLongDouble)
                {
                    double result = (double)integralPart + ((double)decimalPart / (double)decimalPartFraction);
                    if (negative) { result = -result; }
                    if (exponantPart != 0)
                    {
                        if (exponantNegative) { result *= -System.Math.Pow(10.0, (double)exponantPart); }
                        else { result *= System.Math.Pow(10.0, (double)exponantPart); }
                    }
                    if (isFloat) { return new Runic.AST.Node.Expression.Constant.F32(token.StartLine, token.StartColumn, token.EndLine, token.EndColumn, token.File, (float)result); }
                    else if (isDouble) { return new Runic.AST.Node.Expression.Constant.F64(token.StartLine, token.StartColumn, token.EndLine, token.EndColumn, token.File, result); }
                    else { return new Runic.AST.Node.Expression.Constant.F64(token.StartLine, token.StartColumn, token.EndLine, token.EndColumn, token.File, result); }
                }
                else
                {
                    if (isLong) { return new Runic.AST.Node.Expression.Constant.I32(token.StartLine, token.StartColumn, token.EndLine, token.EndColumn, token.File, (int)(integralPart)); }
                    else if (isLongLong) { return new Runic.AST.Node.Expression.Constant.I64(token.StartLine, token.StartColumn, token.EndLine, token.EndColumn, token.File, (long)(integralPart)); }
                    else { return new Runic.AST.Node.Expression.Constant.I32(token.StartLine, token.StartColumn, token.EndLine, token.EndColumn, token.File, (int)(integralPart)); }
                }
            }
        }
        Runic.AST.Node.Expression BuildCompoundLiterals(ICScope parent, Parser.Expression.CompoundLiteralsStruct compoundLiterals)
        {
            Dictionary<Runic.AST.Type.StructOrUnion.Field, Runic.AST.Node.Expression> translatedInit = new Dictionary<Runic.AST.Type.StructOrUnion.Field, Runic.AST.Node.Expression>();
            Runic.AST.Type.StructOrUnion structType = GetType(compoundLiterals.StructType) as Runic.AST.Type.StructOrUnion;
            foreach (KeyValuePair<Runic.C.Parser.Field, Runic.C.Parser.Expression> init in compoundLiterals.Values)
            {
                Runic.AST.Type.StructOrUnion.Field field = structType.Fields[init.Key.Index];
                Runic.AST.Node.Expression value = BuildExpression(this, parent, field.Type, init.Value);
                translatedInit.Add(field, value);
            }
            return new Runic.AST.Node.Expression.StructInitializer(structType, translatedInit);
        }
        Runic.AST.Node.Expression BuildArrayInitializer(ICScope parent, Parser.Expression.ArrayInitializer arrayInitializer)
        {
            Runic.AST.Type.Pointer type = GetType(arrayInitializer.ArrayType) as Runic.AST.Type.Pointer;
            Runic.AST.Type elementType = type.TargetType;
            Runic.AST.Node.Expression[] values = new Runic.AST.Node.Expression[arrayInitializer.Values.Length];
            for (int i = 0; i < arrayInitializer.Values.Length; i++)
            {
                values[i] = BuildExpression(this, parent, elementType, arrayInitializer.Values[i]);
            }
            return new Runic.AST.Node.Expression.ArrayInitializer(type, values);
        }
    }
}