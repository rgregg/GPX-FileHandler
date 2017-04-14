namespace MVCO365Demo.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Xml;
    using Models;

    public class GPXHelper
    {
        public XmlDocument doc { get; set; }

        public bool LoadGPXFromStream(Stream stream) {
            
            try 
            {
                this.doc = new XmlDocument();
                StreamReader reader = new StreamReader(stream);
                String responseString = reader.ReadToEnd();
                this.doc.LoadXml(responseString);
            } 
            catch(Exception)
            {
                this.doc = null;
                throw;
            }
            finally
            {
                stream.Close();
            }

            return this.doc != null;
        }

        public List<GeoCoordinate> getPointsFromGPX()
        {
            XmlNodeList nodes = this.doc.GetElementsByTagName("trkpt");
            List<GeoCoordinate> x = new List<GeoCoordinate>();

            foreach (XmlNode node in nodes)
            {
                String latNum = node.Attributes["lat"].Value;
                String longNum = node.Attributes["lon"].Value;
                x.Add(new GeoCoordinate(latNum, longNum));
            }
          
            return x;
        }

        public String getTitle()
        {
            XmlNodeList nodes = this.doc.GetElementsByTagName("name");
            return nodes[0].InnerText;
        }

        public void setTitle(String newtitle)
        {
            XmlNodeList nodes = this.doc.GetElementsByTagName("name");
            nodes[0].InnerText = newtitle;

            nodes = this.doc.GetElementsByTagName("name");
            var x = nodes[0].InnerText;

        }

    }
}