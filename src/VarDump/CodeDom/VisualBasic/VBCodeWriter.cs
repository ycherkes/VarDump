// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.CodeDom.Resources;

namespace VarDump.CodeDom.VisualBasic;

internal sealed class VBCodeWriter : ICodeWriter
{
    private const int MaxLineLength = int.MaxValue;
    private readonly ExposedTabStringIndentedTextWriter _output;
    private readonly CodeWriterOptions _options;

    public int Indent
    {
        get => _output.Indent;
        set => _output.Indent = value;
    }

    public TextWriter Output => _output;

    public string NullToken => "Nothing";


    public VBCodeWriter(TextWriter w, CodeWriterOptions o)
    {
        _options = o ?? new CodeWriterOptions();
        _output = new ExposedTabStringIndentedTextWriter(w, _options.IndentString);
    }

    public void WriteType(CodeTypeInfo typeInfo) =>
        OutputType(typeInfo);

    public void WriteFlagsBitwiseOrOperator(IEnumerable<Action> operandActions)
    {
        bool isFirst = true;

        foreach (var operand in operandActions)
        {
            if (isFirst)
            {
                operand();
                isFirst = false;
            }
            else
            {
                Output.Write(' ');
                OutputBitwiseOrOperator();
                Output.Write(' ');
                operand();
            }
        }
    }

    private static void EnsureInDoubleQuotes(ref bool fInDoubleQuotes, StringBuilder b)
    {
        if (fInDoubleQuotes) return;
        b.Append("&\"");
        fInDoubleQuotes = true;
    }

    private static void EnsureNotInDoubleQuotes(ref bool fInDoubleQuotes, StringBuilder b)
    {
        if (!fInDoubleQuotes) return;
        b.Append('\"');
        fInDoubleQuotes = false;
    }

    public string QuoteSnippetString(string value)
    {
        StringBuilder b = new StringBuilder(value.Length + 5);

        bool fInDoubleQuotes = true;
        Indentation indentObj = new Indentation(_output, Indent + 1);

        b.Append('\"');

        int i = 0;
        while (i < value.Length)
        {
            char ch = value[i];
            switch (ch)
            {
                case '\"':
                // These are the inward sloping quotes used by default in some cultures like CHS. 
                // VBC.EXE does a mapping ANSI that results in it treating these as syntactically equivalent to a
                // regular double quote.
                case '\u201C':
                case '\u201D':
                case '\uFF02':
                    EnsureInDoubleQuotes(ref fInDoubleQuotes, b);
                    b.Append(ch);
                    b.Append(ch);
                    break;
                case '\r':
                    EnsureNotInDoubleQuotes(ref fInDoubleQuotes, b);
                    if (i < value.Length - 1 && value[i + 1] == '\n')
                    {
                        b.Append("&Global.Microsoft.VisualBasic.ChrW(13)&Global.Microsoft.VisualBasic.ChrW(10)");
                        i++;
                    }
                    else
                    {
                        b.Append("&Global.Microsoft.VisualBasic.ChrW(13)");
                    }
                    break;
                case '\t':
                    EnsureNotInDoubleQuotes(ref fInDoubleQuotes, b);
                    b.Append("&Global.Microsoft.VisualBasic.ChrW(9)");
                    break;
                case '\0':
                    EnsureNotInDoubleQuotes(ref fInDoubleQuotes, b);
                    b.Append("&Global.Microsoft.VisualBasic.ChrW(0)");
                    break;
                case '\n':
                    EnsureNotInDoubleQuotes(ref fInDoubleQuotes, b);
                    b.Append("&Global.Microsoft.VisualBasic.ChrW(10)");
                    break;
                case '\u2028':
                case '\u2029':
                    EnsureNotInDoubleQuotes(ref fInDoubleQuotes, b);
                    AppendEscapedChar(b, ch);
                    break;
                default:
                    EnsureInDoubleQuotes(ref fInDoubleQuotes, b);
                    b.Append(value[i]);
                    break;
            }

            if (i > 0 && i % MaxLineLength == 0)
            {
                //
                // If current character is a high surrogate and the following 
                // character is a low surrogate, don't break them. 
                // Otherwise, when we write the string to a file, we might lose 
                // the characters.
                // 
                if (char.IsHighSurrogate(value[i])
                    && i < value.Length - 1
                    && char.IsLowSurrogate(value[i + 1]))
                {
                    b.Append(value[++i]);
                }

                if (fInDoubleQuotes)
                    b.Append('\"');
                fInDoubleQuotes = true;

                b.Append("& _ ");
                b.Append(Environment.NewLine);
                b.Append(indentObj.IndentationString);
                b.Append('\"');
            }
            ++i;
        }

        if (fInDoubleQuotes)
            b.Append('\"');

        return b.ToString();
    }

    private static void AppendEscapedChar(StringBuilder b, char value)
    {
        b.Append("&Global.Microsoft.VisualBasic.ChrW(");
        b.Append(((int)value).ToString(CultureInfo.InvariantCulture));
        b.Append(")");
    }

    public void WriteNamedArgument(string argumentName, Action value)
    {
        Output.Write(argumentName);
        Output.Write(":=");
        value();
    }

    public void WriteAssign(Action left, Action right)
    {
        left();
        Output.Write(" = ");
        right();
    }

    public void WriteDefaultValue(CodeTypeInfo typeInfo)
    {
        Output.Write("CType(Nothing, " + GetTypeOutput(typeInfo) + ")");
    }

    private void OutputBitwiseOrOperator()
    {
        Output.Write("Or");
    }

    private void OutputIdentifier(string ident)
    {
        Output.Write(CreateEscapedIdentifier(ident));
    }

    public void OutputType(CodeTypeInfo typeInfo)
    {
        Output.Write(GetTypeOutputWithoutArrayPostFix(typeInfo));
    }


    private void OutputTypeNamePair(CodeTypeInfo typeInfo, string name)
    {
        if (string.IsNullOrEmpty(name))
            name = "__exception";

        OutputIdentifier(name);
        OutputArrayPostfix(typeInfo);
        if (!(typeInfo is CodeEmptyTypeInfo))
        {
            Output.Write(" As ");
            OutputType(typeInfo);
        }
    }

    private string GetArrayPostfix(CodeTypeInfo typeInfo)
    {
        string s = "";
        if (typeInfo.ArrayElementType != null)
        {
            // Recurse up
            s = GetArrayPostfix(typeInfo.ArrayElementType);
        }

        if (typeInfo.ArrayRank > 0)
        {
            char[] results = new char[typeInfo.ArrayRank + 1];
            results[0] = '(';
            results[typeInfo.ArrayRank] = ')';
            for (int i = 1; i < typeInfo.ArrayRank; i++)
            {
                results[i] = ',';
            }
            s = new string(results) + s;
        }

        return s;
    }

    private void OutputArrayPostfix(CodeTypeInfo typeInfo)
    {
        if (typeInfo.ArrayRank > 0)
        {
            Output.Write(GetArrayPostfix(typeInfo));
        }
    }

    public void WritePrimitive(object obj)
    {
        if (obj is char)
        {
            Output.Write("Global.Microsoft.VisualBasic.ChrW(" + ((IConvertible)obj).ToInt32(CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture) + ")");
        }
        else if (obj is sbyte @sbyte)
        {
            Output.Write("CSByte(");
            Output.Write(@sbyte.ToString(CultureInfo.InvariantCulture));
            Output.Write(')');
        }
        else if (obj is ushort @ushort)
        {
            Output.Write(@ushort.ToString(CultureInfo.InvariantCulture));
            Output.Write("US");
        }
        else if (obj is uint u)
        {
            Output.Write(u.ToString(CultureInfo.InvariantCulture));
            Output.Write("UI");
        }
        else if (obj is ulong @ulong)
        {
            Output.Write(@ulong.ToString(CultureInfo.InvariantCulture));
            Output.Write("UL");
        }
        else
        {
            DefaultWritePrimitiveExpression(obj);
        }
    }

    private void DefaultWritePrimitiveExpression(object obj)
    {
        if (obj == null)
        {
            Output.Write(NullToken);
        }
        else if (obj is string s)
        {
            Output.Write(QuoteSnippetString(s));
        }
        else if (obj is char)
        {
            Output.Write("'" + obj + "'");
        }
        else if (obj is byte b)
        {
            Output.Write(b.ToString(CultureInfo.InvariantCulture));
        }
        else if (obj is short s1)
        {
            Output.Write(s1.ToString(CultureInfo.InvariantCulture));
        }
        else if (obj is int i)
        {
            Output.Write(i.ToString(CultureInfo.InvariantCulture));
        }
        else if (obj is long l)
        {
            Output.Write(l.ToString(CultureInfo.InvariantCulture));
        }
        else if (obj is float f)
        {
            WriteSingleFloatValue(f);
        }
        else if (obj is double d)
        {
            WriteDoubleValue(d);
        }
        else if (obj is decimal @decimal)
        {
            WriteDecimalValue(@decimal);
        }
        else if (obj is bool b1)
        {
            Output.Write(b1 ? "true" : "false");
        }
        else
        {
            throw new ArgumentException(string.Format(SR.InvalidPrimitiveType, obj.GetType()));
        }
    }

    public void WriteArrayCreate(CodeTypeInfo typeInfo, IEnumerable<Action> initializers, int size = 0)
    {
        if (!(typeInfo is CodeEmptyTypeInfo))
        {
            Output.Write("New ");
        }

        using var initializersEnumerator = initializers.GetEnumerator();

        if (initializersEnumerator.MoveNext())
        {
            if (!(typeInfo is CodeEmptyTypeInfo))
            {
                string typeName = GetTypeOutput(typeInfo);
                Output.Write(typeName);
            }

            Output.Write("{");
            Output.WriteLine("");
            OutputActions(initializersEnumerator, newlineBetweenItems: true, newLineContinuation: false);
            Output.WriteLine("");
            Output.Write('}');
        }
        else
        {
            string typeName = GetTypeOutput(typeInfo);

            int index = typeName.IndexOf('(');
            if (index == -1)
            {
                Output.Write(typeName);
                Output.Write('(');
            }
            else
            {
                Output.Write(typeName.Substring(0, index + 1));
            }

            // The tricky thing is we need to declare the size - 1
            Output.Write(size - 1);

            if (index == -1)
            {
                Output.Write(')');
            }
            else
            {
                Output.Write(typeName.Substring(index + 1));
            }

            Output.Write(" {}");
        }
    }
    
    public void OutputActions(IEnumerable<Action> actions, bool newlineBetweenItems,
        bool newLineContinuation = true)
    {
        using var enumerator = actions.GetEnumerator();
        if (enumerator.MoveNext())
        {
            OutputActions(enumerator, newlineBetweenItems, newLineContinuation);
        }
        
    }

    public void OutputActions(IEnumerator<Action> actions, bool newlineBetweenItems,
        bool newLineContinuation = true)
    {
        bool first = true;
        Indent++;
        do
        {
            if (first)
            {
                first = false;
            }
            else
            {
                if (newlineBetweenItems)
                    ContinueOnNewLine(",", newLineContinuation);
                else
                    Output.Write(", ");
            }

            actions.Current?.Invoke();
        } while (actions.MoveNext());
        Indent--;
    }

    public void WriteArrayDimension(IEnumerable<Action> initializers)
    {
        Output.Write("{");
        Output.WriteLine();
        OutputActions(initializers, newlineBetweenItems: true, newLineContinuation: false);
        Output.WriteLine();
        Output.Write("}");
    }
    
    public void WriteCast(CodeTypeInfo typeInfo, Action action)
    {
        Output.Write("CType(");
        action();
        Output.Write(", ");
        OutputType(typeInfo);
        OutputArrayPostfix(typeInfo);
        Output.Write(')');
    }

    public void WriteFieldReference(string fieldName, Action targetObjectAction)
    {
        if (targetObjectAction != null)
        {
            targetObjectAction();
            Output.Write('.');
        }
        OutputIdentifier(fieldName);
    }

    
    public void WriteSingleFloatValue(float s)
    {
        if (float.IsNaN(s))
        {
            Output.Write("Single.NaN");
        }
        else if (float.IsNegativeInfinity(s))
        {
            Output.Write("Single.NegativeInfinity");
        }
        else if (float.IsPositiveInfinity(s))
        {
            Output.Write("Single.PositiveInfinity");
        }
        else
        {
            Output.Write(s.ToString(CultureInfo.InvariantCulture));
            Output.Write('!');
        }
    }

    public void WriteDoubleValue(double d)
    {
        if (double.IsNaN(d))
        {
            Output.Write("Double.NaN");
        }
        else if (double.IsNegativeInfinity(d))
        {
            Output.Write("Double.NegativeInfinity");
        }
        else if (double.IsPositiveInfinity(d))
        {
            Output.Write("Double.PositiveInfinity");
        }
        else
        {
            Output.Write(d.ToString("R", CultureInfo.InvariantCulture));
            // always mark a double as being a double in case we have no decimal portion (e.g write 1D instead of 1 which is an int)
            Output.Write('R');
        }
    }

    public void WriteDecimalValue(decimal d)
    {
        Output.Write(d.ToString(CultureInfo.InvariantCulture));
        Output.Write('D');
    }

    public void WriteVariableReference(string variableName) =>
        OutputIdentifier(variableName);

    public void WriteMethodInvoke(Action methodReferenceAction, IEnumerable<Action> parametersActions)
    {
        methodReferenceAction();
        Output.Write('(');
        OutputActions(parametersActions, newlineBetweenItems: false);
        Output.Write(')');
    }
    
    public void WriteMethodReference(Action targetObject, string methodName, params CodeTypeInfo[] typeParameters)
    {
        if (targetObject != null)
        {
            targetObject();
            Output.Write('.');
        }
        OutputIdentifier(methodName);

        if (typeParameters.Length > 0)
        {
            Output.Write(GetTypeArgumentsOutput(typeParameters));
        }
    }

    public void WriteObjectCreate(CodeTypeInfo typeInfo, IEnumerable<Action> parametersActions)
    {
        Output.Write("New ");
        OutputType(typeInfo);

        using var parametersEnumerator = parametersActions.GetEnumerator();

        // always write out the () to disambiguate cases like "New System.Random().Next(x,y)"
        Output.Write('(');
        if (parametersEnumerator.MoveNext())
        {
            OutputActions(parametersEnumerator, newlineBetweenItems: false);
        }
        Output.Write(')');
    }

    public void WriteObjectCreateAndInitialize(CodeTypeInfo typeInfo, IEnumerable<Action> parametersActions, IEnumerable<Action> initializeActions)
    {
        Output.Write("New ");
        OutputType(typeInfo);

        using var parametersEnumerator = parametersActions.GetEnumerator();
        using var initializeEnumerator = initializeActions.GetEnumerator();

        var parametersExist = parametersEnumerator.MoveNext();
        var initializeExist = initializeEnumerator.MoveNext();

        if (parametersExist || !initializeExist)
        {
            // always write out the () to disambiguate cases like "New System.Random().Next(x,y)"
            Output.Write('(');
            if (parametersExist)
            {
                OutputActions(parametersEnumerator, newlineBetweenItems: false);
            }
            Output.Write(')');
        }

        if (!initializeExist)
        {
            return;
        }

        Output.Write(typeInfo switch
        {
            CodeEmptyTypeInfo => "With ",
            CodeCollectionTypeInfo => " From ",
            _ => " With "
        });
        
        Output.WriteLine('{');
        OutputActions(initializeEnumerator, newlineBetweenItems: true, newLineContinuation: false);
        Output.WriteLine();
        Output.Write("}");
    }

    public void WriteValueTupleCreate(IEnumerable<Action> actions)
    {
        Output.Write('(');
        OutputActions(actions, newlineBetweenItems: false);
        Output.Write(')');
    }

    public void WriteImplicitKeyValuePairCreate(Action keyAction, Action valueAction)
    {
        Output.WriteLine('{');
        OutputActions([keyAction, valueAction], newlineBetweenItems: true, false);
        Output.WriteLine();
        Output.Write('}');
    }

    public void WriteSeparator()
    {
        Output.Write(", ");
    }

    public void WriteLambdaExpression(Action lambda, Action[] parameters)
    {
        Output.Write("Function (");
        bool first = true;

        foreach (Action current in parameters)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                Output.Write(", ");
            }
            current();
        }

        Output.Write(')');
        Output.Write(" ");
        lambda();
    }
    
    public void WriteComment(string comment, bool noNewLine)
    {
        const string commentLineStart = "'";
        Output.Write(commentLineStart);
        string value = comment;
        for (int i = 0; i < value.Length; i++)
        {
            Output.Write(value[i]);

            if (value[i] == '\r')
            {
                if (i < value.Length - 1 && value[i + 1] == '\n')
                { // if next char is '\n', skip it
                    Output.Write('\n');
                    i++;
                }
                ((ExposedTabStringIndentedTextWriter)Output).InternalOutputTabs();
                Output.Write(commentLineStart);
            }
            else if (value[i] == '\n')
            {
                ((ExposedTabStringIndentedTextWriter)Output).InternalOutputTabs();
                Output.Write(commentLineStart);
            }
            else if (value[i] == '\u2028' || value[i] == '\u2029' || value[i] == '\u0085')
            {
                Output.Write(commentLineStart);
            }
        }
        if (!noNewLine)
        {
            Output.WriteLine();
        }
    }

    public void WriteVariableDeclarationStatement(CodeTypeInfo typeInfo, string variableName, Action initAction)
    {
        Output.Write("Dim ");

        OutputTypeNamePair(typeInfo, variableName);

        if (initAction != null)
        {
            Output.Write(" = ");
            initAction();
        }

        Output.WriteLine();
    }

    public void WritePropertyReference(string propertyName, Action targetObjectAction)
    {
        targetObjectAction?.Invoke();
        Output.Write('.');
        OutputIdentifier(propertyName);
    }
    
    public void WriteTypeOf(CodeTypeInfo typeInfo)
    {
        Output.Write("GetType(");
        Output.Write(GetTypeOutput(typeInfo));
        Output.Write(')');
    }

    public static bool IsKeyword(string value)
    {
        return VBHelpers.IsKeyword(value);
    }

    public bool IsValidIdentifier(string value)
    {
        return VBHelpers.IsValidIdentifier(value);
    }

    public string CreateValidIdentifier(string name)
    {
        return VBHelpers.CreateValidIdentifier(name);
    }

    public string CreateEscapedIdentifier(string name)
    {
        return VBHelpers.CreateEscapedIdentifier(name);
    }

    private string GetBaseTypeOutput(CodeTypeInfo typeInfo, bool preferBuiltInTypes = true)
    {
        string s = typeInfo.BaseType;

        if (s == "System.Nullable`1" && typeInfo.TypeArguments.Count > 0)
        {
            return GetBaseTypeOutput(typeInfo.TypeArguments[0]) + "?";
        }

        if (preferBuiltInTypes)
        {
            if (typeInfo is CodeEmptyTypeInfo)
            {
                return string.Empty;
            }

            if (s.Length == 0)
            {
                return "Void";
            }

            string lowerCaseString = s.ToLowerInvariant();

            switch (lowerCaseString)
            {
                case "system.byte":
                    return "Byte";
                case "system.sbyte":
                    return "SByte";
                case "system.int16":
                    return "Short";
                case "system.int32":
                    return "Integer";
                case "system.int64":
                    return "Long";
                case "system.uint16":
                    return "UShort";
                case "system.uint32":
                    return "UInteger";
                case "system.uint64":
                    return "ULong";
                case "system.string":
                    return "String";
                case "system.datetime":
                    return "Date";
                case "system.decimal":
                    return "Decimal";
                case "system.single":
                    return "Single";
                case "system.double":
                    return "Double";
                case "system.boolean":
                    return "Boolean";
                case "system.char":
                    return "Char";
                case "system.object":
                    return "Object";
            }
        }

        var sb = new StringBuilder(s.Length + 10);

        string baseType = _options.UseFullTypeName
            ? typeInfo.BaseType
            : typeInfo.BaseType.Split('.').Last().Split('+').Last();

        int lastIndex = 0;
        int currentTypeArgStart = 0;
        for (int i = 0; i < baseType.Length; i++)
        {
            switch (baseType[i])
            {
                case '+':
                case '.':
                    sb.Append(CreateEscapedIdentifier(baseType.Substring(lastIndex, i - lastIndex)));
                    sb.Append('.');
                    i++;
                    lastIndex = i;
                    break;

                case '`':
                    sb.Append(CreateEscapedIdentifier(baseType.Substring(lastIndex, i - lastIndex)));
                    i++;    // skip the '
                    int numTypeArgs = 0;
                    while (i < baseType.Length && baseType[i] >= '0' && baseType[i] <= '9')
                    {
                        numTypeArgs = numTypeArgs * 10 + (baseType[i] - '0');
                        i++;
                    }

                    GetTypeArgumentsOutput(typeInfo.TypeArguments, currentTypeArgStart, numTypeArgs, sb);
                    currentTypeArgStart += numTypeArgs;

                    // Arity can be in the middle of a nested type name, so we might have a . or + after it. 
                    // Skip it if so. 
                    if (i < baseType.Length && (baseType[i] == '+' || baseType[i] == '.'))
                    {
                        sb.Append('.');
                        i++;
                    }

                    lastIndex = i;
                    break;
            }
        }

        if (lastIndex < baseType.Length)
        {
            sb.Append(CreateEscapedIdentifier(baseType.Substring(lastIndex)));
        }

        return sb.ToString();
    }

    private string GetTypeOutputWithoutArrayPostFix(CodeTypeInfo typeInfo)
    {
        StringBuilder sb = new StringBuilder();

        while (typeInfo.ArrayElementType != null)
        {
            typeInfo = typeInfo.ArrayElementType;
        }

        sb.Append(GetBaseTypeOutput(typeInfo));
        return sb.ToString();
    }

    private string GetTypeArgumentsOutput(CodeTypeInfo[] typeArguments)
    {
        typeArguments ??= [];
        StringBuilder sb = new StringBuilder(128);
        GetTypeArgumentsOutput(typeArguments, 0, typeArguments.Length, sb);
        return sb.ToString();
    }


    private void GetTypeArgumentsOutput(IReadOnlyList<CodeTypeInfo> typeArguments, int start, int length, StringBuilder sb)
    {
        typeArguments ??= [];
        sb.Append("(Of ");
        bool first = true;
        for (int i = start; i < start + length; i++)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                sb.Append(", ");
            }

            // it's possible that we call GetTypeArgumentsOutput with an empty typeArguments collection.  This is the case
            // for open types, so we want to just output the brackets and commas. 
            if (i < typeArguments.Count)
                sb.Append(GetTypeOutput(typeArguments[i]));
        }
        sb.Append(')');
    }

    public string GetTypeOutput(CodeTypeInfo typeInfo)
    {
        string s = string.Empty;
        s += GetTypeOutputWithoutArrayPostFix(typeInfo);

        if (typeInfo.ArrayRank > 0)
        {
            s += GetArrayPostfix(typeInfo);
        }
        return s;
    }

    public void ContinueOnNewLine(string st, bool newLineContinuation = true)
    {
        Output.Write(st);
        Output.WriteLine(newLineContinuation ? " _" : "");
    }
}