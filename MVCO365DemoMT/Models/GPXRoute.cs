using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace MVCO365Demo.Models
{

    public class GPXBaseElement
    {
        [XmlElement("name", Namespace = GPXFile.NS_GPX)]
        public string Name { get; set; }

        [XmlElement("cmt", Namespace = GPXFile.NS_GPX)]
        public string Comment { get; set; }

        [XmlElement("desc", Namespace = GPXFile.NS_GPX)]
        public string Description { get; set; }

        [XmlAnyElement]
        public XmlElement[] OtherElements { get; set; }

        [XmlAnyAttribute]
        public XmlAttribute[] OtherAttributes { get; set; }
    }

    public class GPXRoute : GPXBaseElement
    {
        [XmlElement("rtept", Namespace = GPXFile.NS_GPX)]
        public GeoCoordinate[] Points { get; set; }

    }

}