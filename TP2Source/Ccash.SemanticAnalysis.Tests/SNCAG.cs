using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Ccash.SemanticAnalysis.Attributes;
using Ccash.SemanticAnalysis.Nodes.Expressions;
using Ccash.SemanticAnalysis.Nodes.Statements;
using NUnit.Framework;

namespace Ccash.SemanticAnalysis.Tests
{
    public class Tests
    {
        [Test]
        public void SNCAG()
        {
            var types = GetImplementors<IStatement>().Concat(GetImplementors<IExpression>()).ToList();

            var issiGraphs = new Dictionary<Type, Dictionary<string, HashSet<string>>>();
            foreach (var type in types)
            {
                issiGraphs[type] = NewISSIGraph(type);
            }

            var dependencyGraphs = new Dictionary<Type, Dictionary<string, HashSet<string>>>();
            foreach (var type in types)
            {
                dependencyGraphs[type] = NewDependencyGraph(type);
            }

            bool changed;
            do
            {
                changed = false;
                foreach (var type in types)
                {
                    var dependencyGraph = dependencyGraphs[type];
                    foreach (var variable in type.GetProperties().Where(IsChildNode))
                    {
                        if (variable.PropertyType.IsCollection<IStatement>() ||
                            variable.PropertyType.IsCollection<IExpression>())
                        {
                            continue;
                        }

                        foreach (var (attribute, dependantAttributes) in issiGraphs[variable.PropertyType])
                        {
                            if (dependencyGraph.ContainsKey(attribute))
                            {
                                dependencyGraph[attribute].UnionWith(dependantAttributes);
                            }
                            else
                            {
                                dependencyGraph.Add(attribute, dependantAttributes);
                            }
                        }
                    }

                    if (ContainsCycle(dependencyGraph))
                    {
                        Assert.Fail("Found cycle");
                    }

                    foreach (var variable in type.GetProperties().Where(IsChildNode))
                    {
                        var attributes = ExtractAttributes(type, variable.Name);
                        foreach (var (b, c) in attributes.CartesianProduct())
                        {
                            if (ContainsPath(dependencyGraph, b, c))
                            {
                                issiGraphs[variable.PropertyType][b].Add(c);
                                changed = true;
                            }
                        }
                    }
                }
            } while (changed);

            Assert.Pass();
        }

        private static Dictionary<string, HashSet<string>> NewDependencyGraph(Type type)
        {
            var graph = new Dictionary<string, HashSet<string>>();

            var semanticRules = ExtractSemanticRules(type);
            foreach (var rule in semanticRules)
            {
                if (Regex.IsMatch(rule, ".*=.*"))
                {
                    var components = rule.Split("=").Select(s => s.Trim()).ToList();
                    var lhs = components.First().Replace("this.", "");
                    var dependencies = components.Skip(1).Select(d => d.Replace("this.", ""));

                    graph.TryAdd(lhs, new HashSet<string>());

                    foreach (var dependency in dependencies)
                    {
                        if (!graph.ContainsKey(dependency))
                        {
                            graph[dependency] = new HashSet<string>();
                        }

                        graph[dependency].Add(lhs);
                    }
                }
            }

            return graph;
        }

        private static Dictionary<string, HashSet<string>> NewISSIGraph(Type type)
        {
            var graph = new Dictionary<string, HashSet<string>>();
            foreach (var attribute in ExtractAttributes(type))
            {
                graph[attribute] = new HashSet<string>();
            }

            return graph;
        }

        private static bool ContainsCycle(Dictionary<string, HashSet<string>> d)
        {
            foreach (var node in d.Keys)
            {
                if (ContainsPath(d, node, node))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsPath(Dictionary<string, HashSet<string>> d, string b, string c)
        {
            var stack = new Stack<string>(d[b]);
            while (stack.Any())
            {
                var node = stack.Pop();
                if (node == c)
                {
                    return true;
                }

                foreach (var neighbor in d[node])
                {
                    stack.Push(neighbor);
                }
            }

            return false;
        }

        private static List<string> ExtractSemanticRules(Type type)
        {
            var semanticRules = new List<string>();
            var rules = type.GetCustomAttributes()
                            .OfType<SemanticRule>()
                            .Select(r => r.Rule);

            foreach (var rule in rules)
            {
                if (Regex.IsMatch(rule, "^.*\\.inherited.*"))
                {
                    semanticRules.AddRange(ExpandInheritedRules(rule));
                }
                else
                {
                    semanticRules.Add(rule);
                }
            }

            return semanticRules;
        }

        private static List<string> ExpandInheritedRules(string rule)
        {
            var expandedRules = new List<string>();

            var components = rule.Split('=').Select(s => s.Trim()).ToArray();
            var leftHandSide = components.First();

            var nodeName = leftHandSide.Split(".").First();
            expandedRules.AddRange(typeof(InheritedAttributes)
                                   .GetProperties()
                                   .Select(property => $"{nodeName}.{property.Name} = this.{property.Name}"));

            return expandedRules;
        }

        private static HashSet<string> ExtractAttributes(Type type)
        {
            var attributes = new HashSet<string>();
            foreach (var semanticRule in type.GetCustomAttributes().OfType<SemanticRule>())
            {
                var attributesFromRule = Regex.Matches(semanticRule.Rule, "this\\.(\\w+)")
                                              .Select(m => m.Value.Substring("this.".Length))
                                              .Where(m => IsInheritedAttribute(m) || !IsChildNode(type.GetProperty(m)))
                                              .ToList();

                foreach (var attribute in attributesFromRule.Where(a => !IsInheritedAttribute(a)))
                {
                    attributes.Add(attribute);
                }

                if (attributesFromRule.Any(IsInheritedAttribute))
                {
                    var inheritedAttributes = typeof(InheritedAttributes).GetProperties().Select(p => p.Name);
                    foreach (var attribute in inheritedAttributes)
                    {
                        attributes.Add(attribute);
                    }
                }
            }

            return attributes;
        }

        private static HashSet<string> ExtractAttributes(Type type, string childName)
        {
            var attributes = new HashSet<string>();
            foreach (var semanticRule in type.GetCustomAttributes().OfType<SemanticRule>())
            {
                var attributesFromRule = Regex.Matches(semanticRule.Rule, $"{childName}\\.(\\w+)")
                                              .Select(m => m.Value)
                                              .ToList();

                foreach (var attribute in attributesFromRule.Where(a => !IsInheritedAttribute(a)))
                {
                    attributes.Add(attribute);
                }

                if (attributesFromRule.Any(IsInheritedAttribute))
                {
                    var inheritedAttributes = typeof(InheritedAttributes).GetProperties().Select(p => p.Name);
                    foreach (var attribute in inheritedAttributes)
                    {
                        attributes.Add($"{childName}.{attribute}");
                    }
                }
            }

            return attributes;
        }

        private static IEnumerable<Type> GetImplementors<T>()
        {
            return Assembly.GetAssembly(typeof(T))
                           ?.GetTypes()
                           .Where(t => t.IsConcreteClass())
                           .Where(t => t.HasInterface<T>());
        }

        private static bool IsChildNode(PropertyInfo propertyInfo)
        {
            var type = propertyInfo.PropertyType;
            return type.IsCollection<IStatement>()
                   || type.IsCollection<IExpression>()
                   || type.HasInterface<IStatement>()
                   || type.HasInterface<IExpression>();
        }

        private static bool IsInheritedAttribute(string attribute)
        {
            return attribute == "inherited"
                   || Regex.IsMatch(attribute, "\\w+\\.inherited")
                   || typeof(InheritedAttributes).GetProperties().Any(p => p.Name == attribute);
        }
    }
}
