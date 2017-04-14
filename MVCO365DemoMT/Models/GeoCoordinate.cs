using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace MVCO365Demo.Models
{
    [XmlRoot("rtept")]
    public class GeoCoordinate
    {
        [XmlAttribute("lat", Namespace = GPXFile.NS_GPX)]
        public string Latitude
        {
            get; set;
        }

        [XmlAttribute("lon", Namespace = GPXFile.NS_GPX)]
        public string Longitude
        {
            get; set;
        }

        [XmlElement("name", Namespace = GPXFile.NS_GPX)]
        public string Name
        {
            get; set;
        }

        [XmlElement("cmt", Namespace = GPXFile.NS_GPX)]
        public string Comment
        {
            get; set;
        }

        public GeoCoordinate()
        {

        }
    }
}