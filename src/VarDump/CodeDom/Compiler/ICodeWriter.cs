using System.Collections;
using System.Collections.Generic;
using System;
using VarDump.CodeDom.Common;

namespace VarDump.CodeDom.Compiler;

public interface ICodeWriter
{
    int Indent { get; set; }
    bool SupportsCollectionExpression { get; }

    void WriteArrayCreateItems(CodeTypeInfo typeInfo, IEnumerable items, Action<object> writeItem, bool singleLine, int size = 0);
    void WriteDictionaryCreateItems(CodeTypeInfo typeInfo, IEnumerable items, Action<object> writeItem);
    void WriteCast(CodeTypeInfo typeInfo, Action action);

    void WriteArrayDimensionItems(IEnumerable items, Action<object> writeItem, bool singleLine = false);
    void WriteCollectionExpressionItems(IEnumerable items, Action<object> writeItem, bool singleLine = false);
    void WriteMemberAssignmentStart(string memberName);

    void WriteImplicitKeyValuePairCreate(Action keyAction, Action valueAction);

    void WriteComment(string comment, bool noNewLine);

    void WriteDefaultValue(CodeTypeInfo typeInfo);

    void WriteFieldReference(string fieldName, Action targetObjectAction);

    void WriteFlagsBitwiseOrOperator(IEnumerable<Action> operandActions);
    void WriteLambdaExpression(Action lambda, Action[] parameters);

    void WriteMethodInvoke(Action methodReferenceAction, IEnumerable<Action> parametersActions);

    void WriteMethodReference(Action targetObject, string methodName, params CodeTypeInfo[] typeParameters);

    void WriteNamedArgument(string argumentName, Action value);

    void WriteObjectCreateAndInitializeItems(CodeTypeInfo typeInfo, IEnumerable<Action> parametersActions, IEnumerable initializers, Action<object> writeInitializer, bool singleLine = false);

    void WriteObjectCreate(CodeTypeInfo typeInfo, IEnumerable<Action> parametersActions);

    void WritePrimitive(object obj, string numericFormat = "D");

    void WritePropertyReference(string propertyName, Action targetObjectAction);

    void WriteSeparator();

    void WriteTypeOf(CodeTypeInfo typeInfo);

    void WriteType(CodeTypeInfo typeInfo);

    void WriteValueTupleCreate(IEnumerable<Action> actions);

    void WriteVariableDeclarationStatement(CodeTypeInfo typeInfo, string variableName, Action initAction);

    void WriteVariableReference(string variableName);
}
