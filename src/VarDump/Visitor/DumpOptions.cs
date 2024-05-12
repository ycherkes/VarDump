using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using VarDump.CodeDom.Compiler;
using VarDump.Collections;
using VarDump.Visitor.Descriptors;

namespace VarDump.Visitor;

public class DumpOptions : Dictionary<string, object>
{
    public DumpOptions(Dictionary<string, object> source) : base(source)
    {
        Descriptors ??= [];
        Format ??= new Format();
    }

    public DumpOptions() : this([])
    {
        DateKind = DateKind.Original;
        DateTimeInstantiation = DateTimeInstantiation.Parse;
        GenerateVariableInitializer = true;
        GetPropertiesBindingFlags = BindingFlags.Public | BindingFlags.Instance;
        IgnoreDefaultValues = true;
        IgnoreNullValues = true;
        IndentString = "    ";
        MaxCollectionSize = int.MaxValue;
        MaxDepth = 25;
        WritablePropertiesOnly = true;
    }

    public Action<IKnownObjectsCollection, INextDepthVisitor, DumpOptions, ICodeWriter> ConfigureKnownObjects
    {
        get => TryGetValue(nameof(ConfigureKnownObjects), out var value) ? (Action<IKnownObjectsCollection, INextDepthVisitor, DumpOptions, ICodeWriter>)value : null;
        set => this[nameof(ConfigureKnownObjects)] = value;
    }

    public DateKind DateKind
    {
        get => TryGetValue(nameof(DateKind), out var value) ? (DateKind)value : default;
        set => this[nameof(DateKind)] = value;
    }

    public DateTimeInstantiation DateTimeInstantiation
    {
        get => TryGetValue(nameof(DateTimeInstantiation), out var value) ? (DateTimeInstantiation)value : default;
        set => this[nameof(DateTimeInstantiation)] = value;
    }

    public List<IObjectDescriptorMiddleware> Descriptors
    {
        get => TryGetValue(nameof(Descriptors), out var value) ? (List<IObjectDescriptorMiddleware>)value : default;
        set => this[nameof(Descriptors)] = value;
    }
    public bool GenerateVariableInitializer
    {
        get => TryGetValue(nameof(GenerateVariableInitializer), out var value) && (bool)value;
        set => this[nameof(GenerateVariableInitializer)] = value;
    }

    public BindingFlags? GetFieldsBindingFlags
    {
        get => TryGetValue(nameof(GetFieldsBindingFlags), out var value) ? (BindingFlags?)value : default;
        set => this[nameof(GetFieldsBindingFlags)] = value;
    }

    public BindingFlags GetPropertiesBindingFlags
    {
        get => TryGetValue(nameof(GetPropertiesBindingFlags), out var value) ? (BindingFlags)value : default;
        set => this[nameof(GetPropertiesBindingFlags)] = value;
    }

    public bool IgnoreDefaultValues
    {
        get => TryGetValue(nameof(IgnoreDefaultValues), out var value) && (bool)value;
        set => this[nameof(IgnoreDefaultValues)] = value;
    }
    public bool IgnoreNullValues
    {
        get => TryGetValue(nameof(IgnoreNullValues), out var value) && (bool)value;
        set => this[nameof(IgnoreNullValues)] = value;
    }

    public string IndentString
    {
        get => TryGetValue(nameof(IndentString), out var value) ? (string)value : default;
        set => this[nameof(IndentString)] = value;
    }

    public int MaxCollectionSize
    {
        get => TryGetValue(nameof(MaxCollectionSize), out var value) ? (int)value : default;
        set => this[nameof(MaxCollectionSize)] = value;
    }

    public int MaxDepth
    {
        get => TryGetValue(nameof(MaxDepth), out var value) ? (int)value : default;
        set => this[nameof(MaxDepth)] = value;
    }

    public ListSortDirection? SortDirection
    {
        get => TryGetValue(nameof(SortDirection), out var value) ? (ListSortDirection?)value : default;
        set => this[nameof(SortDirection)] = value;
    }

    public bool UseNamedArgumentsInConstructors
    {
        get => TryGetValue(nameof(UseNamedArgumentsInConstructors), out var value) && (bool)value;
        set => this[nameof(UseNamedArgumentsInConstructors)] = value;
    }

    public bool UseTypeFullName
    {
        get => TryGetValue(nameof(UseTypeFullName), out var value) && (bool)value;
        set => this[nameof(UseTypeFullName)] = value;
    }

    public bool WritablePropertiesOnly
    {
        get => TryGetValue(nameof(WritablePropertiesOnly), out var value) && (bool)value;
        set => this[nameof(WritablePropertiesOnly)] = value;
    }

    public Format Format
    {
        get => TryGetValue(nameof(Format), out var value) ? (Format)value : null;
        set => this[nameof(Format)] = value;
    }

    public DumpOptions Clone()
    {
        return new DumpOptions(this);
    }
}

public class Format : Dictionary<string, object>
{
    public Format(Dictionary<string, object> source): base(source)
    {
    }

    public Format()
    {
    }

    public bool CollectionOfPrimitivesAsSingleLine
    {
        get => TryGetValue(nameof(CollectionOfPrimitivesAsSingleLine), out var value) && value is true;
        set => base[nameof(CollectionOfPrimitivesAsSingleLine)] = value;
    }
}