using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvvmTools.Core.Models;
using MvvmTools.Core.Services;

namespace MvvmTools.Core.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void CreateInitialFactoryTemplateFile()
        {
            const string filename = "C:\\src\\Factory.xml";

            var view = GetFromResources("MvvmTools.Core.Tests.Data.View.tt");
            var cbcs = GetFromResources("MvvmTools.Core.Tests.Data.CodeBehindCSharp.tt");
            var vmcs = GetFromResources("MvvmTools.Core.Tests.Data.ViewModelCSharp.tt");

            var templates = new List<Template>
            {
                new Template(true, "Simple WPF Window")
                {
                    Description = "This is a simple WPF window template, with d:DesignContext wired up.",
                    Framework = null,
                    FormFactors = new HashSet<FormFactor> { FormFactor.Desktop, FormFactor.Tablet },
                    Platforms = new HashSet<Platform> { Platform.WPF, Platform.Silverlight },
                    Tags = "Simple,Window",
                    Fields = new List<Field>
                    {
                        new Field("Field1", FieldType.CheckBox, "true", "Prompt 1", "This is a single line description"),
                        new Field("Field2", FieldType.ComboBox, "Value 1", "Prompt 2", "Value 1|Value 2", "This is a\nmulti-line\ndescription."),
                        new Field("Field3", FieldType.ComboBoxOpen, "Open value", "Prompt 3", "Value 1|Value 2", "This is a\nmulti-line\ndescription."),
                        new Field("Field4", FieldType.TextBox, "Some default text.", "Prompt 4", "This is a single line description"),
                        new Field("Field5", FieldType.TextBoxMultiLine, "Some default text,\nmultiple lines.", "Prompt 5", "This is a single line description")
                    },
                    View = view,
                    CodeBehindCSharp = cbcs,
                    ViewModelCSharp = vmcs
                }
            };

            var str = TemplateService.Serialize(templates);

            File.WriteAllText(filename, str);
        }

        private string GetFromResources(string resourceName)
        {
            var assem = GetType().Assembly;

            using (var stream = assem.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                    using (var reader = new StreamReader(stream))
                        return reader.ReadToEnd();
                return null;
            }
        }
    }
}
