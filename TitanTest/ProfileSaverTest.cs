using System;
using System.IO;
using Titan.Proof;
using Xunit;

namespace TitanTest
{
    public class ProfileSaverTest
    {

        [Fact]
        public void TestSaveProfile()
        {
            var saver = new ProfileSaver(new DirectoryInfo(Environment.CurrentDirectory));
            Assert.False(string.IsNullOrWhiteSpace(saver.SaveWebsite("https://steamcommunity.com/profiles/marc3842h")));
        }
        
    }
}
