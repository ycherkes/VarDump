using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VarDumpExtended.CodeDom.Common;
using VarDumpExtended.Utils;

namespace VarDumpExtended.Visitor.KnownTypes;

internal sealed class GroupingVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;

    public GroupingVisitor(IObjectVisitor rootObjectVisitor)
    {
        _rootObjectVisitor = rootObjectVisitor;
    }

    public string Id => "Grouping";
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsGrouping();
    }

    public CodeExpression Visit(object o, Type objectType)
    {
        CodeExpression expr = VisitGroupings(new[] { o });

        var variableReferenceExpression = new CodeVariableReferenceExpression("grp");
        var keyLambdaExpression = new CodeLambdaExpression(new CodePropertyReferenceExpression(variableReferenceExpression, "Key"), variableReferenceExpression);
        var valueLambdaExpression = new CodeLambdaExpression(new CodePropertyReferenceExpression(variableReferenceExpression, "Element"), variableReferenceExpression);

        expr = new CodeMethodInvokeExpression(expr, "GroupBy", keyLambdaExpression, valueLambdaExpression);
        expr = new CodeMethodInvokeExpression(expr, "Single");

        return expr;
    }

    private static KeyValuePair<object, IEnumerable> GetIGroupingValue(object o)
    {
        var objectType = o.GetType();
        var fieldValues = objectType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(x => x.Name is "_key" or "key" or "_elements" or "elements")
            .Select(p => ReflectionUtils.GetValue(p, o))
            .ToArray();

        return new KeyValuePair<object, IEnumerable>(fieldValues[0], (IEnumerable)fieldValues[1]);
    }

    private CodeExpression VisitGroupings(IEnumerable<object> objects)
    {
        var groupingValues = objects.Select(GetIGroupingValue)
            .SelectMany(g => g.Value.Cast<object>().Select(e => new { g.Key, Element = e }))
            .ToArray();

        return _rootObjectVisitor.Visit(groupingValues);
    }
}