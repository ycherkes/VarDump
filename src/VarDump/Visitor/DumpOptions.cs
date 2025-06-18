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
    /// <summary>
    /// Configure the known objects collection.
    /// </summary>
    public Action<IKnownObjectsCollection, INextDepthVisitor, DumpOptions, ICodeWriter> ConfigureKnownObjects { get; set; }

    /// <summary>
    /// The kind of date to output when dumping, default is <see cref="DateKind.Original"/>.
    /// </summary>
    public DateKind DateKind { get; set; } = DateKind.Original;

    /// <summary>
    /// The way to instantiate <see cref="DateTime"/> objects, default is <see cref="DateTimeInstantiation.Parse"/>.
    /// </summary>
    public DateTimeInstantiation DateTimeInstantiation { get; set; } = DateTimeInstantiation.Parse;

    /// <summary>
    /// The descriptors to use when dumping objects. Default is an empty list.
    /// </summary>
    public List<IObjectDescriptorMiddleware> Descriptors { get; set; } = [];

    /// <summary>
    /// Generate variable initializer for the dumped object, default is <c>true</c>.
    /// </summary>
    public bool GenerateVariableInitializer { get; set; } = true;

    /// <summary>
    /// The binding flags to use when getting fields, default is <c>null</c>.
    /// </summary>
    public BindingFlags? GetFieldsBindingFlags { get; set; }

    /// <summary>
    /// Enabling this option instructs the visitor visit base types.
    /// </summary>
    public bool GetBaseClassFields { get; set; }

    /// <summary>
    /// The binding flags to use when getting properties, default is <see cref="BindingFlags.Public"/> | <see cref="BindingFlags.Instance"/>.
    /// </summary>
    public BindingFlags GetPropertiesBindingFlags { get; set; } = BindingFlags.Public | BindingFlags.Instance;

    /// <summary>
    /// Ignore default values when dumping, default is <c>true</c>.
    /// </summary>
    public bool IgnoreDefaultValues { get; set; } = true;

    /// <summary>
    /// Ignore null values when dumping, default is <c>true</c>.
    /// </summary>
    public bool IgnoreNullValues { get; set; } = true;

    /// <summary>
    /// Ignore readonly properties when dumping, default is <c>true</c>.
    /// </summary>
    public bool IgnoreReadonlyProperties { get; set; } = true;

    /// <summary>
    /// The string to use for indentation, default is four spaces.
    /// </summary>
    public string IndentString { get; set; } = "    ";

    /// <summary>
    /// The format to use for integral numeric values, default is an empty string.
    /// </summary>
    public string IntegralNumericFormat { get; set; } = "";

    /// <summary>
    /// The maximum collection size to dump, default is <see cref="int.MaxValue"/>.
    /// </summary>
    public int MaxCollectionSize { get; set; } = int.MaxValue;

    /// <summary>
    /// The maximum depth to dump, default is <c>25</c>.
    /// </summary>
    public int MaxDepth { get; set; } = 25;

    /// <summary>
    /// The layout to use for primitive collections, default is <see cref="CollectionLayout.MultiLine"/>.
    /// </summary>
    public CollectionLayout PrimitiveCollectionLayout { get; set; } = CollectionLayout.MultiLine;

    /// <summary>
    /// The sort direction to use when sorting properties and fields, default is <c>null</c>.
    /// </summary>
    public ListSortDirection? SortDirection { get; set; }

    /// <summary>
    /// Use named arguments in constructors, default is <c>false</c>.
    /// </summary>
    public bool UseNamedArgumentsInConstructors { get; set; }

    /// <summary>
    /// Use predefined constants, default is <c>true</c>.
    /// </summary>
    public bool UsePredefinedConstants { get; set; } = true;

    /// <summary>
    /// Use predefined methods, default is <c>true</c>.
    /// </summary>
    public bool UsePredefinedMethods { get; set; } = true;

    /// <summary>
    /// Use the full name of the type when dumping, default is <c>false</c>.
    /// </summary>
    [Obsolete("Use NamingPolicy instead")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool UseTypeFullName
    {
        get => NamingPolicy == TypeNamingPolicy.FullName;
        set => NamingPolicy = value ? TypeNamingPolicy.FullName : TypeNamingPolicy.ShortName;
    }

    /// <summary>
    /// 
    /// </summary>
    public TypeNamingPolicy NamingPolicy { get; set; } = TypeNamingPolicy.ShortName;

    /// <summary>
    /// Gets or sets a value indicating whether to dump only writable properties.
    /// </summary>
    [Obsolete("Use IgnoreReadonlyProperties instead")]
    [EditorBrowsable(EditorBrowsableState.Never)]
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
            GetBaseClassFields = GetBaseClassFields,
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
            NamingPolicy = NamingPolicy
        };
    }
}