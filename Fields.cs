using System.Collections.Generic;
using System.Reflection;

namespace AssemblyDiff
{
    static class Fields
    {
        public static Diff[] Diff(FieldInfo field, FieldInfo other)
        {
            var diffs = new List<Diff>();

            if (field.FieldType.GetName() != other.FieldType.GetName())
                diffs.Add(new Diff(DiffType.Change, $"{field.ToString()} => {other.ToString()}"));

            return diffs.ToArray();
        }
    }
}
