using System;

using Newtonsoft.Json;

namespace AldurSoft.WurmApi
{
    /// <summary>
    /// Represents a single date/time in Wurm calendar.
    /// </summary>
    /// <remarks>
    /// 24h days, there are 7 days in each of 4 weeks of every starfall,
    /// there are 12 starfalls in each year. Years start at 0 and max at 99999.
    /// Resulution is to a single second only.
    /// Day, Week and Starfall start at 1 (including casting enums to values)
    /// Regular .NET TimeSpan can be added to and subtracted from this date (TimeSpan interpreted as wurm total days/time).
    /// Supports comparison, equality, hashing and serialization.
    /// Built for efficient access to members, not storage efficiency (each public field has cached value).
    /// Note: day value represents day in particular week not starfall, 
    /// which is Wurm's default way of presenting date.
    /// Overrides == != <![CDATA[<]]> <![CDATA[<=]]> <![CDATA[>]]> <![CDATA[>=]]>
    /// </remarks>
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public struct WurmDateTime : IComparable<WurmDateTime>
    {
        /// <summary>
        /// wurm time (val) / realtime (1)
        /// </summary>
        public const double WurmTimeToRealTimeRatio = 8.0D;

        private readonly long totalseconds;

        public long TotalSeconds
        {
            get
            {
                return this.totalseconds;
            }
        }

        private readonly int second;

        public int Second
        {
            get
            {
                return this.second;
            }
        }

        private readonly int minute;

        public int Minute
        {
            get
            {
                return this.minute;
            }
        }

        private readonly int hour;

        public int Hour
        {
            get
            {
                return this.hour;
            }
        }

        private readonly WurmDay day;

        public WurmDay Day
        {
            get
            {
                return this.day;
            }
        }

        private readonly int week;

        public int Week
        {
            get
            {
                return this.week;
            }
        }

        private readonly WurmStarfall starfall;

        public WurmStarfall Starfall
        {
            get
            {
                return this.starfall;
            }
        }

        private readonly int year;

        public int Year
        {
            get
            {
                return this.year;
            }
        }

        private const int MinuteSecs = 60,
                          HourSecs = 60 * 60,
                          DaySecs = 24 * 60 * 60,
                          WeekSecs = 7 * 24 * 60 * 60,
                          StarfallSecs = 4 * 7 * 24 * 60 * 60,
                          YearSecs = 12 * 4 * 7 * 24 * 60 * 60;

        public static WurmDateTime MinValue
        {
            get
            {
                return new WurmDateTime(0);
            }
        }

        public static WurmDateTime MaxValue
        {
            get
            {
                return new WurmDateTime(99999, 12, 4, 7, 23, 59, 59);
            }
        }

        public int DayInYear
        {
            get
            {
                return this.day.Number + (this.week - 1) * 7 + (this.starfall.Number - 1) * 28;
            }
        }

        public TimeSpan DayAndTimeOfYear
        {
            get
            {
                return new TimeSpan(this.DayInYear, this.hour, this.minute, this.second);
            }
        }

        /// <summary>
        /// Creates a new Wurm date/time object
        /// </summary>
        /// <param name="year">0 to 99999</param>
        /// <param name="starfall">1 to 12</param>
        /// <param name="week">1 to 4</param>
        /// <param name="day">1 to 7</param>
        /// <param name="hour">0 to 23</param>
        /// <param name="minute">0 to 59</param>
        /// <param name="second">0 to 59</param>
        public WurmDateTime(int year, int starfall, int week, int day, int hour, int minute, int second)
            : this(year, new WurmStarfall(starfall), week, new WurmDay(day), hour, minute, second)
        {
        }

        /// <summary>
        /// Create a new Wurm date/time object
        /// </summary>
        /// <param name="year">0 to 99999</param>
        /// <param name="starfall">starfall name</param>
        /// <param name="week">1 to 4</param>
        /// <param name="day">day name</param>
        /// <param name="hour">0 to 23</param>
        /// <param name="minute">0 to 59</param>
        /// <param name="second">0 to 59</param>
        public WurmDateTime(int year, WurmStarfall starfall, int week, WurmDay day, int hour, int minute, int second)
        {
            ValidateParameter(0, 99999, year, "year");
            ValidateParameter(1, 4, week, "week");
            ValidateParameter(0, 23, hour, "hour");
            ValidateParameter(0, 59, minute, "minute");
            ValidateParameter(0, 59, second, "second");
            // starfalls and days should be validated in their constructors

            this.second = second;
            this.minute = minute;
            this.hour = hour;
            this.day = day;
            this.week = week;
            this.starfall = starfall;
            this.year = year;

            this.totalseconds = second;
            this.totalseconds += minute * MinuteSecs;
            this.totalseconds += hour * HourSecs;
            this.totalseconds += (day.Number - 1) * DaySecs;
            this.totalseconds += (week - 1) * WeekSecs;
            this.totalseconds += (starfall.Number - 1) * StarfallSecs;
            this.totalseconds += (long)year * (long)YearSecs;
        }

        private static void ValidateParameter(int minInclusive, int maxInclusive, int value, string sourceName)
        {
            if (value < minInclusive)
            {
                throw new ArgumentException(
                    string.Format(
                        "Invalid WurmDateTime parameter, value {0} is lower than min {1} for argument {2}",
                        value,
                        minInclusive,
                        sourceName));
            }
            if (value > maxInclusive)
            {
                throw new ArgumentException(
                    string.Format(
                        "Invalid WurmDateTime parameter, value {0} is higher than max {1} for argument {2}",
                        value,
                        maxInclusive,
                        sourceName));
            }
        }

        /// <summary>
        /// Construct Wurm date/time from total wurm seconds value
        /// </summary>
        /// <param name="totalseconds"></param>
        public WurmDateTime(long totalseconds)
        {
            this.totalseconds = totalseconds;

            int yearNum = (int)(totalseconds / (long)YearSecs);
            totalseconds -= (long)yearNum * (long)YearSecs;

            int starfallNum = (int)(totalseconds / StarfallSecs);
            totalseconds -= starfallNum * StarfallSecs;

            int weekNum = (int)(totalseconds / WeekSecs);
            totalseconds -= weekNum * WeekSecs;

            int dayNum = (int)(totalseconds / DaySecs);
            totalseconds -= dayNum * DaySecs;

            int hourNum = (int)(totalseconds / HourSecs);
            totalseconds -= hourNum * HourSecs;

            int minuteNum = (int)(totalseconds / MinuteSecs);
            totalseconds -= minuteNum * MinuteSecs;

            int secondNum = (int)(totalseconds);

            this.second = secondNum;
            this.minute = minuteNum;
            this.hour = hourNum;
            this.day = new WurmDay(dayNum + 1);
            this.week = weekNum + 1;
            this.starfall = new WurmStarfall(starfallNum + 1);
            this.year = yearNum;
        }

        /// <summary>
        /// Returns true if this instance points to time point within supplied min and max constraints.
        /// </summary>
        /// <param name="minDate"></param>
        /// <param name="maxDate"></param>
        /// <returns></returns>
        public bool IsWithin(WurmDateTime minDate, WurmDateTime maxDate)
        {
            return this > minDate && this < maxDate;
        }

        /// <summary>
        /// Returns time difference between this date and supplied date. 
        /// Positive if supplied date is later than this date.
        /// </summary>
        /// <remarks>
        /// Note: breaking change since WA2, reversed polarity (take that borg!).
        /// </remarks>
        /// <param name="otherDate"></param>
        /// <returns></returns>
        public TimeSpan TimeTo(WurmDateTime otherDate)
        {
            return TimeSpan.FromSeconds(otherDate.TotalSeconds - this.TotalSeconds);
        }

        /// <summary>
        /// Returns string representing this date, in a format similar to how Wurm client shows it.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format(
                "{0:00}:{1:00}:{2:00} on day of {3} in week {4} of the {5} starfall in the year of {6}",
                this.hour,
                this.minute,
                this.second,
                this.day,
                this.week,
                this.starfall,
                this.year);
        }

        public int CompareTo(WurmDateTime other)
        {
            return this.TotalSeconds.CompareTo(other.TotalSeconds);
        }

        public static WurmDateTime operator +(WurmDateTime wdt, TimeSpan ts)
        {
            long val = wdt.TotalSeconds + (long)ts.TotalSeconds;
            if (val > MaxValue.TotalSeconds)
                val = MaxValue.TotalSeconds;
            return new WurmDateTime(val);
        }

        public static WurmDateTime operator -(WurmDateTime wdt, TimeSpan ts)
        {
            long val = wdt.TotalSeconds - (long)ts.TotalSeconds;
            if (val < MinValue.TotalSeconds)
                val = MinValue.TotalSeconds;
            return new WurmDateTime(val);
        }

        public static bool operator >(WurmDateTime arg1, WurmDateTime arg2)
        {
            return arg1.CompareTo(arg2) > 0;
        }

        public static bool operator <(WurmDateTime arg1, WurmDateTime arg2)
        {
            return arg1.CompareTo(arg2) < 0;
        }

        public static bool operator >=(WurmDateTime arg1, WurmDateTime arg2)
        {
            return arg1.CompareTo(arg2) >= 0;
        }

        public static bool operator <=(WurmDateTime arg1, WurmDateTime arg2)
        {
            return arg1.CompareTo(arg2) <= 0;
        }

        public static bool operator ==(WurmDateTime arg1, WurmDateTime arg2)
        {
            return arg1.CompareTo(arg2) == 0;
        }

        public static bool operator !=(WurmDateTime arg1, WurmDateTime arg2)
        {
            return arg1.CompareTo(arg2) != 0;
        }

        /// <summary>
        /// Checks for equality down to a single second resultion
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is WurmDateTime)
            {
                WurmDateTime wdt = (WurmDateTime)obj;
                return this.totalseconds == wdt.totalseconds;
            }
            else return false;
        }

        /// <summary>
        /// Checks for equality down to a single second resultion
        /// </summary>
        /// <param name="wdt"></param>
        /// <returns></returns>
        public bool Equals(WurmDateTime wdt)
        {
            return this.totalseconds == wdt.totalseconds;
        }

        public override int GetHashCode()
        {
            return this.totalseconds.GetHashCode();
        }
    }
}