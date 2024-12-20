using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HourData.Constants
{
    internal static class Constant
    {
        public const int HOUR_DATA_INTERVAL = -60;
        public const int DAILY_DATA_INTERVAL = -1;
        public const int WEEKLY_DATA_INTERVAL = -7;
        public const int HOUR_FOR_THINNING = 12;

        /// <summary>
        /// Массив каналов
        /// </summary>
        public static readonly string[] CHANNELS_NUMBERS = { 
            "107", "110", "113",
            "207", "210", "213",
            "307", "310", "313",
            "407", "410", "413",
            "507", "510", "513",
            "607", "610", "613",
            "707", "710", "713",
            "807", "810", "813",
            "907", "910", "913",
            "1007", "1010", "1013",
            "1107", "1110", "1113",
            "1207", "1210", "1213",
            "1307", "1310", "1313",
            "1407", "1410", "1413",
            "1507", "1510", "1513",
            "1607", "1610", "1613",
        };
    }                               
}
