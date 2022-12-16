using System;
using System.Threading.Tasks;

namespace StockAnalyzer.StockHelpers
{
    public static class TaskExtension
    {
        public static async Task<TResult> WithTimeout<TResult>(this Task<TResult> task, int milliSeconds)
        {
            if (task == await Task.WhenAny(task, Task.Delay(TimeSpan.FromMilliseconds(milliSeconds))))
            {
                return await task;
            }
            throw new TimeoutException();
        }
    }
}
