using System;
using System.Threading.Tasks;
using Titan.Util;
using Xunit;

namespace Titan.Test
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
                    WaitFor<bool>.Run(TimeSpan.FromSeconds(3), () =>
                    {
                        for(;;)
                        {
                            // Let the thread run for a infinite timespan so it can time out.
                        }
                    });
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