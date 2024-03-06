using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using VarDump.CodeDom.Common;
using VarDump.Visitor;
using VarDump.Visitor.Describers;
using Xunit;

namespace VarDump.UnitTests;

public class ObjectDescriberMiddlewareSpec
{
    [Fact]
    public void DumpObjectSkipWritingCardNumberCsharp()
    {
        var obj = new
        {
            FullName = "BRUCE LEE",
            CardNumber = "4953089013607",
            OtherInfo = new
            {
                FullName = "BRUCE LEE",
                CardNumber = "5201294442453002",
            }
        };

        var options = new DumpOptions
        {
            Describers = { new CardNumberSkippingMiddleware() }
        };

        var dumper = new CSharpDumper(options);

        var result = dumper.Dump(obj);

        Assert.Equal(
            """
            var anonymousType = new 
            {
                FullName = "BRUCE LEE",
                OtherInfo = new 
                {
                    FullName = "BRUCE LEE"
                }
            };
            
            """, result);
    }

    [Fact]
    public void DumpObjectMaskCardNumberCsharp()
    {
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
            Describers = { new CardNumberMaskingMiddleware() }
        };

        var dumper = new CSharpDumper(options);

        var result = dumper.Dump(obj);

        Assert.Equal(
            """
            var anonymousType = new 
            {
                FullName = "BRUCE LEE",
                CardNumber = "*********3607",
                OtherInfo = new 
                {
                    CardNumber = "************3002"
                }
            };
            
            """, result);
    }

    [Fact]
    public void DumpFormattableStringCsharp()
    {
        const string name = "World";
        FormattableString str = $"Hello, {name}";

        var options = new DumpOptions
        {
            Describers =
            {
                new FileSystemInfoMiddleware(),
                new FileInfoMiddleware(),
                new DriveInfoMiddleware(),
                new MemberInfoMiddleware(),
                new FormattableStringMiddleware()
            }
        };

        var dumper = new CSharpDumper(options);

        var result = dumper.Dump(str);

        Assert.Equal(
            @"var concreteFormattableString = new ConcreteFormattableString
{
    Format = ""Hello, {0}"",
    Arguments = new object[]
    {
        ""World""
    }
};
", result);
    }

    [Fact]
    public void DumpDelegateCsharp()
    {
        static void EventHandler(object sender, EventArgs args)
        {
        }

        var options = new DumpOptions
        {
            Describers = { new MemberInfoMiddleware() },
            WritablePropertiesOnly = false
        };

        var dumper = new CSharpDumper(options);
        var delegateObject = (Delegate)(EventHandler)EventHandler;

        var result = dumper.Dump(delegateObject);

#if NET8_0_OR_GREATER

        Assert.Equal(
            $$"""
              var eventHandler = new EventHandler
              {
                  Method = new RuntimeMethodInfo
                  {
                      Name = "{{delegateObject.Method.Name}}",
                      DeclaringType = typeof({{nameof(ObjectDescriberMiddlewareSpec)}}),
                      ReflectedType = typeof({{nameof(ObjectDescriberMiddlewareSpec)}}),
                      MemberType = MemberTypes.Method,
                      Attributes = MethodAttributes.Assembly | MethodAttributes.Static | MethodAttributes.HideBySig
                  }
              };

              """, result);

#else

        Assert.Equal(
            $$"""
              var eventHandler = new EventHandler
              {
                  Method = new RuntimeMethodInfo
                  {
                      Name = "{{delegateObject.Method.Name}}",
                      DeclaringType = typeof({{nameof(ObjectDescriberMiddlewareSpec)}}),
                      ReflectedType = typeof({{nameof(ObjectDescriberMiddlewareSpec)}}),
                      MemberType = MemberTypes.Method,
                      Attributes = MethodAttributes.PrivateScope | MethodAttributes.Assembly | MethodAttributes.Static | MethodAttributes.HideBySig
                  }
              };

              """, result);

#endif
    }

    [Fact]
    public void DumpDirectoryInfoCsharp()
    {
        var options = new DumpOptions
        {
            Describers = { new FileSystemInfoMiddleware() },
            DateKind = DateKind.ConvertToUtc,
            WritablePropertiesOnly = false,
            SortDirection = ListSortDirection.Ascending
        };

        var directoryName = Guid.NewGuid().ToString();

        var dumper = new CSharpDumper(options);

        var actualString = dumper.Dump(new DirectoryInfo(directoryName));

        var expectedFullName = Path.Combine(Directory.GetCurrentDirectory(), directoryName).Replace(@"\", @"\\");

#if NET8_0_OR_GREATER

        var expectedString = $$"""
                               var directoryInfo = new DirectoryInfo
                               {
                                   Attributes = FileAttributes.-1,
                                   CreationTime = DateTime.ParseExact("1601-01-01T00:00:00.0000000Z", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                                   CreationTimeUtc = DateTime.ParseExact("1601-01-01T00:00:00.0000000Z", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                                   Extension = "",
                                   FullName = "{{expectedFullName}}",
                                   LastAccessTime = DateTime.ParseExact("1601-01-01T00:00:00.0000000Z", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                                   LastAccessTimeUtc = DateTime.ParseExact("1601-01-01T00:00:00.0000000Z", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                                   LastWriteTime = DateTime.ParseExact("1601-01-01T00:00:00.0000000Z", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                                   LastWriteTimeUtc = DateTime.ParseExact("1601-01-01T00:00:00.0000000Z", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                                   Name = "{{directoryName}}",
                                   UnixFileMode = UnixFileMode.-1
                               };

                               """;

#else

        var expectedString = $$"""
                               var directoryInfo = new DirectoryInfo
                               {
                                   Attributes = FileAttributes.-1,
                                   CreationTime = DateTime.ParseExact("1601-01-01T00:00:00.0000000Z", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                                   CreationTimeUtc = DateTime.ParseExact("1601-01-01T00:00:00.0000000Z", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                                   Extension = "",
                                   FullName = "{{expectedFullName}}",
                                   LastAccessTime = DateTime.ParseExact("1601-01-01T00:00:00.0000000Z", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                                   LastAccessTimeUtc = DateTime.ParseExact("1601-01-01T00:00:00.0000000Z", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                                   LastWriteTime = DateTime.ParseExact("1601-01-01T00:00:00.0000000Z", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                                   LastWriteTimeUtc = DateTime.ParseExact("1601-01-01T00:00:00.0000000Z", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                                   Name = "{{directoryName}}"
                               };

                               """;

#endif

        Assert.Equal(expectedString, actualString);
    }

    [Fact]
    public void DumpFileInfoCsharp()
    {
        var options = new DumpOptions
        {
            Describers = { new FileInfoMiddleware() }
        };

        var fileName = $"{Guid.NewGuid()}.txt";

        var dumper = new CSharpDumper(options);

        var actualString = dumper.Dump(new FileInfo(fileName));

        var expectedFullName = Path.Combine(Directory.GetCurrentDirectory(), fileName).Replace(@"\", @"\\");
        var expectedString = $"var fileInfo = new FileInfo(\"{expectedFullName}\");\r\n";

        Assert.Equal(expectedString, actualString);
    }

    [Fact]
    public void DumpDriveInfoCsharp()
    {
        var options = new DumpOptions
        {
            Describers = { new DriveInfoMiddleware() }
        };

        var driveName = "C:";

        var dumper = new CSharpDumper(options);

        var actualString = dumper.Dump(new DriveInfo(driveName));

        var expectedString = "var driveInfo = new DriveInfo(\"C:\\\\\");\r\n";

        Assert.Equal(expectedString, actualString);
    }


    [Fact]
    public void DumpDelegateVb()
    {
        static void EventHandler(object sender, EventArgs args)
        {
        }

        var options = new DumpOptions
        {
            Describers = { new MemberInfoMiddleware() },
            WritablePropertiesOnly = false
        };

        var dumper = new VisualBasicDumper(options);

        var delegateObject = (Delegate)(EventHandler)EventHandler;

        var result = dumper.Dump(delegateObject);

#if NET8_0_OR_GREATER

        Assert.Equal(
            $$"""
              Dim eventHandlerValue = New EventHandler With {
                  .Method = New RuntimeMethodInfo With {
                      .Name = "{{delegateObject.Method.Name}}",
                      .DeclaringType = GetType({{nameof(ObjectDescriberMiddlewareSpec)}}),
                      .ReflectedType = GetType({{nameof(ObjectDescriberMiddlewareSpec)}}),
                      .MemberType = MemberTypes.Method,
                      .Attributes = MethodAttributes.[Assembly] Or MethodAttributes.[Static] Or MethodAttributes.HideBySig
                  }
              }

              """, result);

#else

        Assert.Equal(
            $$"""
              Dim eventHandlerValue = New EventHandler With {
                  .Method = New RuntimeMethodInfo With {
                      .Name = "{{delegateObject.Method.Name}}",
                      .DeclaringType = GetType({{nameof(ObjectDescriberMiddlewareSpec)}}),
                      .ReflectedType = GetType({{nameof(ObjectDescriberMiddlewareSpec)}}),
                      .MemberType = MemberTypes.Method,
                      .Attributes = MethodAttributes.PrivateScope Or MethodAttributes.[Assembly] Or MethodAttributes.[Static] Or MethodAttributes.HideBySig
                  }
              }

              """, result);

#endif
    }

    private class MemberInfoMiddleware : IObjectDescriberMiddleware
    {
        private readonly HashSet<string> _includeProperties =
        [
            "Name",
            "DeclaringType",
            "ReflectedType",
            "MemberType",
            "Attributes"
        ];

        public ObjectDescriptor DescribeObject(object @object, Type objectType, Func<ObjectDescriptor> prev)
        {
            var objectDescriptor = prev();

            if (typeof(MemberInfo).IsAssignableFrom(objectType))
            {
                objectDescriptor.Members = objectDescriptor.Members.Where(m => _includeProperties.Contains(m.Name)).ToList();
            }

            return objectDescriptor;
        }
    }

    private class FileSystemInfoMiddleware : IObjectDescriberMiddleware
    {
        private readonly HashSet<string> _excludeProperties =
        [
            "Parent",
            "Directory",
            "Root"
        ];

        public ObjectDescriptor DescribeObject(object @object, Type objectType, Func<ObjectDescriptor> prev)
        {
            var objectDescriptor = prev();

            if (typeof(FileSystemInfo).IsAssignableFrom(objectType))
            {
                objectDescriptor.Members = objectDescriptor.Members.Where(m => !_excludeProperties.Contains(m.Name)).ToList();
            }

            return objectDescriptor;
        }
    }

    private class FileInfoMiddleware : IObjectDescriberMiddleware
    {
        public ObjectDescriptor DescribeObject(object @object, Type objectType, Func<ObjectDescriptor> prev)
        {
            if (@object is FileInfo fileInfo)
            {
                return new ObjectDescriptor
                {
                    Type = new CodeTypeInfo(objectType),
                    ConstructorParameters = new []
                    {
                        new ReflectionDescriptor(fileInfo.FullName)
                        {
                            ReflectionType = ReflectionType.ConstructorParameter
                        }
                    }
                };
            }

            return prev();
        }
    }

    private class DriveInfoMiddleware : IObjectDescriberMiddleware
    {
        public ObjectDescriptor DescribeObject(object @object, Type objectType, Func<ObjectDescriptor> prev)
        {
            if (@object is DriveInfo driveInfo)
            {
                return new ObjectDescriptor
                {
                    Type = new CodeTypeInfo(objectType),
                    ConstructorParameters = new []
                    {
                        new ReflectionDescriptor(driveInfo.Name)
                        {
                            ReflectionType = ReflectionType.ConstructorParameter
                        }
                    }
                };
            }

            return prev();
        }
    }
    private class FormattableStringMiddleware : IObjectDescriberMiddleware
    {
        public ObjectDescriptor DescribeObject(object @object, Type objectType, Func<ObjectDescriptor> prev)
        {
            if (@object is FormattableString fs)
            {
                return ObjectDescriptor.FromObject(new
                {
                    fs.Format,
                    Arguments = fs.GetArguments()
                }, objectType);
            }

            return prev();
        }
    }

    private class CardNumberSkippingMiddleware : IObjectDescriberMiddleware
    {
        public ObjectDescriptor DescribeObject(object @object, Type objectType, Func<ObjectDescriptor> prev)
        {
            var objectDescriptor = prev();

            return new ObjectDescriptor
            {
                Type = objectDescriptor.Type,
                ConstructorParameters = objectDescriptor.ConstructorParameters,
                Members = objectDescriptor.Members.Where(memberDescriptor => !string.Equals(memberDescriptor.Name, "cardnumber", StringComparison.OrdinalIgnoreCase))
            };
        }
    }

    private class CardNumberMaskingMiddleware : IObjectDescriberMiddleware
    {
        public ObjectDescriptor DescribeObject(object @object, Type objectType, Func<ObjectDescriptor> prev)
        {
            var objectDescriptor = prev();

            return new ObjectDescriptor
            {
                Type = objectDescriptor.Type,
                ConstructorParameters = objectDescriptor.ConstructorParameters,
                Members = objectDescriptor.Members.Select(ReplaceCardNumberDescriptor)
            };
        }

        private static IReflectionDescriptor ReplaceCardNumberDescriptor(IReflectionDescriptor memberDescriptor)
        {
            if (memberDescriptor.Type != typeof(string) 
                || !string.Equals(memberDescriptor.Name, "cardnumber", StringComparison.OrdinalIgnoreCase) 
                || string.IsNullOrWhiteSpace((string)memberDescriptor.Value))
            {
                return memberDescriptor;
            }

            var stringValue = (string)memberDescriptor.Value;

            var maskedValue = stringValue.Length - 4 > 0
                    ? new string('*', stringValue.Length - 4) + stringValue.Substring(stringValue.Length - 4)
                    : stringValue;

            return new ReflectionDescriptor(maskedValue)
            {
                Name = memberDescriptor.Name, 
                Type = memberDescriptor.Type, 
                ReflectionType = memberDescriptor.ReflectionType
            };
        }
    }
}