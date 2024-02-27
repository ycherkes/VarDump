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
    public void DumpFormattableStringCsharp()
    {
        const string name = "World";
        FormattableString str = $"Hello, {name}";

        var opts = new DumpOptions
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

        var dumper = new CSharpDumper(opts);

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

        var opts = new DumpOptions
        {
            Descriptors = { new MemberInfoMiddleware() },
            WritablePropertiesOnly = false
        };

        var dumper = new CSharpDumper(opts);
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
        var opts = new DumpOptions
        {
            Descriptors = { new FileSystemInfoMiddleware() },
            DateKind = DateKind.ConvertToUtc,
            WritablePropertiesOnly = false,
            SortDirection = ListSortDirection.Ascending
        };

        var directoryName = Guid.NewGuid().ToString();

        var dumper = new CSharpDumper(opts);

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
        var opts = new DumpOptions
        {
            Descriptors = { new FileInfoMiddleware() }
        };

        var fileName = $"{Guid.NewGuid()}.txt";

        var dumper = new CSharpDumper(opts);

        var actualString = dumper.Dump(new FileInfo(fileName));

        var expectedFullName = Path.Combine(Directory.GetCurrentDirectory(), fileName).Replace(@"\", @"\\");
        var expectedString = $"var fileInfo = new FileInfo(\"{expectedFullName}\");\r\n";

        Assert.Equal(expectedString, actualString);
    }

    [Fact]
    public void DumpDriveInfoCsharp()
    {
        var opts = new DumpOptions
        {
            Descriptors = { new DriveInfoMiddleware() }
        };

        var driveName = "C:";

        var dumper = new CSharpDumper(opts);

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

        var opts = new DumpOptions
        {
            Descriptors = { new MemberInfoMiddleware() },
            WritablePropertiesOnly = false
        };

        var dumper = new VisualBasicDumper(opts);

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

        public ObjectDescriptionInfo Describe(object @object, Type objectType, Func<ObjectDescriptionInfo> prev)
        {
            var info = prev();

            if (typeof(MemberInfo).IsAssignableFrom(objectType))
            {
                info.Members = info.Members.Where(m => _includeProperties.Contains(m.Name)).ToList();
            }

            return info;
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

        public ObjectDescriptionInfo Describe(object @object, Type objectType, Func<ObjectDescriptionInfo> prev)
        {
            var info = prev();

            if (typeof(FileSystemInfo).IsAssignableFrom(objectType))
            {
                info.Members = info.Members.Where(m => !_excludeProperties.Contains(m.Name)).ToList();
            }

            return info;
        }
    }

    private class FileInfoMiddleware : IObjectDescriptorMiddleware
    {
        public ObjectDescriptionInfo Describe(object @object, Type objectType, Func<ObjectDescriptionInfo> prev)
        {
            if (@object is FileInfo fileInfo)
            {
                return new ObjectDescriptionInfo
                {
                    Type = new CodeDotnetTypeReference(objectType),
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

    private class DriveInfoMiddleware : IObjectDescriptorMiddleware
    {
        public ObjectDescriptionInfo Describe(object @object, Type objectType, Func<ObjectDescriptionInfo> prev)
        {
            if (@object is DriveInfo driveInfo)
            {
                return new ObjectDescriptionInfo
                {
                    Type = new CodeDotnetTypeReference(objectType),
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
    private class FormattableStringMiddleware : IObjectDescriptorMiddleware
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
}