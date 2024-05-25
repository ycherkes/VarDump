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
    public bool IgnoreReadonlyProperties { get; set; } = true;
    public string IndentString { get; set; } = "    ";
    public string IntegralNumericFormat { get; set; } = "";
    public int MaxCollectionSize { get; set; } = int.MaxValue;
    public int MaxDepth { get; set; } = 25;
    public CollectionLayout PrimitiveCollectionLayout { get; set; }
    public ListSortDirection? SortDirection { get; set; }
    public bool UseNamedArgumentsInConstructors { get; set; }
    public bool UsePredefinedConstants { get; set; } = true;
    public bool UsePredefinedMethods { get; set; } = true;
    public bool UseTypeFullName { get; set; }

    [Obsolete("Use IgnoreReadonlyProperties instead")]
    public bool WritablePropertiesOnly
    {
        get => IgnoreReadonlyProperties;
        set => IgnoreReadonlyProperties = value;
    }

    public DumpOptions Clone()
    {
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
            IgnoreReadonlyProperties = IgnoreReadonlyProperties,
            IndentString = IndentString,
            IntegralNumericFormat = IntegralNumericFormat,
            MaxCollectionSize = MaxCollectionSize,
            MaxDepth = MaxDepth,
            PrimitiveCollectionLayout = PrimitiveCollectionLayout,
            SortDirection = SortDirection,
            UseNamedArgumentsInConstructors = UseNamedArgumentsInConstructors,
            UsePredefinedConstants = UsePredefinedConstants,
            UsePredefinedMethods = UsePredefinedMethods,
            UseTypeFullName = UseTypeFullName,
        };
    }
}