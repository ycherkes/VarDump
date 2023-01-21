using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace VarDump.Visitor
{
    public class DumpOptions
    {
        public BindingFlags GetPropertiesBindingFlags { get; set; }
        public BindingFlags? GetFieldsBindingFlags { get; set; }
        public bool IgnoreDefaultValues { get; set; }
        public bool IgnoreNullValues { get; set; }
        public bool UseNamedArgumentsForReferenceRecordTypes { get; set; }
        public bool UseTypeFullName { get; set; }
        public bool WritablePropertiesOnly { get; set; } = true;
        public DateKind DateKind { get; set; }
        public DateTimeInstantiation DateTimeInstantiation { get; set; }
        public ICollection<string> ExcludeTypes { get; set; }
        public int MaxDepth { get; set; }
        public ListSortDirection? SortDirection { get; set; }

        public static DumpOptions Default { get; } = new()
        {
            DateKind = DateKind.ConvertToUtc,
            DateTimeInstantiation = DateTimeInstantiation.New,
            ExcludeTypes = new string[0],
            GetPropertiesBindingFlags = BindingFlags.Instance | BindingFlags.Public,
            IgnoreDefaultValues = true,
            IgnoreNullValues = true,
            MaxDepth = 25,
            UseNamedArgumentsForReferenceRecordTypes = false,
            UseTypeFullName = false,
            WritablePropertiesOnly = true
        };

        public DumpOptions Clone()
        {
            return new DumpOptions
            {
                DateKind = DateKind,
                DateTimeInstantiation = DateTimeInstantiation,
                ExcludeTypes = ExcludeTypes?.ToArray() ?? new string[0],
                GetFieldsBindingFlags = GetFieldsBindingFlags,
                GetPropertiesBindingFlags = GetPropertiesBindingFlags,
                IgnoreDefaultValues = IgnoreDefaultValues,
                IgnoreNullValues = IgnoreNullValues,
                MaxDepth = MaxDepth,
                SortDirection = SortDirection,
                UseNamedArgumentsForReferenceRecordTypes = UseNamedArgumentsForReferenceRecordTypes,
                UseTypeFullName = UseTypeFullName,
                WritablePropertiesOnly = WritablePropertiesOnly
            };
        }
    }
}
