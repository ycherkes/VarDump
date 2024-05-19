# [![Made in Ukraine](https://img.shields.io/badge/made_in-ukraine-ffd700.svg?labelColor=0057b7&style=for-the-badge)](https://stand-with-ukraine.pp.ua) [Stand with the people of Ukraine: How to Help](https://stand-with-ukraine.pp.ua)

VarDump is a highly customizable  utility for serialization runtime objects to C# or Visual Basic string.
===========================================================================================

Developed as a free alternative to [ObjectDumper.NET](https://github.com/thomasgalliker/ObjectDumper), which is not free for commercial use.

[![nuget version](https://img.shields.io/badge/Nuget-v1.0.4.5.alpha-blue)](https://www.nuget.org/packages/VarDump)
[![nuget downloads](https://img.shields.io/nuget/dt/VarDump?label=Downloads)](https://www.nuget.org/packages/VarDump)

### C# & VB Dumper:
<p align="right"><a href="https://dotnetfiddle.net/vI6Wq4">Run .NET fiddle</a></p>

```csharp
using System;
using VarDump;

var anonymousObject = new { Name = "Name", Surname = "Surname" };
var cs = new CSharpDumper().Dump(anonymousObject);
Console.WriteLine(cs);
var vb = new VisualBasicDumper().Dump(anonymousObject);
Console.WriteLine(vb);
```
### C# & VB Dumper, how to use DumpOptions:
<p align="right"><a href="https://dotnetfiddle.net/p4oIKX">Run .NET fiddle</a></p>

```csharp
using System;
using System.ComponentModel;
using VarDump;
using VarDump.Visitor;

var person = new Person { Name = "Nick", Age = 23 };
var options = new DumpOptions { SortDirection = ListSortDirection.Ascending };

var csDumper = new CSharpDumper(options);
var cs = csDumper.Dump(person);

var vbDumper = new VisualBasicDumper(options);
var vb = vbDumper.Dump(person);

// C# string
Console.WriteLine(cs);
// VB string
Console.WriteLine(vb);

class Person
{
    public string Name {get; set;}
    public int Age {get; set;}
}
```

## To dump with extension methods, please install a separate [VarDump.Extensions](https://www.nuget.org/packages/VarDump.Extensions) package.

### Dump Extension methods:
<p align="right"><a href="https://dotnetfiddle.net/n9kjiF">Run .NET fiddle</a></p>

```csharp
using System;
using System.Linq;

var dictionary = new[]
{
    new
    {
        Name = "Name1",
        Surname = "Surname1"
    }
}.ToDictionary(x => x.Name, x => x);

Console.WriteLine(dictionary.Dump());
```

### Dump Extension methods, how to switch default dumper to VB:
<p align="right"><a href="https://dotnetfiddle.net/OGCcrk">Run .NET fiddle</a></p>

```csharp
using System;
using System.Linq;

VarDumpExtensions.VarDumpFactory = VarDumpFactories.VisualBasic;

var dictionary = new[]
{
    new
    {
        Name = "Name1",
        Surname = "Surname1"
    }
}.ToDictionary(x => x.Name, x => x);

Console.WriteLine(dictionary.Dump());
```

## Extensibility:

### With middleware:
<p align="right"><a href="https://dotnetfiddle.net/hfrbo6">Run .NET fiddle</a></p>

```csharp
using System;
using VarDump;
using VarDump.Visitor;
using VarDump.Visitor.Descriptors;
using VarDump.Visitor.Descriptors.Specific;

// For more examples see https://github.com/ycherkes/VarDump/blob/main/test/VarDump.UnitTests/ObjectDescriptorMiddlewareSpec.cs

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

var csDumper = new CSharpDumper(options);
var cs = csDumper.Dump(obj);

var vbDumper = new VisualBasicDumper(options);
var vb = vbDumper.Dump(obj);

// C# string
Console.WriteLine(cs);

// VB string
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

### With KnownObjectVisitor:
<p align="right"><a href="https://dotnetfiddle.net/kScIyR">Run .NET fiddle</a></p>

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using VarDump;
using VarDump.CodeDom.Compiler;
using VarDump.Visitor;

// For more examples see https://github.com/ycherkes/VarDump/blob/main/test/VarDump.UnitTests/KnownObjectsSpec.cs

const string name = "World";
FormattableString str = $"Hello, {name}";

var options = new DumpOptions
{
    ConfigureKnownObjects = (knownObjects, nextDepthVisitor, _, codeWriter) =>
    {
        knownObjects.Add(new FormattableStringVisitor(nextDepthVisitor, codeWriter));
    }
};

var dumper = new CSharpDumper(options);
var result = dumper.Dump(str);
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

For more examples see [Unit Tests](https://github.com/ycherkes/VarDump/tree/main/test)

[Compare VarDump with ObjectDumper.NET - Run .NET fiddle](https://dotnetfiddle.net/vRX7vO)

# Powered By

| Repository  | License |
| ------------- | ------------- |
| [Heavily customized version](https://github.com/ycherkes/VarDump/tree/main/src/VarDump/CodeDom) of [System.CodeDom](https://github.com/dotnet/runtime/tree/main/src/libraries/System.CodeDom)  | [![MIT](https://img.shields.io/github/license/dotnet/runtime?style=flat-square)](https://github.com/dotnet/runtime/blob/main/LICENSE.TXT)  |

**Privacy Notice:** No personal data is collected at all.

This tool has been working well for my personal needs, but outside that its future depends on your feedback. Feel free to [open an issue](https://github.com/ycherkes/VarDump/issues).

🍪 Sponsor me on [GitHub](https://github.com/sponsors/ycherkes) or [PayPal](https://www.paypal.com/donate/?business=KXGF7CMW8Y8WJ&no_recurring=0&item_name=Help+VarDump+become+better%21).
