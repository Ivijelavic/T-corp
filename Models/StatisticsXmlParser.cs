using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace TCorp.Models {
    public class StatisticsXmlParser {
        public XmlDocument Parse(Statistics statistics) {
            XmlDocument xml = new XmlDocument();
            XmlDeclaration declaration = xml.CreateXmlDeclaration("1.0", "UTF-8", String.Empty);
            xml.AppendChild(declaration);

            XmlElement root = xml.CreateElement("graphs");
            XmlAttribute width = xml.CreateAttribute("width");
            width.Value = statistics.Width.ToString();
            root.Attributes.Append(width);

            foreach (StatisticsItem si in statistics.Items) {
                XmlElement data = xml.CreateElement("graph");

                XmlAttribute name = xml.CreateAttribute("name");
                name.Value = si.Name;
                data.Attributes.Append(name);
                XmlAttribute value = xml.CreateAttribute("value");
                value.Value = si.Value.ToString();
                data.Attributes.Append(value);
                XmlAttribute color = xml.CreateAttribute("color");
                color.Value = si.Color;
                data.Attributes.Append(color);

                root.AppendChild(data);
            }

            xml.AppendChild(root);
            return xml;
        }
    }
}