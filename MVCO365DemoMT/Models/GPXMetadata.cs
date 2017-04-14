using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace MVCO365Demo.Models
{
    public class GPXMetadata
    {
        [XmlElement("name", Namespace = GPXFile.NS_GPX)]
        public string Name { get; set; }

        [XmlAnyElement]
        public XmlElement[] OtherElements { get; set; }

        [XmlAnyAttribute]
        public XmlAttribute[] OtherAttributes { get; set; }
    }
}