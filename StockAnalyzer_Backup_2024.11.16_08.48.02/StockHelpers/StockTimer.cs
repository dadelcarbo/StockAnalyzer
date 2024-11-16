using StockAnalyzer.StockClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace StockAnalyzer.StockHelpers
{
    public class StockTimer
    {
        public delegate void StockTimerCallback();
        public event StockTimerCallback TimerTick;

        public delegate void EndOfDayHandler();
        public event EndOfDayHandler OnEndOfDay;

        public delegate void StockAlertTimerCallback(List<BarDuration> barDurations);
        public event StockAlertTimerCallback AlertTimerTick;

        public static bool TimerSuspended { get; set; }

        private readonly Timer timer;

        /// <summary>
        /// 5 seconds
        /// </summary>
        const int refreshPeriod = 5000;

        static public List<StockTimer> Timers = new List<StockTimer>();
        public static void StopAll()
        {
            Timers.ForEach((t) => t.timer.Dispose());
            Timers.Clear();
        }

        public static void Stop(StockTimer timer)
        {
            if (Timers.Contains(timer))
                Timers.Remove(timer);

            timer.timer.Dispose();
        }

        readonly private TimeSpan startTime, endTime, period;
        #region Basic Timer (parameterless)
        private StockTimer(TimeSpan startTime, TimeSpan endTime, TimeSpan period, StockTimerCallback callback)
        {
            this.startTime = startTime;
            this.endTime = endTime;
            this.period = period;

            this.TimerTick += callback;
            this.timer = new Timer(period.TotalMilliseconds) { AutoReset = true, Enabled = true };
            this.timer.Elapsed += Timer_Elapsed;
            Timers.Add(this);
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var time = e.SignalTime.TimeOfDay;
            if (TimerSuspended || time < startTime || time > endTime)
                return;

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
            { BarDuration.H_2, new PeriodTick { Tick = 0, PeriodSeconds = 60*120} },
            { BarDuration.H_3, new PeriodTick { Tick = 0, PeriodSeconds = 60*180} },
            { BarDuration.H_4, new PeriodTick { Tick = 0, PeriodSeconds = 60*240} },
            };

        public static StockTimer CreatePeriodicTimer(TimeSpan startTime, TimeSpan endTime, TimeSpan period, StockTimerCallback callback)
        {
            return new StockTimer(startTime, endTime, period, callback);
        }
        public static StockTimer CreateAlertTimer(TimeSpan startTime, TimeSpan endTime, StockAlertTimerCallback callback)
        {
            return new StockTimer(startTime, endTime, callback);
        }

        private StockTimer(TimeSpan startTime, TimeSpan endTime, StockAlertTimerCallback callback)
        {
            this.startTime = startTime;
            this.endTime = endTime;

            var timeSeconds = (int)DateTime.Now.TimeOfDay.TotalSeconds;
            foreach (var tickPeriod in periodTicks)
            {
                var currentTick = (int)(timeSeconds / tickPeriod.Value.PeriodSeconds);
                tickPeriod.Value.Tick = currentTick;
            }

            this.AlertTimerTick += callback;
            this.timer = new Timer(20000) { AutoReset = true, Enabled = true };
            this.timer.Elapsed += Timer_Elapsed1;
            Timers.Add(this);
        }

        private void Timer_Elapsed1(object sender, ElapsedEventArgs e)
        {
            var time = DateTime.Now.TimeOfDay;
            if (TimerSuspended || time < startTime || time > endTime)
                return;

            var timeSeconds = (int)time.TotalSeconds;
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

        public static StockTimer CreateDurationTimer(BarDuration duration, TimeSpan startTime, TimeSpan endTime, StockTimerCallback callback)
        {
            return new StockTimer(duration, startTime, endTime, callback);
        }
        private StockTimer(BarDuration duration, TimeSpan startTime, TimeSpan endTime, StockTimerCallback callback)
        {
            this.durationTicks = periodTicks[duration].PeriodSeconds;
            this.startTime = startTime;
            this.endTime = endTime;
            this.TimerTick += callback;

            this.timer = new Timer(500) { AutoReset = true, Enabled = true };
            this.timer.Elapsed += DurationTimer_Elapsed;
            Timers.Add(this);
        }

        int durationTicks;

        TimeSpan previousTickTime;
        private void DurationTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var time = new TimeSpan(e.SignalTime.Hour, e.SignalTime.Minute, 0);

            if (time > endTime)
            {
                this.OnEndOfDay?.Invoke();
            }

            if (TimerSuspended || previousTickTime == time || time < startTime || time > endTime)
                return;

            var timeSeconds = (int)time.TotalSeconds;

            if (timeSeconds % durationTicks == 0)
            {
                previousTickTime = time;
                TimerTick?.Invoke();
            }
        }
    }
}

