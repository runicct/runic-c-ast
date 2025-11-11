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
using static Runic.C.Parser.Type;

namespace Runic.C
{
    public partial class AST : Runic.AST.INodeStream
    {
        internal class Type
        {
            internal static CInt _cint32 = new CInt(32);
            internal static CInt _cint64 = new CInt(64);
            internal static CLong _clong32 = new CLong(32);
            internal static CLong _clong64 = new CLong(64);
            internal static CChar _cchar = new CChar(); 
            internal static CLongLong _clonglong = new CLongLong();
            internal static CFloat _cfloat = new CFloat();
            internal static CDouble _cdouble = new CDouble();

            public class CInt : Runic.AST.Type.Integer
            {
                public override ulong SizeOf(uint pointerSize, uint packing, uint padding)
                {
                    return base.SizeOf(pointerSize, packing, padding);
                }
                internal CInt(uint bitSize) : base(true, bitSize) { }
            }
            public class CLong : Runic.AST.Type.Integer
            {
                public override ulong SizeOf(uint pointerSize, uint packing, uint padding)
                {
                    return base.SizeOf(pointerSize, packing, padding);
                }
                internal CLong(uint bitSize) : base(true, bitSize) { }
            }
            public class CLongLong : Runic.AST.Type.Integer
            {
                public override ulong SizeOf(uint pointerSize, uint packing, uint padding) { return 8; }
                internal CLongLong() : base(true, 64) { }
            }
            public class CFloat : Runic.AST.Type.FloatingPoint
            {
                public override ulong SizeOf(uint pointerSize, uint packing, uint padding) { return 4; }
                internal CFloat() : base(32) { }
            }
            public class CDouble : Runic.AST.Type.FloatingPoint
            {
                public override ulong SizeOf(uint pointerSize, uint packing, uint padding) { return 4; }
                internal CDouble() : base(64) { }
            }
            public class CChar : Runic.AST.Type.Char
            {
                internal CChar() : base(CharEncoding.UTF8) { }
            }
        }
    }
}
