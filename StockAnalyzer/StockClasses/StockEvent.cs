using System;
using System.Collections.Generic;
using System.Linq;

namespace StockAnalyzer.StockClasses
{
   public class StockEvent
   {
      #region Type Definition
      public enum EventType
      {
         BBOverrun = 0,
         BBUnderrun,
         EMA3Overrun,
         EMA3Underrun,
         RSIOverrun,
         RSIUnderrun,
         VIXOverrun,
         VIXUnderrun,
         GVZOverrun,
         GVZUnderrun,
         EVZOverrun,
         EVZUnderrun,
         EPCROverrun,
         EPCRUnderrun,
         HigherHigh,
         LowerLow,
         Top,
         Bottom,
         CloseToResistance,
         CloseToSupport,
         CrossedResistance,
         CrossedSupport,
         EndOfFlag,
         OutOfFlag,
         GapUp,
         GapDown,
         OopsUp,
         OopsDown,
         OVXOverrun,
         OVXUnderrun,
         PullBack,
         EndOfTrend,
         // Volume bar profile
         VolPro,
         VolAm,
         VolNoSupply,
         VolNoDemand,
         VolStoppingDown,
         VolStoppingUp,
         VolProfitTakingDown,
         VolProfitTakingUp,
         VolSpikeUp,
         VolSpikeDown,
         // Volume bar Types
         VolClimaxUp,
         VolClimaxDown,
         VolLowVolume,
         VolChurn,
         VolClimaxChurn
      }
      public enum EventFilterMode
      {
         EventAll,
         EventOne
      }
      #endregion
      #region CONSTANTS
      public const float RSI_UP_BOUND = 75.0f;
      public const float RSI_LOW_BOUND = 25.0f;
      public const float VAR_TRESHOLD = 0.03f;  // 3%
      public const float EVENT_TOLERANCE = 0.03f; // 5%
      public const float DAYS = 5;
      public const float DOJI_ACCURACY = 0.1f; // 10% of amlitude
      #endregion
      #region Constructor
      protected StockEvent()
      {
      }
      public StockEvent(EventType type, System.DateTime date)
      {
         this.Type = type;
         this.Date = date;
      }
      #endregion
      #region Public Properties
      public EventType Type { get; set; }
      public System.DateTime Date { get; set; }
      #endregion
      static public string[] GetSupportedEventTypes()
      {
         string[] eventTypeNames = Enum.GetNames(typeof(EventType));
         if (eventTypeNames.Count() > 64)
         {
            throw new System.OverflowException("Too many events => please reduce the amount of events");
         }
         return eventTypeNames;
      }
      static public StockEvent.EventFilterMode GetEventFilterMode()
      {
         return (StockEvent.EventFilterMode)Enum.Parse(typeof(StockEvent.EventFilterMode), StockAnalyzerSettings.Properties.Settings.Default.EventFilterMode);
      }
      #region event mask helper functions
      static public string EventTypesToString(StockEvent.EventType[] eventTypes)
      {
         string eventMask = string.Empty;
         foreach (StockEvent.EventType eventType in eventTypes)
         {
            if (!string.IsNullOrEmpty(eventMask))
            {
               eventMask += ";";
            }
            eventMask += eventType.ToString();
         }
         return eventMask;
      }
      static public StockEvent.EventType[] EventTypesFromString(string eventMask)
      {
         List<EventType> eventTypeList = new List<EventType>();
         foreach (string eventName in eventMask.Split(';'))
         {
            StockEvent.EventType eventType = (EventType)(-1);
            if (Enum.TryParse<EventType>(eventName, out eventType) && Enum.IsDefined(typeof(EventType), eventType))
            {
               eventTypeList.Add(eventType);
            }
         }
         return eventTypeList.ToArray();
      }
      static public bool EventMaskContains(string eventMask, StockEvent.EventType eventType)
      {
         return eventMask.Contains(eventType.ToString());
      }
      static public bool EventMaskMatch(string eventMask1, string eventMask2)
      {
         bool match = true;
         string[] events1 = eventMask1.Split(';');
         string[] events2 = eventMask2.Split(';');
         if (eventMask1.Length == eventMask2.Length)
         {
            foreach (string event1 in events1)
            {
               if (!events2.Contains(event1))
               {
                  match = false;
                  break;
               }
            }
         }
         else
         {
            match = false;
         }
         return match;
      }
      static public bool EventsMatch(StockEvent.EventType[] events1, StockEvent.EventType[] events2)
      {
         bool match = true;
         if (events1.Length == events2.Length)
         {
            foreach (StockEvent.EventType event1 in events1)
            {
               if (!events2.Contains(event1))
               {
                  match = false;
                  break;
               }
            }
         }
         else
         {
            match = false;
         }
         return match;
      }
      static public StockEvent.EventType[] EventsFromSettings()
      {
         return EventTypesFromString(StockAnalyzerSettings.Properties.Settings.Default.EventMarquees);
      }
      static public StockEvent.EventType[] AllEvents()
      {
         Array eventTypes = Enum.GetValues(typeof(StockEvent.EventType));
         StockEvent.EventType[] allEvents = new EventType[eventTypes.Length];
         for (int i = 0; i < eventTypes.Length; i++)
         {
            allEvents[i] = (StockEvent.EventType)eventTypes.GetValue(i);
         }
         return allEvents;
      }
      #endregion
   }
}
