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
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Runic.C
{
    public partial class AST : Runic.AST.INodeStream
    {
        internal static class LiteralParser
        {
            static bool IsValidHexNumber(char chr)
            {
                if (chr >= '0' && chr <= '9') { return true; }
                if (chr >= 'a' && chr <= 'f') { return true; }
                if (chr >= 'A' && chr <= 'F') { return true; }
                return false;
            }
            static int ValueFromHex(char chr)
            {
                if (chr >= '0' && chr <= '9') { return chr - '0'; }
                if (chr >= 'a' && chr <= 'f') { return chr - 'a' + 10; }
                if (chr >= 'A' && chr <= 'F') { return chr - 'A' + 10; }
                return 0;
            }
            static void AppendString(Runic.AST.Type.CharEncoding wcharEncoding, string str, StringBuilder result, out Runic.AST.Type.CharEncoding preferredEncoding)
            {
                Runic.AST.Type.CharEncoding charEncoding = Runic.AST.Type.CharEncoding.UTF8;
                preferredEncoding = Runic.AST.Type.CharEncoding.UTF8;
                if (str.Length == 0) { return; }
                int index = 0;
                if (str[0] == 'L') { charEncoding = wcharEncoding; preferredEncoding = Runic.AST.Type.CharEncoding.UTF32; index++; }
                if (index >= str.Length) { return; }
                if (str[index] != '\"') { return; }
                index++;
                bool escape = false;
                for (; index < str.Length; index++)
                {
                    if (escape)
                    {
                        switch (str[index])
                        {
                            case '"': result.Append('"'); break;
                            case '\'': result.Append('\''); break;
                            case 'a': result.Append('\a'); break;
                            case 'n': result.Append('\n'); break;
                            case 'r': result.Append('\r'); break;
                            case 't': result.Append('\t'); break;
                            case 'v': result.Append('\v'); break;
                            case '\\': result.Append('\\'); break;
                            case '?': result.Append('?'); break;
                            case 'x':
                                {
                                    index++;
                                    int value = 0;
                                    for (; index < str.Length; index++)
                                    {
                                        if (!IsValidHexNumber(str[index])) { break; }
                                        value = value * 16 + ValueFromHex(str[index]);
                                    }
                                    if (charEncoding == Runic.AST.Type.CharEncoding.UTF8) { if (value > 0xFFF) { value = value & 0xFF; } result.Append(char.ConvertFromUtf32(value)); }
                                    else if (charEncoding == Runic.AST.Type.CharEncoding.UTF16) { if (value > 0xFFFF) { value = value & 0xFFFF; } result.Append(char.ConvertFromUtf32(value)); }
                                    else if (charEncoding == Runic.AST.Type.CharEncoding.UTF32) { result.Append(char.ConvertFromUtf32(value)); }
                                }
                                break;
                            case 'u':
                                {
                                }
                                break;
                            case 'U':
                                {

                                }
                                break;
                            default: result.Append(str[index]); break;
                        }
                        escape = false;
                    }
                    else
                    {
                        switch (str[index])
                        {
                            case '\"': return;
                            case '\\': escape = true; break;
                            default: result.Append(str[index]); break;
                        }
                    }
                }
            }
            public static string ParseLiteralString(Runic.AST.Type.CharEncoding wcharEncoding, Token[] tokens, out Runic.AST.Type.CharEncoding preferredEncoding)
            {
                preferredEncoding = Runic.AST.Type.CharEncoding.UTF8;
                StringBuilder result = new StringBuilder();
                for (int n = 0; n < tokens.Length; n++)
                {
                    Runic.Token token = tokens[n];
#if NET6_0_OR_GREATER
                    string? tokenValue = token.Value;
#else
                    string tokenValue = token.Value;
#endif
                    if (tokenValue != null && tokenValue.Length > 0)
                    {
                        Runic.AST.Type.CharEncoding segmentEncoding = Runic.AST.Type.CharEncoding.UTF8;
                        AppendString(wcharEncoding, tokenValue, result, out segmentEncoding);
                        if (segmentEncoding != preferredEncoding) { preferredEncoding = segmentEncoding; }
                    }
                }
                return result.ToString();
            }

            public static string ParseLiteralChar(Runic.AST.Type.CharEncoding wcharEncoding, Token[] tokens, out Runic.AST.Type.CharEncoding preferredEncoding)
            {
                preferredEncoding = Runic.AST.Type.CharEncoding.UTF8;
                if (tokens.Length == 0) { return ""; }
                StringBuilder result = new StringBuilder();
                Runic.Token token = tokens[0];
#if NET6_0_OR_GREATER
                string? tokenValue = token.Value;
#else
                string tokenValue = token.Value;
#endif
                if (tokenValue != null && tokenValue.Length > 0)
                {
                    Runic.AST.Type.CharEncoding segmentEncoding = Runic.AST.Type.CharEncoding.UTF8;
                    AppendString(wcharEncoding, tokenValue, result, out segmentEncoding);
                    if (segmentEncoding != preferredEncoding) { preferredEncoding = segmentEncoding; }
                }
                return result.ToString();
            }


            public static bool ParseNumerical(Token Token, out bool IsHex, out bool IsBinary, out bool IsOctal, out bool IsUnsigned, out bool IsLong, out bool IsLongLong, out bool IsFloat, out bool IsDouble, out bool IsLongDouble, out bool Negative, out BigInteger IntegralPart, out BigInteger DecimalPart, out BigInteger DecimalPartFraction, out BigInteger ExponantPart, out bool ExponantNegative)
            {
                return ParseNumerical(Token.Value, out IsHex, out IsBinary, out IsOctal, out IsUnsigned, out IsLong, out IsLongLong, out IsFloat, out IsDouble, out IsLongDouble, out Negative, out IntegralPart, out DecimalPart, out DecimalPartFraction, out ExponantPart, out ExponantNegative);
            }
            static bool ParseUnsignedLongLongSuffix(string Token, int Index, out bool IsUnsigned, out bool IsLong, out bool IsLongLong)
            {

                IsUnsigned = false;
                IsLong = false;
                IsLongLong = false;
                if (Index >= Token.Length) { return true; }

                // Check for U, UL, ULL, LU and LLU
                if (Token[Index] == 'u' || Token[Index] == 'U')
                {
                    IsUnsigned = true;
                    Index++;
                    if (Index >= Token.Length) { return true; }
                    if (Token[Index] == 'l' || Token[Index] == 'L')
                    {
                        IsLong = true;
                        Index++;
                        if (Index >= Token.Length) { return true; }
                        if (Token[Index] == 'l' || Token[Index] == 'L')
                        {
                            IsLong = false;
                            IsLongLong = true;
                            Index++;
                            if (Index >= Token.Length) { return true; }
                            return false;
                        }
                    }
                }
                else if (Token[Index] == 'l' || Token[Index] == 'L')
                {
                    IsLong = true;
                    Index++;
                    if (Index >= Token.Length) { return true; }
                    if (Token[Index] == 'l' || Token[Index] == 'L')
                    {
                        IsLong = false;
                        IsLongLong = true;
                        Index++;
                        if (Index >= Token.Length) { return true; }
                        if (Token[Index] == 'u' || Token[Index] == 'U')
                        {
                            IsUnsigned = true;
                            Index++;
                            if (Index >= Token.Length) { return true; }
                            return false;
                        }
                    }
                }
                return false;
            }
            static bool Contains(string Token, char Chr)
            {
                for (int n = 0; n < Token.Length; n++)
                {
                    if (Token[n] == Chr) { return true; }
                }
                return false;
            }
            public static bool ParseNumerical(string Token, out bool IsHex, out bool IsBinary, out bool IsOctal, out bool IsUnsigned, out bool IsLong, out bool IsLongLong, out bool IsFloat, out bool IsDouble, out bool IsLongDouble, out bool Negative, out BigInteger IntegralPart, out BigInteger DecimalPart, out BigInteger DecimalPartFraction, out BigInteger ExponantPart, out bool ExponantNegative)
            {
                IsUnsigned = false;
                IsLong = false;
                IsLongLong = false;
                IsLongDouble = false;

                IsHex = false;
                IsBinary = false;
                IsOctal = false;
                IsFloat = false;
                IsDouble = false;
                Negative = false;
                IntegralPart = 0;
                DecimalPart = 0;
                DecimalPartFraction = 0;
                ExponantPart = 0;
                ExponantNegative = false;

                int index = 0;

                if (Token[0] == '-') { Negative = true; index++; }
                if (index >= Token.Length) { return false; }
                if (Token[index] == '0' && !Contains(Token, '.'))
                {
                    index++;
                    if (index >= Token.Length) { return true; }
                    if (Token[index] == 'x' || Token[index] == 'X')
                    {
                        index++;
                        IsHex = true;
                        // Parse as hex
                        for (; index < Token.Length; index++)
                        {
                            if (Token[index] >= '0' && Token[index] <= '9')
                            {
                                IntegralPart = (IntegralPart * 16UL) + (ulong)(Token[index] - '0');
                            }
                            else if (Token[index] >= 'A' && Token[index] <= 'F')
                            {
                                IntegralPart = (IntegralPart * 16UL) + ((ulong)(Token[index] - 'A') + 10UL);
                            }
                            else if (Token[index] >= 'a' && Token[index] <= 'f')
                            {
                                IntegralPart = (IntegralPart * 16UL) + ((ulong)(Token[index] - 'a') + 10UL);
                            }
                            else { break; }
                        }
                        if (index >= Token.Length) { return true; }
                        return ParseUnsignedLongLongSuffix(Token, index, out IsUnsigned, out IsLong, out IsLongLong);
                    }
                    if (Token[index] == 'b' || Token[index] == 'B')
                    {
                        index++;
                        IsBinary = true;
                        // Parse as binary (Not in every standard)
                        for (; index < Token.Length; index++)
                        {
                            if (Token[index] < '0' || Token[index] > '1') { break; }
                            IntegralPart = (IntegralPart * 2UL) + (ulong)(Token[index] - '0');
                        }
                        if (index >= Token.Length) { return true; }
                        return ParseUnsignedLongLongSuffix(Token, index, out IsUnsigned, out IsLong, out IsLongLong);
                    }
                    else
                    {
                        index++;
                        IsOctal = true;
                        // Parse as octal
                        for (; index < Token.Length; index++)
                        {
                            if (Token[index] < '0' || Token[index] > '7') { break; }
                            IntegralPart = (IntegralPart * 8UL) + (ulong)(Token[index] - '0');
                        }
                        if (index >= Token.Length) { return true; }
                        return ParseUnsignedLongLongSuffix(Token, index, out IsUnsigned, out IsLong, out IsLongLong);
                    }
                }
                else
                {
                    for (; index < Token.Length; index++)
                    {
                        if (Token[index] < '0' || Token[index] > '9') { break; }
                        IntegralPart = (IntegralPart * 10UL) + (ulong)(Token[index] - '0');
                    }
                    if (index >= Token.Length) { return true; }
                    if (Token[index] == '.')
                    {
                        DecimalPartFraction = 1;
                        IsDouble = true;
                        index++;
                        for (; index < Token.Length; index++)
                        {
                            if (Token[index] < '0' || Token[index] > '9') { break; }
                            DecimalPart = (ulong)(DecimalPart * 10UL) + (ulong)(Token[index] - '0');
                            DecimalPartFraction *= 10;
                        }
                        if (index >= Token.Length) { return true; }
                        if (Token[index] == 'e' || Token[index] == 'E')
                        {
                            IsDouble = true;
                            index++;
                            if (index >= Token.Length) { return true; }
                            if (Token[index] == '-')
                            {
                                ExponantNegative = true;
                                index++;
                                if (index >= Token.Length) { return true; }
                            }
                            for (; index < Token.Length; index++)
                            {
                                if (Token[index] < '0' || Token[index] > '9') { break; }
                                ExponantPart = (ExponantPart * 10UL) + (ulong)(Token[index] - '0');
                            }
                            if (index >= Token.Length) { return true; }
                        }
                        if (Token[index] == 'f' || Token[index] == 'F')
                        {
                            IsDouble = false;
                            IsFloat = true;
                            index++;
                            if (index >= Token.Length) { return true; }
                            return false;
                        }
                        if (Token[index] == 'l' || Token[index] == 'L')
                        {
                            IsDouble = false;
                            IsLongDouble = true;
                            index++;
                            if (index >= Token.Length) { return true; }
                            return false;
                        }

                        // Parse as double (Or Float / Long Double)
                    }
                    else if (Token[index] == 'e' || Token[index] == 'E')
                    {
                        IsDouble = true;
                        index++;
                        if (index >= Token.Length) { return true; }
                        if (Token[index] == '-')
                        {
                            ExponantNegative = true;
                            index++;
                            if (index >= Token.Length) { return true; }
                        }
                        for (; index < Token.Length; index++)
                        {
                            if (Token[index] < '0' || Token[index] > '9') { break; }
                            ExponantPart = (ExponantPart * 10UL) + (ulong)(Token[index] - '0');
                        }
                        if (index >= Token.Length) { return true; }
                        if (Token[index] == 'f' || Token[index] == 'F')
                        {
                            IsDouble = false;
                            IsFloat = true;
                            index++;
                            if (index >= Token.Length) { return true; }
                            return false;
                        }
                        if (Token[index] == 'l' || Token[index] == 'L')
                        {
                            IsDouble = false;
                            IsLongDouble = true;
                            index++;
                            if (index >= Token.Length) { return true; }
                            return false;
                        }
                        return false;
                    }
                    else if (Token[index] == 'f' || Token[index] == 'F')
                    {
                        IsFloat = true;
                        index++;
                        if (index >= Token.Length) { return true; }
                        return false;
                    }
                    else
                    {
                        return ParseUnsignedLongLongSuffix(Token, index, out IsUnsigned, out IsLong, out IsLongLong);
                    }
                    return false;
                }
            }
        }
    }

}