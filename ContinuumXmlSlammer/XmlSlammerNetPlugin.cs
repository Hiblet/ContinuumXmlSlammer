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

        private RecordCopier _recordCopier;


        private string _selectedField = Constants.DEFAULTSELECTEDFIELD;
        private string _pathDelimiter = Constants.DEFAULTPATHDELIMITER;
        private string _attrDelimiter = Constants.DEFAULTATTRDELIMITER;
        private string _sIndexGroups = Constants.DEFAULTINDEXGROUPS;
        private bool _indexGroups = true;





        public void PI_Init(int nToolID, EngineInterface engineInterface, XmlElement pXmlProperties)
        {
            _toolID = nToolID;
            _engineInterface = engineInterface;
            _xmlProperties = pXmlProperties;

            // Use the information in the pXmlProperties parameter to get the input xml field name
            XmlElement configElement = XmlHelpers.GetFirstChildByName(_xmlProperties, "Configuration", true);
            if (configElement != null)
            {
                getConfigSetting(configElement, Constants.SELECTEDFIELDKEY, ref _selectedField);
                getConfigSetting(configElement, Constants.PATHDELIMITERKEY, ref _pathDelimiter);
                getConfigSetting(configElement, Constants.ATTRDELIMITERKEY, ref _attrDelimiter);

                getConfigSetting(configElement, Constants.INDEXGROUPSKEY, ref _sIndexGroups);
                _indexGroups = isTrueString(_sIndexGroups);

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

            prep();

            return true;
        }


        // Receive an inbound record and process it.
        // Information about the record is held in the recordInfo object
        // that was passed in to II_Init(), and (hopefully) cached.
        public bool II_PushRecord(AlteryxRecordInfoNet.RecordData recordDataIn)
        {
            // If we have no selected field, we can't continue.
            if (string.IsNullOrWhiteSpace(_selectedField)) return false;

            // Get the FieldBase for the "Input" data the contains the XML data.
            FieldBase xmlFieldBase = _recordInfoIn.GetFieldByName(_selectedField, false);

            // If that field doesn't exist, we can't continue.
            if (xmlFieldBase == null) return false;

            // Get the XML data from the current record.
            string xmlText = xmlFieldBase.GetAsString(recordDataIn);

            // Load XML doc
            XDocument doc = XDocument.Parse(xmlText);

            // If we haven't already done so, set up the output RecordInfo to describe the output data.
            //prepRecordInfoOut();

            // Walk the XML input.
            // Create one output record for each hierachy level.
            // Add one record for every attribute

            foreach (var xElement in doc.Root.DescendantsAndSelf())
            {
                string xPath = xElement.GetAbsoluteXPath(_indexGroups, _pathDelimiter); // The xml hierachy path
                string textContent = xElement.ShallowValue().Trim(); // Value in the text node

                pushRecord(xPath, textContent, recordDataIn);

                foreach (XAttribute xAttribute in xElement.Attributes())
                {
                    string xPathAndAttribute = xPath + _attrDelimiter + xAttribute.Name;
                    string attrContent = xAttribute.Value;

                    pushRecord(xPathAndAttribute, attrContent, recordDataIn);
                }
            }

            return true;
        }

        private void pushRecord(string path, string value, RecordData recordDataIn)
        {
            Record recordOut = _recordInfoOut.CreateRecord();
            recordOut.Reset();

            _recordCopier.Copy(recordOut, recordDataIn);

            var outFieldCount = _recordInfoOut.NumFields();
            int indexPathField = (int)outFieldCount - 2;
            int indexValueField = (int)outFieldCount - 1;

            // Path Field
            FieldBase fieldBase = _recordInfoOut[indexPathField];
            fieldBase.SetFromString(recordOut, path);

            // Value Field
            fieldBase = _recordInfoOut[indexValueField];
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

        private void prep()
        {
            // Exit if already done (safety)
            if (_recordInfoOut != null)
                return;

            _recordInfoOut = new AlteryxRecordInfoNet.RecordInfo();

            populateRecordInfoOut();

            _recordCopier = new RecordCopier(_recordInfoOut, _recordInfoIn, true);

            uint countFields = _recordInfoIn.NumFields();
            for (int i = 0; i < countFields; ++i)
            {
                var fieldName = _recordInfoIn[i].GetFieldName();

                var newFieldNum = _recordInfoOut.GetFieldNum(fieldName, false);
                if (newFieldNum == -1)
                    continue;

                _recordCopier.Add(newFieldNum, i);
            }

            _recordCopier.DoneAdding();

            _outputHelper.Init(_recordInfoOut, "Output", null, _xmlProperties);
        }

        private void populateRecordInfoOut()
        {
            _recordInfoOut = new AlteryxRecordInfoNet.RecordInfo();

            // Copy the fieldbase structure of the incoming record
            uint countFields = _recordInfoIn.NumFields();
            for (int i = 0; i < countFields; ++i)
            {
                FieldBase fbIn = _recordInfoIn[i];
                var currentFieldName = fbIn.GetFieldName();

                // Do not include the source data (selectedField) in the output 
                if (!String.Equals(currentFieldName, _selectedField, StringComparison.OrdinalIgnoreCase))
                {
                    _recordInfoOut.AddField(currentFieldName, fbIn.FieldType, (int)fbIn.Size, fbIn.Scale, fbIn.GetSource(), fbIn.GetDescription());
                }
            }

            // Add the output columns at the end
            _recordInfoOut.AddField("Path", FieldType.E_FT_V_WString, Constants.LARGEOUTPUTFIELDSIZE, 0, "", ""); // Empty strings are source and description
            _recordInfoOut.AddField("Value", FieldType.E_FT_V_WString, Constants.LARGEOUTPUTFIELDSIZE, 0, "", "");
        }


        //////////// 
        // HELPERS

        private void getConfigSetting(XmlElement configElement, string key, ref string memberToSet)
        {
            XmlElement xe = XmlHelpers.GetFirstChildByName(configElement, key, false);
            if (xe != null)
            {
                if (!string.IsNullOrWhiteSpace(xe.InnerText))
                    memberToSet = xe.InnerText;
            }
        }

        public static bool isTrueString(string target)
        {
            string cleanTarget = target.Trim().ToUpper();
            switch (cleanTarget)
            {
                case "Y":
                case "TRUE":
                case "1":
                    return true;
                default:
                    break;
            }
            return false;
        }

    }
}
