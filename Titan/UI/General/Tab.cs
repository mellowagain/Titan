using System.Collections.Generic;
using Eto.Drawing;
using Eto.Forms;

namespace Titan.UI.General
{
    public abstract class Tab
    {

        public UIManager UIManager;
        
        public Size TabSize;
        
        public List<DropDown> DropDownIndex = new List<DropDown>();

        protected Tab(UIManager uiManager, Size tabSize)
        {
            UIManager = uiManager;
            TabSize = tabSize;
        }
        
        public static void RefreshIndexesDropDown(DropDown drop)
        {
            drop.Items.Clear();
            
            foreach(var i in Titan.Instance.AccountManager.Accounts)
            {
                if(i.Key != -1)
                {
                    drop.Items.Add("#" + i.Key + " (" + i.Value.Count + " accounts)");
                }
            }

            drop.SelectedIndex = Titan.Instance.AccountManager.Index;
        }

        public abstract TabPage GetTabPage();

    }
}
