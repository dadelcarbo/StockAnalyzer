using StockAnalyzer.StockClasses;
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

        private Timer timer;

        static public List<StockTimer> Timers = new List<StockTimer>();

        private StockTimer(StockAlertTimerCallback callBack, TimeSpan dueTime, TimeSpan endTime, TimeSpan period, StockAlertConfig alertConfig, StockBarDuration barDuration)
        {
            timer = new Timer(x =>
            {
                if (!TimerSuspended && DateTime.Now.TimeOfDay <= endTime)
                {
                    Task.Run(() => callBack(alertConfig, barDuration));
                }
            }, null, dueTime, period);
        }
        private StockTimer(StockTimerCallback callBack, TimeSpan dueTime, TimeSpan endTime, TimeSpan period)
        {
            this.timer = new Timer(x =>
            {
                if (!TimerSuspended && DateTime.Now.TimeOfDay < endTime)
                {
                    callBack();
                }
            }, null, dueTime, period);

            Timers.Add(this);
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
