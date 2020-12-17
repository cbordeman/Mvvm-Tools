using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace MvvmTools.Models
{

    [XmlSchemaProvider("GenerateSchema")]
    public sealed class CDataWrapper : IXmlSerializable
    {
        // implicit to/from string
        public static implicit operator string(CDataWrapper value)
        {
            return value?.Value;
        }

        public static implicit operator CDataWrapper(string value)
        {
            return value == null ? null : new CDataWrapper {Value = value};
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        // return "xs:string" as the type in scheme generation
        public static XmlQualifiedName GenerateSchema(XmlSchemaSet xs)
        {
            return XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.String).QualifiedName;
        }

        // "" => <Node/>
        // "Foo" => <Node><![CDATA[Foo]]></Node>
        public void WriteXml(XmlWriter writer)
        {
            if (!string.IsNullOrEmpty(Value))
            {
                writer.WriteCData(Value);
            }
        }

        // <Node/> => ""
        // <Node></Node> => ""
        // <Node>Foo</Node> => "Foo"
        // <Node><![CDATA[Foo]]></Node> => "Foo"
        public void ReadXml(XmlReader reader)
        {
            if (reader.IsEmptyElement)
            {
                Value = "";
            }
            else
            {
                reader.Read();

                switch (reader.NodeType)
                {
                    case XmlNodeType.EndElement:
                        Value = ""; // empty after all...
                        break;
                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                        Value = reader.ReadContentAsString();
                        break;
                    default:
                        throw new InvalidOperationException("Expected text/cdata");
                }
            }
        }

        // underlying value
        public string Value { get; set; }

        public override string ToString()
        {
            return Value;
        }
    }
}