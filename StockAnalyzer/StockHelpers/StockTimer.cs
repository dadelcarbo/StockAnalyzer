using StockAnalyzer.StockClasses;
using StockAnalyzer.StockLogging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StockAnalyzer.StockHelpers
{
    public class StockTimer
    {
        public delegate void StockAlertTimerCallback(StockAlertConfig alertConfig, StockBarDuration barDuration);
        public delegate void StockTimerCallback();

        public static bool TimerSuspended { get; set; }
        private List<StockTimer> timers = new List<StockTimer>();

        private static ManualResetEvent resetEvent = new ManualResetEvent(true);
        private Timer timer;

        private StockTimer(StockAlertTimerCallback callBack, TimeSpan dueTime, TimeSpan endTime, TimeSpan period, StockAlertConfig alertConfig, StockBarDuration barDuration)
        {
            this.timer = new Timer(x =>
            {
                var time = DateTime.Now.TimeOfDay;
                StockLog.Write($"AlertTimer Time: {time} BarDuration: {barDuration}");
                if (!TimerSuspended && time < endTime)
                {
                    try
                    {
                        lock (resetEvent)
                        {
                            resetEvent.WaitOne();
                            resetEvent.Reset();
                        }
                        callBack(alertConfig, barDuration);
                    }
                    finally
                    {
                        resetEvent.Set();
                    }
                }
            }, null, dueTime, period);
        }
        private StockTimer(StockTimerCallback callBack, TimeSpan dueTime, TimeSpan endTime, TimeSpan period)
        {
            this.timer = new Timer(x =>
            {
                var time = DateTime.Now.TimeOfDay;
                StockLog.Write($"Timer Time: {time}");
                if (!TimerSuspended && time < endTime)
                {
                    try
                    {
                        lock (resetEvent)
                        {
                            resetEvent.WaitOne();
                            resetEvent.Reset();
                        }
                        callBack();
                    }
                    finally
                    {
                        resetEvent.Set();
                    }
                }
            }, null, dueTime, period);

            timers.Add(this);
        }

        public static StockTimer CreateAlertTimer(TimeSpan startTime, TimeSpan endTime, TimeSpan period, StockAlertConfig alertConfig, StockBarDuration barDuration, StockAlertTimerCallback callBack)
        {
            DateTime now = DateTime.Now;
            DateTime firstRun = DateTime.Today + startTime;
            while (now > firstRun)
            {
                firstRun = firstRun + period;
            }

            TimeSpan dueTime = firstRun - now + new TimeSpan(0, 0, 10);
            return new StockTimer(callBack, dueTime, endTime, period, alertConfig, barDuration);
        }
        public static StockTimer CreateRefreshTimer(TimeSpan startTime, TimeSpan endTime, TimeSpan period, StockTimerCallback callBack)
        {
            DateTime now = DateTime.Now;
            DateTime firstRun = DateTime.Today + startTime;
            while (now > firstRun)
            {
                firstRun = firstRun + period;
            }

            TimeSpan dueTime = firstRun - now + new TimeSpan(0, 0, 10);
            return new StockTimer(callBack, dueTime, endTime, period);
        }
    }
}
