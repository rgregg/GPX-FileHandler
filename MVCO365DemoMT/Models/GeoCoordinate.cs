using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCO365Demo.Models
{
    public class GeoCoordinate
    {
        public string Latitude
        {
            get;
            private set;
        }
        public string Longitude
        {
            get; private set;
        }

        public GeoCoordinate(string latitude, string longitude)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
        }

        public override string ToString()
        {
            return $"{Latitude},{Longitude}";
        }
    }
}