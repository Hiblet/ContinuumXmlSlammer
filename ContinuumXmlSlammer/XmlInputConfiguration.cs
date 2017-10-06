using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;

namespace ContinuumXmlSlammer
{
    public class XmlInputConfiguration
    {
        public string FieldNames { get; private set; }
        public string SelectedField { get; private set; }
        public string PathDelimiter { get; private set; }
        public string AttrDelimiter { get; private set; }

        // Note that the constructor is private.  Instances are created through the LoadFromConfigration method.
        private XmlInputConfiguration(string selectedField, string pathDelimiter, string attrDelimiter, string fieldNames)
        {
            SelectedField = selectedField;
            PathDelimiter = pathDelimiter;
            AttrDelimiter = attrDelimiter;
            FieldNames = fieldNames;
        }

        public static XmlInputConfiguration LoadFromConfiguration(XmlElement eConfig)
        {
            string pathDelimiter = "/";
            string attrDelimiter = ".";
            string selectedField = "DownloadData";
            string fieldNames = "";

            // Get the saved pathDelimiter string
            XmlElement xmlElement = eConfig.SelectSingleNode(Constants.PATHDELIMITERKEY) as XmlElement;

            if (xmlElement != null)
            {
                if (!string.IsNullOrEmpty(xmlElement.InnerText))
                {
                    // We path have a Delimiter element and it contains text
                    pathDelimiter = xmlElement.InnerText;
                }
            }


            // Get the saved attrDelimiter string
            xmlElement = eConfig.SelectSingleNode(Constants.ATTRDELIMITERKEY) as XmlElement;
            if (xmlElement != null)
            {
                if (!string.IsNullOrEmpty(xmlElement.InnerText))
                {
                    attrDelimiter = xmlElement.InnerText;
                }
            }


            // Get the saved SelectedField string
            xmlElement = eConfig.SelectSingleNode(Constants.SELECTEDFIELDKEY) as XmlElement;

            if (xmlElement != null)
            {
                if (!string.IsNullOrWhiteSpace(xmlElement.InnerText))
                {
                    selectedField = xmlElement.InnerText;
                }
            }

            // Get the saved FieldNames string
            xmlElement = eConfig.SelectSingleNode(Constants.FIELDNAMESKEY) as XmlElement;

            if (xmlElement != null)
            {
                if (!string.IsNullOrWhiteSpace(xmlElement.InnerText))
                {
                    fieldNames = xmlElement.InnerText;
                }
            }

            return new XmlInputConfiguration(selectedField, pathDelimiter, attrDelimiter, fieldNames);
        }
    }
}
