using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using VarDump.CodeDom.Compiler;
using VarDump.Collections;
using VarDump.Visitor.Descriptors;
using VarDump.Visitor.KnownTypes;

namespace VarDump.Visitor;

public class DumpOptions
{
    public Action<IOrderedDictionary<string, IKnownObjectVisitor>, IObjectVisitor, DumpOptions, ICodeWriter> ConfigureKnownTypes { get; set; }
    public DateKind DateKind { get; set; } = DateKind.Original;
    public DateTimeInstantiation DateTimeInstantiation { get; set; } = DateTimeInstantiation.Parse;
    public ICollection<IObjectDescriptorMiddleware> Descriptors { get; set; } = [];
    public ICollection<string> ExcludeTypes { get; set; } = [];
    public bool GenerateVariableInitializer { get; set; } = true;
    public BindingFlags? GetFieldsBindingFlags { get; set; }
    public BindingFlags GetPropertiesBindingFlags { get; set; } = BindingFlags.Public | BindingFlags.Instance;
    public bool IgnoreDefaultValues { get; set; } = true;
    public bool IgnoreNullValues { get; set; } = true;
    public string IndentString { get; set; } = "    ";
    public int MaxCollectionSize { get; set; } = int.MaxValue;
    public int MaxDepth { get; set; } = 25;
    public ListSortDirection? SortDirection { get; set; }
    public bool UseNamedArgumentsForReferenceRecordTypes { get; set; }
    public bool UseTypeFullName { get; set; }
    public bool WritablePropertiesOnly { get; set; } = true;

    public DumpOptions Clone()
    {
        return new DumpOptions
        {
            ConfigureKnownTypes = ConfigureKnownTypes,
            DateKind = DateKind,
            DateTimeInstantiation = DateTimeInstantiation,
            Descriptors = Descriptors.ToArray(),
            ExcludeTypes = ExcludeTypes?.ToArray() ?? [],
            GenerateVariableInitializer = GenerateVariableInitializer,
            GetFieldsBindingFlags = GetFieldsBindingFlags,
            GetPropertiesBindingFlags = GetPropertiesBindingFlags,
            IgnoreDefaultValues = IgnoreDefaultValues,
            IgnoreNullValues = IgnoreNullValues,
            IndentString = IndentString,
            MaxCollectionSize = MaxCollectionSize,
            MaxDepth = MaxDepth,
            SortDirection = SortDirection,
            UseNamedArgumentsForReferenceRecordTypes = UseNamedArgumentsForReferenceRecordTypes,
            UseTypeFullName = UseTypeFullName,
            WritablePropertiesOnly = WritablePropertiesOnly
        };
    }
}