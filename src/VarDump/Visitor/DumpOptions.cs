using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using VarDump.CodeDom.Compiler;
using VarDump.Collections;
using VarDump.Visitor.Descriptors;
using VarDump.Visitor.Format;

namespace VarDump.Visitor;

public class DumpOptions
{
    public Action<IKnownObjectsCollection, INextDepthVisitor, DumpOptions, ICodeWriter> ConfigureKnownObjects { get; set; }
    public DateKind DateKind { get; set; } = DateKind.Original;
    public DateTimeInstantiation DateTimeInstantiation { get; set; } = DateTimeInstantiation.Parse;
    public List<IObjectDescriptorMiddleware> Descriptors { get; set; } = [];
    public bool GenerateVariableInitializer { get; set; } = true;
    public BindingFlags? GetFieldsBindingFlags { get; set; }
    public BindingFlags GetPropertiesBindingFlags { get; set; } = BindingFlags.Public | BindingFlags.Instance;
    public bool IgnoreDefaultValues { get; set; } = true;
    public bool IgnoreNullValues { get; set; } = true;
    public string IndentString { get; set; } = "    ";
    public int MaxCollectionSize { get; set; } = int.MaxValue;
    public int MaxDepth { get; set; } = 25;
    public ListSortDirection? SortDirection { get; set; }
    public bool UseNamedArgumentsInConstructors { get; set; }
    public bool UseTypeFullName { get; set; }
    public bool WritablePropertiesOnly { get; set; } = true;
    public Formatting Formatting { get; set; } = new Formatting();

    public DumpOptions Clone()
    {
        var formatting = Formatting ?? new Formatting();

        return new DumpOptions
        {
            ConfigureKnownObjects = ConfigureKnownObjects,
            DateKind = DateKind,
            DateTimeInstantiation = DateTimeInstantiation,
            Descriptors = Descriptors?.ToList() ?? [],
            GenerateVariableInitializer = GenerateVariableInitializer,
            GetFieldsBindingFlags = GetFieldsBindingFlags,
            GetPropertiesBindingFlags = GetPropertiesBindingFlags,
            IgnoreDefaultValues = IgnoreDefaultValues,
            IgnoreNullValues = IgnoreNullValues,
            IndentString = IndentString,
            MaxCollectionSize = MaxCollectionSize,
            MaxDepth = MaxDepth,
            SortDirection = SortDirection,
            UseNamedArgumentsInConstructors = UseNamedArgumentsInConstructors,
            UseTypeFullName = UseTypeFullName,
            WritablePropertiesOnly = WritablePropertiesOnly,
            Formatting = new Formatting
            {
                PrimitiveCollectionLayout = formatting.PrimitiveCollectionLayout,
                IntegralNumericFormat = formatting.IntegralNumericFormat
            }
        };
    }
}

public class Formatting
{
    public CollectionLayout PrimitiveCollectionLayout { get; set; }
    public string IntegralNumericFormat { get; set; } = "";
}