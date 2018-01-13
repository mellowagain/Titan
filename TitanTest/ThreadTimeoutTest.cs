using System;
using System.Threading.Tasks;
using Titan.Util;
using Xunit;

namespace TitanTest
{
    public class ThreadTimeoutTest
    {

        [Fact]
        public void TestTimeout()
        {
            Task.Run(() =>
            {
                try
                {
                    var origin = Task.Run(() =>
                    {
                        for (;;)
                        {
                            // Let the thread run for a infinite timespan so it can time out.
                        }

                        return 0; // Code is unreachable but we still need it here or else it won't get the result as int
                    });
                    var result = origin.RunUntil(TimeSpan.FromSeconds(3));
                    
                    Assert.True(false, "The thread didn't timeout and we received our result: " + result.Result);
                }
                catch (TimeoutException ex)
                {
                    Assert.True(true, "The thread timed successfully out after " + ex.Message + " seconds.");
                }

                Assert.True(false, "The thread never timed out.");
            });
        }

    }
}