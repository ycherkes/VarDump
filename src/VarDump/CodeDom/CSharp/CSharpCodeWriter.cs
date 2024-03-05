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

namespace VarDump.CodeDom.CSharp;

internal sealed class CSharpCodeWriter : ICodeWriter
{
    private readonly ExposedTabStringIndentedTextWriter _output;
    private readonly CodeWriterOptions _options;
    private const int MaxLineLength = int.MaxValue;

    public int Indent
    {
        get => _output.Indent;
        set => _output.Indent = value;
    }

    public string NullToken => "null";

    public TextWriter Output => _output;

    public CSharpCodeWriter(TextWriter w, CodeWriterOptions o)
    {
       _options = o ?? new CodeWriterOptions();
       _output = new ExposedTabStringIndentedTextWriter(w, _options.IndentString);
    }
    
    private string QuoteSnippetStringCStyle(string value)
    {
        var b = new StringBuilder(value.Length + 5);

        var indentObj = new Indentation(_output, Indent + 1);

        b.Append('\"');

        int i = 0;
        while (i < value.Length)
        {
            switch (value[i])
            {
                case '\r':
                    b.Append("\\r");
                    break;
                case '\t':
                    b.Append("\\t");
                    break;
                case '\"':
                    b.Append("\\\"");
                    break;
                case '\'':
                    b.Append("\\\'");
                    break;
                case '\\':
                    b.Append("\\\\");
                    break;
                case '\0':
                    b.Append("\\0");
                    break;
                case '\n':
                    b.Append("\\n");
                    break;
                case '\u2028':
                case '\u2029':
                    AppendEscapedChar(b, value[i]);
                    break;

                default:
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
                if (char.IsHighSurrogate(value[i]) && (i < value.Length - 1) && char.IsLowSurrogate(value[i + 1]))
                {
                    b.Append(value[++i]);
                }

                b.Append("\" +");
                b.Append(Environment.NewLine);
                b.Append(indentObj.IndentationString);
                b.Append('\"');
            }
            ++i;
        }

        b.Append('\"');

        return b.ToString();
    }

    private static string QuoteSnippetStringVerbatimStyle(string value)
    {
        var b = new StringBuilder(value.Length + 5);

        b.Append("@\"");

        for (var i = 0; i < value.Length; i++)
        {
            if (value[i] == '\"')
                b.Append("\"\"");
            else
                b.Append(value[i]);
        }

        b.Append('\"');

        return b.ToString();
    }

    public string QuoteSnippetString(string value)
    {
        // If the string is short, use C style quoting (e.g "\r\n")
        // Also do it if it is too long to fit in one line
        // If the string contains '\0', verbatim style won't work.
        if (value.Length < 256 || value.Length > 1500 || (value.IndexOf('\0') != -1))
            return QuoteSnippetStringCStyle(value);

        // Otherwise, use 'verbatim' style quoting (e.g. @"foo")
        return QuoteSnippetStringVerbatimStyle(value);
    }

    public void ContinueOnNewLine(string st) => Output.WriteLine(st);

    private void OutputIdentifier(string ident) => Output.Write(CreateEscapedIdentifier(ident));

    public void OutputType(CodeTypeInfo typeInfo) => Output.Write(GetTypeOutput(typeInfo));

    public void WriteArrayCreate(CodeTypeInfo typeInfo, IEnumerable<Action> initializers, int size=0)
    {
        Output.Write("new ");

        using var initializersEnumerator = initializers.GetEnumerator();

        if (initializersEnumerator.MoveNext())
        {
            OutputType(typeInfo);
            Output.WriteLine("");
            Output.WriteLine("{");
            OutputActions(initializersEnumerator, newlineBetweenItems: true);
            Output.WriteLine();
            Output.Write("}");
        }
        else
        {
            Output.Write(GetBaseTypeOutput(typeInfo));

            Output.Write('[');
            Output.Write(size);
            Output.Write(']');

            int nestedArrayDepth = typeInfo.NestedArrayDepth;
            for (int i = 0; i < nestedArrayDepth - 1; i++)
            {
                Output.Write("[]");
            }
        }
    }

    public void WriteArrayDimension(IEnumerable<Action> initializers)
    {
        Output.Write("{");
        Output.WriteLine();
        OutputActions(initializers, newlineBetweenItems: true);
        Output.WriteLine();
        Output.Write("}");
    }

    public void WriteCast(CodeTypeInfo typeInfo, Action action)
    {
        Output.Write("(");
        OutputType(typeInfo);
        Output.Write(")");
        action();
    }

   public void WriteAssign(Action left, Action right)
    {
        left();
        Output.Write(" = ");
        right();
    }

    public void WriteDefaultValue(CodeTypeInfo typeInfo)
    {
        Output.Write("default(");
        OutputType(typeInfo);
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
        Output.Write("new ");
        OutputType(typeInfo);

        using var parametersEnumerator = parametersActions.GetEnumerator();

        Output.Write('(');
        if (parametersEnumerator.MoveNext())
        {
            OutputActions(parametersEnumerator, newlineBetweenItems: false);
        }
        Output.Write(')');
    }

    public void WriteObjectCreateAndInitialize(CodeTypeInfo typeInfo, IEnumerable<Action> parametersActions, IEnumerable<Action> initializeActions)
    {
        Output.Write("new ");
        OutputType(typeInfo);

        using var parametersEnumerator = parametersActions.GetEnumerator();
        using var initializeEnumerator = initializeActions.GetEnumerator();

        var parametersExist = parametersEnumerator.MoveNext();
        var initializeExist = initializeEnumerator.MoveNext();

        if (parametersExist || !initializeExist)
        {
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

        Output.WriteLine();
        Output.WriteLine('{');
        OutputActions(initializeEnumerator, newlineBetweenItems: true);
        Output.WriteLine();
        Output.Write("}");
    }

    public void WriteValueTupleCreate(IEnumerable<Action> actions)
    {
        Output.Write('(');
        OutputActions(actions, newlineBetweenItems: false);
        Output.Write(')');
    }

    public void WritePrimitive(object obj)
    {
        if (obj is char c)
        {
            WritePrimitiveChar(c);
        }
        else if (obj is sbyte @sbyte)
        {
            // C# has no literal marker for types smaller than Int32                
            Output.Write(@sbyte.ToString(CultureInfo.InvariantCulture));
        }
        else if (obj is ushort @ushort)
        {
            // C# has no literal marker for types smaller than Int32, and you will
            // get a conversion error if you use "u" here.
            Output.Write(@ushort.ToString(CultureInfo.InvariantCulture));
        }
        else if (obj is uint u)
        {
            Output.Write(u.ToString(CultureInfo.InvariantCulture));
            Output.Write('u');
        }
        else if (obj is ulong @ulong)
        {
            Output.Write(@ulong.ToString(CultureInfo.InvariantCulture));
            Output.Write("ul");
        }
        else
        {
            WritePrimitiveExpressionBase(obj);
        }
    }

    private void WritePrimitiveExpressionBase(object obj)
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
            if (b1)
            {
                Output.Write("true");
            }
            else
            {
                Output.Write("false");
            }
        }
        else
        {
            throw new ArgumentException(string.Format(SR.InvalidPrimitiveType, obj.GetType()));
        }
    }

    private void WritePrimitiveChar(char c)
    {
        Output.Write('\'');
        switch (c)
        {
            case '\r':
                Output.Write("\\r");
                break;
            case '\t':
                Output.Write("\\t");
                break;
            case '\"':
                Output.Write("\\\"");
                break;
            case '\'':
                Output.Write("\\\'");
                break;
            case '\\':
                Output.Write("\\\\");
                break;
            case '\0':
                Output.Write("\\0");
                break;
            case '\n':
                Output.Write("\\n");
                break;
            case '\u2028':
            case '\u2029':
            case '\u0084':
            case '\u0085':
                AppendEscapedChar(null, c);
                break;

            default:
                if (char.IsSurrogate(c))
                {
                    AppendEscapedChar(null, c);
                }
                else
                {
                    Output.Write(c);
                }
                break;
        }
        Output.Write('\'');
    }

    private void AppendEscapedChar(StringBuilder b, char value)
    {
        if (b == null)
        {
            Output.Write("\\u");
            Output.Write(((int)value).ToString("X4", CultureInfo.InvariantCulture));
        }
        else
        {
            b.Append("\\u");
            b.Append(((int)value).ToString("X4", CultureInfo.InvariantCulture));
        }
    }

    public void WriteComment(string comment, bool noNewLine)
    {
        const string commentLineStart = "//";
        Output.Write(commentLineStart);
        Output.Write(' ');

        string value = comment;
        for (int i = 0; i < value.Length; i++)
        {
            if (value[i] == '\u0000')
            {
                continue;
            }

            Output.Write(value[i]);

            if (value[i] == '\r')
            {
                if (i < value.Length - 1 && value[i + 1] == '\n')
                {
                    // if next char is '\n', skip it
                    Output.Write('\n');
                    i++;
                }

                _output.InternalOutputTabs();
                Output.Write(commentLineStart);
            }
            else if (value[i] == '\n')
            {
                _output.InternalOutputTabs();
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
        OutputTypeNamePair(typeInfo, variableName);
        if (initAction != null)
        {
            Output.Write(" = ");
            initAction();
        }
        Output.WriteLine(';');
    }

    public void WriteNamedArgument(string argumentName, Action value)
    {
        Output.Write(argumentName);
        Output.Write(": ");
        value();
    }

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

    public void WriteSeparator()
    {
        Output.Write(", ");
    }

    public void WriteImplicitKeyValuePairCreate(Action keyAction, Action valueAction)
    {
        Output.WriteLine('{');
        OutputActions([keyAction, valueAction], newlineBetweenItems: true);
        Output.WriteLine();
        Output.Write('}');
    }

    public void WriteLambdaExpression(Action lambda, Action[] parameters)
    {
        if (parameters.Length != 1)
        {
            Output.Write('(');
        }
        bool first = true;
        foreach (var current in parameters)
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

        if (parameters.Length != 1)
        {
            Output.Write(')');
        }
        Output.Write(" => ");
        lambda();
    }

    public void WriteSingleFloatValue(float s)
    {
        if (float.IsNaN(s))
        {
            Output.Write("float.NaN");
        }
        else if (float.IsNegativeInfinity(s))
        {
            Output.Write("float.NegativeInfinity");
        }
        else if (float.IsPositiveInfinity(s))
        {
            Output.Write("float.PositiveInfinity");
        }
        else
        {
            Output.Write(s.ToString(CultureInfo.InvariantCulture));
            Output.Write('F');
        }
    }

    public void WriteDoubleValue(double d)
    {
        if (double.IsNaN(d))
        {
            Output.Write("double.NaN");
        }
        else if (double.IsNegativeInfinity(d))
        {
            Output.Write("double.NegativeInfinity");
        }
        else if (double.IsPositiveInfinity(d))
        {
            Output.Write("double.PositiveInfinity");
        }
        else
        {
            Output.Write(d.ToString("R", CultureInfo.InvariantCulture));
            // always mark a double as being a double in case we have no decimal portion (e.q. write 1D instead of 1 which is an int)
            Output.Write('D');
        }
    }

    public void WriteDecimalValue(decimal d)
    {
        Output.Write(d.ToString(CultureInfo.InvariantCulture));
        Output.Write('m');
    }

    private void OutputBitwiseOrOperator()
    {
        Output.Write('|');
    }

    public void WritePropertyReference(string propertyName, Action targetObjectAction)
    {
        if (targetObjectAction != null)
        {
            targetObjectAction();
            Output.Write('.');
        }
        OutputIdentifier(propertyName);
    }

    public void WriteType(CodeTypeInfo typeInfo) =>
        OutputType(typeInfo);

    public void WriteTypeOf(CodeTypeInfo typeInfo)
    {
        Output.Write("typeof(");
        OutputType(typeInfo);
        Output.Write(')');
    }

    public void OutputActions(IEnumerable<Action> actions, bool newlineBetweenItems)
    {
        using var enumerator = actions.GetEnumerator();
        if (enumerator.MoveNext())
        {
            OutputActions(enumerator, newlineBetweenItems);
        }
    }

    public void OutputActions(IEnumerator<Action> actions, bool newlineBetweenItems)
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
                    ContinueOnNewLine(",");
                else
                    Output.Write(", ");
            }
            actions.Current?.Invoke();
        } while (actions.MoveNext());
        Indent--;
    }

    private void OutputTypeNamePair(CodeTypeInfo typeInfo, string name)
    {
        OutputType(typeInfo);
        Output.Write(' ');
        OutputIdentifier(name);
    }

    public bool IsValidIdentifier(string value)
    {
        // identifiers must be 1 char or longer
        //
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        if (value.Length > 512)
        {
            return false;
        }

        // identifiers cannot be a keyword, unless they are escaped with an '@'
        //
        if (value[0] != '@')
        {
            if (CSharpHelpers.IsKeyword(value))
            {
                return false;
            }
        }
        else
        {
            value = value.Substring(1);
        }

        return CSharpHelpers.IsValidTypeNameOrIdentifier(value, false);
    }

    public string CreateEscapedIdentifier(string name)
    {
        return CSharpHelpers.CreateEscapedIdentifier(name);
    }

    // returns the type name without any array declaration.
    private string GetBaseTypeOutput(CodeTypeInfo typeInfo, bool preferBuiltInTypes = true)
    {
        string s = typeInfo.BaseType;

        if (s == "System.Nullable`1" && typeInfo.TypeArguments.Count > 0)
        {
            return GetBaseTypeOutput(typeInfo.TypeArguments[0]) + "?";
        }

        if (preferBuiltInTypes)
        {
            if (typeInfo is CodeAnonymousTypeInfo)
            {
                return string.Empty;
            }

            if (typeInfo is CodeVarTypeInfo)
            {
                return "var";
            }

            if (s.Length == 0)
            {
                return "void";
            }

            string lowerCaseString = s.ToLower(CultureInfo.InvariantCulture).Trim();

            switch (lowerCaseString)
            {
                case "system.int16":
                    return "short";
                case "system.int32":
                    return "int";
                case "system.int64":
                    return "long";
                case "system.string":
                    return "string";
                case "system.object":
                    return "object";
                case "system.boolean":
                    return "bool";
                case "system.void":
                    return "void";
                case "system.char":
                    return "char";
                case "system.byte":
                    return "byte";
                case "system.uint16":
                    return "ushort";
                case "system.uint32":
                    return "uint";
                case "system.uint64":
                    return "ulong";
                case "system.sbyte":
                    return "sbyte";
                case "system.single":
                    return "float";
                case "system.double":
                    return "double";
                case "system.decimal":
                    return "decimal";
            }
        }

        // replace + with . for nested classes.
        //
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
            sb.Append(CreateEscapedIdentifier(baseType.Substring(lastIndex)));

        return sb.ToString();
    }

    private string GetTypeArgumentsOutput(CodeTypeInfo[] typeArguments)
    {
        typeArguments ??= [];
        var sb = new StringBuilder(128);
        GetTypeArgumentsOutput(typeArguments, 0, typeArguments.Length, sb);
        return sb.ToString();
    }

    private void GetTypeArgumentsOutput(IReadOnlyList<CodeTypeInfo> typeArguments, int start, int length, StringBuilder sb)
    {
        typeArguments ??= [];
        sb.Append('<');
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
        sb.Append('>');
    }

    public string GetTypeOutput(CodeTypeInfo typeInfo)
    {
        string s = string.Empty;

        CodeTypeInfo baseTypeInfo = typeInfo;
        while (baseTypeInfo.ArrayElementType != null)
        {
            baseTypeInfo = baseTypeInfo.ArrayElementType;
        }
        s += GetBaseTypeOutput(baseTypeInfo);

        while (typeInfo != null && typeInfo.ArrayRank > 0)
        {
            char[] results = new char[typeInfo.ArrayRank + 1];
            results[0] = '[';
            results[typeInfo.ArrayRank] = ']';
            for (int i = 1; i < typeInfo.ArrayRank; i++)
            {
                results[i] = ',';
            }
            s += new string(results);
            typeInfo = typeInfo.ArrayElementType;
        }

        return s;
    }
}