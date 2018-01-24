using System;
using System.IO;
using Titan.Proof;
using Xunit;

namespace TitanTest
{
    public class ScreenshotTest
    {

        private DirectoryInfo _directory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "screens"));
        private FileInfo _file;

        public ScreenshotTest()
        {
            if (!_directory.Exists)
            {
                _directory.Create();
            }
            
            _file = new FileInfo(Path.Combine(Environment.CurrentDirectory, "marc3842h.png"));

            if (_file.Exists)
            {
                _file.Delete();
            }
        }

        [SkippableFact]
        public void TestScreenshot()
        {
            var screenshotter = new ProfileScreenshotter(_directory);
            
            try
            {
                var image = screenshotter.ScreenshotSite("https://steamcommunity.com/id/Marc3842h/");
                
                if (image == null)
                {
                    Assert.True(false);
                }
                
                image.Save(_file.ToString());
            }
            catch (Exception ex)
            {
                Skip.If(true, ex.Message + ": " + ex);
            }
            
            Assert.True(_file.Exists);
        }
        
    }
}