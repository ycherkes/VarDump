# DumpOptions API Guide

`DumpOptions` controls how VarDump builds C# and Visual Basic output.

Namespace:

```csharp
using VarDump.Visitor;
using VarDump.Visitor.Format;
```

## Quick start

```csharp
using System.ComponentModel;
using VarDump;
using VarDump.Visitor;

var options = new DumpOptions
{
    SortDirection = ListSortDirection.Ascending,
    IgnoreNullValues = false,
    PrimitiveCollectionLayout = CollectionLayout.SingleLine
};

var dumper = new CSharpDumper(options);
var text = dumper.Dump(new { Name = "Nick", Age = 23, Tags = new[] { "a", "b" } });
```

## DumpOptions reference table

| Property | Type | Default | Description | Variations / Notes |
| --- | --- | --- | --- | --- |
| `CollectionLiteralStyle` | `CollectionLiteralStyle` | `CollectionLiteralStyle.Initializer` | Controls C# collection literal emission style. | Enum values: `Initializer` emits `new [] { ... }`; `Expression` emits `[ ... ]` (when supported). |
| `ConfigureKnownObjects` | `Action<IKnownObjectsCollection, INextDepthVisitor, DumpOptions, ICodeWriter>` | `null` | Lets you register/replace known object visitors. | Use to call `knownObjects.Add(...)` with custom `IKnownObjectVisitor` implementations. |
| `DateKind` | `DateKind` | `DateKind.Original` | Controls date kind behavior in dumped `DateTime` values. | Enum values: `DateKind.Original` keeps original kind/value; `DateKind.ConvertToUtc` converts to UTC before dump. |
| `DateTimeInstantiation` | `DateTimeInstantiation` | `DateTimeInstantiation.Parse` | Controls how `DateTime` instances are emitted in code. | Enum values: `DateTimeInstantiation.Parse` emits parse-style construction; `DateTimeInstantiation.New` emits constructor-style creation. |
| `Descriptors` | `List<IObjectDescriptorMiddleware>` | empty list | Middleware pipeline for object description customization. | Add descriptor middleware (for example replacers/maskers) to transform values before writing. |
| `GenerateVariableInitializer` | `bool` | `true` | Emits variable initializer in top-level output. | Set `false` to emit object expression only. |
| `GetBaseClassFields` | `bool` | `false` | Controls whether fields from base classes are included. | Applies only when `GetFieldsBindingFlags` is set. `false` = current type only; `true` = walk inheritance chain and include base fields too. |
| `GetFieldsBindingFlags` | `BindingFlags?` | `null` | Controls **which fields** are inspected, and enables field dumping when set. | Important: when `null` (default), fields are not included in output at all. Set a value like `BindingFlags.Instance \| BindingFlags.Public \| BindingFlags.NonPublic` to include fields. |
| `GetPropertiesBindingFlags` | `BindingFlags` | `BindingFlags.Public \| BindingFlags.Instance` | Controls **which properties** are inspected. | Default includes public instance properties only. Add `NonPublic` for private/protected/internal properties, and `Static` for static properties. |
| `IgnoreDefaultValues` | `bool` | `true` | Skips members equal to type default values. | Set `false` to always include defaults like `0`, `false`, etc. |
| `IgnoreNullValues` | `bool` | `true` | Skips members with `null` values. | Set `false` to include explicit `null` assignments. |
| `IgnoreReadonlyProperties` | `bool` | `true` | Skips read-only properties. | Set `false` to include read-only properties in output. |
| `IndentString` | `string` | four spaces | Indentation text for multiline formatting. | Common values: `"  "` (2 spaces), `"    "` (4 spaces), `"\t"` (tab). |
| `IntegralNumericFormat` | `string` | `""` | Numeric format string for integral values (`sbyte`, `byte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`). | VarDump-specific grammar: `<fmt><digits>_<groupSize>` where `<fmt>` is `d/D` (decimal), `b/B` (binary), `x/X` (hex). `digits` and `_groupSize` are optional. |
| `MaxCollectionSize` | `int` | `int.MaxValue` | Maximum number of items emitted per collection. | Lower value truncates output after limit. |
| `MaxDepth` | `int` | `25` | Maximum recursion depth for object graph traversal. | Lower value prevents deep/recursive graphs from expanding too far. |
| `NewLineStyle` | `NewLineStyle` | `NewLineStyle.Auto` | Controls line endings in generated output. | Enum values: `Auto` (environment default), `Windows` (`\r\n`), `Unix` (`\n`). |
| `PrimitiveCollectionLayout` | `CollectionLayout` | `CollectionLayout.MultiLine` | Layout for collections of primitive values. | Enum values: `CollectionLayout.MultiLine` writes one item per line; `CollectionLayout.SingleLine` writes inline. |
| `SortDirection` | `ListSortDirection?` | `null` | Sort order for properties/fields. | Enum values: `ListSortDirection.Ascending` or `ListSortDirection.Descending`; `null` keeps reflection/native order. |
| `StringLiteralStyle` | `StringLiteralStyle` | `StringLiteralStyle.Auto` | Controls C# string literal emission style. | Enum values: `Auto`, `Escaped`, `Verbatim`, `Raw`. |
| `TypeNamePolicy` | `TypeNamingPolicy` | `TypeNamingPolicy.ShortName` | Controls how type names are emitted. | Enum values: `TypeNamingPolicy.ShortName` (type only), `TypeNamingPolicy.NestedQualified` (includes enclosing type for nested types), `TypeNamingPolicy.FullName` (fully qualified namespace + type). |
| `UseNamedArgumentsInConstructors` | `bool` | `false` | Uses named constructor arguments in emitted code where supported. | Example: `new Regex(pattern: "\\d+", options: RegexOptions.IgnoreCase)` instead of positional arguments. |
| `UsePredefinedConstants` | `bool` | `true` | Uses predefined constants where possible. | Example: emits `int.MaxValue` instead of `2147483647` when applicable. |
| `UsePredefinedMethods` | `bool` | `true` | Enables method-based output for known types that support it (currently `TimeSpan`). | For `TimeSpan`, `true` can emit `TimeSpan.FromSeconds(5)` / `FromHours(3)` / `FromTicks(...)`; `false` falls back to `TimeSpan.ParseExact(...)` (default) or `new TimeSpan(...)` when `DateTimeInstantiation = New`. |

### Binding flags quick guide

| Goal | Recommended options |
| --- | --- |
| Public instance properties only (default behavior) | `GetPropertiesBindingFlags = BindingFlags.Public \| BindingFlags.Instance` |
| Include non-public properties | `GetPropertiesBindingFlags = BindingFlags.Public \| BindingFlags.NonPublic \| BindingFlags.Instance` |
| Include private instance fields declared on current type | `GetFieldsBindingFlags = BindingFlags.Public \| BindingFlags.NonPublic \| BindingFlags.Instance` (without this, no fields are dumped) |
| Include base class fields too | `GetFieldsBindingFlags = BindingFlags.Public \| BindingFlags.NonPublic \| BindingFlags.Instance` and `GetBaseClassFields = true` |


### `IntegralNumericFormat` details

`IntegralNumericFormat` is validated by `IntegralNumericFormat.TryParse(...)` and supports only decimal, binary, and hexadecimal styles.

| Format piece | Meaning | Notes |
| --- | --- | --- |
| `<fmt>` | Base/style selector | `d`/`D` decimal, `b`/`B` binary, `x`/`X` hexadecimal. |
| `<digits>` | Minimum width (left-zero padding) | Optional. Example `X8` pads to 8 chars. |
| `_<groupSize>` | Underscore grouping from right to left | Optional. Example `d_3` -> `18_446_744...`; `b_4` -> nibble groups. |

| Supported specifier | Output behavior | Example input `37` |
| --- | --- | --- |
| `""`, `"D"`, `"d"`, `"D0"` | Default decimal | `37` |
| `"d8"` | Decimal, min 8 digits | `00000037` |
| `"d_3"` | Decimal with underscores every 3 digits | `18_446_744_073_709_551_615` (for `ulong.MaxValue`) |
| `"b"` | Binary lowercase prefix (`0b` / `&b`) | `100101` |
| `"B"` | Binary uppercase prefix (`0B` / `&B`) | `100101` |
| `"b8"` | Binary, min 8 digits | `00100101` |
| `"b8_4"` | Binary, min width + underscore groups | `0010_0101` |
| `"x"` | Hex lowercase prefix (`0x` / `&h`) | `25` |
| `"X"` | Hex uppercase prefix (`0X` / `&H`) | `25` |
| `"X2"` | Hex uppercase, min 2 digits | `25` (or `0F` for `(byte)15`) |
| `"X8_4"` | Hex uppercase, padded + grouped | `0001_E240` |

| Invalid examples | Why invalid |
| --- | --- |
| `"N0"`, `"G"`, `"F2"` | Not supported by VarDump integral parser. |
| `"_4"`, `"8"` | Must start with a format letter. |
| `"b8_"`, `"x_"` | Underscore must be followed by digits. |

If parsing fails, `CSharpDumper` and `VisualBasicDumper` throw `FormatException` with `Bad format specifier`.


## Full options example

This example sets every active (non-obsolete) option in one place.

```csharp
using System;
using System.ComponentModel;
using System.Reflection;
using VarDump;
using VarDump.Visitor;
using VarDump.Visitor.Descriptors;
using VarDump.Visitor.Format;

var options = new DumpOptions
{
    CollectionLiteralStyle = CollectionLiteralStyle.Initializer,
    StringLiteralStyle = StringLiteralStyle.Auto,
    ConfigureKnownObjects = (knownObjects, nextDepthVisitor, opts, codeWriter) =>
    {
        // Add custom IKnownObjectVisitor instances here.
    },
    DateKind = DateKind.Original,
    DateTimeInstantiation = DateTimeInstantiation.Parse,
    Descriptors =
    {
        // Add custom descriptor middleware here.
        // Example: new ObjectContentReplacer { Replacement = MyReplacement }
    },
    GenerateVariableInitializer = true,
    GetFieldsBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
    GetBaseClassFields = false,
    GetPropertiesBindingFlags = BindingFlags.Public | BindingFlags.Instance,
    IgnoreDefaultValues = true,
    IgnoreNullValues = true,
    IgnoreReadonlyProperties = true,
    IndentString = "    ",
    IntegralNumericFormat = "d_3",
    MaxCollectionSize = int.MaxValue,
    NewLineStyle = NewLineStyle.Auto,
    MaxDepth = 25,
    PrimitiveCollectionLayout = CollectionLayout.MultiLine,
    SortDirection = ListSortDirection.Ascending,
    UseNamedArgumentsInConstructors = false,
    UsePredefinedConstants = true,
    UsePredefinedMethods = true,
    TypeNamePolicy = TypeNamingPolicy.ShortName
};

var cs = new CSharpDumper(options).Dump(new { Value = 12345 });
var vb = new VisualBasicDumper(options).Dump(new { Value = 12345 });
```

## Extensibility examples

### `Descriptors`

```csharp
using VarDump.Visitor.Descriptors;
using VarDump.Visitor.Descriptors.Specific;

var options = new DumpOptions
{
    Descriptors =
    {
        new ObjectContentReplacer
        {
            Replacement = description => description
        }
    }
};
```

### `ConfigureKnownObjects`

```csharp
using System;
using VarDump.CodeDom.Compiler;

var options = new DumpOptions
{
    ConfigureKnownObjects = (knownObjects, nextDepthVisitor, _, codeWriter) =>
    {
        knownObjects.Add(new GuidVisitor(nextDepthVisitor, codeWriter));
    }
};

sealed class GuidVisitor(INextDepthVisitor nextDepthVisitor, ICodeWriter codeWriter) : IKnownObjectVisitor
{
    public string Id => nameof(Guid);

    public bool IsSuitableFor(object obj, Type objectType) => obj is Guid;

    public void ConfigureOptions(Action<DumpOptions> configure)
    {
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        codeWriter.WriteConstructor(() => codeWriter.WriteType(typeof(Guid)), () =>
            codeWriter.WritePrimitive(obj.ToString()));
    }
}
```

## Reusing options safely

Use `Clone()` to copy current settings before making changes:

```csharp
var baseOptions = new DumpOptions { IgnoreNullValues = false };
var specificOptions = baseOptions.Clone();
specificOptions.MaxDepth = 5;
```
