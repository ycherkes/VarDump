// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System;
using VarDump.CodeDom.Common;

namespace VarDump.CodeDom.Compiler
{
    public interface ICodeGenerator
    {
        void GenerateArrayCreate(CodeTypeReference typeReference, IEnumerable<Action> generateInitializers, int size = 0);
        void GenerateCodeArrayDimension(IEnumerable<Action> initializers);
        void GenerateFlagsBinaryOperator(CodeBinaryOperatorType @operator, IEnumerable<Action> generateOperandActions);
        void GenerateCast(CodeTypeReference typeReference, Action generateAction);
        void GenerateFieldReference(string fieldName, Action generateTargetObjectAction);
        void GenerateVariableReference(string variableName);
        void GenerateMethodInvoke(Action methodReferenceAction, IEnumerable<Action> parametersActions);
        void GenerateMethodReference(Action targetObject, string methodName, params CodeTypeReference[] typeParameters);
        void GenerateValueTupleCreate(IEnumerable<Action> actions);
        void GenerateCodeImplicitKeyValuePairCreate(Action generateKeyAction, Action generateValueAction);
        void GenerateLambdaExpression(Action generateLambda, Action[] generateParameters);
        void GenerateObjectCreateAndInitialize(CodeTypeReference type, IEnumerable<Action> generateParametersActions, IEnumerable<Action> generateInitializeActions);
        void GenerateNamedArgument(string argumentName, Action generateValue);
        void GenerateCodeAssign(Action left, Action right);
        void GeneratePrimitive(object obj);
        void GenerateDefaultValue(CodeTypeReference typeRef);
        void GeneratePropertyReference(string propertyName, Action targetObjectAction);
        void GenerateTypeReference(CodeTypeReference typeReference);
        void GenerateTypeOf(CodeTypeReference e);
        void GenerateComment(string comment, bool noNewLine);
        void GenerateVariableDeclarationStatement(CodeTypeReference typeReference, string variableName, Action initAction);
        void GenerateSeparator();
    }
}
