//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using MvvmTools.Services;
//using System.Collections.Generic;

//namespace MvvmTools.Commands.GoToVM
//{
//    internal class ClassVirtualizationVisitor : CSharpSyntaxRewriter
//    {
//        private readonly Workspace workspace;
//        private readonly string projectName;
//        private readonly Solution solution;
//        private readonly Project project;
//        private readonly List<string> allCandidateTypes;

//        public List<RoslynProjectItemAndType> Items = new();

//        public ClassVirtualizationVisitor(Workspace workspace, string projectName, Solution solution, 
//            Microsoft.CodeAnalysis.Project project,
//            List<string> allCandidateTypes)
//        {
//            this.workspace = workspace;
//            this.projectName = projectName;
//            this.solution = solution;
//            this.project = project;
//            this.allCandidateTypes = allCandidateTypes;
//        }

//        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
//        {
//            node = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);
//            if (node == null)
//                return null;
//            if (!allCandidateTypes.Contains(node.Identifier.Text))
//                return node;

//            var ns = node.GetNamespace();
//            var document = solution.GetDocument(node.SyntaxTree);
//            var className = node.GetName();
//            if (document == null)
//                return null;
//            var pi = new RoslynProjectItemAndType(
//                workspace,
//                document,
//                new NamespaceClass(className, ns),
//                projectName);
//            Items.Add(pi);

//            //if (document.Name.EndsWith(".xaml.cs", System.StringComparison.OrdinalIgnoreCase))
//            //{
//            //    var newPath = document.FilePath.Substring(0, document.FilePath.Length - 3);
//            //    var newName = document.Name.Substring(0, document.Name.Length - 3);
//            //    var xamlDocument = 
//            //        project.GetDocument(
//            //            document.WithFilePath(newPath)
//            //                .WithName(newName));
//            //    pi = new RoslynProjectItemAndType(
//            //        workspace,
//            //        xamlDocument,
//            //        new NamespaceClass(className, ns),
//            //        projectName);
//            //}

//            return node;
//        }
//    }
//}
