# [![Made in Ukraine](https://img.shields.io/badge/made_in-ukraine-ffd700.svg?labelColor=0057b7&style=for-the-badge)](https://stand-with-ukraine.pp.ua) [Stand with the people of Ukraine: How to Help](https://stand-with-ukraine.pp.ua)

VarDump is a utility for serialization of runtime objects to C# or Visual Basic string.
===========================================================================================

Developed as a free alternative of [ObjectDumper.NET](https://github.com/thomasgalliker/ObjectDumper), which is not free for commercial use.

[![nuget version](https://img.shields.io/badge/Nuget-v0.3.0.alpha-blue)](https://www.nuget.org/packages/VarDump)
[![nuget downloads](https://img.shields.io/nuget/dt/VarDump?label=Downloads)](https://www.nuget.org/packages/VarDump)

## C# & VB Dumper:
<p align="right"><a href="https://dotnetfiddle.net/4ARhwR">Run .NET fiddle</a></p>

```csharp
using System;
using VarDump;

var anonymousObject = new { Name = "Name", Surname = "Surname" };
var cs = new CSharpDumper().Dump(anonymousObject);
Console.WriteLine(cs);
var vb = new VisualBasicDumper().Dump(anonymousObject);
Console.WriteLine(vb);
```
## C# & VB Dumper, how to use DumpOptions:
<p align="right"><a href="https://dotnetfiddle.net/CxsDtN">Run .NET fiddle</a></p>

```csharp
using System;
using System.ComponentModel;
using VarDump;
using VarDump.Visitor;

var person = new Person { Name = "Nick", Age = 23 };
var dumpOptions = new DumpOptions { SortDirection = ListSortDirection.Ascending };

var csDumper = new CSharpDumper(dumpOptions);
var cs = csDumper.Dump(person);

var vbDumper = new VisualBasicDumper(dumpOptions);
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

## Object Extension methods:
<p align="right"><a href="https://dotnetfiddle.net/Lz9duL">Run .NET fiddle</a></p>

```csharp
using System;
using System.Linq;
using VarDump.Extensions;
using VarDump.Visitor;

var dictionary = new[]
{
    new
    {
        Name = "Name1",
        Surname = "Surname1"
    }
}.ToDictionary(x => x.Name, x => x);

Console.WriteLine(dictionary.Dump(DumpOptions.Default));
```

## Object Extension methods, how to switch default dumper to VB:
<p align="right"><a href="https://dotnetfiddle.net/sM1lML">Run .NET fiddle</a></p>

```csharp
using System;
using System.Linq;
using VarDump.Extensions;
using VarDump.Visitor;

VarDumpExtensions.VarDumpFactory = VarDumpFactories.VisualBasic;

var dictionary = new[]
{
    new
    {
        Name = "Name1",
        Surname = "Surname1"
    }
}.ToDictionary(x => x.Name, x => x);

Console.WriteLine(dictionary.Dump(DumpOptions.Default));
```

## Extensibility:

### Light with ObjectDescriptorMiddleware:
<p align="right"><a href="https://dotnetfiddle.net/hfrbo6">Run .NET fiddle</a></p>

```csharp
using System;
using VarDump;
using VarDump.Visitor;
using VarDump.Visitor.Descriptors;

// For more examples see https://github.com/ycherkes/VarDump/blob/main/test/VarDump.UnitTests/ObjectDescriptorMiddlewareSpec.cs

const string name = "World";
FormattableString formattableString = $"Hello, {name}";

var dumpOptions = new DumpOptions
{
    Descriptors = { new FormattableStringMiddleware() }
};

var csDumper = new CSharpDumper(dumpOptions);
var cs = csDumper.Dump(formattableString);

var vbDumper = new VisualBasicDumper(dumpOptions);
var vb = vbDumper.Dump(formattableString);

// C# string
Console.WriteLine(cs);

// VB string
Console.WriteLine(vb);

class FormattableStringMiddleware : IObjectDescriptorMiddleware
{
    public ObjectDescriptionInfo Describe(object @object, Type objectType, Func<ObjectDescriptionInfo> prev)
    {
        if (@object is FormattableString fs)
        {
            return Descriptor.FromObject(new
            {
                fs.Format,
                Arguments = fs.GetArguments()
            }, objectType);
        }

        return prev();
    }
}
```

### Full feature with KnownObjectVisitor:
<p align="right"><a href="https://dotnetfiddle.net/kScIyR">Run .NET fiddle</a></p>

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using VarDump;
using VarDump.CodeDom.Compiler;
using VarDump.Extensions;
using VarDump.Visitor;
using VarDump.Visitor.KnownTypes;

// For more examples see https://github.com/ycherkes/VarDump/blob/main/test/VarDump.UnitTests/KnownTypesSpec.cs

const string name = "World";
FormattableString str = $"Hello, {name}";

var dumpOptions = new DumpOptions
{
    ConfigureKnownTypes = (knownObjects, rootObjectVisitor, _, codeWriter) =>
    {
        var fsv = new FormattableStringVisitor(rootObjectVisitor, codeWriter);
        knownObjects.Add(fsv.Id, fsv);
    }
};

var dumper = new CSharpDumper(dumpOptions);
var result = dumper.Dump(str);
Console.WriteLine(result);

return;

class FormattableStringVisitor(IObjectVisitor rootObjectVisitor, ICodeWriter codeWriter) : IKnownObjectVisitor
{
    public string Id => "ServiceDescriptor";
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return obj is FormattableString;
    }

    public void Visit(object obj, Type objectType)
    {
        var formattableString = (FormattableString)obj;

        IEnumerable<Action> argumentActions =
        [
            () => codeWriter.WritePrimitive(formattableString.Format)
        ];

        argumentActions = argumentActions.Concat(formattableString.GetArguments().Select(a => (Action)(() => rootObjectVisitor.Visit(a))));

        codeWriter.WriteMethodInvoke(() =>
            codeWriter.WriteMethodReference(
                () => codeWriter.WriteType(typeof(FormattableStringFactory)),
                nameof(FormattableStringFactory.Create)),
            argumentActions);
    }
}
```

For more examples see [Unit Tests](https://github.com/ycherkes/VarDump/tree/main/test)

[Compare VarDump with ObjectDumper.NET - Run .NET fiddle](https://dotnetfiddle.net/vRX7vO)

# Powered By

| Repository  | License |
| ------------- | ------------- |
| [Heavily customized version](https://github.com/ycherkes/VarDump/tree/main/src/CodeDom) of [System.CodeDom](https://github.com/dotnet/runtime/tree/main/src/libraries/System.CodeDom)  | [![MIT](https://img.shields.io/github/license/dotnet/runtime?style=flat-square)](https://github.com/dotnet/runtime/blob/main/LICENSE.TXT)  |

**Privacy Notice:** No personal data is collected at all.

This tool has been working well for my personal needs, but outside that its future depends on your feedback. Feel free to [open an issue](https://github.com/ycherkes/VarDump/issues).

[![PayPal](https://img.shields.io/badge/Donate-PayPal-ffd700.svg?labelColor=0057b7&style=for-the-badge)](https://www.paypal.com/donate/?business=KXGF7CMW8Y8WJ&no_recurring=0&item_name=Help+VarDump+become+better%21)

Any donations during this time will be directed to local charities at my own discretion.
