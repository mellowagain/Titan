using System;
using System.Threading.Tasks;

namespace Titan.Util
{
    public static class ThreadTimeout
    {
        
        public static T RunUntil<T>(this Func<T> function, TimeSpan timeout)
        {
            var result = default(T);
            void Action() => result = function();

            try
            {
                var task = Task.Factory.StartNew(Action, Task.Factory.CancellationToken);

                if (task.Wait(timeout))
                {
                    return result;
                }
                
                throw new AggregateException();
            }
            catch (AggregateException ex)
            {
                throw new TimeoutException("Thread timed out.", ex);
            }
        }
        
    }
}