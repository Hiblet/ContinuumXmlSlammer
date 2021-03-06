﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContinuumXmlSlammer
{
    public class Constants
    {
        // NOTE: THESE STRINGS SHOULD MATCH THE ACCESSORS IN XmlInputConfiguration.cs
        public static string SELECTEDFIELDKEY = "SelectedField";
        public static string PATHDELIMITERKEY = "PathDelimiter";
        public static string ATTRDELIMITERKEY = "AttrDelimiter";
        public static string FIELDNAMESKEY = "FieldNames"; // Last seen field
        public static string INDEXGROUPSKEY = "IndexGroups";

        // Default Values
        public static string DEFAULTINDEXGROUPS = "Y";
        public static string DEFAULTPATHDELIMITER = "■";
        public static string DEFAULTATTRDELIMITER = "▪";
        public static string DEFAULTSELECTEDFIELD = "Field_1";
        public static string DEFAULTFIELDNAMES = "Field_1";

        public static int LARGEOUTPUTFIELDSIZE = Int32.MaxValue;
        public static int SMALLOUTPUTFIELDSIZE = 512;



        // XMLDeclarations and ProcessingInstructions

        public static string XML_DECL = "XmlDeclaration";
        public static string XML_PI = "XmlProcessingInstruction";

        public static string XML_Version = "Version";
        public static string XML_Version_DEFAULT = "1.0";

        public static string XML_Encoding = "Encoding";
        public static string XML_Encoding_DEFAULT = "UTF-8";

        public static string XML_Standalone = "Standalone";
        public static string XML_Standalone_DEFAULT = "yes";

        
    }
}
