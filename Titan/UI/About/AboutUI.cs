using System;
using System.Diagnostics;
using System.Reflection;
using Eto.Drawing;
using Eto.Forms;

namespace Titan.UI.About
{
    public class AboutUI : AboutDialog
    {
        
        public AboutUI() : base(Assembly.GetExecutingAssembly())
        {
            var assembly = Assembly.GetExecutingAssembly();
            
            Copyright = "Copyright \u00A9 2017-" + DateTime.Now.Year + " Marc3842h";
            Developers = new [] { "Marc3842h", "raspbianlike", "ra1N1336", "bananasss00" };
            Documenters = new[] { "Marc3842h", "ikfe", "BoberMod", "ZeroMemes", "matnguyen" };
            Logo = Titan.Instance.UIManager.SharedResources.TITAN_ICON;
            License = _license;
            
            var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>(); 
            Version = attribute != null ? attribute.InformationalVersion : 
                assembly.GetName().Version.Major + "." +
                assembly.GetName().Version.Minor + "." +
                assembly.GetName().Version.Build;
            
            Website = new Uri("https://github.com/Marc3842h/Titan");
            WebsiteLabel = "GitHub";
        }

        private string _license = 
            "MIT License\n\n" +
            "Copyright \u00A9 2017-" + DateTime.Now.Year + " Marc3842h\n\n" +
            "Permission is hereby granted, free of charge, to any person obtaining a copy\n" +
            "of this software and associated documentation files (the \"Software\"), to deal\n" +
            "in the Software without restriction, including without limitation the rights\n" +
            "to use, copy, modify, merge, publish, distribute, sublicense, and/or sell\n" +
            "copies of the Software, and to permit persons to whom the Software is\n" +
            "furnished to do so, subject to the following conditions:\n\n" +
            "The above copyright notice and this permission notice shall be included in all\n" +
            "copies or substantial portions of the Software.\n\n" +
            "THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR" +
            "IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY," +
            "FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE\n" +
            "AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER\n" +
            "LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,\n" +
            "OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE\n" +
            "SOFTWARE.";

    }
}