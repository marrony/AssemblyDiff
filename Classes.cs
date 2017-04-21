using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AssemblyDiff
{
    static class Classes
    {
        public static Diff[] Diff(TypeInfo type, TypeInfo other)
        {
            var diffs = new List<Diff>();

            if (DeclarationChanged(type, other))
                diffs.Add(new Diff(DiffType.Change, $"{type.GetSignature()} => {other.GetSignature()}"));

            diffs.AddRange(GetFieldsDiffs(type, other));
            diffs.AddRange(GetMethodsDiffs(type, other));
            diffs.AddRange(GetConstructorsDiffs(type, other));

            return diffs.ToArray();
        }

        static Diff[] GetConstructorsDiffs(TypeInfo typeA, TypeInfo typeB)
        {
            var diffs = new List<Diff>();
            var aMethods = GetConstructors(typeA).ToDictionary(x => x.GetSignature());
            var bMethods = GetConstructors(typeB).ToDictionary(x => x.GetSignature());

            diffs.AddRange(
                aMethods
                    .Where(x => !bMethods.ContainsKey(x.Key))
                    .Select(x => new Diff(DiffType.Remove, x.Key))
            );

            diffs.AddRange(
                bMethods
                    .Where(x => !aMethods.ContainsKey(x.Key))
                    .Select(x => new Diff(DiffType.Add, x.Key))
            );

            diffs.AddRange(
                aMethods
                    .Where(x => bMethods.ContainsKey(x.Key))
                    .SelectMany(x => Methods.Diff(x.Value, bMethods[x.Key]))
            );

            return diffs.ToArray();
        }

        static Diff[] GetMethodsDiffs(TypeInfo typeA, TypeInfo typeB)
        {
            var diffs = new List<Diff>();
            var aMethods = GetMethods(typeA).ToDictionary(x => x.GetSignature());
            var bMethods = GetMethods(typeB).ToDictionary(x => x.GetSignature());

            diffs.AddRange(
                aMethods
                    .Where(x => !bMethods.ContainsKey(x.Key))
                    .Select(x => new Diff(DiffType.Remove, x.Key))
            );

            diffs.AddRange(
                bMethods
                    .Where(x => !aMethods.ContainsKey(x.Key))
                    .Select(x => new Diff(DiffType.Add, x.Key))
            );

            diffs.AddRange(
                aMethods
                    .Where(x => bMethods.ContainsKey(x.Key))
                    .SelectMany(x => Methods.Diff(x.Value, bMethods[x.Key]))
            );

            return diffs.ToArray();
        }

        static Diff[] GetFieldsDiffs(TypeInfo typeA, TypeInfo typeB)
        {
            var diffs = new List<Diff>();
            var aFields = GetFields(typeA).ToDictionary(x => x.Name);
            var bFields = GetFields(typeB).ToDictionary(x => x.Name);

            diffs.AddRange(
                aFields
                    .Where(x => !bFields.ContainsKey(x.Key))
                    .Select(x => new Diff(DiffType.Remove, x.Value.Name))
            );

            diffs.AddRange(
                bFields
                    .Where(x => !aFields.ContainsKey(x.Key))
                    .Select(x => new Diff(DiffType.Add, x.Value.Name))
            );

            diffs.AddRange(
                aFields
                    .Where(x => bFields.ContainsKey(x.Key))
                    .ToDictionary(kv => kv.Key, kv => kv.Value)
                    .SelectMany(x => Fields.Diff(x.Value, bFields[x.Key]))
            );

            return diffs.ToArray();
        }

        static FieldInfo[] GetFields(TypeInfo type) =>
            type.GetFields().Where(f => f.DeclaringType == type && f.IsPublic).ToArray();

        static MethodInfo[] GetMethods(TypeInfo type) =>
            type.GetMethods().Where(m => m.DeclaringType == type && m.IsPublic).ToArray();

        static ConstructorInfo[] GetConstructors(TypeInfo type) =>
            type.GetConstructors().Where(c => c.DeclaringType == type && c.IsPublic).ToArray();

        static bool DeclarationChanged(TypeInfo a, TypeInfo b)
        {
            if (a.IsSealed != b.IsSealed) return true;
            if (a.IsClass != b.IsClass) return true;
            if (a.IsInterface != b.IsInterface) return true;
            if (a.IsAbstract != b.IsAbstract) return true;
            if (a.IsValueType != b.IsValueType) return true;
            if (!a.IsInterface && a.BaseType.GetName() != b.BaseType.GetName()) return true;

            return false;
        }

    }
}
