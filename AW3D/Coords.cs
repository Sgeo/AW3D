using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AW3D
{
    public class Coords
    {
        /// <summary>
        /// North is positive
        /// </summary>
        public decimal NS { get; set; }

        /// <summary>
        /// East is positive (to simplify the math. AW itself has east negative)
        /// </summary>
        public decimal EW { get; set; }

        public decimal Altitude { get; set; }

        /// <summary>
        /// Degrees counterclockwise from North
        /// </summary>
        public decimal Angle { get; set; }

        private static Regex REGEX = new Regex(@"(\d+\.\d\d)(N|S) (\d+\.\d\d)(W|E) (-?\d+\.\d\d)a (\d+)");

        public static Coords Parse(string coordstring)
        {
            Coords coords = new Coords();
            Match match = REGEX.Match(coordstring);
            coords.NS = Decimal.Parse(match.Groups[1].Value);
            if(match.Groups[2].Value == "S")
            {
                coords.NS *= -1;
            }
            coords.EW = Decimal.Parse(match.Groups[3].Value);
            if(match.Groups[4].Value == "W")
            {
                coords.EW *= -1;
            }
            coords.Altitude = Decimal.Parse(match.Groups[5].Value);
            coords.Angle = Decimal.Parse(match.Groups[6].Value);
            return coords;
        }

    }
}
