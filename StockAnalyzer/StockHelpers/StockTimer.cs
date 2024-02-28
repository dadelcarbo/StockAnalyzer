using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;
using System.Timers;

namespace StockAnalyzer.StockHelpers
{
    public class StockTimer
    {
        public delegate void StockTimerCallback();
        public event StockTimerCallback TimerTick;

        public delegate void StockAlertTimerCallback(List<BarDuration> barDurations);
        public event StockAlertTimerCallback AlertTimerTick;

        public static bool TimerSuspended { get; set; }

        private readonly Timer timer;
        const int refreshPeriod = 10000; // 10 seconds

        static public List<StockTimer> Timers = new List<StockTimer>();
        public static void StopAll()
        {
            Timers.ForEach((t) => t.timer.Dispose());
            Timers.Clear();
        }

        readonly private TimeSpan startTime, endTime, period;
        #region Basic Timer (parameterless)
        private StockTimer(TimeSpan startTime, TimeSpan endTime, TimeSpan period, StockTimerCallback callback)
        {
            this.startTime = startTime;
            this.endTime = endTime;
            this.period = period;

            this.TimerTick += callback;
            this.timer = new Timer(refreshPeriod) { AutoReset = true, Enabled = true };
            this.timer.Elapsed += Timer_Elapsed;
            Timers.Add(this);
        }

        int previousTick = 0;
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var time = DateTime.Now.TimeOfDay;
            if (TimerSuspended || time < startTime)
                return;
            if (time > endTime)
            {
                this.timer.Stop();
                return;
            }
            var timeSeconds = time.TotalSeconds;
            var periodSeconds = period.TotalSeconds;
            var currentTick = (int)(timeSeconds / periodSeconds);
            if (currentTick == previousTick)
            {
                return;
            }
            previousTick = currentTick;

            TimerTick?.Invoke();
        }

        public static StockTimer CreateRefreshTimer(TimeSpan startTime, TimeSpan endTime, TimeSpan period, StockTimerCallback callback)
        {
            return new StockTimer(startTime, endTime, period, callback);
        }
        #endregion

        class PeriodTick
        {
            public int Tick { get; set; }
            public int PeriodSeconds { get; set; }
        }

        private readonly SortedDictionary<BarDuration, PeriodTick> periodTicks = new SortedDictionary<BarDuration, PeriodTick> {
            { BarDuration.M_5, new PeriodTick { Tick = 0, PeriodSeconds = 60*5} },
            { BarDuration.M_15, new PeriodTick { Tick = 0, PeriodSeconds = 60*15} },
            { BarDuration.M_30, new PeriodTick { Tick = 0, PeriodSeconds = 60*30} },
            { BarDuration.H_1, new PeriodTick { Tick = 0, PeriodSeconds = 60*60} },
            };

        public static StockTimer CreateAlertTimer(TimeSpan startTime, TimeSpan endTime, StockAlertTimerCallback callback)
        {
            return new StockTimer(startTime, endTime, callback);
        }


        private StockTimer(TimeSpan startTime, TimeSpan endTime, StockAlertTimerCallback callback)
        {
            this.startTime = startTime;
            this.endTime = endTime;

            this.AlertTimerTick += callback;
            this.timer = new Timer(refreshPeriod) { AutoReset = true, Enabled = true };
            this.timer.Elapsed += Timer_Elapsed1;
            Timers.Add(this);
        }

        private void Timer_Elapsed1(object sender, ElapsedEventArgs e)
        {
            var time = DateTime.Now.TimeOfDay;
            if (TimerSuspended || time < startTime || time > endTime)
                return;

            var timeSeconds = time.TotalSeconds;
            var barDurations = new List<BarDuration>();
            foreach (var tickPeriod in periodTicks)
            {
                var currentTick = (int)(timeSeconds / tickPeriod.Value.PeriodSeconds);
                if (currentTick != tickPeriod.Value.Tick)
                {
                    barDurations.Add(tickPeriod.Key);
                    tickPeriod.Value.Tick = currentTick;
                }
            }
            if (barDurations.Count > 0)
            {
                AlertTimerTick?.Invoke(barDurations);
            }
        }
    }
}

