using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace VarDump.Visitor
{
    public class DumpOptions
    {
        public BindingFlags GetPropertiesBindingFlags { get; set; } = BindingFlags.Public | BindingFlags.Instance;
        public BindingFlags? GetFieldsBindingFlags { get; set; }
        public bool IgnoreDefaultValues { get; set; } = true;
        public bool IgnoreNullValues { get; set; } = true;
        public bool UseNamedArgumentsForReferenceRecordTypes { get; set; }
        public bool UseTypeFullName { get; set; }
        public bool WritablePropertiesOnly { get; set; } = true;
        public DateKind DateKind { get; set; } = DateKind.ConvertToUtc;
        public DateTimeInstantiation DateTimeInstantiation { get; set; } = DateTimeInstantiation.New;
        public ICollection<string> ExcludeTypes { get; set; } = new string[0];
        public int MaxDepth { get; set; } = 25;
        public ListSortDirection? SortDirection { get; set; }

        public static DumpOptions Default { get; } = new();

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
