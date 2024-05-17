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

namespace VarDump.CodeDom.VisualBasic;

internal sealed class VisualBasicCodeWriter : ICodeWriter
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


    public VisualBasicCodeWriter(TextWriter w, CodeWriterOptions o)
    {
        _options = o ?? new CodeWriterOptions();
        _output = new ExposedTabStringIndentedTextWriter(w, _options.IndentString);
    }

    public void WriteType(CodeTypeInfo typeInfo) => OutputType(typeInfo);

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

    private void EnsureInDoubleQuotes(ref bool fInDoubleQuotes)
    {
        if (fInDoubleQuotes) return;
        _output.Write("&\"");
        fInDoubleQuotes = true;
    }

    private void EnsureNotInDoubleQuotes(ref bool fInDoubleQuotes)
    {
        if (!fInDoubleQuotes) return;
        _output.Write('\"');
        fInDoubleQuotes = false;
    }

    private void OutputQuoteSnippetString(string value)
    {
        bool fInDoubleQuotes = true;

        _output.Write('\"');

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
                    EnsureInDoubleQuotes(ref fInDoubleQuotes);
                    _output.Write(ch);
                    _output.Write(ch);
                    break;
                case '\r':
                    EnsureNotInDoubleQuotes(ref fInDoubleQuotes);
                    if (i < value.Length - 1 && value[i + 1] == '\n')
                    {
                        _output.Write("&Global.Microsoft.VisualBasic.ChrW(13)&Global.Microsoft.VisualBasic.ChrW(10)");
                        i++;
                    }
                    else
                    {
                        _output.Write("&Global.Microsoft.VisualBasic.ChrW(13)");
                    }
                    break;
                case '\t':
                    EnsureNotInDoubleQuotes(ref fInDoubleQuotes);
                    _output.Write("&Global.Microsoft.VisualBasic.ChrW(9)");
                    break;
                case '\0':
                    EnsureNotInDoubleQuotes(ref fInDoubleQuotes);
                    _output.Write("&Global.Microsoft.VisualBasic.ChrW(0)");
                    break;
                case '\n':
                    EnsureNotInDoubleQuotes(ref fInDoubleQuotes);
                    _output.Write("&Global.Microsoft.VisualBasic.ChrW(10)");
                    break;
                case '\u2028':
                case '\u2029':
                    EnsureNotInDoubleQuotes(ref fInDoubleQuotes);
                    AppendEscapedChar(ch);
                    break;
                default:
                    EnsureInDoubleQuotes(ref fInDoubleQuotes);
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
                if (char.IsHighSurrogate(value[i])
                    && i < value.Length - 1
                    && char.IsLowSurrogate(value[i + 1]))
                {
                    _output.Write(value[++i]);
                }

                if (fInDoubleQuotes)
                    _output.Write('\"');
                fInDoubleQuotes = true;

                _output.Write("& _ ");
                _output.Write(_output.NewLine);
                _output.OutputIndents(Indent + 1);
                _output.Write('\"');
            }
            ++i;
        }

        if (fInDoubleQuotes)
            _output.Write('\"');
    }

    private void AppendEscapedChar(char value)
    {
        _output.Write("&Global.Microsoft.VisualBasic.ChrW(");
        _output.Write(((int)value).ToString(CultureInfo.InvariantCulture));
        _output.Write(")");
    }

    public void WriteNamedArgument(string argumentName, Action value)
    {
        _output.Write(argumentName);
        _output.Write(":=");
        value();
    }

    public void WriteAssign(Action left, Action right)
    {
        left();
        _output.Write(" = ");
        right();
    }

    public void WriteDefaultValue(CodeTypeInfo typeInfo)
    {
        _output.Write("CType(Nothing, ");
        TypeOutput(typeInfo);
        _output.Write(")");
    }

    private void OutputBitwiseOrOperator()
    {
        _output.Write("Or");
    }

    private void OutputIdentifier(string ident)
    {
        _output.Write(VisualBasicHelpers.CreateEscapedIdentifier(ident));
    }

    public void OutputType(CodeTypeInfo typeInfo)
    {
        OutputTypeWithoutArrayPostFix(typeInfo);
    }

    private void OutputTypeNamePair(CodeTypeInfo typeInfo, string name)
    {
        if (string.IsNullOrEmpty(name))
            name = "__exception";

        OutputIdentifier(name);
        OutputArrayPostfix(typeInfo);
        if (typeInfo is not CodeEmptyTypeInfo)
        {
            _output.Write(" As ");
            OutputType(typeInfo);
        }
    }

    private void OutputArrayPostfixInternal(CodeTypeInfo typeInfo)
    {
        if (typeInfo.ArrayElementType != null)
        {
            // Recurse up
            OutputArrayPostfixInternal(typeInfo.ArrayElementType);
        }

        if (typeInfo.ArrayRank > 0)
        {
            _output.Write('(');
            for (int i = 1; i < typeInfo.ArrayRank; i++)
            {
                _output.Write(',');
            }
            _output.Write(')');
        }
    }

    private void OutputArrayPostfixInternal(CodeTypeInfo typeInfo, int size)
    {
        if (typeInfo.ArrayElementType != null)
        {
            // Recurse up
            OutputArrayPostfixInternal(typeInfo.ArrayElementType, size);
        }

        if (typeInfo.ArrayRank > 0)
        {
            _output.Write('(');
            _output.Write(size);
            for (int i = 1; i < typeInfo.ArrayRank; i++)
            {
                _output.Write(", ");
                _output.Write(size);
            }
            _output.Write(')');
        }
    }

    private void OutputArrayPostfix(CodeTypeInfo typeInfo)
    {
        if (typeInfo.ArrayRank > 0)
        {
            OutputArrayPostfixInternal(typeInfo);
        }
    }

    public void WritePrimitive(object obj, IntegralNumericFormat numericFormat)
    {
        switch (obj)
        {
            case char:
                _output.Write("Global.Microsoft.VisualBasic.ChrW(");
                _output.Write(((IConvertible)obj).ToInt32(CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture));
                _output.Write(')');
                break;
            case sbyte @sbyte:
                _output.Write("CSByte(");
                _output.Write($"{GetPrefix(numericFormat)}{NumericUtil.ToString(@sbyte, numericFormat)}");
                _output.Write(')');
                break;
            case ushort @ushort:
                _output.Write($"{GetPrefix(numericFormat)}{NumericUtil.ToString(@ushort, numericFormat)}");
                _output.Write("US");
                break;
            case uint u:
                _output.Write($"{GetPrefix(numericFormat)}{NumericUtil.ToString(u, numericFormat)}");
                _output.Write("UI");
                break;
            case ulong @ulong:
                _output.Write($"{GetPrefix(numericFormat)}{NumericUtil.ToString(@ulong, numericFormat)}");
                _output.Write("UL");
                break;
            default:
                DefaultWritePrimitiveExpression(obj, numericFormat);
                break;
        }
    }

    private static string GetPrefix(IntegralNumericFormat numericFormat)
    {
        return numericFormat.Format switch
        {
            NumericFormat.Binary => "&B",
            NumericFormat.Decimal => "",
            NumericFormat.HexadecimalLowerCase => "&h",
            NumericFormat.HexadecimalUpperCase => "&H",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void DefaultWritePrimitiveExpression(object obj, IntegralNumericFormat numericFormat)
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

    public void WriteArrayCreate(CodeTypeInfo typeInfo, IEnumerable<Action> initializers, bool singleLine, int size = 0)
    {
        if (typeInfo is not CodeEmptyTypeInfo)
        {
            _output.Write("New ");
        }

        using var initializersEnumerator = initializers.GetEnumerator();

        if (initializersEnumerator.MoveNext())
        {
            if (typeInfo is not CodeEmptyTypeInfo)
            {
                TypeOutput(typeInfo);
            }

            if (singleLine)
            {
                _output.Write("{ ");
                OutputActions(initializersEnumerator, newlineBetweenItems: false, newLineContinuation: false);
                _output.Write(" }");
            }
            else
            {
                _output.Write("{");
                _output.WriteLine("");
                OutputActions(initializersEnumerator, newlineBetweenItems: true, newLineContinuation: false);
                _output.WriteLine("");
                _output.Write('}');
            }
        }
        else
        {
            OutputTypeWithoutArrayPostFix(typeInfo);
            OutputArrayPostfixInternal(typeInfo, size);

            _output.Write(" {}");
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
                    _output.Write(", ");
            }

            actions.Current?.Invoke();
        } while (actions.MoveNext());
        Indent--;
    }

    public void WriteArrayDimension(IEnumerable<Action> initializers, bool singleLine = false)
    {
        if (singleLine)
        {
            _output.Write("{ ");
            OutputActions(initializers, newlineBetweenItems: false, newLineContinuation: false);
            _output.Write(" }");
        }
        else
        {
            _output.Write("{");
            _output.WriteLine();
            OutputActions(initializers, newlineBetweenItems: true, newLineContinuation: false);
            _output.WriteLine();
            _output.Write("}");
        }
    }

    public void WriteCast(CodeTypeInfo typeInfo, Action action)
    {
        _output.Write("CType(");
        action();
        _output.Write(", ");
        OutputType(typeInfo);
        OutputArrayPostfix(typeInfo);
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

    private void OutputFloatValue(float s)
    {
        if (float.IsNaN(s))
        {
            _output.Write("Single.NaN");
        }
        else if (float.IsNegativeInfinity(s))
        {
            _output.Write("Single.NegativeInfinity");
        }
        else if (float.IsPositiveInfinity(s))
        {
            _output.Write("Single.PositiveInfinity");
        }
        else
        {
            _output.Write(s.ToString(CultureInfo.InvariantCulture));
            _output.Write('!');
        }
    }

    private void OutputDoubleValue(double d)
    {
        if (double.IsNaN(d))
        {
            _output.Write("Double.NaN");
        }
        else if (double.IsNegativeInfinity(d))
        {
            _output.Write("Double.NegativeInfinity");
        }
        else if (double.IsPositiveInfinity(d))
        {
            _output.Write("Double.PositiveInfinity");
        }
        else
        {
            _output.Write(d.ToString("R", CultureInfo.InvariantCulture));
            // always mark a double as being a double in case we have no decimal portion (e.g write 1D instead of 1 which is an int)
            _output.Write('R');
        }
    }

    private void OutputDecimalValue(decimal d)
    {
        _output.Write(d.ToString(CultureInfo.InvariantCulture));
        _output.Write('D');
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
        _output.Write("New ");
        OutputType(typeInfo);

        using var parametersEnumerator = parametersActions.GetEnumerator();

        // always write out the () to disambiguate cases like "New System.Random().Next(x,y)"
        _output.Write('(');
        if (parametersEnumerator.MoveNext())
        {
            OutputActions(parametersEnumerator, newlineBetweenItems: false);
        }
        _output.Write(')');
    }

    public void WriteObjectCreateAndInitialize(CodeTypeInfo typeInfo, IEnumerable<Action> parametersActions, IEnumerable<Action> initializeActions)
    {
        _output.Write("New ");
        OutputType(typeInfo);

        using var parametersEnumerator = parametersActions.GetEnumerator();
        using var initializeEnumerator = initializeActions.GetEnumerator();

        var parametersExist = parametersEnumerator.MoveNext();
        var initializeExist = initializeEnumerator.MoveNext();

        if (parametersExist || !initializeExist)
        {
            // always write out the () to disambiguate cases like "New System.Random().Next(x,y)"
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

        _output.Write(typeInfo switch
        {
            CodeEmptyTypeInfo => "With ",
            CodeCollectionTypeInfo => " From ",
            _ => " With "
        });

        _output.WriteLine('{');
        OutputActions(initializeEnumerator, newlineBetweenItems: true, newLineContinuation: false);
        _output.WriteLine();
        _output.Write('}');
    }

    public void WriteValueTupleCreate(IEnumerable<Action> actions)
    {
        _output.Write('(');
        OutputActions(actions, newlineBetweenItems: false);
        _output.Write(')');
    }

    public void WriteImplicitKeyValuePairCreate(Action keyAction, Action valueAction)
    {
        _output.WriteLine('{');
        OutputActions([keyAction, valueAction], newlineBetweenItems: true, false);
        _output.WriteLine();
        _output.Write('}');
    }

    public void WriteSeparator()
    {
        _output.Write(", ");
    }

    public void WriteLambdaExpression(Action lambda, Action[] parameters)
    {
        _output.Write("Function (");
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

        _output.Write(')');
        _output.Write(' ');
        lambda();
    }

    public void WriteComment(string comment, bool noNewLine)
    {
        const char commentLineStart = '\'';
        _output.Write(commentLineStart);
        string value = comment;
        for (int i = 0; i < value.Length; i++)
        {
            _output.Write(value[i]);

            if (value[i] == '\r')
            {
                if (i < value.Length - 1 && value[i + 1] == '\n')
                { // if next char is '\n', skip it
                    _output.Write('\n');
                    i++;
                }
                ((ExposedTabStringIndentedTextWriter)Output).OutputIndents();
                _output.Write(commentLineStart);
            }
            else if (value[i] == '\n')
            {
                ((ExposedTabStringIndentedTextWriter)Output).OutputIndents();
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
        _output.Write("Dim ");

        OutputTypeNamePair(typeInfo, variableName);

        if (initAction != null)
        {
            _output.Write(" = ");
            initAction();
        }

        _output.WriteLine();
    }

    public void WritePropertyReference(string propertyName, Action targetObjectAction)
    {
        targetObjectAction?.Invoke();
        _output.Write('.');
        OutputIdentifier(propertyName);
    }

    public void WriteTypeOf(CodeTypeInfo typeInfo)
    {
        _output.Write("GetType(");
        TypeOutput(typeInfo);
        _output.Write(')');
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

        if (typeInfo is CodeEmptyTypeInfo)
        {
            return;
        }

        if (baseType.Length == 0)
        {
            _output.Write("Void");
            return;
        }

        if (preferBuiltInTypes)
        {
            switch (baseType)
            {
                case "System.Byte":
                    _output.Write("Byte");
                    return;
                case "System.SByte":
                    _output.Write("SByte");
                    return;
                case "System.Int16":
                    _output.Write("Short");
                    return;
                case "System.Int32":
                    _output.Write("Integer");
                    return;
                case "System.Int64":
                    _output.Write("Long");
                    return;
                case "System.UInt16":
                    _output.Write("UShort");
                    return;
                case "System.UInt32":
                    _output.Write("UInteger");
                    return;
                case "system.uint64":
                    _output.Write("ULong");
                    return;
                case "System.String":
                    _output.Write("String");
                    return;
                case "System.DateTime":
                    _output.Write("Date");
                    return;
                case "System.Decimal":
                    _output.Write("Decimal");
                    return;
                case "System.Single":
                    _output.Write("Single");
                    return;
                case "System.Double":
                    _output.Write("Double");
                    return;
                case "System.Boolean":
                    _output.Write("Boolean");
                    return;
                case "System.Char":
                    _output.Write("Char");
                    return;
                case "System.Object":
                    _output.Write("Object");
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
                    _output.Write(VisualBasicHelpers.CreateEscapedIdentifier(baseType.Substring(lastIndex, i - lastIndex)));
                    _output.Write('.');
                    i++;
                    lastIndex = i;
                    break;

                case '`':
                    _output.Write(VisualBasicHelpers.CreateEscapedIdentifier(baseType.Substring(lastIndex, i - lastIndex)));
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
        {
            _output.Write(VisualBasicHelpers.CreateEscapedIdentifier(baseType.Substring(lastIndex)));
        }
    }

    private void OutputTypeWithoutArrayPostFix(CodeTypeInfo typeInfo)
    {
        while (typeInfo.ArrayElementType != null)
        {
            typeInfo = typeInfo.ArrayElementType;
        }

        BaseTypeOutput(typeInfo);
    }

    private void OutputTypeArguments(CodeTypeInfo[] typeArguments)
    {
        OutputTypeArguments(typeArguments, 0, typeArguments.Length);
    }

    private void OutputTypeArguments(IReadOnlyList<CodeTypeInfo> typeArguments, int start, int length)
    {
        typeArguments ??= [];
        _output.Write("(Of ");
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

            // it's possible that we call GetTypeArgumentsOutput with an empty typeArguments collection.  This is the case
            // for open types, so we want to just output the brackets and commas. 
            if (i < typeArguments.Count)
                TypeOutput(typeArguments[i]);
        }
        _output.Write(')');
    }

    private void TypeOutput(CodeTypeInfo typeInfo)
    {
        OutputTypeWithoutArrayPostFix(typeInfo);

        if (typeInfo.ArrayRank > 0)
        {
            OutputArrayPostfix(typeInfo);
        }
    }

    public void ContinueOnNewLine(string st, bool newLineContinuation = true)
    {
        _output.Write(st);
        _output.WriteLine(newLineContinuation ? " _" : "");
    }
}