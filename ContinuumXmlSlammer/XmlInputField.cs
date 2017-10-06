using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ContinuumXmlSlammer
{
    public class XmlInputField
    {
        public string Name { get; set; }
        public AlteryxRecordInfoNet.FieldType FieldType { get; set; }
        public int Size { get; set; }
        public int Scale { get; set; }
    }
}
