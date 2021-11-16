using System;
using System.Collections.Generic;
using System.Linq;

namespace Ccash.SemanticAnalysis.Tests
{
    public static class Extensions
    {
        public static bool IsCollection<T>(this Type type) =>
            type.HasInterface<IEnumerable<T>>() && type.GenericTypeArguments.Contains(typeof(T));

        public static bool HasInterface<T>(this Type type) => type.GetInterface(typeof(T).Name) != null;

        public static bool IsConcreteClass(this Type type) => type.IsClass && !type.IsAbstract;

        public static IEnumerable<(T, T)> CartesianProduct<T>(this IEnumerable<T> collection)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            return collection.SelectMany(e => collection, (e1, e2) => (e1, e2));
        }
    }
}
