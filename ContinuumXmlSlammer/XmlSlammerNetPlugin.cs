using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Linq;
using AlteryxRecordInfoNet;

namespace ContinuumXmlSlammer
{
    public class XmlSlammerNetPlugin : INetPlugin, IIncomingConnectionInterface
    {

        private int _toolID; // Integer identifier provided by Alteryx, this tools tool number.
        private EngineInterface _engineInterface; // Reference provided by Alteryx so that we can talk to the engine.
        private XmlElement _xmlProperties; // Xml configuration for this custom tool

        private PluginOutputConnectionHelper _outputHelper;

        private RecordInfo _recordInfoIn;
        private RecordInfo _recordInfoOut;

        private string _selectedField;
        private string _pathDelimiter = "/";
        private string _attrDelimiter = ".";





        public void PI_Init(int nToolID, EngineInterface engineInterface, XmlElement pXmlProperties)
        {
            _toolID = nToolID;
            _engineInterface = engineInterface;
            _xmlProperties = pXmlProperties;

            // Use the information in the pXmlProperties parameter to get the input xml field name
            XmlElement configElement = XmlHelpers.GetFirstChildByName(_xmlProperties, "Configuration", true);
            if (configElement != null)
            {
                XmlElement selectedFieldElement = XmlHelpers.GetFirstChildByName(configElement, Constants.SELECTEDFIELDKEY, false);
                if (selectedFieldElement != null)
                    _selectedField = selectedFieldElement.InnerText;

                XmlElement pathDelimiterElement = XmlHelpers.GetFirstChildByName(configElement, Constants.PATHDELIMITERKEY, false);
                if (pathDelimiterElement != null)
                    _pathDelimiter = pathDelimiterElement.InnerText;

                XmlElement attrDelimiterElement = XmlHelpers.GetFirstChildByName(configElement, Constants.ATTRDELIMITERKEY, false);
                if (attrDelimiterElement != null)
                    _attrDelimiter = attrDelimiterElement.InnerText;
            }

            _outputHelper = new PluginOutputConnectionHelper(_toolID, _engineInterface);
        }

        public IIncomingConnectionInterface PI_AddIncomingConnection(string pIncomingConnectionType, string pIncomingConnectionName)
        {
            return this;
        }

        public bool PI_AddOutgoingConnection(string pOutgoingConnectionName, OutgoingConnection outgoingConnection)
        {
            // Add the outgoing connection to our PluginOutputConnectionHelper so it can manage it.
            _outputHelper.AddOutgoingConnection(outgoingConnection);
            return true;
        }

        public bool PI_PushAllRecords(long nRecordLimit)
        {
            return true;
        }

        public void PI_Close(bool bHasErrors)
        {
            // Release any resources used by the control
            _outputHelper.Close();
        }

        public bool ShowDebugMessages()
        {
            // Return true to help us debug our tool. This should be set to false for general distribution.
            return true;
        }

        public XmlElement II_GetPresortXml(XmlElement pXmlProperties)
        {
            return null;
        }

        public bool II_Init(RecordInfo recordInfo)
        {
            _recordInfoIn = recordInfo;
            return true;
        }


        // Receive an inbound record and process it.
        // Information about the record is held in the recordInfo object
        // that was passed in to II_Init(), and (hopefully) cached.
        public bool II_PushRecord(AlteryxRecordInfoNet.RecordData pRecord)
        {
            // If we have no selected field, we can't continue.
            if (string.IsNullOrWhiteSpace(_selectedField)) return false;

            // Get the FieldBase for the "Input" data the contains the XML data.
            FieldBase xmlFieldBase = _recordInfoIn.GetFieldByName(_selectedField, false);

            // If that field doesn't exist, we can't continue.
            if (xmlFieldBase == null) return false;

            // Get the XML data from the current record.
            string xmlText = xmlFieldBase.GetAsString(pRecord);

            // Load XML doc
            XDocument doc = XDocument.Parse(xmlText);

            // If we haven't already done so, set up the output RecordInfo to describe the output data.
            if (_recordInfoOut == null)
            {
                // Create the schema of the output record
                AlteryxRecordInfoNet.RecordInfo recordInfo = new AlteryxRecordInfoNet.RecordInfo();

                recordInfo.AddField("Path", FieldType.E_FT_String, 4096, 0, "", ""); // Empty strings are source and description
                recordInfo.AddField("Value", FieldType.E_FT_String, 1024, 0, "", "");

                _recordInfoOut = recordInfo;

                // Use the new RecordInfo object to initialize the PluginOutputConnectionHelper.
                // The PluginOutputConnectionHelper can't be used until this step is performed.
                _outputHelper.Init(_recordInfoOut, "Output", null, _xmlProperties);
            }

            // Walk the XML input.
            // Create one output record for each hierachy level.
            // Add one record for every attribute

            foreach (var xElement in doc.Root.DescendantsAndSelf())
            {
                string xPath = xElement.GetAbsoluteXPath(_pathDelimiter); // The xml hierachy path
                string textContent = xElement.ShallowValue().Trim(); // Value in the text node

                pushRecord(xPath, textContent);

                foreach (XAttribute xAttribute in xElement.Attributes())
                {
                    string xPathAndAttribute = xPath + _attrDelimiter + xAttribute.Name;
                    string attrContent = xAttribute.Value;

                    pushRecord(xPathAndAttribute, attrContent);
                }
            }

            return true;
        }

        private void pushRecord(string path, string value)
        {
            Record recordOut = _recordInfoOut.CreateRecord();
            recordOut.Reset();

            // Path Field
            FieldBase fieldBase = _recordInfoOut[0];
            fieldBase.SetFromString(recordOut, path);

            // Value Field
            fieldBase = _recordInfoOut[1];
            fieldBase.SetFromString(recordOut, value);

            _outputHelper.PushRecord(recordOut.GetRecord());
        }

        public void II_UpdateProgress(double dPercent)
        {
            // Since our progress is directly proportional to he progress of the
            // upstream tool, we can simply output it's percentage as our own.
            if (_engineInterface.OutputToolProgress(_toolID, dPercent) != 0)
            {
                // If this returns anything but 0, then the user has canceled the operation.
                throw new AlteryxRecordInfoNet.UserCanceledException();
            }

            // Have the PluginOutputConnectionHelper ask the downstream tools to update their progress.
            _outputHelper.UpdateProgress(dPercent);
        }

        public void II_Close()
        {
        }


    }
}
