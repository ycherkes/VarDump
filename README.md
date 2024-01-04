# [![Made in Ukraine](https://img.shields.io/badge/made_in-ukraine-ffd700.svg?labelColor=0057b7&style=for-the-badge)](https://stand-with-ukraine.pp.ua) [Stand with the people of Ukraine: How to Help](https://stand-with-ukraine.pp.ua)

VarDump is a utility for serialization of runtime objects to C# or Visual Basic string.
===========================================================================================

Developed as a free alternative of [ObjectDumper.NET](https://github.com/thomasgalliker/ObjectDumper), which is not free for commercial use.

[![nuget version](https://img.shields.io/badge/Nuget-v0.2.11-blue)](https://www.nuget.org/packages/VarDump)
[![nuget downloads](https://img.shields.io/nuget/dt/VarDump?label=Downloads)](https://www.nuget.org/packages/VarDump)

# C# Dumper, Default Options:
<p align="right"><a href="https://dotnetfiddle.net/4ZA1wG">Run .NET fiddle</a></p>

```csharp
using System;
using VarDump;

var anonymousObject = new
{
    Name = "Name",
    Surname = "Surname"
};

var dumper = new CSharpDumper();
Console.WriteLine(dumper.Dump(anonymousObject));
```


# Extension methods example:
<p align="right"><a href="https://dotnetfiddle.net/BRKwPK">Run .NET fiddle</a></p>

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
    },
    new
    {
        Name = "Name2",
        Surname = "Surname2"
    }
}.ToDictionary(x => x.Name, x => x);

Console.WriteLine(dictionary.Dump(DumpOptions.Default));
```


# Powered By

| Repository  | License |
| ------------- | ------------- |
| [Heavily customized version](https://github.com/ycherkes/VarDump/tree/main/src/CodeDom) of [System.CodeDom](https://github.com/dotnet/runtime/tree/main/src/libraries/System.CodeDom)  | [![MIT](https://img.shields.io/github/license/dotnet/runtime?style=flat-square)](https://github.com/dotnet/runtime/blob/main/LICENSE.TXT)  |

**Privacy Notice:** No personal data is collected at all.

This tool has been working well for my personal needs, but outside that its future depends on your feedback. Feel free to [open an issue](https://github.com/ycherkes/VarDump/issues).

[![PayPal](https://img.shields.io/badge/Donate-PayPal-ffd700.svg?labelColor=0057b7&style=for-the-badge)](https://www.paypal.com/donate/?business=KXGF7CMW8Y8WJ&no_recurring=0&item_name=Help+VarDump+become+better%21)

Any donations during this time will be directed to local charities at my own discretion.
