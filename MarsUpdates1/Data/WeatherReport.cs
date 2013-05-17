using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace MarsUpdates.Data
{
    public enum WindDirection { East, West, South, North }

    [DataContract]
    public struct WeatherReport
    {
        public WeatherReport(int sol, DateTime datePublished, DateTime dateMeasured, string weather, double tempretureHighC,
                             double tempretureHighF, double tempretureLowC, double tempretureLowF, double pressurehPa, WindDirection wind,
                             double windSpeedKmh, double windSpeedMph, double daylightStartAM, double daylightEndPM)
        {
            this.sol = sol;
            this.datePublished = datePublished;
            this.dateMeasured = dateMeasured;
            this.weather = weather;

            this.tempretureHighC = tempretureHighC;
            this.tempretureHighF = tempretureHighF;
            this.tempretureLowC = tempretureLowC;
            this.tempretureLowF = tempretureLowF;

            this.pressurehPa = pressurehPa;
            this.wind = wind;

            this.windSpeedKmh = windSpeedKmh;
            this.windSpeedMph = windSpeedMph;

            this.daylightStartAM = daylightStartAM;
            this.daylightEndPM = daylightEndPM;
        }

        [DataMember]
        int sol;
        [DataMember]
        DateTime datePublished; //date published on twitter
        [DataMember]
        DateTime dateMeasured;  //date measured on mars
        [DataMember]
        string weather;
        [DataMember]
        double tempretureHighC;
        [DataMember]
        double tempretureHighF;
        [DataMember]
        double tempretureLowC;
        [DataMember]
        double tempretureLowF;
        [DataMember]
        double pressurehPa;
        [DataMember]
        WindDirection wind;
        [DataMember]
        double windSpeedKmh;
        [DataMember]
        double windSpeedMph;
        [DataMember]
        double daylightStartAM;
        [DataMember]
        double daylightEndPM;

        public int Sol
        {
            get { return sol; }//return string.Format("Sol: {0}", sol); }
            set { this.sol = value; }
        }

        public string Weather
        {
            get { return string.Format("{0}", weather); }
            set { weather = value; }
        }
        
        public string PubDateString
        {
            get { return string.Format("Published: '{0}",  datePublished.ToString("yy.MM.dd")); }
        }

        public DateTime PubDate
        {
            get { return datePublished; }//string.Format("Published: '{0}",  datePublished.ToString("yy.MM.dd")); }
            set { datePublished = value; }
        }

        public string MeasuredDateString
        {
            get { return string.Format("Measured: '{0}", dateMeasured.ToString("yy.MM.dd")); }
        }

        public string FullDateString
        {
            get { return string.Format(dateMeasured.ToString("yyyy.MM.dd")); }
        }

        public DateTime MeasuredDate
        {
            get { return dateMeasured; }
            set { dateMeasured = value; }
        }

        public double TempretureHighC
        {
            get { return tempretureHighC; }
            set { tempretureHighC = value; }
        }

        public double TempretureLowC
        {
            get { return tempretureLowC; }
            set { tempretureLowC = value; }
        }

        public double TempretureHighF
        {
            get { return tempretureHighF; }
            set { tempretureHighF = value; }
        }

        public double TempretureLowF
        {
            get { return tempretureLowF; }
            set { tempretureLowF = value; }
        }

        public double PressureHPA
        {
            get { return pressurehPa; }
            set { pressurehPa = value; }
        }

        public WindDirection WindDirection
        {
            get { return wind; }
            set { wind = value; }
        }

        public double WindSpeedKmh
        {
            get { return windSpeedKmh; }
            set { windSpeedKmh = value; }
        }

        public double WindSpeedMph
        {
            get { return windSpeedMph; }
            set { windSpeedMph = value; }
        }

        public double DaylightStartAM
        {
            get { return daylightStartAM; }
            set { daylightStartAM = value; }
        }

        public double DaylightEndPM
        {
            get { return daylightEndPM; }
            set { daylightEndPM = value; }
        }

        public string HighTemps
        {
            get { return string.Format("High: {0}°C/{1}°F", tempretureHighC, tempretureHighF); }
        }

        public string LowTemps
        {
            get { return string.Format("Low: {0}°C/{1}°F", tempretureLowC, tempretureLowF); }
        }

        public string PressureString
        {
            get { return string.Format("Pressure {0} hPa", pressurehPa); }
        }

        public string Daylight
        {
            get { return string.Format("Daylight: {0}am-{1}pm", daylightStartAM, daylightEndPM); }
        }

        public string WindDirectionAndSpeed
        {
            get 
            {
                string direction = "";
                switch (wind)
                {
                    case Data.WindDirection.East: direction = "⇐"; break;
                    case Data.WindDirection.West: direction = "⇒"; break;
                    case Data.WindDirection.North: direction = "⇑"; break;
                    case Data.WindDirection.South: direction = "⇓"; break;
                }
                return string.Format("Wind direction: {0}{1} at speed {2}kmh/{3}mph", direction, wind, windSpeedKmh, windSpeedMph); 
            }

        }

        public string DirectionAndSpeed
        {
            get
            {
                string direction = "";
                switch (wind)
                {
                    case Data.WindDirection.East: direction = "⇐"; break;
                    case Data.WindDirection.West: direction = "⇒"; break;
                    case Data.WindDirection.North: direction = "⇑"; break;
                    case Data.WindDirection.South: direction = "⇓"; break;
                }
                return string.Format("{0}{1} at {2}kmh/{3}mph", direction, wind, windSpeedKmh, windSpeedMph);
            }

        }
    }
}
