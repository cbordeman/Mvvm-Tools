using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using MvvmTools.Core.Models;
using MvvmTools.Core.Utilities;

namespace MvvmTools.Core.Services
{
    public interface ITemplateService
    {
        List<Template> LoadTemplates(string localTemplateFolder);
    }

    public enum Section
    {
        Template, Field, ViewModelVisualBasic, ViewModelCSharp, CodeBehindVisualBasic, CodeBehindCSharp, View
    }

    public class TemplateService : ITemplateService
    {
        #region Data

        public const string LocalTemplatesFilename = "LocalTemplates.xml";
        
        List<ParseError> _errors;

        #endregion Data

        #region Ctor
        
        #endregion Ctor

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
                var rval = new List<Template>();

                var templates = Deserialize<List<Template>>(data);

                foreach (var t in templates)
                    t.IsInternal = isInternal;

                return rval;
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
        
    }
}