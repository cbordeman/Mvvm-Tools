using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MvvmTools.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MvvmTools.Commands.GoToVM
{
    internal class ClassVirtualizationVisitor : CSharpSyntaxRewriter
    {
        private readonly Workspace workspace;
        private readonly string projectName;
        private readonly Solution solution;
        private readonly List<string> allCandidateTypes;

        public List<RoslynProjectItemAndType> Items = new();

        public ClassVirtualizationVisitor(Workspace workspace, string projectName, Solution solution,
            List<string> allCandidateTypes)
        {
            this.workspace = workspace;
            this.projectName = projectName;
            this.solution = solution;
            this.allCandidateTypes = allCandidateTypes;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            node = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);
            if (node == null)
                return null;
            if (!allCandidateTypes.Contains(node.Identifier.Text))
                return node;

            var ns = node.GetFullNamespace();
            var document = solution.GetDocument(node.SyntaxTree);
            var className = node.GetName();
            if (document == null)
                return null;
            var pi = new RoslynProjectItemAndType(
                workspace,
                document,
                new NamespaceClass(className, ns),
                projectName);
            Items.Add(pi);

            return node;
        }
    }

    public static class TypeDeclarationSyntaxExtensions
    {
        const char NESTED_CLASS_DELIMITER = '+';
        const char NAMESPACE_CLASS_DELIMITER = '.';
        const char TYPEPARAMETER_CLASS_DELIMITER = '`';

        public static string GetFullNamespace(this TypeDeclarationSyntax source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            var namespaces = new LinkedList<BaseNamespaceDeclarationSyntax>();
            var types = new LinkedList<TypeDeclarationSyntax>();
            for (var parent = source.Parent; parent is not null; parent = parent.Parent)
            {
                if (parent is BaseNamespaceDeclarationSyntax @namespace)
                {
                    namespaces.AddFirst(@namespace);
                }
                else if (parent is TypeDeclarationSyntax type)
                {
                    types.AddFirst(type);
                }
            }

            var result = new StringBuilder();
            for (var item = namespaces.First; item is not null; item = item.Next)
            {
                result.Append(item.Value.Name).Append(NAMESPACE_CLASS_DELIMITER);
            }
            for (var item = types.First; item is not null; item = item.Next)
            {
                var type = item.Value;
                AppendName(result, type);
                result.Append(NESTED_CLASS_DELIMITER);
            }

            return result.ToString();
        }

        public static string GetName(this TypeDeclarationSyntax type)
        {
            var typeArguments = type.TypeParameterList?.ChildNodes()
                .Count(node => node is TypeParameterSyntax) ?? 0;
            if (typeArguments != 0)
                return type.Identifier.Text + TYPEPARAMETER_CLASS_DELIMITER + typeArguments;
            return type.Identifier.Text;
        }

        static void AppendName(StringBuilder builder, TypeDeclarationSyntax type)
        {
            builder.Append(type.Identifier.Text);
            var typeArguments = type.TypeParameterList?.ChildNodes()
                .Count(node => node is TypeParameterSyntax) ?? 0;
            if (typeArguments != 0)
                builder.Append(TYPEPARAMETER_CLASS_DELIMITER).Append(typeArguments);
        }
    }
}
