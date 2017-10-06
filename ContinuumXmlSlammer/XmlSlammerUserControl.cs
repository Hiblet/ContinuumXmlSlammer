using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using AlteryxGuiToolkit.Plugins;
using System.Xml;

namespace ContinuumXmlSlammer
{
    public partial class XmlSlammerUserControl : UserControl, IPluginConfiguration
    {
        public XmlSlammerUserControl()
        {
            InitializeComponent();
        }

        public Control GetConfigurationControl(
            AlteryxGuiToolkit.Document.Properties docProperties, 
            XmlElement eConfig, 
            XmlElement[] eIncomingMetaInfo, 
            int nToolId, 
            string strToolName)
        {
            // Call LoadFromConfiguration to get the xml file name and field information from eConfig.
            XmlInputConfiguration xmlConfig = XmlInputConfiguration.LoadFromConfiguration(eConfig);

            if (xmlConfig == null)
                return this;

            // Populate the ComboBox with field names
            // If there is no incoming connection, use what is stored

            string selectedField = xmlConfig.SelectedField;

            if (eIncomingMetaInfo == null || eIncomingMetaInfo[0] == null)
            {
                string fieldNames = xmlConfig.FieldNames;
                string[] arrFieldNames = fieldNames.Split(',');

                comboboxXmlSlammerField.Items.Clear();
                foreach (string fieldName in arrFieldNames)
                    comboboxXmlSlammerField.Items.Add(fieldName);

                // Select the saved field                
                if (!string.IsNullOrWhiteSpace(selectedField))
                {
                    int selectedIndex = comboboxXmlSlammerField.FindStringExact(selectedField);
                    if (selectedIndex > 0)
                        comboboxXmlSlammerField.SelectedIndex = selectedIndex;
                }
            }
            else
            {
                comboboxXmlSlammerField.Items.Clear();

                var xmlElementMetaInfo = eIncomingMetaInfo[0];
                var xmlElementRecordInfo = xmlElementMetaInfo.FirstChild;

                foreach (XmlElement elementChild in xmlElementRecordInfo)
                {
                    string fieldName = elementChild.GetAttribute("name");
                    string fieldType = elementChild.GetAttribute("type");

                    if (isStringType(fieldType))
                    {
                        comboboxXmlSlammerField.Items.Add(fieldName);
                    }
                }

                // If the selectedField matches a possible field in the combo box,
                // make it the selected field.
                // If the selectedField does not match, do not select anything and 
                // blank the selectedField.
                if (!string.IsNullOrWhiteSpace(selectedField))
                {
                    int selectedIndex = comboboxXmlSlammerField.FindStringExact(selectedField);
                    if (selectedIndex == -1)
                    {
                        // Not Found
                        XmlElement xmlElementSelectedField = XmlHelpers.GetOrCreateChildNode(eConfig, Constants.SELECTEDFIELDKEY);
                        xmlElementSelectedField.InnerText = "";
                    }
                    else
                    {
                        // Found
                        comboboxXmlSlammerField.SelectedIndex = selectedIndex;
                    }
                }
            } // end of "if (eIncomingMetaInfo == null || eIncomingMetaInfo[0] == null)"

            // Update the Delimiter textbox.
            textboxXmlSlammerPathDelim.Text = xmlConfig.PathDelimiter;

            // Update the Attribute textbox.
            textboxXmlSlammerAttrDelim.Text = xmlConfig.AttrDelimiter;

            return this;
        }

        // Helper
        private static bool isStringType(string fieldType)
        {
            return string.Equals(fieldType, "string", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(fieldType, "v_string", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(fieldType, "wstring", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(fieldType, "v_wstring", StringComparison.OrdinalIgnoreCase);
        }


        public void SaveResultsToXml(XmlElement eConfig, out string strDefaultAnnotation)
        {
            XmlElement xmlElementFieldNames = XmlHelpers.GetOrCreateChildNode(eConfig, Constants.FIELDNAMESKEY);
            List<string> fieldNames = new List<string>();
            foreach (var item in comboboxXmlSlammerField.Items)
                fieldNames.Add(item.ToString());

            xmlElementFieldNames.InnerText = string.Join(",", fieldNames);

            XmlElement xmlElementSelectedField = XmlHelpers.GetOrCreateChildNode(eConfig, Constants.SELECTEDFIELDKEY);
            var selectedItem = comboboxXmlSlammerField.SelectedItem;
            string selectedField = "";
            if (selectedItem != null) selectedField = comboboxXmlSlammerField.SelectedItem.ToString();
            xmlElementSelectedField.InnerText = selectedField;

            XmlElement xmlElementPathDelimiter = XmlHelpers.GetOrCreateChildNode(eConfig, Constants.PATHDELIMITERKEY);
            xmlElementPathDelimiter.InnerText = textboxXmlSlammerPathDelim.Text;

            XmlElement xmlElementAttrDelimiter = XmlHelpers.GetOrCreateChildNode(eConfig, Constants.ATTRDELIMITERKEY);
            xmlElementAttrDelimiter.InnerText = textboxXmlSlammerAttrDelim.Text;

            // Set the default annotation to be the name of the xml file.
            strDefaultAnnotation = "XML Slammer";
        }
    }
}
