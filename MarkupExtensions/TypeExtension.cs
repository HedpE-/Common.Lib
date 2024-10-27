using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Common.Lib.MarkupExtensions
{
    [MarkupExtensionReturnType(typeof(Type))]
    public class TypeExtension : MarkupExtension
    {
        public TypeExtension() { }
        public TypeExtension(string typeName) { TypeName = typeName; }
        public TypeExtension(Type type) { Type = type; }

        public string TypeName { get; set; }
        public Type Type { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Type == null)
            {
                IXamlTypeResolver typeResolver = serviceProvider.GetService(typeof(IXamlTypeResolver)) as IXamlTypeResolver;
                if (typeResolver == null) throw new InvalidOperationException("EDF Type markup extension used without XAML context");
                if (TypeName == null) throw new InvalidOperationException("EDF Type markup extension used without Type or TypeName");
                Type = ResolveGenericTypeName(TypeName, (name) =>
                {
                    Type result = typeResolver.Resolve(name);
                    if (result == null) throw new Exception("EDF Type markup extension could not resolve type " + name);
                    return result;
                });
            }
            return Type;
        }

        public static Type ResolveGenericTypeName(string name, Func<string, Type> resolveSimpleName)
        {
            if (name.Contains('{'))
                name = name.Replace('{', '<').Replace('}', '>');  // Note:  For convenience working with XAML, we allow {} instead of <> for generic type parameters

            if (name.Contains('<'))
            {
                var match = _genericTypeRegex.Match(name);
                if (match.Success)
                {
                    Type[] typeArgs = (
                      from arg in SplitOutsideParenthesis(match.Groups["typeArgs"].Value, ',')
                      select ResolveGenericTypeName(arg, resolveSimpleName)
                      ).ToArray();
                    string genericTypeName = match.Groups["genericTypeName"].Value + "`" + typeArgs.Length;
                    Type genericType = resolveSimpleName(genericTypeName);
                    if (genericType != null && !typeArgs.Contains(null))
                        return genericType.MakeGenericType(typeArgs);
                }
            }
            return resolveSimpleName(name);
        }
        static Regex _genericTypeRegex = new Regex(@"^(?<genericTypeName>\w+)<(?<typeArgs>\w+(,\w+)*)>$");

        private static IEnumerable<string> SplitOutsideParenthesis(string input, char separator)
        {
            var regex = separator + @"(?=.*?(?:\(|\{|\[).*?(?:\]|\}|\)).*?)|(?<=(?:\(|\[|\{).*?(?:\}|\]|\)).*?)" + separator;
            var matches = Regex.Matches(input, regex);

            return matches.Cast<Match>().Select(m => m.Value);
        }
    }
}
