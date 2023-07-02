using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using VarDump;
using VarDump.Visitor;
using VarDump.Visitor.Descriptors;
using Xunit;

namespace UnitTests
{
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

            Assert.Equal(
@$"var eventHandler = new EventHandler
{{
    Method = new RuntimeMethodInfo
    {{
        Name = ""{delegateObject.Method.Name}"",
        DeclaringType = typeof({nameof(ObjectDescriptorMiddlewareSpec)}),
        ReflectedType = typeof({nameof(ObjectDescriptorMiddlewareSpec)}),
        MemberType = MemberTypes.Method,
        Attributes = MethodAttributes.PrivateScope | MethodAttributes.Assembly | MethodAttributes.Static | MethodAttributes.HideBySig
    }}
}};
", result);
        }

        [Fact]
        public void DumpDirectoryInfoCsharp()
        {
            var opts = new DumpOptions
            {
                Descriptors = { new FileSystemInfoMiddleware() },
                DateKind = DateKind.ConvertToUtc,
                WritablePropertiesOnly = false
            };

            var directoryName = Guid.NewGuid().ToString();

            var dumper = new CSharpDumper(opts);

            var actualString = dumper.Dump(new DirectoryInfo(directoryName));

            var expectedFullName = Path.Combine(Directory.GetCurrentDirectory(), directoryName).Replace(@"\", @"\\");
            var expectedString = @$"var directoryInfo = new DirectoryInfo
{{
    FullName = ""{expectedFullName}"",
    Extension = """",
    Name = ""{directoryName}"",
    CreationTime = DateTime.ParseExact(""1601-01-01T00:00:00.0000000Z"", ""O"", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
    CreationTimeUtc = DateTime.ParseExact(""1601-01-01T00:00:00.0000000Z"", ""O"", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
    LastAccessTime = DateTime.ParseExact(""1601-01-01T00:00:00.0000000Z"", ""O"", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
    LastAccessTimeUtc = DateTime.ParseExact(""1601-01-01T00:00:00.0000000Z"", ""O"", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
    LastWriteTime = DateTime.ParseExact(""1601-01-01T00:00:00.0000000Z"", ""O"", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
    LastWriteTimeUtc = DateTime.ParseExact(""1601-01-01T00:00:00.0000000Z"", ""O"", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
    Attributes = FileAttributes.-1
}};
";
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

            Assert.Equal(
@$"Dim eventHandlerValue = New EventHandler With {{
    .Method = New RuntimeMethodInfo With {{
        .Name = ""{delegateObject.Method.Name}"",
        .DeclaringType = GetType({nameof(ObjectDescriptorMiddlewareSpec)}),
        .ReflectedType = GetType({nameof(ObjectDescriptorMiddlewareSpec)}),
        .MemberType = MemberTypes.Method,
        .Attributes = MethodAttributes.PrivateScope Or MethodAttributes.[Assembly] Or MethodAttributes.[Static] Or MethodAttributes.HideBySig
    }}
}}
", result);
        }

        private class MemberInfoMiddleware : IObjectDescriptorMiddleware
        {
            private readonly HashSet<string> _includeProperties = new()
            {
                "Name",
                "DeclaringType",
                "ReflectedType",
                "MemberType",
                "Attributes"
            };

            public IEnumerable<IReflectionDescriptor> Describe(object @object, Type objectType, Func<IEnumerable<IReflectionDescriptor>> prev)
            {
                var members = prev();

                if (typeof(MemberInfo).IsAssignableFrom(objectType))
                {
                    members = members.Where(m => _includeProperties.Contains(m.Name));
                }

                return members;
            }
        }

        private class FileSystemInfoMiddleware : IObjectDescriptorMiddleware
        {
            private readonly HashSet<string> _excludeProperties = new()
            {
                "Parent",
                "Directory",
                "Root"
            };

            public IEnumerable<IReflectionDescriptor> Describe(object @object, Type objectType, Func<IEnumerable<IReflectionDescriptor>> prev)
            {
                var members = prev();

                if (typeof(FileSystemInfo).IsAssignableFrom(objectType))
                {
                    members = members.Where(m => !_excludeProperties.Contains(m.Name));
                }

                return members;
            }
        }

        private class FileInfoMiddleware : IObjectDescriptorMiddleware
        {
            public IEnumerable<IReflectionDescriptor> Describe(object @object, Type objectType, Func<IEnumerable<IReflectionDescriptor>> prev)
            {
                if (@object is FileInfo fileInfo)
                {
                    return new[]
                    {
                        new ReflectionDescriptor(fileInfo.FullName)
                        {
                            ReflectionType = ReflectionType.ConstructorParameter
                        }
                    };
                }

                return prev();
            }
        }

        private class DriveInfoMiddleware : IObjectDescriptorMiddleware
        {
            public IEnumerable<IReflectionDescriptor> Describe(object @object, Type objectType, Func<IEnumerable<IReflectionDescriptor>> prev)
            {
                if (@object is DriveInfo driveInfo)
                {
                    return new[]
                    {
                            new ReflectionDescriptor(driveInfo.Name)
                            {
                                ReflectionType = ReflectionType.ConstructorParameter
                            }
                        };
                }

                return prev();
            }
        }
        private class FormattableStringMiddleware : IObjectDescriptorMiddleware
        {
            public IEnumerable<IReflectionDescriptor> Describe(object @object, Type objectType, Func<IEnumerable<IReflectionDescriptor>> prev)
            {
                if (@object is FormattableString fs)
                {
                    return Descriptor.FromObject(new
                    {
                        fs.Format,
                        Arguments = fs.GetArguments()
                    });
                }

                return prev();
            }
        }
    }
}