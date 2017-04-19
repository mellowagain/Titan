using System;
using System.IO;
using Eto.Drawing;
using Eto.Forms;
using Titan.UI.Commands.Links;

namespace Titan.UI
{
    public sealed class MainForm : Form
    {

        private readonly string _icon = Environment.CurrentDirectory + Path.DirectorySeparatorChar + "Resources" +
                                        Path.DirectorySeparatorChar + "Logo.ico";

        public MainForm()
        {
            Title = "Titan";
            ClientSize = new Size(600, 160);
            Resizable = false;
            Icon = new Icon(File.Open(_icon, FileMode.Open));

            Content = new TableLayout
            {
                Spacing = new Size(5, 5),
                Padding = new Padding(10, 10, 10, 10),
                Rows =
                {
                    new TableRow(
                        new TableCell(new Label { Text = "Mode" }, true),
                        new TableCell(new DropDown { Items = { "Report"/*, "Commend"*/ }, SelectedIndex = 0 }, true)
                    ),
                    new TableRow(
                        new Label { Text = "Target" },
                        new TextBox { PlaceholderText = "76561198224231904" }
                    ),
                    new TableRow(
                        new Label { Text = "Match ID" },
                        new TextBox { PlaceholderText = "3203363151840018511" }
                    ),
                    new TableRow(new TableCell(), new TableCell()),
                    new TableRow(new TableCell(), new TableCell()),
                    new TableRow(new TableCell(), new TableCell()),
                    new TableRow(
                        new TableCell(),
                        new Button { Text = "Bomb!" }
                    ),
                    new TableRow { ScaleHeight = true }
                }
            };

            Menu = new MenuBar
            {
                Items =
                {
                    new ButtonMenuItem { Text = "&Links", Items = {
                        new SteamIO(),
                        new JsonValidator()
                    }}
                },
                AboutItem = new Commands.About(),
                QuitItem = new Commands.Quit()
            };

        }

    }
}