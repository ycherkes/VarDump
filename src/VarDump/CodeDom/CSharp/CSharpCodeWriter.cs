// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.CodeDom.Resources;
using VarDump.Utils;
using VarDump.Visitor;

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

    private void QuoteSnippetStringCStyle(string value)
    {
        _output.Write('\"');
        int i = 0;
        while (i < value.Length)
        {
            switch (value[i])
            {
                case '\r':
                    _output.Write("\\r");
                    break;

                case '\t':
                    _output.Write("\\t");
                    break;

                case '\"':
                    _output.Write("\\\"");
                    break;

                case '\'':
                    _output.Write("\\\'");
                    break;

                case '\\':
                    _output.Write("\\\\");
                    break;

                case '\0':
                    _output.Write("\\0");
                    break;

                case '\n':
                    _output.Write("\\n");
                    break;

                case '\u2028':
                case '\u2029':
                    OutputEscapedChar(value[i]);
                    break;

                default:
                    _output.Write(value[i]);
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
                if (char.IsHighSurrogate(value[i]) && i < value.Length - 1 && char.IsLowSurrogate(value[i + 1]))
                {
                    _output.Write(value[++i]);
                }

                _output.Write("\" +");
                _output.Write(_output.NewLine);
                _output.OutputIndents(Indent + 1);
                _output.Write('\"');
            }
            ++i;
        }

        _output.Write('\"');
    }

    private void QuoteSnippetStringVerbatimStyle(string value)
    {
        _output.Write("@\"");

        for (var i = 0; i < value.Length; i++)
        {
            if (value[i] == '\"')
                _output.Write("\"\"");
            else
                _output.Write(value[i]);
        }

        _output.Write('\"');
    }

    private void OutputQuoteSnippetString(string value)
    {
        // If the string is short, use C style quoting (e.g "\r\n")
        // Also do it if it is too long to fit in one line
        // If the string contains '\0', verbatim style won't work.
        if (value.Length < 256 || value.Length > 1500 || value.IndexOf('\0') != -1)
        {
            QuoteSnippetStringCStyle(value);
        }
        else
        {
            // Otherwise, use 'verbatim' style quoting (e.g. @"foo")
            QuoteSnippetStringVerbatimStyle(value);
        }
    }

    private void ContinueOnNewLine(string st) => _output.WriteLine(st);

    private void OutputIdentifier(string ident) => _output.Write(CSharpHelpers.CreateEscapedIdentifier(ident));

    public void WriteArrayCreate(CodeTypeInfo typeInfo, IEnumerable<Action> initializers, bool singleLine, int size = 0)
    {
        _output.Write("new ");

        using var initializersEnumerator = initializers.GetEnumerator();

        if (initializersEnumerator.MoveNext())
        {
            if (singleLine)
            {
                OutputType(typeInfo);
                _output.Write("{ ");
                OutputActions(initializersEnumerator, newlineBetweenItems: false);
                _output.Write(" }");
            }
            else
            {
                OutputType(typeInfo);
                _output.WriteLine("");
                _output.WriteLine("{");
                OutputActions(initializersEnumerator, newlineBetweenItems: true);
                _output.WriteLine();
                _output.Write("}");
            }
        }
        else
        {
            BaseTypeOutput(typeInfo);

            _output.Write('[');
            _output.Write(size);
            for (int i = 0; i < typeInfo.ArrayRank - 1; i++)
            {
                _output.Write(", ");
                _output.Write(size);
            }
            _output.Write(']');

            int nestedArrayDepth = typeInfo.NestedArrayDepth;
            for (int i = 0; i < nestedArrayDepth - 1; i++)
            {
                _output.Write("[]");
            }
        }
    }

    public void WriteArrayDimension(IEnumerable<Action> initializers, bool singleLine = false)
    {
        if (singleLine)
        {
            _output.Write("{ ");
            OutputActions(initializers, newlineBetweenItems: false);
            _output.Write(" }");
        }
        else
        {
            _output.Write("{");
            _output.WriteLine();
            OutputActions(initializers, newlineBetweenItems: true);
            _output.WriteLine();
            _output.Write("}");
        }
    }

    public void WriteCast(CodeTypeInfo typeInfo, Action action)
    {
        _output.Write("(");
        OutputType(typeInfo);
        _output.Write(")");
        action();
    }

    public void WriteAssign(Action left, Action right)
    {
        left();
        _output.Write(" = ");
        right();
    }

    public void WriteDefaultValue(CodeTypeInfo typeInfo)
    {
        _output.Write("default(");
        OutputType(typeInfo);
        _output.Write(')');
    }

    public void WriteFieldReference(string fieldName, Action targetObjectAction)
    {
        if (targetObjectAction != null)
        {
            targetObjectAction();
            _output.Write('.');
        }
        OutputIdentifier(fieldName);
    }

    public void WriteVariableReference(string variableName) => OutputIdentifier(variableName);

    public void WriteMethodInvoke(Action methodReferenceAction, IEnumerable<Action> parametersActions)
    {
        methodReferenceAction();
        _output.Write('(');
        OutputActions(parametersActions, newlineBetweenItems: false);
        _output.Write(')');
    }

    public void WriteMethodReference(Action targetObject, string methodName, params CodeTypeInfo[] typeParameters)
    {
        if (targetObject != null)
        {
            targetObject();
            _output.Write('.');
        }
        OutputIdentifier(methodName);

        if (typeParameters.Length > 0)
        {
            OutputTypeArguments(typeParameters);
        }
    }

    public void WriteObjectCreate(CodeTypeInfo typeInfo, IEnumerable<Action> parametersActions)
    {
        _output.Write("new ");
        OutputType(typeInfo);

        using var parametersEnumerator = parametersActions.GetEnumerator();

        _output.Write('(');
        if (parametersEnumerator.MoveNext())
        {
            OutputActions(parametersEnumerator, newlineBetweenItems: false);
        }
        _output.Write(')');
    }

    public void WriteObjectCreateAndInitialize(CodeTypeInfo typeInfo, IEnumerable<Action> parametersActions, IEnumerable<Action> initializeActions, bool singleLine = false)
    {
        _output.Write("new ");
        OutputType(typeInfo);

        using var parametersEnumerator = parametersActions.GetEnumerator();
        using var initializeEnumerator = initializeActions.GetEnumerator();

        var parametersExist = parametersEnumerator.MoveNext();
        var initializeExist = initializeEnumerator.MoveNext();

        if (parametersExist || !initializeExist)
        {
            _output.Write('(');
            if (parametersExist)
            {
                OutputActions(parametersEnumerator, newlineBetweenItems: false);
            }
            _output.Write(')');
        }

        if (!initializeExist)
        {
            return;
        }

        if (singleLine)
        {
            _output.Write(" { ");
            OutputActions(initializeEnumerator, newlineBetweenItems: false);
            _output.Write(" }");
        }
        else
        {
            _output.WriteLine();
            _output.WriteLine('{');
            OutputActions(initializeEnumerator, newlineBetweenItems: true);
            _output.WriteLine();
            _output.Write("}");
        }
    }

    public void WriteValueTupleCreate(IEnumerable<Action> actions)
    {
        _output.Write('(');
        OutputActions(actions, newlineBetweenItems: false);
        _output.Write(')');
    }

    public void WritePrimitive(object obj, IntegralNumericFormat numericFormat)
    {
        if (obj is char c)
        {
            OutputPrimitiveChar(c);
        }
        else if (obj is sbyte @sbyte)
        {
            // C# has no literal marker for types smaller than Int32
            _output.Write($"{GetPrefix(numericFormat)}{NumericUtil.ToString(@sbyte, numericFormat)}");
        }
        else if (obj is ushort @ushort)
        {
            // C# has no literal marker for types smaller than Int32, and you will
            // get a conversion error if you use "u" here.
            _output.Write($"{GetPrefix(numericFormat)}{NumericUtil.ToString(@ushort, numericFormat)}");
        }
        else if (obj is uint u)
        {
            _output.Write($"{GetPrefix(numericFormat)}{NumericUtil.ToString(u, numericFormat)}");
            _output.Write('u');
        }
        else if (obj is ulong @ulong)
        {
            _output.Write($"{GetPrefix(numericFormat)}{NumericUtil.ToString(@ulong, numericFormat)}");
            _output.Write("ul");
        }
        else
        {
            OutputPrimitiveExpressionBase(obj, numericFormat);
        }
    }

    private static string GetPrefix(IntegralNumericFormat numericFormat)
    {
        return numericFormat.Format switch
        {
            NumericFormat.Binary => "0b",
            NumericFormat.Decimal => "",
            NumericFormat.HexadecimalLowerCase => "0x",
            NumericFormat.HexadecimalUpperCase => "0X",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void OutputPrimitiveExpressionBase(object obj, IntegralNumericFormat numericFormat)
    {
        switch (obj)
        {
            case null:
                _output.Write(NullToken);
                break;

            case string s:
                OutputQuoteSnippetString(s);
                break;

            case char:
                _output.Write('\'');
                _output.Write(obj);
                _output.Write('\'');
                break;

            case byte b:
                _output.Write($"{GetPrefix(numericFormat)}{NumericUtil.ToString(b, numericFormat)}");
                break;

            case short s1:
                _output.Write($"{GetPrefix(numericFormat)}{NumericUtil.ToString(s1, numericFormat)}");
                break;

            case int i:
                _output.Write($"{GetPrefix(numericFormat)}{NumericUtil.ToString(i, numericFormat)}");
                break;

            case long l:
                _output.Write($"{GetPrefix(numericFormat)}{NumericUtil.ToString(l, numericFormat)}");
                break;

            case float f:
                OutputFloatValue(f);
                break;

            case double d:
                OutputDoubleValue(d);
                break;

            case decimal @decimal:
                OutputDecimalValue(@decimal);
                break;

            case bool b1:
                _output.Write(b1 ? "true" : "false");
                break;

            default:
                throw new ArgumentException(string.Format(SR.InvalidPrimitiveType, obj.GetType()));
        }
    }

    private void OutputPrimitiveChar(char c)
    {
        _output.Write('\'');
        switch (c)
        {
            case '\r':
                _output.Write("\\r");
                break;

            case '\t':
                _output.Write("\\t");
                break;

            case '\"':
                _output.Write("\\\"");
                break;

            case '\'':
                _output.Write("\\\'");
                break;

            case '\\':
                _output.Write("\\\\");
                break;

            case '\0':
                _output.Write("\\0");
                break;

            case '\n':
                _output.Write("\\n");
                break;

            case '\u2028':
            case '\u2029':
            case '\u0084':
            case '\u0085':
                OutputEscapedChar(c);
                break;

            default:
                if (char.IsSurrogate(c))
                {
                    OutputEscapedChar(c);
                }
                else
                {
                    _output.Write(c);
                }
                break;
        }
        _output.Write('\'');
    }

    private void OutputEscapedChar(char value)
    {
        _output.Write("\\u");
        _output.Write(((int)value).ToString("X4", CultureInfo.InvariantCulture));
    }

    public void WriteComment(string comment, bool noNewLine)
    {
        const string commentLineStart = "//";
        _output.Write(commentLineStart);
        _output.Write(' ');

        string value = comment;
        for (int i = 0; i < value.Length; i++)
        {
            if (value[i] == '\u0000')
            {
                continue;
            }

            _output.Write(value[i]);

            if (value[i] == '\r')
            {
                if (i < value.Length - 1 && value[i + 1] == '\n')
                {
                    // if next char is '\n', skip it
                    _output.Write('\n');
                    i++;
                }

                _output.OutputIndents();
                _output.Write(commentLineStart);
            }
            else if (value[i] == '\n')
            {
                _output.OutputIndents();
                _output.Write(commentLineStart);
            }
            else if (value[i] == '\u2028' || value[i] == '\u2029' || value[i] == '\u0085')
            {
                _output.Write(commentLineStart);
            }
        }
        if (!noNewLine)
        {
            _output.WriteLine();
        }
    }

    public void WriteVariableDeclarationStatement(CodeTypeInfo typeInfo, string variableName, Action initAction)
    {
        OutputTypeNamePair(typeInfo, variableName);
        if (initAction != null)
        {
            _output.Write(" = ");
            initAction();
        }
        _output.WriteLine(';');
    }

    public void WriteNamedArgument(string argumentName, Action value)
    {
        _output.Write(argumentName);
        _output.Write(": ");
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
                _output.Write(' ');
                OutputBitwiseOrOperator();
                _output.Write(' ');
                operand();
            }
        }
    }

    public void WriteSeparator()
    {
        _output.Write(", ");
    }

    public void WriteImplicitKeyValuePairCreate(Action keyAction, Action valueAction)
    {
        _output.WriteLine('{');
        OutputActions([keyAction, valueAction], newlineBetweenItems: true);
        _output.WriteLine();
        _output.Write('}');
    }

    public void WriteLambdaExpression(Action lambda, Action[] parameters)
    {
        if (parameters.Length != 1)
        {
            _output.Write('(');
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
                _output.Write(", ");
            }
            current();
        }

        if (parameters.Length != 1)
        {
            _output.Write(')');
        }
        _output.Write(" => ");
        lambda();
    }

    private void OutputFloatValue(float s)
    {
        if (float.IsNaN(s))
        {
            _output.Write("float.NaN");
        }
        else if (float.IsNegativeInfinity(s))
        {
            _output.Write("float.NegativeInfinity");
        }
        else if (float.IsPositiveInfinity(s))
        {
            _output.Write("float.PositiveInfinity");
        }
        else
        {
            _output.Write(s.ToString(CultureInfo.InvariantCulture));
            _output.Write('F');
        }
    }

    private void OutputDoubleValue(double d)
    {
        if (double.IsNaN(d))
        {
            _output.Write("double.NaN");
        }
        else if (double.IsNegativeInfinity(d))
        {
            _output.Write("double.NegativeInfinity");
        }
        else if (double.IsPositiveInfinity(d))
        {
            _output.Write("double.PositiveInfinity");
        }
        else
        {
            _output.Write(d.ToString("R", CultureInfo.InvariantCulture));
            // always mark a double as being a double in case we have no decimal portion (e.q. write 1D instead of 1 which is an int)
            _output.Write('D');
        }
    }

    private void OutputDecimalValue(decimal d)
    {
        _output.Write(d.ToString(CultureInfo.InvariantCulture));
        _output.Write('m');
    }

    private void OutputBitwiseOrOperator()
    {
        _output.Write('|');
    }

    public void WritePropertyReference(string propertyName, Action targetObjectAction)
    {
        if (targetObjectAction != null)
        {
            targetObjectAction();
            _output.Write('.');
        }
        OutputIdentifier(propertyName);
    }

    public void WriteType(CodeTypeInfo typeInfo) => OutputType(typeInfo);

    public void WriteTypeOf(CodeTypeInfo typeInfo)
    {
        _output.Write("typeof(");
        OutputType(typeInfo);
        _output.Write(')');
    }

    private void OutputActions(IEnumerable<Action> actions, bool newlineBetweenItems)
    {
        using var enumerator = actions.GetEnumerator();
        if (enumerator.MoveNext())
        {
            OutputActions(enumerator, newlineBetweenItems);
        }
    }

    private void OutputActions(IEnumerator<Action> actions, bool newlineBetweenItems)
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
                    _output.Write(", ");
            }
            actions.Current?.Invoke();
        } while (actions.MoveNext());
        Indent--;
    }

    private void OutputTypeNamePair(CodeTypeInfo typeInfo, string name)
    {
        OutputType(typeInfo);
        _output.Write(' ');
        OutputIdentifier(name);
    }

    private void OutputTypeArguments(CodeTypeInfo[] typeArguments)
    {
        OutputTypeArguments(typeArguments, 0, typeArguments.Length);
    }

    // outputs the type name without any array declaration.
    private void OutputTypeArguments(IReadOnlyList<CodeTypeInfo> typeArguments, int start, int length)
    {
        typeArguments ??= [];
        _output.Write('<');
        bool first = true;
        for (int i = start; i < start + length; i++)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                _output.Write(", ");
            }

            // it's possible that we call WriteTypeArgumentsOutput with an empty typeArguments collection.  This is the case
            // for open types, so we want to just output the brackets and commas.
            if (i < typeArguments.Count)
                OutputType(typeArguments[i]);
        }
        _output.Write('>');
    }

    private void OutputType(CodeTypeInfo typeInfo)
    {
        if (typeInfo == null)
            throw new ArgumentNullException(nameof(typeInfo));

        BaseTypeOutput(typeInfo);

        while (typeInfo?.ArrayRank > 0)
        {
            _output.Write('[');
            _output.Write(new string(',', typeInfo.ArrayRank - 1));
            _output.Write(']');
            typeInfo = typeInfo.ArrayElementType;
        }
    }

    private void BaseTypeOutput(CodeTypeInfo typeInfo, bool preferBuiltInTypes = true)
    {
        string baseType = typeInfo.BaseType;

        if (baseType == "System.Nullable`1" && typeInfo.TypeArguments.Count > 0)
        {
            BaseTypeOutput(typeInfo.TypeArguments[0]);
            _output.Write('?');
            return;
        }

        switch (typeInfo)
        {
            case CodeAnonymousTypeInfo:
                return;

            case CodeVarTypeInfo:
                _output.Write("var");
                return;
        }

        if (baseType.Length == 0)
        {
            _output.Write("void");
            return;
        }

        if (preferBuiltInTypes)
        {
            switch (baseType.Trim())
            {
                case "System.Int16":
                    _output.Write("short");
                    return;

                case "System.Int32":
                    _output.Write("int");
                    return;

                case "System.Int64":
                    _output.Write("long");
                    return;

                case "System.String":
                    _output.Write("string");
                    return;

                case "System.Object":
                    _output.Write("object");
                    return;

                case "System.Boolean":
                    _output.Write("bool");
                    return;

                case "System.Void":
                    _output.Write("void");
                    return;

                case "System.Char":
                    _output.Write("char");
                    return;

                case "System.Byte":
                    _output.Write("byte");
                    return;

                case "System.UInt16":
                    _output.Write("ushort");
                    return;

                case "System.UInt32":
                    _output.Write("uint");
                    return;

                case "System.UInt64":
                    _output.Write("ulong");
                    return;

                case "System.SByte":
                    _output.Write("sbyte");
                    return;

                case "System.Single":
                    _output.Write("float");
                    return;

                case "System.Double":
                    _output.Write("double");
                    return;

                case "System.Decimal":
                    _output.Write("decimal");
                    return;
            }
        }

        if (!_options.UseFullTypeName)
        {
            var lastIndex0 = baseType.LastIndexOfAny(['.', '+']);
            if (lastIndex0 >= 0)
            {
                baseType = baseType.Substring(lastIndex0 + 1);
            }
        }

        int lastIndex = 0;
        int currentTypeArgStart = 0;
        for (int i = 0; i < baseType.Length; i++)
        {
            switch (baseType[i])
            {
                case '+':
                case '.':
                    _output.Write(CSharpHelpers.CreateEscapedIdentifier(baseType.Substring(lastIndex, i - lastIndex)));
                    _output.Write('.');
                    i++;
                    lastIndex = i;
                    break;

                case '`':
                    _output.Write(CSharpHelpers.CreateEscapedIdentifier(baseType.Substring(lastIndex, i - lastIndex)));
                    i++;    // skip the '
                    int numTypeArgs = 0;
                    while (i < baseType.Length && baseType[i] >= '0' && baseType[i] <= '9')
                    {
                        numTypeArgs = numTypeArgs * 10 + (baseType[i] - '0');
                        i++;
                    }

                    OutputTypeArguments(typeInfo.TypeArguments, currentTypeArgStart, numTypeArgs);
                    currentTypeArgStart += numTypeArgs;

                    // Arity can be in the middle of a nested type name, so we might have a . or + after it.
                    // Skip it if so.
                    if (i < baseType.Length && (baseType[i] == '+' || baseType[i] == '.'))
                    {
                        _output.Write('.');
                        i++;
                    }

                    lastIndex = i;
                    break;
            }
        }

        if (lastIndex < baseType.Length)
            _output.Write(CSharpHelpers.CreateEscapedIdentifier(baseType.Substring(lastIndex)));
    }
}