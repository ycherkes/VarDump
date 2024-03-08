using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VarDump.CodeDom.Compiler;
using VarDump.Utils;

namespace VarDump.Visitor.KnownTypes;

internal sealed class GroupingVisitor(IRootObjectVisitor rootObjectVisitor, ICodeWriter codeWriter) : IKnownObjectVisitor
{
    public string Id => "Grouping";
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsGrouping();
    }

    public void Visit(object o, Type objectType, VisitContext context)
    {
        codeWriter.WriteMethodInvoke(() => 
                codeWriter.WriteMethodReference(() => 
                    codeWriter.WriteMethodInvoke(() => 
                            codeWriter.WriteMethodReference(() => VisitGroupings(o, context), 
                            "GroupBy"),
                [
                    WriteKeyLambda,
                    WriteValueLambda
                ]), "Single"),
        []);

        void WriteVariable() => codeWriter.WriteVariableReference("grp");
        void WriteKeyLambdaProperty() => codeWriter.WritePropertyReference("Key", WriteVariable);
        void WriteKeyLambda() => codeWriter.WriteLambdaExpression(WriteKeyLambdaProperty, [WriteVariable]);
        void WriteValueLambdaProperty() => codeWriter.WritePropertyReference("Element", WriteVariable);
        void WriteValueLambda() => codeWriter.WriteLambdaExpression(WriteValueLambdaProperty, [WriteVariable]);
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

    private void VisitGroupings(object o, VisitContext context)
    {
        var grouping = GetIGroupingValue(o);
        var groupingValues = grouping.Value.Cast<object>().Select(e => new { grouping.Key, Element = e });
        rootObjectVisitor.Visit(groupingValues, context);
    }
}