[![Stand With Ukraine](https://raw.githubusercontent.com/vshymanskyy/StandWithUkraine/main/banner2-direct.svg)](https://stand-with-ukraine.pp.ua)

# VarDump

[![nuget version](https://img.shields.io/nuget/v/VarDump.svg?style=flat-square)](https://www.nuget.org/packages/VarDump)
[![nuget downloads](https://img.shields.io/nuget/dt/VarDump?label=Downloads)](https://www.nuget.org/packages/VarDump)

VarDump turns runtime .NET objects into readable C# or Visual Basic source code.

Use it when you want copyable object initializers for tests, diagnostics, documentation, samples, or debugging workflows where plain text dumps are not enough.

```powershell
dotnet add package VarDump
```

## Quick Start

### C# Source Output

This is the smallest useful VarDump example: create a dumper, pass any object, and print the generated source.

<p align="right"><a href="https://dotnetfiddle.net/vI6Wq4">Run .NET fiddle</a></p>

```csharp
using System;
using VarDump;

var person = new
{
    Name = "Nick",
    Age = 23,
    Tags = new[] { "admin", "active" }
};

var source = new CSharpDumper().Dump(person);

Console.WriteLine(source);
```

### Visual Basic Source Output

Use `VisualBasicDumper` when the output needs to be Visual Basic source.

```csharp
using System;
using VarDump;

var person = new { Name = "Nick", Age = 23 };
var source = new VisualBasicDumper().Dump(person);

Console.WriteLine(source);
```

## Core Features

VarDump is built for source-code output, not console-style object inspection.

| Feature | Notes |
| --- | --- |
| C# and Visual Basic output | Use `CSharpDumper` or `VisualBasicDumper`. |
| Complex collection support | Anonymous collections, groups, lookups, read-only, immutable, frozen, and queryable collections. |
| Extended array support | Includes multi-dimensional arrays and layout handling. |
| Date and time support | Handles common .NET date/time types with configurable output style. |
| Numeric formatting | Supports decimal, binary, hexadecimal, padding, and digit separators for integral values. |
| Output limits | Cap collection size and traversal depth for large graphs. |
| Type name policy | Choose short, nested-qualified, or fully qualified type names. |
| `TextWriter` output | Stream output when you do not want to allocate one large string. |

For behavior examples, see the [unit tests](https://github.com/ycherkes/VarDump/tree/main/test).

## Configure Output

### Member Sorting And Skipped Values

Pass `DumpOptions` to either dumper to control member selection, sorting, formatting, collection layout, string literal style, and more.

<p align="right"><a href="https://dotnetfiddle.net/p4oIKX">Run .NET fiddle</a></p>

```csharp
using System;
using System.ComponentModel;
using VarDump;
using VarDump.Visitor;

var options = new DumpOptions
{
    SortDirection = ListSortDirection.Ascending,
    IgnoreNullValues = false,
    IgnoreDefaultValues = false
};

var person = new { Name = "Nick", Age = 23, MiddleName = (string?)null };
var source = new CSharpDumper(options).Dump(person);

Console.WriteLine(source);
```

Full reference: [DumpOptions API Guide](docs/API_GUIDE_DumpOptions.md).

### Modern C# Literal Styles

Use literal style options when generated C# should target newer language features.

```csharp
using VarDump;
using VarDump.Visitor;

var options = new DumpOptions
{
    StringLiteralStyle = StringLiteralStyle.Raw,
    CollectionLiteralStyle = CollectionLiteralStyle.Expression
};

var source = new CSharpDumper(options).Dump(new[] { "one", "two" });
```

`Raw` string literals require modern C# language support. `Expression` collection literals require C# 12 support.

## Extension Methods

Install the separate extension package when you want `DumpText()`, `DumpConsole()`, `DumpDebug()`, or `DumpTrace()` on any object.

```powershell
dotnet add package VarDump.Extensions
```

### Object Extension Methods

<p align="right"><a href="https://dotnetfiddle.net/n9kjiF">Run .NET fiddle</a></p>

```csharp
using System;
using System.Linq;

var dictionary = new[]
{
    new { Name = "Name1", Surname = "Surname1" }
}.ToDictionary(x => x.Name, x => x);

Console.WriteLine(dictionary.DumpText());
dictionary.DumpConsole();
dictionary.DumpDebug();
dictionary.DumpTrace();
```

### Visual Basic Extension Output

<p align="right"><a href="https://dotnetfiddle.net/OGCcrk">Run .NET fiddle</a></p>

```csharp
using System;
using System.Linq;

VarDumpExtensions.VarDumpFactory = VarDumpFactories.VisualBasic;

var dictionary = new[]
{
    new { Name = "Name1", Surname = "Surname1" }
}.ToDictionary(x => x.Name, x => x);

Console.WriteLine(dictionary.DumpText());
```

## Advanced And Extensibility

Use the advanced APIs when you need to transform object descriptions before output or teach VarDump how to serialize a specific type.

| Need | Start here |
| --- | --- |
| Mask or rewrite member values before output | [Extensibility Guide: descriptor middleware](docs/API_GUIDE_Extensibility.md#descriptor-middleware) |
| Add support for a known object type | [Extensibility Guide: known object visitors](docs/API_GUIDE_Extensibility.md#known-object-visitors) |
| Tune every output option | [DumpOptions API Guide](docs/API_GUIDE_DumpOptions.md) |
| Compare behavior with ObjectDumper.NET | [Comparison fiddle](https://dotnetfiddle.net/vRX7vO) |

## Comparison

VarDump grew out of work on [Object Dumper Visual Studio, Visual Studio Code, and Rider extensions](https://github.com/ycherkes/ObjectDumper). It focuses on richer source-code generation options than ObjectDumper.NET.

| Feature | VarDump | ObjectDumper |
| --- | --- | --- |
| [Console-style dump](https://github.com/thomasgalliker/ObjectDumper?tab=readme-ov-file#dumping-c-objects-to-consolewriteline) | N/A | Yes |
| Collections: anonymous, groups, lookups, read-only, immutable, frozen, queryable | Yes | No |
| [Extended array support](https://github.com/ycherkes/VarDump/blob/main/test/VarDump.UnitTests/ArraySpec.cs) | Yes | No |
| [Extended Date-Time support](https://github.com/ycherkes/VarDump/blob/main/test/VarDump.UnitTests/DateTimeSpec.cs) | Yes | No |
| [Hex/Binary formatting and digit separator](https://github.com/ycherkes/VarDump/blob/main/test/VarDump.UnitTests/IntegralTypesSpec.cs) | Yes | No |
| [Max collection size](https://github.com/ycherkes/VarDump/blob/main/test/VarDump.UnitTests/TooManyItemsCollectionSpec.cs) | Yes | N/A |
| [Nested types](https://github.com/ycherkes/VarDump/blob/main/test/VarDump.UnitTests/TypeNamingPolicySpec.cs) | Yes | No |
| [Output to TextWriter](https://github.com/ycherkes/VarDump/blob/main/test/VarDump.UnitTests/LazinessSpec.cs) | Yes | No |

## Powered By

| Repository | License |
| --- | --- |
| [Heavily customized version](https://github.com/ycherkes/VarDump/tree/main/src/VarDump/CodeDom) of [System.CodeDom](https://github.com/dotnet/runtime/tree/main/src/libraries/System.CodeDom) | [![MIT](https://img.shields.io/github/license/dotnet/runtime?style=flat-square)](https://github.com/dotnet/runtime/blob/main/LICENSE.TXT) |

**Privacy Notice:** No personal data is collected.

This tool has been working well for my personal needs, but outside that its future depends on your feedback. Please [open an issue](https://github.com/ycherkes/VarDump/issues) with problems, ideas, or examples that should work better.

Sponsor the project on [GitHub](https://github.com/sponsors/ycherkes) or [PayPal](https://www.paypal.com/donate/?business=KXGF7CMW8Y8WJ&no_recurring=0&item_name=Help+VarDump+become+better%21).
