using CodeBase;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDentBusiness {
	public class TimeCardL {
		///<summary>Returns the date that represents the start of the week in regard to the week of the date passed in.
		///The first day for the start of the week is determined by the TimeCardOvertimeFirstDayOfWeek preference.
		///Returns the date passed in when it is the start of the week.</summary>
		public static DateTime GetStartOfWeek(DateTime dateTime) {
			//Figure out what week of the year the date passed in part of.
			int weekOfYear=GetWeekOfYear(dateTime.Date);
			int weekOfYearPrevious=-1;
			//Find the date that is the official beginning of the week by subtracting one day at a time until the week of the year value changes.
			DateTime dateTimeStartOfWeek=new DateTime(dateTime.Ticks);//Make a deep copy of the date passed in that is safe to manipulate.
			for(int i=1;i<7;i++) {//This assumes every calendar has 7 days in a week.
				//Subtract one day at a time in order to find the official start of the week.
				weekOfYearPrevious=GetWeekOfYear(dateTime.AddDays(-i));
				if(weekOfYear!=weekOfYearPrevious) {
					//This day is within a different week and we have already found the beginning of the week at this point.
					return dateTimeStartOfWeek;
				}
				//This day is within the same week so subtract yet another day.
				dateTimeStartOfWeek=dateTime.AddDays(-i);
			}
			return dateTimeStartOfWeek;
		}

		///<summary>Returns the date that represents the end of the week in regard to the week of the date passed in.
		///The end of the week is determined by the TimeCardOvertimeFirstDayOfWeek preference.
		///Returns the date passed in when it is the end of the week.
		///Always goes backwards to find the most recent end of the week.
		///Meaning, this method can purposefully return a date that represents the end of the week prior to the week passed in.
		///E.g. If the preference is set to Wednesday and a date with a day of week set to Monday is passed in then this method will loop backwards until it comes across Tuesday (end of the week).
		///The first Tuesday this method comes across will be within a different week than the date passed in (the previous week).</summary>
		public static DateTime GetEndOfWeekForOvertime(DateTime dateTime) {
			DayOfWeek dayOfWeek=dateTime.DayOfWeek;
			DayOfWeek timeCardOvertimeFirstDayOfWeek=(DayOfWeek)PrefC.GetInt(PrefName.TimeCardOvertimeFirstDayOfWeek);
			//Figure out the day of the week that is considered the end of a complete week.
			DayOfWeek dayOfWeekComplete;
			if(timeCardOvertimeFirstDayOfWeek==DayOfWeek.Sunday) {//Enum value of 0 needs to wrap around to 6 - Saturday.
				dayOfWeekComplete=DayOfWeek.Saturday;
			}
			else {//Simply subtract one day from the day of week.
				int dayOfWeekCompleteWeek=(int)timeCardOvertimeFirstDayOfWeek - 1;
				dayOfWeekComplete=(DayOfWeek)dayOfWeekCompleteWeek;
			}
			DateTime dateTimeEndOfCompleteWeek=new DateTime(dateTime.Ticks);//Make a deep copy of the date passed in that is safe to manipulate.
			//Find the date that is the last date in the last 'official week' in relation to the date that was passed in.
			for(int i=0;i<7;i++) {//This assumes every calendar has 7 days in a week.
				dateTimeEndOfCompleteWeek=dateTime.AddDays(-i);
				if(dateTimeEndOfCompleteWeek.DayOfWeek==dayOfWeekComplete) {
					return dateTimeEndOfCompleteWeek;
				}
			}
			throw new ODException("End of week could not be found.");//This can only happen if the calendar has more than 7 days in a week.
		}

		///<summary>Returns the week of the year for the date passed in. The first day of the week is determined by the TimeCardOvertimeFirstDayOfWeek preference.</summary>
		public static int GetWeekOfYear(DateTime dateTime) {
			Calendar calendar=CultureInfo.CurrentCulture.Calendar;
			CalendarWeekRule calendarWeekRule=CalendarWeekRule.FirstFullWeek;
			DayOfWeek timeCardOvertimeFirstDayOfWeek=(DayOfWeek)PrefC.GetInt(PrefName.TimeCardOvertimeFirstDayOfWeek);
			//Figure out what week of the year the date passed in is.
			return calendar.GetWeekOfYear(dateTime.Date,calendarWeekRule,timeCardOvertimeFirstDayOfWeek);
		}

		///<summary>Returns a list of objects that represent groupings of both clock events and time adjusts as individual weeks.</summary>
		public static List<TimeCardWeek> GetTimeCardWeeks(List<ClockEvent> listClockEvents,List<TimeAdjust> listTimeAdjusts) {
			List<TimeCardWeek> listTimeCardWeeks=new List<TimeCardWeek>();
			//Merge the list of ClockEvent and the list of TimeAdjust objects into a list of TimeCardObjects sorted by ClockEvent.TimeDisplayed1 and TimeAdjust.TimeEntry.
			List<TimeCardObject> listTimeCardObjects=new List<TimeCardObject>();
			for(int i=0;i<listClockEvents.Count;i++) {
				listTimeCardObjects.Add(new TimeCardObject(listClockEvents[i]));
			}
			for(int i=0;i<listTimeAdjusts.Count;i++) {
				listTimeCardObjects.Add(new TimeCardObject(listTimeAdjusts[i]));
			}
			listTimeCardObjects=listTimeCardObjects.OrderBy(x => x.TimeEntry).ToList();
			//Group up all of the time card objects into their respective weeks.
			int weekOfYearPrevious=-1;
			for(int i=0;i<listTimeCardObjects.Count;i++) {
				int weekOfYear=GetWeekOfYear(listTimeCardObjects[i].TimeEntry);
				if(weekOfYear!=weekOfYearPrevious) {
					weekOfYearPrevious=weekOfYear;
				}
				TimeCardWeek timeCardWeek=listTimeCardWeeks.Find(x => x.WeekOfYear==weekOfYear);
				if(timeCardWeek==null) {
					timeCardWeek=new TimeCardWeek(weekOfYear);
					listTimeCardWeeks.Add(timeCardWeek);
				}
				timeCardWeek.ListTimeCardObjects.Add(listTimeCardObjects[i]);
			}
			return listTimeCardWeeks;
		}
	}

	public class TimeCardWeek {
		public int WeekOfYear=1;
		public List<TimeCardObject> ListTimeCardObjects=new List<TimeCardObject>();

		//Parameterless constructor for XmlConverterSerializer
		public TimeCardWeek() {
		}

		public TimeCardWeek(int weekOfYear) {
			WeekOfYear=weekOfYear;
			ListTimeCardObjects=new List<TimeCardObject>();
		}
	}

	///<summary>A wrapper class for ClockEvent and TimeAdjust objects.</summary>
	public class TimeCardObject {
		///<summary>Set to TimeDisplayed1 for ClockEvent objects and TimeEntry for TimeAdjust objects.</summary>
		public DateTime TimeEntry=DateTime.MinValue;
		///<summary>The ClinicNum set on the corresponding object.</summary>
		public long ClinicNum;
		///<summary>Shallow copy of the original object that this TimeCardObject was created from.</summary>
		public object Tag;

		//Parameterless constructor for XmlConverterSerializer
		public TimeCardObject() {
		}

		public TimeCardObject(ClockEvent clockEvent) {
			TimeEntry=clockEvent.TimeDisplayed1;
			ClinicNum=clockEvent.ClinicNum;
			Tag=clockEvent;
		}

		public TimeCardObject(TimeAdjust timeAdjust) {
			TimeEntry=timeAdjust.TimeEntry;
			ClinicNum=timeAdjust.ClinicNum;
			Tag=timeAdjust;
		}

		///<summary>Returns a TimeSpan that represents the sum of time worked and adjustments.</summary>
		public TimeSpan GetTimeSpanStraightTime() {
			TimeSpan timeSpanWorked=GetTimeSpanWorked();
			TimeSpan timeSpanAdjust=GetTimeSpanAdjust();
			TimeSpan timeSpan=timeSpanWorked + timeSpanAdjust;
			return timeSpan;
		}

		///<summary>Returns a TimeSpan that represents the sum of time worked, adjustments, and overtime.</summary>
		public TimeSpan GetTimeSpanTotal() {
			TimeSpan timeSpan=GetTimeSpanWorked();
			timeSpan=timeSpan.Add(GetTimeSpanAdjust());
			timeSpan=timeSpan.Add(GetTimeSpanOvertime());
			return timeSpan;
		}

		///<summary>Returns a TimeSpan that represents the difference between TimeDisplayed2 and TimeDisplayed1 when this object is a ClockEvent.
		///Returns TimeSpan.Zero if TimeDisplayed2 has not been set (user still clocked in) or this object is not a ClockEvent.</summary>
		private TimeSpan GetTimeSpanWorked() {
			TimeSpan timeSpan=TimeSpan.Zero;
			//Time worked is only supported for ClockEvent objects where the user has clocked out.
			if(Tag is ClockEvent clockEvent && clockEvent.TimeDisplayed2.Year > 1880) {
				timeSpan=(clockEvent.TimeDisplayed2 - clockEvent.TimeDisplayed1);
			}
			return timeSpan;
		}

		///<summary>Returns a TimeSpan that represents the amount of time that has been adjusted.
		///Returns AdjustAuto or Adjust when this object is a ClockEvent.
		///Returns RegHours when this object is a TimeAdjust.</summary>
		private TimeSpan GetTimeSpanAdjust() {
			TimeSpan timeSpan=TimeSpan.Zero;
			if(Tag is ClockEvent clockEvent) {
				timeSpan=clockEvent.AdjustAuto;
				if(clockEvent.AdjustIsOverridden) {
					timeSpan=clockEvent.Adjust;
				}
			}
			else if(Tag is TimeAdjust timeAdjust) {
				timeSpan=timeAdjust.RegHours;
			}
			return timeSpan;
		}

		///<summary>Returns a TimeSpan that represents hours worked at Rate 2.</summary>
		public TimeSpan GetTimeSpanRate2() {
			TimeSpan timeSpan=TimeSpan.Zero;
			if(Tag is ClockEvent clockEvent) {
				timeSpan=clockEvent.Rate2Auto;
				if(clockEvent.Rate2Hours!=TimeSpan.FromHours(-1)) {//Manual override
					timeSpan=clockEvent.Rate2Hours;
				}
			}
			return timeSpan;
		}

		///<summary>Returns a TimeSpan that represents hours worked at Rate 3.</summary>
		public TimeSpan GetTimeSpanRate3() {
			TimeSpan timeSpan=TimeSpan.Zero;
			if(Tag is ClockEvent clockEvent) {
				timeSpan=clockEvent.Rate3Auto;
				if(clockEvent.Rate3Hours!=TimeSpan.FromHours(-1)) {//Manual override
					timeSpan=clockEvent.Rate3Hours;
				}
			}
			return timeSpan;
		}

		///<summary>Returns a TimeSpan that represents overtime.
		///Returns OTimeAuto or OTimeHours when this object is a ClockEvent.
		///Returns OTimeHours when this object is a TimeAdjust.</summary>
		public TimeSpan GetTimeSpanOvertime() {
			TimeSpan timeSpan=TimeSpan.Zero;
			if(Tag is ClockEvent clockEvent) {
				timeSpan=clockEvent.OTimeAuto;
				if(clockEvent.OTimeHours!=TimeSpan.FromHours(-1)) {
					timeSpan=clockEvent.OTimeHours;//Manual overtime entry.
				}
			}
			else if(Tag is TimeAdjust timeAdjust) {
				timeSpan=timeAdjust.OTimeHours;
			}
			return timeSpan;
		}
	}
}
