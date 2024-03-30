// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace VarDump.CodeDom.Common;

public class CodeTypeInfo
{
    private string _baseType;
    private bool _needsFixup;
    private List<CodeTypeInfo> _typeArguments;
    public CodeTypeInfo(Type type)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        if (type.IsArray)
        {
            ArrayRank = type.GetArrayRank();
            ArrayElementType = new CodeTypeInfo(type.GetElementType());
            _baseType = null;
        }
        else
        {
            InitializeFromType(type);
            ArrayRank = 0;
            ArrayElementType = null;
        }
    }

    private CodeTypeInfo(string typeName)
    {
        Initialize(typeName);
    }

    public CodeTypeInfo(CodeTypeInfo arrayType, int rank)
    {
        _baseType = null;
        ArrayRank = rank;
        ArrayElementType = arrayType;
    }

    internal CodeTypeInfo()
    {
    }

    public CodeTypeInfo ArrayElementType { get; set; }

    public int ArrayRank { get; set; }

    public string BaseType
    {
        get
        {
            if (ArrayRank > 0 && ArrayElementType != null)
            {
                return ArrayElementType.BaseType;
            }

            if (string.IsNullOrEmpty(_baseType))
            {
                return string.Empty;
            }

            string returnType = _baseType;
            return _needsFixup && TypeArguments.Count > 0 ?
                returnType + '`' + TypeArguments.Count.ToString(CultureInfo.InvariantCulture) :
                returnType;
        }
        set
        {
            _baseType = value;
            Initialize(_baseType);
        }
    }

    public List<CodeTypeInfo> TypeArguments
    {
        get
        {
            if (ArrayRank > 0 && ArrayElementType != null)
            {
                return ArrayElementType.TypeArguments;
            }

            return _typeArguments ??= [];
        }
    }

    public static implicit operator CodeTypeInfo(Type type) => new(type);

    internal int NestedArrayDepth => ArrayElementType == null ? 0 : 1 + ArrayElementType.NestedArrayDepth;

    private void Initialize(string typeName)
    {
        if (string.IsNullOrEmpty(typeName))
        {
            typeName = typeof(void).FullName;
            _baseType = typeName;
            ArrayRank = 0;
            ArrayElementType = null;
            return;
        }
        if (this is CodeEmptyTypeInfo)
        {
            _baseType = typeName;
            ArrayRank = 0;
            ArrayElementType = null;
            return;
        }

        typeName = RipOffAssemblyInformationFromTypeName(typeName);

        int end = typeName.Length - 1;
        int current = end;
        _needsFixup = true; // default to true, and if we find arity or generic type args, we'll clear the flag.

        // Scan the entire string for valid array tails and store ranks for array tails
        // we found in a queue.
        var q = new Queue<int>();
        while (current >= 0)
        {
            int rank = 1;
            if (typeName[current--] == ']')
            {
                while (current >= 0 && typeName[current] == ',')
                {
                    rank++;
                    current--;
                }

                if (current >= 0 && typeName[current] == '[')
                {
                    // found a valid array tail
                    q.Enqueue(rank);
                    current--;
                    end = current;
                    continue;
                }
            }
            break;
        }

        // Try find generic type arguments
        current = end;
        var typeArgumentList = new List<CodeTypeInfo>();
        var subTypeNames = new Stack<string>();
        if (current > 0 && typeName[current--] == ']')
        {
            _needsFixup = false;
            int unmatchedRightBrackets = 1;
            int subTypeNameEndIndex = end;

            // Try find the matching '[', if we can't find it, we will not try to parse the string
            while (current >= 0)
            {
                if (typeName[current] == '[')
                {
                    // break if we found matched brackets
                    if (--unmatchedRightBrackets == 0) break;
                }
                else if (typeName[current] == ']')
                {
                    ++unmatchedRightBrackets;
                }
                else if (typeName[current] == ',' && unmatchedRightBrackets == 1)
                {
                    //
                    // Type name can contain nested generic types. Following is an example:
                    // System.Collections.Generic.Dictionary`2[[System.string, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089], 
                    //          [System.Collections.Generic.List`1[[System.Int32, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], 
                    //           mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]
                    // 
                    // Spliltting by ',' won't work. We need to do first-level split by ','. 
                    //
                    if (current + 1 < subTypeNameEndIndex)
                    {
                        subTypeNames.Push(typeName.Substring(current + 1, subTypeNameEndIndex - current - 1));
                    }

                    subTypeNameEndIndex = current;
                }
                --current;
            }

            if (current > 0 && end - current - 1 > 0)
            {
                // push the last generic type argument name if there is any
                if (current + 1 < subTypeNameEndIndex)
                {
                    subTypeNames.Push(typeName.Substring(current + 1, subTypeNameEndIndex - current - 1));
                }

                // we found matched brackets and the brackets contains some characters.                    
                while (subTypeNames.Count > 0)
                {
                    string name = RipOffAssemblyInformationFromTypeName(subTypeNames.Pop());
                    typeArgumentList.Add(new CodeTypeInfo(name));
                }
                end = current - 1;
            }
        }

        if (end < 0)
        {
            // this can happen if we have some string like "[...]"
            _baseType = typeName;
            return;
        }

        if (q.Count > 0)
        {
            CodeTypeInfo type = new CodeTypeInfo(typeName.Substring(0, end + 1));

            type.TypeArguments.AddRange(typeArgumentList);

            while (q.Count > 1)
            {
                type = new CodeTypeInfo(type, q.Dequeue());
            }

            // we don't need to create a new CodeTypeInfo for the last one.
            Debug.Assert(q.Count == 1, "We should have one and only one in the rank queue.");
            _baseType = null;
            ArrayRank = q.Dequeue();
            ArrayElementType = type;
        }
        else if (typeArgumentList.Count > 0)
        {
            TypeArguments.AddRange(typeArgumentList);

            _baseType = typeName.Substring(0, end + 1);
        }
        else
        {
            _baseType = typeName;
        }

        // Now see if we have some arity.  baseType could be null if this is an array type. 
        if (_baseType != null && _baseType.IndexOf('`') != -1)
        {
            _needsFixup = false;
        }
    }

    private void InitializeFromType(Type type)
    {
        _baseType = type.Name;
        if (!type.IsGenericParameter)
        {
            Type currentType = type;
            while (currentType.IsNested)
            {
                currentType = currentType.DeclaringType;
                _baseType = currentType.Name + "+" + _baseType;
            }

            if (!string.IsNullOrEmpty(type.Namespace))
            {
                _baseType = type.Namespace + "." + _baseType;
            }
        }

        // pick up the type arguments from an instantiated generic type but not an open one    
        if (type.IsGenericType && !type.ContainsGenericParameters)
        {
            TypeArguments.AddRange(type.GetGenericArguments().Select(x => new CodeTypeInfo(x)));
        }
        else if (!type.IsGenericTypeDefinition)
        {
            // if the user handed us a non-generic type, but later
            // appends generic type arguments, we'll pretend
            // it's a generic type for their sake - this is good for
            // them if they pass in System.Nullable class when they
            // meant the System.Nullable<T> value type.
            _needsFixup = true;
        }
    }
    //
    // The string for generic type argument might contain assembly information and square bracket pair.
    // There might be leading spaces in front the type name.
    // Following function will rip off assembly information and brackets 
    // Following is an example:
    // " [System.Collections.Generic.List[[System.string, mscorlib, Version=2.0.0.0, Culture=neutral,
    //   PublicKeyToken=b77a5c561934e089]], mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]"
    //
    private string RipOffAssemblyInformationFromTypeName(string typeName)
    {
        int start = 0;
        int end = typeName.Length - 1;
        string result = typeName;

        // skip whitespace in the beginning
        while (start < typeName.Length && char.IsWhiteSpace(typeName[start])) start++;
        while (end >= 0 && char.IsWhiteSpace(typeName[end])) end--;

        if (start < end)
        {
            if (typeName[start] == '[' && typeName[end] == ']')
            {
                start++;
                end--;
            }

            // if we still have a ] at the end, there's no assembly info. 
            if (typeName[end] != ']')
            {
                int commaCount = 0;
                for (int index = end; index >= start; index--)
                {
                    if (typeName[index] == ',')
                    {
                        commaCount++;
                        if (commaCount == 4)
                        {
                            result = typeName.Substring(start, index - start);
                            break;
                        }
                    }
                }
            }
        }

        return result;
    }
}