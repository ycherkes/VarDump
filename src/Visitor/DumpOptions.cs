using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace VarDump.Visitor
{
    public class DumpOptions
    {
        public DateTimeInstantiation DateTimeInstantiation { get; set; }
        public DateKind DateKind { get; set; }
        public ICollection<string> ExcludeTypes { get; set; }
        public int MaxDepth { get; set; }
        public bool IgnoreNullValues { get; set; }
        public bool IgnoreDefaultValues { get; set; }
        public bool UseTypeFullName { get; set; }
        public bool UseNamedArgumentsForReferenceRecordTypes { get; set; }
        public BindingFlags GetPropertiesBindingFlags { get; set; }
        public bool WritablePropertiesOnly { get; set; } = true;
        public ListSortDirection? SortDirection { get; set; }
        public BindingFlags? GetFieldsBindingFlags { get; set; }
    }
}
