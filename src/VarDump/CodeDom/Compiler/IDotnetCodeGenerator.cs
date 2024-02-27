// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System;
using VarDump.CodeDom.Common;
using System.IO;

namespace VarDump.CodeDom.Compiler
{
    public interface IDotnetCodeGenerator
    {
        int Indent { get; set; }
        TextWriter Output { get; }

        void GenerateArrayCreate(CodeDotnetTypeReference typeReference, IEnumerable<Action> generateInitializers, int size = 0);
        void GenerateCast(CodeDotnetTypeReference typeReference, Action generateAction);

        void GenerateCodeArrayDimension(IEnumerable<Action> initializers);
        void GenerateCodeAssign(Action left, Action right);

        void GenerateCodeImplicitKeyValuePairCreate(Action generateKeyAction, Action generateValueAction);

        void GenerateComment(string comment, bool noNewLine);

        void GenerateDefaultValue(CodeDotnetTypeReference typeRef);

        void GenerateFieldReference(string fieldName, Action generateTargetObjectAction);

        void GenerateFlagsBinaryOperator(IEnumerable<Action> generateOperandActions);
        void GenerateLambdaExpression(Action generateLambda, Action[] generateParameters);

        void GenerateMethodInvoke(Action methodReferenceAction, IEnumerable<Action> parametersActions);

        void GenerateMethodReference(Action targetObject, string methodName, params CodeDotnetTypeReference[] typeParameters);

        void GenerateNamedArgument(string argumentName, Action generateValue);

        void GenerateObjectCreateAndInitialize(CodeDotnetTypeReference type, IEnumerable<Action> generateParametersActions, IEnumerable<Action> generateInitializeActions);

        void GeneratePrimitive(object obj);

        void GeneratePropertyReference(string propertyName, Action targetObjectAction);

        void GenerateSeparator();

        void GenerateTypeOf(CodeDotnetTypeReference e);

        void GenerateTypeReference(CodeDotnetTypeReference typeReference);

        void GenerateValueTupleCreate(IEnumerable<Action> actions);

        void GenerateVariableDeclarationStatement(CodeDotnetTypeReference typeReference, string variableName, Action initAction);

        void GenerateVariableReference(string variableName);
    }
}
