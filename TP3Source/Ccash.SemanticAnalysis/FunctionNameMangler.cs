using System.Collections.Generic;
using System.Linq;
using Ccash.SemanticAnalysis.Types;

namespace Ccash.SemanticAnalysis
{
    public static class FunctionNameMangler
    {
        public static string Mangle(string identifier, params CcashType[] parameterTypes)
        {
            if (identifier == "main" || identifier == "printf")
            {
                return identifier;
            }

            return $"{identifier}::<{string.Join(",", parameterTypes.Select(t => t.Name))}>";
        }

        public static string Mangle(string identifier, FunctionType functionType)
        {
            return Mangle(identifier, functionType.ParameterTypes);
        }

        public static string Mangle(string identifier, IEnumerable<CcashType> parameterTypes)
        {
            return Mangle(identifier, parameterTypes.ToArray());
        }
    }
}
