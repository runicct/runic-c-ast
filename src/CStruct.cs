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

namespace Runic.C
{
    public partial class AST : Runic.AST.INodeStream
    {
        internal class CStruct : Runic.AST.Type.StructOrUnion
        {
            public override ulong SizeOf(uint pointerSize, uint packing, uint padding)
            {
                ulong size = 0;
                for (int n = 0; n < Fields.Length; n++)
                {
                    ulong fieldSize = Fields[n].Type.SizeOf(pointerSize, packing, padding);
                    // TODO Apply packing and padding here
                    size += fieldSize;
                }
                return size;
            }
#if NET6_0_OR_GREATER
            CStruct(Field[] fields, string? name) : base(fields, name)
#else
            CStruct(Field[] fields, string name) : base(fields, name)
#endif
            {
            }
            public static CStruct Create(AST parent, Parser.Type.StructOrUnion src)
            {
                Field[] fields = new Field[src.Fields.Length];
                for (int n = 0; n < src.Fields.Length; n++)
                {
                    Runic.C.Parser.Field field = src.Fields[n];
                    fields[n] = new Field(parent.GetType(field.Type), field.Name == null ? null : src.Name.Value);
                }
                return new CStruct(fields, src.Name == null ? null : src.Name.Value);
            }
        }
    }
}
