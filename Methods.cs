using System.Collections.Generic;
using System.Reflection;

namespace AssemblyDiff
{
    static class Methods
    {
        public static Diff[] Diff(MethodBase method, MethodBase other)
        {
            var diffs = new List<Diff>();

            if (method.FormatParameters() != other.FormatParameters())
                diffs.Add(new Diff(DiffType.Change, $"{method.FormatParametersDetailed()} => {other.FormatParametersDetailed()}"));

            return diffs.ToArray();
        }
    }
}
