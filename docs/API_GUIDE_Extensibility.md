# Extensibility API Guide

VarDump exposes two main extension points:

- Descriptor middleware rewrites object descriptions before source is generated.
- Known object visitors add source-generation logic for specific object types.

Use descriptor middleware when the normal object shape is correct but values or members need to be filtered, masked, renamed, or replaced. Use known object visitors when a type should be emitted as a constructor, factory method, or other custom expression.

## Descriptor Middleware

### Mask Sensitive Values

This example replaces string values whose member name ends with `CardNumber`. The dumper still walks the object normally, but the generated source receives masked values.

<p align="right"><a href="https://dotnetfiddle.net/hfrbo6">Run .NET fiddle</a></p>

```csharp
using System;
using VarDump;
using VarDump.Visitor;
using VarDump.Visitor.Descriptors;
using VarDump.Visitor.Descriptors.Specific;

var obj = new
{
    FullName = "BRUCE LEE",
    CardNumber = "4953089013607",
    OtherInfo = new
    {
        CardNumber = "5201294442453002",
    }
};

var options = new DumpOptions
{
    Descriptors =
    {
        new ObjectContentReplacer { Replacement = MaskCardNumber }
    }
};

var cs = new CSharpDumper(options).Dump(obj);
var vb = new VisualBasicDumper(options).Dump(obj);

Console.WriteLine(cs);
Console.WriteLine(vb);

static ReflectionDescription MaskCardNumber(ReflectionDescription description)
{
    if (!IsCardNumber(description) || string.IsNullOrWhiteSpace((string)description.Value))
    {
        return description;
    }

    var stringValue = (string)description.Value;

    var maskedValue = stringValue.Length - 4 > 0
        ? new string('*', stringValue.Length - 4) + stringValue.Substring(stringValue.Length - 4)
        : stringValue;

    return description with
    {
        Value = maskedValue
    };

    static bool IsCardNumber(ReflectionDescription description)
    {
        return description.Type == typeof(string)
               && description.Name?.EndsWith("cardnumber", StringComparison.OrdinalIgnoreCase) == true;
    }
}
```

For more descriptor examples, see [ObjectDescriptorMiddlewareSpec.cs](../test/VarDump.UnitTests/ObjectDescriptorMiddlewareSpec.cs).

## Known Object Visitors

### Custom Factory Output

This example teaches VarDump how to emit a `FormattableString` by writing a `FormattableStringFactory.Create(...)` call.

<p align="right"><a href="https://dotnetfiddle.net/kScIyR">Run .NET fiddle</a></p>

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using VarDump;
using VarDump.CodeDom.Compiler;
using VarDump.Visitor;

const string name = "World";
FormattableString str = $"Hello, {name}";

var options = new DumpOptions
{
    ConfigureKnownObjects = (knownObjects, nextDepthVisitor, _, codeWriter) =>
    {
        knownObjects.Add(new FormattableStringVisitor(nextDepthVisitor, codeWriter));
    }
};

var result = new CSharpDumper(options).Dump(str);
Console.WriteLine(result);

return;

class FormattableStringVisitor(INextDepthVisitor nextDepthVisitor, ICodeWriter codeWriter) : IKnownObjectVisitor
{
    public string Id => nameof(FormattableString);

    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is FormattableString;
    }

    public void ConfigureOptions(Action<DumpOptions> configure)
    {
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        var formattableString = (FormattableString)obj;

        IEnumerable<Action> arguments =
        [
            () => codeWriter.WritePrimitive(formattableString.Format)
        ];

        arguments = arguments.Concat(formattableString.GetArguments().Select(a => (Action)(() => nextDepthVisitor.Visit(a, context))));

        codeWriter.WriteMethodInvoke(() =>
            codeWriter.WriteMethodReference(
                () => codeWriter.WriteType(typeof(FormattableStringFactory)),
                nameof(FormattableStringFactory.Create)),
            arguments);
    }
}
```

For more known object examples, see [KnownObjectsSpec.cs](../test/VarDump.UnitTests/KnownObjectsSpec.cs).

## Related APIs

- [DumpOptions API Guide](API_GUIDE_DumpOptions.md)
- [Unit tests](../test)
