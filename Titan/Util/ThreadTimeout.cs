using System;
using System.Threading;
using System.Threading.Tasks;

namespace Titan.Util
{
    public static class ThreadTimeout
    {
        
        // Source: https://stackoverflow.com/a/22078975/7360990
        public static async Task<TResult> RunUntil<TResult>(this Task<TResult> origin, TimeSpan timeout)
        {
            using (var token = new CancellationTokenSource())
            {
                var task = await Task.WhenAny(origin, Task.Delay(timeout, token.Token));

                if (task == origin)
                {
                    token.Cancel();
                    return await origin;
                }
                
                throw new TimeoutException("The Thread timed out.");
            }
        }
        
    }
}
