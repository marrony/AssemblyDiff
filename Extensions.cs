using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AssemblyDiff
{
    public static class Extensions
    {

        public static string GetName(this Type type)
        {
            string signature = "";

            if (type.IsGenericParameter)
                signature = type.Name;
            else
                signature = $"{type.Namespace}.{type.Name}";

            if (type.GenericTypeArguments.Length > 0)
            {
                signature += "[";
                signature += string.Join(",", type.GenericTypeArguments.Select(g => GetName(g)));
                signature += "]";
            }

            return signature;
        }

        public static string GetSignature(this MethodInfo method)
        {
            var genericTypes = string.Empty;

            if (method.IsGenericMethod)
            {
                genericTypes += "[";
                genericTypes += string.Join(",", method.GetGenericArguments().Select(x => x.GetName()));
                genericTypes += "]";
            }

            return $"{GetName(method.ReturnType)} {method.DeclaringType.FullName}.{method.Name}{genericTypes}({FormatParameters(method)})";
        }

        public static string GetSignature(this ConstructorInfo method) =>
            $"{method.DeclaringType.FullName}.{method.Name}({FormatParameters(method)})";

        public static string GetSignature(this TypeInfo type)
        {
            var Signature = "";

            if (type.IsPublic || type.IsNestedPublic) Signature += "public";
            if (type.IsNotPublic || type.IsNestedPrivate) Signature += "private";
            if (type.IsSealed) Signature += " sealed";
            if (type.IsAbstract) Signature += " abstract";
            if (type.IsInterface) Signature += " interface";
            if (type.IsClass)
                Signature += " class";
            else if (type.IsEnum)
                Signature += " enum";
            else
                Signature += " struct";

            Signature += $" {type.FullName}";

            if (type.GenericTypeParameters.Length > 0)
            {
                var args = string.Join(",", type.GenericTypeParameters.Select(x => GetName(x)));
                Signature += $"[{args}]";
            }

            var baseClasses = new List<string>();

            if (type.BaseType != null)
                baseClasses.Add(GetName(type.BaseType));

            if (type.ImplementedInterfaces.Count() > 0)
                baseClasses.AddRange(type.ImplementedInterfaces.Select(x => GetName(x)));

            if (baseClasses.Count > 0)
            {
                var baseCls = string.Join(", ", baseClasses);
                Signature += $" : {baseCls}";
            }

            return Signature;
        }

        public static string FormatParametersDetailed(this MethodBase info) =>
            string.Join(", ", info.GetParameters().Select(x => $"{GetName(x.ParameterType)} {x.Name}"));

        public static string FormatParameters(this MethodBase info) =>
            string.Join(", ", info.GetParameters().Select(x => GetName(x.ParameterType)));
    }
}
