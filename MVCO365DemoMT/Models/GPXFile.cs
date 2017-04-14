namespace MVCO365Demo.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Xml;
    using Models;
    using System.Xml.Serialization;

    

    [XmlRoot("gpx", Namespace = NS_GPX)]
    public class GPXFile
    {
        internal const string NS_GPX = "http://www.topografix.com/GPX/1/1";

        [XmlElement("metadata", Namespace = NS_GPX)]
        public GPXMetadata Metadata { get; set; }

        [XmlElement("rte", Namespace = NS_GPX)]
        public GPXRoute[] Route { get; set;}

        [XmlElement("wpt")]
        public GeoCoordinate[] Waypoints { get; set; }

        [XmlAnyElement]
        public XmlElement[] OtherElements { get; set; }

        [XmlAnyAttribute]
        public XmlAttribute[] OtherAttributes { get; set; }

        public static GPXFile FromStream(Stream stream, bool closeStream = true)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(GPXFile));
                return (GPXFile)serializer.Deserialize(stream);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (closeStream)
                {
                    stream.Close();
                }
            }
            
        }

        public void SerializeToStream(Stream output)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(GPXFile));
            serializer.Serialize(output, this);
        }
    }
}