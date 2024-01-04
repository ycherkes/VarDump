# [![Made in Ukraine](https://img.shields.io/badge/made_in-ukraine-ffd700.svg?labelColor=0057b7&style=for-the-badge)](https://stand-with-ukraine.pp.ua) [Stand with the people of Ukraine: How to Help](https://stand-with-ukraine.pp.ua)

VarDump is a utility for serialization of runtime objects to C# or Visual Basic string.
===========================================================================================

Developed as a free alternative of [ObjectDumper.NET](https://github.com/thomasgalliker/ObjectDumper), which is not free for commercial use.

[![nuget version](https://img.shields.io/badge/Nuget-v0.2.11-blue)](https://www.nuget.org/packages/VarDump)
[![nuget downloads](https://img.shields.io/nuget/dt/VarDump?label=Downloads)](https://www.nuget.org/packages/VarDump)

```csharp
using System;
using System.Collections.Generic;
using VarDump.Extensions;
using VarDump.Visitor;

var dictionary = new Dictionary<string, Person>
{
    {
        "1234",
        null
    },
    {
        "5678",
        new Person
        {
            Name = "TestName",
            Surname = "TestSurname"
        }
    }
};

Console.WriteLine(dictionary.Dump(DumpOptions.Default));

class Person
{
	public string Name {get; set;}
	public string Surname {get; set;}
}
```
[![Static Badge](https://img.shields.io/badge/Try%20Fiddle-blue)](https://dotnetfiddle.net/ZJzIZy)


# Powered By

| Repository  | License |
| ------------- | ------------- |
| [Heavily customized version](https://github.com/ycherkes/VarDump/tree/main/src/CodeDom) of [System.CodeDom](https://github.com/dotnet/runtime/tree/main/src/libraries/System.CodeDom)  | [![MIT](https://img.shields.io/github/license/dotnet/runtime?style=flat-square)](https://github.com/dotnet/runtime/blob/main/LICENSE.TXT)  |

**Privacy Notice:** No personal data is collected at all.

This tool has been working well for my personal needs, but outside that its future depends on your feedback. Feel free to [open an issue](https://github.com/ycherkes/VarDump/issues).

[![PayPal](https://img.shields.io/badge/Donate-PayPal-ffd700.svg?labelColor=0057b7&style=for-the-badge)](https://www.paypal.com/donate/?business=KXGF7CMW8Y8WJ&no_recurring=0&item_name=Help+VarDump+become+better%21)

Any donations during this time will be directed to local charities at my own discretion.
