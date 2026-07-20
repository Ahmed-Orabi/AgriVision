using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgriVision.Domain.ValueObjects
{
    public class Location
    {
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }

        private Location() { }

        public Location(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;

        }
    }
}
