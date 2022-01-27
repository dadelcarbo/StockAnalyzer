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
        public delegate void StockAlertTimerCallback(List<string> alertDefs);

        public static bool TimerSuspended { get; set; }

        private StockTimer(StockAlertTimerCallback callBack, TimeSpan startTime, TimeSpan period, List<string> alertDefs)
        {
            new Timer(x =>
            {
                if (!TimerSuspended)
                {
                    callBack(alertDefs);
                }
            }, null, startTime, period);
        }

        public static StockTimer CreateAlertTimer(TimeSpan startTime, TimeSpan period, List<string> alertDefs, StockAlertTimerCallback callBack)
        {
            DateTime now = DateTime.Now;
            DateTime firstRun = DateTime.Today + startTime;
            while (now > firstRun)
            {
                firstRun = firstRun + period;
            }

            TimeSpan timeToGo = firstRun - now;
            return new StockTimer(callBack, timeToGo, period, alertDefs);
        }
    }

}
