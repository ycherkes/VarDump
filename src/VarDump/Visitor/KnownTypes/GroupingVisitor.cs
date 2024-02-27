using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.Utils;

namespace VarDump.Visitor.KnownTypes;

internal sealed class GroupingVisitor : IKnownObjectVisitor
{
    private readonly IObjectVisitor _rootObjectVisitor;
    private readonly IDotnetCodeGenerator _codeGenerator;

    public GroupingVisitor(IObjectVisitor rootObjectVisitor, IDotnetCodeGenerator codeGenerator)
    {
        _rootObjectVisitor = rootObjectVisitor;
        _codeGenerator = codeGenerator;
    }

    public string Id => "Grouping";
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return objectType.IsGrouping();
    }

    public void Visit(object o, Type objectType)
    {
        _codeGenerator.GenerateMethodInvoke(() => 
                _codeGenerator.GenerateMethodReference(() => 
                    _codeGenerator.GenerateMethodInvoke(() => 
                            _codeGenerator.GenerateMethodReference(() => VisitGroupings(o), 
                            "GroupBy"),
                [
                    GenerateKeyLambdaExpression,
                    GenerateValueLambdaExpression
                ]), "Single"),
        []);

        void GenerateVariableReference() => _codeGenerator.GenerateVariableReference("grp");
        void GenerateKeyLambdaPropertyExpression() => _codeGenerator.GeneratePropertyReference("Key", GenerateVariableReference);
        void GenerateKeyLambdaExpression() => _codeGenerator.GenerateLambdaExpression(GenerateKeyLambdaPropertyExpression, [GenerateVariableReference]);
        void GenerateValueLambdaPropertyExpression() => _codeGenerator.GeneratePropertyReference("Element", GenerateVariableReference);
        void GenerateValueLambdaExpression() => _codeGenerator.GenerateLambdaExpression(GenerateValueLambdaPropertyExpression, [GenerateVariableReference]);
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