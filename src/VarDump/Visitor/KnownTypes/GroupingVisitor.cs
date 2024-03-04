using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VarDump.CodeDom.Compiler;
using VarDump.Utils;

namespace VarDump.Visitor.KnownTypes;

internal sealed class GroupingVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly ICodeWriter _codeWriter;

    public GroupingVisitor(IObjectVisitor rootObjectVisitor, ICodeWriter codeWriter)
    {
        _rootObjectVisitor = rootObjectVisitor;
        _codeWriter = codeWriter;
    }

    public string Id => "Grouping";
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsGrouping();
    }

    public void Visit(object o, Type objectType)
    {
        _codeWriter.WriteMethodInvoke(() => 
                _codeWriter.WriteMethodReference(() => 
                    _codeWriter.WriteMethodInvoke(() => 
                            _codeWriter.WriteMethodReference(() => VisitGroupings(o), 
                            "GroupBy"),
                [
                    WriteKeyLambdaExpression,
                    WriteValueLambdaExpression
                ]), "Single"),
        []);

        void WriteVariableReference() => _codeWriter.WriteVariableReference("grp");
        void WriteKeyLambdaPropertyExpression() => _codeWriter.WritePropertyReference("Key", WriteVariableReference);
        void WriteKeyLambdaExpression() => _codeWriter.WriteLambdaExpression(WriteKeyLambdaPropertyExpression, [WriteVariableReference]);
        void WriteValueLambdaPropertyExpression() => _codeWriter.WritePropertyReference("Element", WriteVariableReference);
        void WriteValueLambdaExpression() => _codeWriter.WriteLambdaExpression(WriteValueLambdaPropertyExpression, [WriteVariableReference]);
    }

    private static KeyValuePair<object, IEnumerable> GetIGroupingValue(object o)
    {
        var objectType = o.GetType();
        var fieldValues = objectType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(x => x.Name is "_key" or "key" or "_elements" or "elements")
            .Select(p => ReflectionUtils.GetValue(p, o))
            .Take(2)
            .ToArray();

        return new KeyValuePair<object, IEnumerable>(fieldValues[0], (IEnumerable)fieldValues[1]);
    }

    private void VisitGroupings(object @object)
    {
        var grouping = GetIGroupingValue(@object);
        var groupingValues = grouping.Value.Cast<object>().Select(e => new { grouping.Key, Element = e });
        _rootObjectVisitor.Visit(groupingValues);
    }
}