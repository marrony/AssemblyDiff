using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AssemblyDiff
{
    enum DiffType
    {
        Add,
        Remove,
        Change
    }

    class Diff
    {
        public DiffType Type { get; }
        public string Message { get; }

        public Diff(DiffType type, string message)
        {
            Type = type;
            Message = message;
        }

        public override string ToString()
        {
            return string.Format("Type={0}, Message={1}", Type, Message);
        }
    }

    static class Module
    {
        public static Diff[] Diff(Assembly assembly, Assembly other)
        {
            var diffs = new List<Diff>();

            diffs.AddRange(
                assembly
                    .ExportedTypes
                    .Where(x => other.GetType(x.FullName) == null)
                    .Select(x => new Diff(DiffType.Remove, x.FullName))
            );

            diffs.AddRange(
                other
                    .ExportedTypes
                    .Where(x => assembly.GetType(x.FullName) == null)
                    .Select(x => new Diff(DiffType.Add, x.FullName))
            );

            diffs.AddRange(
                assembly
                    .ExportedTypes
                    .Where(x => other.GetType(x.FullName) != null)
                    .SelectMany(x => Classes.Diff(x.GetTypeInfo(), other.GetType(x.FullName).GetTypeInfo()))
            );

            return diffs.ToArray();
        }

        public static Version CalculateVersion(Version version, Diff[] diffs)
        {
            var isMajor = diffs.Any(x => x.Type == DiffType.Change || x.Type == DiffType.Remove);
            var isMinor = diffs.Count() > 0 && diffs.All(x => x.Type == DiffType.Add);
            var isPatch = diffs.Count() == 0;

            int major = 0, minor = 0, patch = 0;

            if (isMajor)
            {
                major = version.Major + 1;
                minor = 0;
                patch = 0;
            }

            if (isMinor)
            {
                major = version.Major;
                minor = version.Minor + 1;
                patch = 0;
            }

            if (isPatch)
            {
                major = version.Major;
                minor = version.Minor;
                patch = version.Build + 1;
            }

            return new Version(major, minor, patch, 0);
        }
    }

    class MainClass
    {
        public static void Main(string[] args)
        {
            var oldAssembly = string.Empty;
            var newAssembly = string.Empty;

            if (args.Length >= 2)
            {
                oldAssembly = args[0];
                newAssembly = args[1];
            }
            else
            {
                Console.WriteLine("usage:");
                Console.WriteLine("\tAssemblyDiff.exe old-assembly new-assembly");
                return;
            }

            var oldModule = Assembly.LoadFrom(oldAssembly);
            var newModule = Assembly.LoadFrom(newAssembly);
            var diffs = Module.Diff(oldModule, newModule);

            var oldVersion = oldModule.GetName().Version;
            var newVersion = Module.CalculateVersion(oldVersion, diffs);
            Console.WriteLine($"Old version: {oldVersion}");
            Console.WriteLine($"New version: {newVersion}");

            if (newModule.GetName().Version != newVersion)
                Console.WriteLine($"Does not publish, the version of new assembly is wrong: {newModule.GetName().Version}");

            var diffsStr = diffs.Select(x => x.ToString());
            Console.WriteLine("\nDifferences:");
            Console.WriteLine($"{string.Join("\n", diffsStr)}");
        }
    }
}
