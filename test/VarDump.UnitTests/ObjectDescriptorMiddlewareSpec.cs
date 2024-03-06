using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using VarDump.CodeDom.Common;
using VarDump.Visitor;
using VarDump.Visitor.Descriptors;
using Xunit;

namespace VarDump.UnitTests;

public class ObjectDescriptorMiddlewareSpec
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
            Descriptors = { new CardNumberSkippingMiddleware() }
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
            Descriptors = { new CardNumberMaskingMiddleware() }
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
            Descriptors =
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
            Descriptors = { new MemberInfoMiddleware() },
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
                      DeclaringType = typeof({{nameof(ObjectDescriptorMiddlewareSpec)}}),
                      ReflectedType = typeof({{nameof(ObjectDescriptorMiddlewareSpec)}}),
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
                      DeclaringType = typeof({{nameof(ObjectDescriptorMiddlewareSpec)}}),
                      ReflectedType = typeof({{nameof(ObjectDescriptorMiddlewareSpec)}}),
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
            Descriptors = { new FileSystemInfoMiddleware() },
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
            Descriptors = { new FileInfoMiddleware() }
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
            Descriptors = { new DriveInfoMiddleware() }
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
            Descriptors = { new MemberInfoMiddleware() },
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
                      .DeclaringType = GetType({{nameof(ObjectDescriptorMiddlewareSpec)}}),
                      .ReflectedType = GetType({{nameof(ObjectDescriptorMiddlewareSpec)}}),
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
                      .DeclaringType = GetType({{nameof(ObjectDescriptorMiddlewareSpec)}}),
                      .ReflectedType = GetType({{nameof(ObjectDescriptorMiddlewareSpec)}}),
                      .MemberType = MemberTypes.Method,
                      .Attributes = MethodAttributes.PrivateScope Or MethodAttributes.[Assembly] Or MethodAttributes.[Static] Or MethodAttributes.HideBySig
                  }
              }

              """, result);

#endif
    }

    private class MemberInfoMiddleware : IObjectDescriptorMiddleware
    {
        private readonly HashSet<string> _includeProperties =
        [
            "Name",
            "DeclaringType",
            "ReflectedType",
            "MemberType",
            "Attributes"
        ];

        public IObjectDescription GetObjectDescription(object @object, Type objectType, Func<IObjectDescription> prev)
        {
            var objectInfo = prev();

            if (typeof(MemberInfo).IsAssignableFrom(objectType))
            {
                objectInfo.Members = objectInfo.Members.Where(m => _includeProperties.Contains(m.Name)).ToList();
            }

            return objectInfo;
        }
    }

    private class FileSystemInfoMiddleware : IObjectDescriptorMiddleware
    {
        private readonly HashSet<string> _excludeProperties =
        [
            "Parent",
            "Directory",
            "Root"
        ];

        public IObjectDescription GetObjectDescription(object @object, Type objectType, Func<IObjectDescription> prev)
        {
            var objectDescription = prev();

            if (typeof(FileSystemInfo).IsAssignableFrom(objectType))
            {
                objectDescription.Members = objectDescription.Members.Where(m => !_excludeProperties.Contains(m.Name)).ToList();
            }

            return objectDescription;
        }
    }

    private class FileInfoMiddleware : IObjectDescriptorMiddleware
    {
        public IObjectDescription GetObjectDescription(object @object, Type objectType, Func<IObjectDescription> prev)
        {
            if (@object is FileInfo fileInfo)
            {
                return new ObjectDescription
                {
                    Type = new CodeTypeInfo(objectType),
                    ConstructorParameters = new []
                    {
                        new ReflectionDescription(fileInfo.FullName)
                        {
                            ReflectionType = ReflectionType.ConstructorParameter
                        }
                    }
                };
            }

            return prev();
        }
    }

    private class DriveInfoMiddleware : IObjectDescriptorMiddleware
    {
        public IObjectDescription GetObjectDescription(object @object, Type objectType, Func<IObjectDescription> prev)
        {
            if (@object is DriveInfo driveInfo)
            {
                return new ObjectDescription
                {
                    Type = new CodeTypeInfo(objectType),
                    ConstructorParameters = new []
                    {
                        new ReflectionDescription(driveInfo.Name)
                        {
                            ReflectionType = ReflectionType.ConstructorParameter
                        }
                    }
                };
            }

            return prev();
        }
    }
    private class FormattableStringMiddleware : IObjectDescriptorMiddleware
    {
        public IObjectDescription GetObjectDescription(object @object, Type objectType, Func<IObjectDescription> prev)
        {
            if (@object is FormattableString fs)
            {
                return ObjectDescription.FromObject(new
                {
                    fs.Format,
                    Arguments = fs.GetArguments()
                }, objectType);
            }

            return prev();
        }
    }

    private class CardNumberSkippingMiddleware : IObjectDescriptorMiddleware
    {
        public IObjectDescription GetObjectDescription(object @object, Type objectType, Func<IObjectDescription> prev)
        {
            var objectDescription = prev();

            return new ObjectDescription
            {
                Type = objectDescription.Type,
                ConstructorParameters = objectDescription.ConstructorParameters,
                Members = objectDescription.Members.Where(memberDescriptor => !string.Equals(memberDescriptor.Name, "cardnumber", StringComparison.OrdinalIgnoreCase))
            };
        }
    }

    private class CardNumberMaskingMiddleware : IObjectDescriptorMiddleware
    {
        public IObjectDescription GetObjectDescription(object @object, Type objectType, Func<IObjectDescription> prev)
        {
            var objectDescription = prev();

            return new ObjectDescription
            {
                Type = objectDescription.Type,
                ConstructorParameters = objectDescription.ConstructorParameters,
                Members = objectDescription.Members.Select(ReplaceCardNumberDescriptor)
            };
        }

        private static IReflectionDescription ReplaceCardNumberDescriptor(IReflectionDescription memberDescription)
        {
            if (memberDescription.Type != typeof(string) 
                || !string.Equals(memberDescription.Name, "cardnumber", StringComparison.OrdinalIgnoreCase) 
                || string.IsNullOrWhiteSpace((string)memberDescription.Value))
            {
                return memberDescription;
            }

            var stringValue = (string)memberDescription.Value;

            var maskedValue = stringValue.Length - 4 > 0
                    ? new string('*', stringValue.Length - 4) + stringValue.Substring(stringValue.Length - 4)
                    : stringValue;

            return new ReflectionDescription(maskedValue)
            {
                Name = memberDescription.Name, 
                Type = memberDescription.Type, 
                ReflectionType = memberDescription.ReflectionType
            };
        }
    }
}