using System.Collections.Generic;
using System;
using VarDump.CodeDom.Common;
using System.IO;

namespace VarDump.CodeDom.Compiler;

public interface ICodeWriter
{
    int Indent { get; set; }
    TextWriter Output { get; }

    void WriteArrayCreate(CodeTypeInfo typeInfo, IEnumerable<Action> initializers, int size = 0);
    void WriteCast(CodeTypeInfo typeInfo, Action action);

    void WriteArrayDimension(IEnumerable<Action> initializers);
    void WriteAssign(Action left, Action right);

    void WriteImplicitKeyValuePairCreate(Action keyAction, Action valueAction);

    void WriteComment(string comment, bool noNewLine);

    void WriteDefaultValue(CodeTypeInfo typeInfo);

    void WriteFieldReference(string fieldName, Action targetObjectAction);

    void WriteFlagsBitwiseOrOperator(IEnumerable<Action> operandActions);
    void WriteLambdaExpression(Action lambda, Action[] parameters);

    void WriteMethodInvoke(Action methodReferenceAction, IEnumerable<Action> parametersActions);

    void WriteMethodReference(Action targetObject, string methodName, params CodeTypeInfo[] typeParameters);

    void WriteNamedArgument(string argumentName, Action value);

    void WriteObjectCreateAndInitialize(CodeTypeInfo typeInfo, IEnumerable<Action> parametersActions, IEnumerable<Action> initializeActions);

    void WriteObjectCreate(CodeTypeInfo typeInfo, IEnumerable<Action> parametersActions);

    void WritePrimitive(object obj);

    void WritePropertyReference(string propertyName, Action targetObjectAction);

    void WriteSeparator();

    void WriteTypeOf(CodeTypeInfo typeInfo);

    void WriteType(CodeTypeInfo typeInfo);

    void WriteValueTupleCreate(IEnumerable<Action> actions);

    void WriteVariableDeclarationStatement(CodeTypeInfo typeInfo, string variableName, Action initAction);

    void WriteVariableReference(string variableName);
}