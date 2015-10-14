using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using Microsoft.VisualStudio.TextTemplating;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using MvvmTools.Core.Models;
using Ninject;

namespace MvvmTools.Core.Services
{
    public interface ITemplateService
    {
        List<Template> LoadTemplates(string localTemplateFolder);
        void SaveTemplates(string localTemplateFolder, IEnumerable<Template> templates);
        List<string> Transform(string contents, out string output);
    }

    public enum Section
    {
        Template, Field, ViewModelVisualBasic, ViewModelCSharp, CodeBehindVisualBasic, CodeBehindCSharp, View
    }

    public class TemplateService : ITemplateService
    {
        #region Data

        public const string LocalTemplatesFilename = "LocalTemplates.xml";

        #endregion Data

        #region Ctor

        #endregion Ctor

        #region Properties

        [Inject]
        public ITextTemplating TextTemplating { get; set; }

        //[Inject]
        //public ITextTemplatingEngine TextTemplatingEngine { get; set; }

        [Inject]
        public ITextTemplatingEngineHost TextTemplatingEngineHost { get; set; }

        [Inject]
        public ITextTemplatingSessionHost TextTemplatingSessionHost { get; set; }


        #endregion Properties

        public static string Serialize(object obj)
        {
            var serializer = new DataContractSerializer(obj.GetType());
            using (var writer = new StringWriter())
            {
                using (var stm = XmlWriter.Create(writer,
                    new XmlWriterSettings
                    {
                        Indent = true,
                        NewLineHandling = NewLineHandling.Entitize
                    }))
                {
                    serializer.WriteObject(stm, obj);
                }

                return writer.ToString();
            }
        }

        public static T Deserialize<T>(string xml)
        {
            using (Stream stream = new MemoryStream())
            {
                var data = Encoding.Unicode.GetBytes(xml);
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                var toType = typeof (T);
                var deserializer = new DataContractSerializer(toType);
                return (T) deserializer.ReadObject(stream);
            }
        }

        private List<Template> ParseTemplates(bool isInternal, string data)
        {
            try
            {
                var templates = Deserialize<List<Template>>(data);

                foreach (var t in templates)
                    t.IsInternal = isInternal;

                return templates;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{nameof(ParseTemplates)}() failed: {ex}");
                throw;
            }
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

        public List<Template> LoadTemplates(string localTemplateFolder)
        {
            var rval = new List<Template>();
            try
            {
                // Factory templates.
                var factoryTemplatesText = GetFromResources("MvvmTools.Core.InternalTemplates.xml");
                var tmp1 = ParseTemplates(true, factoryTemplatesText);
                rval.AddRange(tmp1);

                // Local templates folder.
                var fn = Path.Combine(localTemplateFolder, LocalTemplatesFilename);
                if (File.Exists(fn))
                {
                    string contents;
                    try
                    {
                        contents = File.ReadAllText(fn, Encoding.UTF8);
                    }
                    catch (Exception ex1)
                    {
                        Trace.WriteLine($"Template file {fn} can't be deserialized. {ex1.Message}.");
                        throw;
                    }

                    var tmp2 = ParseTemplates(false, contents);

                    rval.AddRange(tmp2);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{nameof(TemplateService)}.{nameof(LoadTemplates)}() failed: {ex}");
            }

            return rval;
        }

        public void SaveTemplates(string localTemplateFolder, IEnumerable<Template> templates)
        {
            try
            {
                string contents;
                try
                {
                    contents = Serialize(templates.Where(t => !t.IsInternal).ToList());
                }
                catch (Exception ex1)
                {
                    Trace.WriteLine($"Templates can't be serialized. {ex1.Message}.");
                    throw;
                }

                // Local templates folder.
                var fn = Path.Combine(localTemplateFolder, LocalTemplatesFilename);
                File.WriteAllText(fn, contents);
            }
            catch (Exception ex2)
            {
                Trace.WriteLine($"{nameof(TemplateService)}.{nameof(SaveTemplates)}() failed: {ex2}");
            }
        }

        public List<string> Transform(string contents, out string output)
        {
            try
            {
                // Create a Session in which to pass parameters:
                TextTemplatingSessionHost.Session = TextTemplatingSessionHost.CreateSession();
                TextTemplatingSessionHost.Session["parameter1"] = "Hello";
                TextTemplatingSessionHost.Session["parameter2"] = DateTime.Now;

                // Process T4.
                var cb = new T4Callback();
                output = TextTemplating.ProcessTemplate(string.Empty, contents, cb);

                return cb.ErrorMessages;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }

    public class T4Callback : ITextTemplatingCallback
    {
        public List<string> ErrorMessages { get; } = new List<string>();
        public string FileExtension { get; private set; } = ".txt";
        public Encoding OutputEncoding { get; private set; } = Encoding.UTF8;

        public void ErrorCallback(bool warning, string message, int line, int column)
        {
            ErrorMessages.Add(message);
        }

        public void SetFileExtension(string extension)
        {
            FileExtension = extension;
        }

        public void SetOutputEncoding(Encoding encoding, bool fromOutputDirective)
        {
            OutputEncoding = encoding;
        }
    }
}