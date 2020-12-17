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
using MvvmTools.Models;
using MvvmTools.ViewModels;

namespace MvvmTools.Services
{
    public interface ITemplateService
    {
        List<Template> LoadTemplates(string localTemplateFolder);
        void SaveTemplates(string localTemplateFolder, IEnumerable<Template> templates);
        List<T4Error> Transform(string contents, List<InsertFieldViewModel> predefinedFieldValues,
            List<InsertFieldViewModel> customFieldValues, out string output);
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

        public ITextTemplating TextTemplating { get; }

        //[Inject]
        //public ITextTemplatingEngine TextTemplatingEngine { get; set; }

        public ITextTemplatingEngineHost TextTemplatingEngineHost { get; }

        public ITextTemplatingSessionHost TextTemplatingSessionHost { get; }


        #endregion Properties

        public TemplateService(ITextTemplatingSessionHost textTemplatingSessionHost,
            ITextTemplatingEngineHost textTemplatingEngineHost,
            ITextTemplating textTemplating)
        {
            TextTemplatingSessionHost = textTemplatingSessionHost;
            TextTemplatingEngineHost = textTemplatingEngineHost;
            TextTemplating = textTemplating;
        }
        
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
#if !DEBUG
                // Factory templates.
                var factoryTemplatesText = GetFromResources("MvvmTools.Core.InternalTemplates.xml");
                var tmp1 = ParseTemplates(true, factoryTemplatesText);
                rval.AddRange(tmp1);
#endif

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

        public List<T4Error> Transform(string contents, List<InsertFieldViewModel> predefinedFieldValues,
            List<InsertFieldViewModel> customFieldValues, out string output)
        {
            try
            {
                // Create a Session in which to pass parameters:
                TextTemplatingSessionHost.Session = TextTemplatingSessionHost.CreateSession();
                foreach (var f in predefinedFieldValues)
                    TextTemplatingSessionHost.Session[f.Name] = f.Value;
                foreach (var f in customFieldValues)
                    TextTemplatingSessionHost.Session[f.Name] = f.Value;

                // Process T4.
                var cb = new T4Callback();
                output = TextTemplating.ProcessTemplate(string.Empty, contents, cb);

                return cb.ErrorMessages;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    public class T4Error
    {
        public T4Error(string message, int line, int column)
        {
            Message = message;
            Line = line + 1;
            Column = column + 1;
        }

        public string Message { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
    }

    public class T4Callback : ITextTemplatingCallback
    {
        public List<T4Error> ErrorMessages { get; } = new List<T4Error>();
        public string FileExtension { get; private set; } = ".txt";
        public Encoding OutputEncoding { get; private set; } = Encoding.UTF8;

        public void ErrorCallback(bool warning, string message, int line, int column)
        {
            if (!warning)
                ErrorMessages.Add(new T4Error(message, line, column));
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