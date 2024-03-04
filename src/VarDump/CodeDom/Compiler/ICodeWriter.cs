using System.Collections.Generic;
using System;
using VarDump.CodeDom.Common;
using System.IO;

namespace VarDump.CodeDom.Compiler
{
    public interface ICodeWriter
    {
        int Indent { get; set; }
        TextWriter Output { get; }

        void WriteArrayCreate(TypeReference typeReference, IEnumerable<Action> generateInitializers, int size = 0);
        void WriteCast(TypeReference typeReference, Action generateAction);

        void WriteArrayDimension(IEnumerable<Action> initializers);
        void WriteAssign(Action left, Action right);

        void WriteImplicitKeyValuePairCreate(Action generateKeyAction, Action generateValueAction);

        void WriteComment(string comment, bool noNewLine);

        void WriteDefaultValue(TypeReference typeRef);

        void WriteFieldReference(string fieldName, Action generateTargetObjectAction);

        void WriteFlagsBinaryOperator(IEnumerable<Action> generateOperandActions);
        void WriteLambdaExpression(Action generateLambda, Action[] generateParameters);

        void WriteMethodInvoke(Action methodReferenceAction, IEnumerable<Action> parametersActions);

        void WriteMethodReference(Action targetObject, string methodName, params TypeReference[] typeParameters);

        void WriteNamedArgument(string argumentName, Action generateValue);

        void WriteObjectCreateAndInitialize(TypeReference type, IEnumerable<Action> generateParametersActions, IEnumerable<Action> generateInitializeActions);

        void WritePrimitive(object obj);

        void WritePropertyReference(string propertyName, Action targetObjectAction);

        void WriteSeparator();

        void WriteTypeOf(TypeReference typeReference);

        void WriteTypeReference(TypeReference typeReference);

        void WriteValueTupleCreate(IEnumerable<Action> actions);

        void WriteVariableDeclarationStatement(TypeReference typeReference, string variableName, Action initAction);

        void WriteVariableReference(string variableName);
    }
}
