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
                    Func<bool> task = () =>
                    {
                        for (;;)
                        {
                            // Let the thread run for a infinite timespan so it can time out.
                        }
                    };

                    task.RunUntil(TimeSpan.FromSeconds(3));
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