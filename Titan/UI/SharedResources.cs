using System.IO;
using Eto.Drawing;

namespace Titan.UI
{

    // ReSharper disable InconsistentNaming
    public class SharedResources
    {

        public Icon TITAN_ICON;

        public SharedResources()
        {
            var resDir = Path.Combine(Titan.Instance.Directory.ToString(), "Resources");

            TITAN_ICON = new Icon(File.Open(Path.Combine(resDir, "Logo.ico"), FileMode.Open));
        }

    }

}
