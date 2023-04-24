using System;
using System.Diagnostics;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Shell;
using Document = Microsoft.CodeAnalysis.Document;

namespace MvvmTools.Services
{
    public abstract class ProjectItemAndType
    {
        public virtual string Filename { get; }
        public NamespaceClass Type { get; protected set; }
        public abstract string ProjectName { get; }

        public string RelativeNamespace
        {
            get
            {
                if (Type.Namespace == ProjectName)
                    return "(same)";
                if (Type.Namespace.StartsWith(ProjectName))
                    return Type.Namespace.Substring(ProjectName.Length);
                return Type.Namespace;
            }
        }

        public abstract Task Open();
    }

    public class DteProjectItemAndType : ProjectItemAndType
    {
        private readonly ProjectItem projectItem;

        public DteProjectItemAndType(ProjectItem projectItem, NamespaceClass type)
        {
            this.projectItem = projectItem;
            Type = type;
        }

        public override string Filename => projectItem?.Name;
        public override string ProjectName => projectItem?.ContainingProject?.Name;
        public override async Task Open()
        {
            try
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                var w = projectItem.Open();
                w.Visible = true;
                w.Activate();
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }
        }
    }

    public class RoslynProjectItemAndType : ProjectItemAndType
    {
        private readonly Workspace workspace;
        private readonly Document document;

        public RoslynProjectItemAndType(Workspace workspace, Document document, NamespaceClass type, string projectName)
        {
            this.workspace = workspace;
            this.document = document;
            ProjectName = projectName;
            Type = type;
        }

        public override string Filename => document.Name;
        public string FilePath => document.FilePath;
        public override string ProjectName { get; }
        
        public override async Task Open()
        {
            try
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                workspace.OpenDocument(this.document.Id);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }
        }
    }
}