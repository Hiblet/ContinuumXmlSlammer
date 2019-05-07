using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;

namespace ContinuumXmlSlammer
{
    public class XmlInputConfiguration
    {
        public string SelectedField { get; private set; }
        public string PathDelimiter { get; private set; }
        public string AttrDelimiter { get; private set; }
        public string FieldNames { get; private set; }
        public string IndexGroups { get; private set; }

        // Note that the constructor is private.  Instances are created through the LoadFromConfigration method.
        private XmlInputConfiguration(
            string selectedField, 
            string pathDelimiter, 
            string attrDelimiter, 
            string fieldNames,
            string indexGroups)
        {
            SelectedField = selectedField;
            PathDelimiter = pathDelimiter;
            AttrDelimiter = attrDelimiter;
            FieldNames = fieldNames;
            IndexGroups = indexGroups;
        }

        public static XmlInputConfiguration LoadFromConfiguration(XmlElement eConfig)
        {
            string pathDelimiter = getStringFromConfig(eConfig, Constants.PATHDELIMITERKEY, Constants.DEFAULTPATHDELIMITER);
            string attrDelimiter = getStringFromConfig(eConfig, Constants.ATTRDELIMITERKEY, Constants.DEFAULTATTRDELIMITER);
            string selectedField = getStringFromConfig(eConfig, Constants.SELECTEDFIELDKEY, Constants.DEFAULTSELECTEDFIELD);
            string fieldNames = getStringFromConfig(eConfig, Constants.FIELDNAMESKEY, Constants.DEFAULTFIELDNAMES);
            string indexGroups = getStringFromConfig(eConfig, Constants.INDEXGROUPSKEY, Constants.DEFAULTINDEXGROUPS);

            return new XmlInputConfiguration(
                selectedField, 
                pathDelimiter, 
                attrDelimiter, 
                fieldNames, 
                indexGroups);
        }

        public static string getStringFromConfig(XmlElement eConfig, string key, string valueDefault)
        {
            string sReturn = valueDefault;

            XmlElement xe = eConfig.SelectSingleNode(key) as XmlElement;
            if (xe != null)
            {
                if (!string.IsNullOrEmpty(xe.InnerText))
                    sReturn = xe.InnerText;
            }

            return sReturn;
        }

        // Property Name Accessor
        public object this[string propertyName]
        {
            get { return this.GetType().GetProperty(propertyName).GetValue(this, null); }
            set { this.GetType().GetProperty(propertyName).SetValue(this, value, null); }
        }

    }
}
