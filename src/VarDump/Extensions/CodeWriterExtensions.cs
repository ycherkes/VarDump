using System.Collections.Generic;
using System;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.Visitor;
using System.Linq;
using VarDump.Utils;

namespace VarDump.Extensions;

public static class CodeWriterExtensions
{
    public static void WriteErrorDetected(this ICodeWriter codeWriter, string errorMessage)
    {
        codeWriter.WritePrimitive(null);
        codeWriter.WriteSeparator();
        codeWriter.WriteComment(errorMessage, true);
    }

    public static void WriteCircularReferenceDetected(this ICodeWriter codeWriter)
    {
        codeWriter.WritePrimitive(null);
        codeWriter.WriteSeparator();
        codeWriter.WriteComment("Circular reference detected", true);
    }

    public static void WriteTooManyItems(this ICodeWriter codeWriter, int maxCollectionSize)
    {
        codeWriter.WriteComment($"Too many items (> {maxCollectionSize}). Consider increasing the {nameof(DumpOptions.MaxCollectionSize)} option.", noNewLine: true);
    }

    public static void WriteMaxDepthExpression(this ICodeWriter codeWriter, object @object)
    {
        if (@object == null || @object.GetType().IsAnonymousType())
        {
            codeWriter.WritePrimitive(null);
        }
        else
        {
            codeWriter.WriteDefaultValue(@object.GetType());
        }

        codeWriter.WriteSeparator();
        codeWriter.WriteComment("Max depth", true);
    }

    public static void WriteArrayCreate(this ICodeWriter codeWriter, Type type, IEnumerable<Action> generateInitializers, int size = 0)
    {
        codeWriter.WriteArrayCreate(new TypeReference(type), generateInitializers, size);
    }

    public static void WriteCast(this ICodeWriter codeWriter, Type type, Action generateAction)
    {
        codeWriter.WriteCast(new TypeReference(type), generateAction);
    }

    public static void WriteDefaultValue(this ICodeWriter codeWriter, Type type)
    {
        codeWriter.WriteDefaultValue(new TypeReference(type));
    }

    public static void WriteMethodReference(this ICodeWriter codeWriter, Action targetObject, string methodName,
        params Type[] typeParameters)
    {
        codeWriter.WriteMethodReference(targetObject, methodName, typeParameters.Select(tp => new TypeReference(tp)).ToArray());
    }

    public static void WriteObjectCreateAndInitialize(this ICodeWriter codeWriter, Type type,
        IEnumerable<Action> generateParametersActions, IEnumerable<Action> generateInitializeActions)
    {
        codeWriter.WriteObjectCreateAndInitialize(new TypeReference(type), generateParametersActions, generateInitializeActions);
    }

    public static void WriteTypeOf(this ICodeWriter codeWriter, Type type)
    {
        codeWriter.WriteTypeOf(new TypeReference(type));
    }

    public static void WriteTypeReference(this ICodeWriter codeWriter, Type type)
    {
        codeWriter.WriteTypeReference(new TypeReference(type));
    }
}