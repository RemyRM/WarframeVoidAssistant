using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WarframeVoidRewardChecker
{
    public partial class Inventory : Form
    {
        static List<WarframeMarketItemClass> allPrimeItems;
        static List<InventoryEntry> inventory = new List<InventoryEntry>();

        static Font itemEntryFont = new Font("Arial", 11);
        static Font headerFont = new Font("Arial", 12, FontStyle.Bold);

        static int startY = 0;
        static int itemBoxHeight = 30;

        static int mainPanelHeightOffset = 125;

        static Label itemImageLabel;
        static Label itemIDLabel;

        /// <summary>
        /// Inventory constructor, gets called before the load
        /// </summary>
        public Inventory()
        {
            InitializeComponent();

            allPrimeItems = WarframeMarketApi.GetAllPrimeItems();

            MainItemPanel.AutoScroll = true;
            for (int i = 0; i < allPrimeItems.Count; i++)
            {
                int yOffset = startY + (i * itemBoxHeight);
                inventory.Add(new InventoryEntry(allPrimeItems[i].item_name, false, false));

                Panel SubItemPanel = new Panel()
                {
                    Location = new Point(0, yOffset),
                    Width = 660,
                    Height = itemBoxHeight,
                };

                if (i % 2 == 0)
                {
                    SubItemPanel.BackColor = Color.WhiteSmoke;
                }

                LinkLabel itemLabel = new LinkLabel()
                {
                    AutoSize = true,
                    Location = new Point(0, 5),
                    Text = allPrimeItems[i].item_name,
                    Font = itemEntryFont,
                    ActiveLinkColor = Color.Black,
                    LinkColor = Color.Black,
                    LinkBehavior = LinkBehavior.NeverUnderline
                };
                itemLabel.Click += ItemLabel_Click;

                CheckBox craftedBPCheckbox = new CheckBox()
                {
                    Location = new Point(305, 5),
                };

                CheckBox craftedFinalItemCheckbox = new CheckBox()
                {
                    Location = new Point(525, 5),
                };


                SubItemPanel.Controls.Add(itemLabel);
                SubItemPanel.Controls.Add(craftedBPCheckbox);
                SubItemPanel.Controls.Add(craftedFinalItemCheckbox);

                MainItemPanel.Controls.Add(SubItemPanel);
            }
        }

        /// <summary>
        /// Fills the selected information panel with the clicked item's information
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemLabel_Click(object sender, EventArgs e)
        {
            LinkLabel s = (LinkLabel)sender;

            WarframeMarketItemClass item = allPrimeItems.FirstOrDefault(o => o.item_name.Equals(s.Text));

            itemIDLabel.Text = item.id;

            //Get the thumbnail of the item and display it in the image label
            Image thumbnail = null;
            using (var client = new WebClient())
            {
                byte[] buffer = client.DownloadData(item.thumb);
                using (var stream = new MemoryStream(buffer))
                {
                    thumbnail = Image.FromStream(stream);
                }
            }

            itemImageLabel.Image = thumbnail;
        }

        /// <summary>
        /// Gets called when the form gets loaded in
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Inventory_Load(object sender, EventArgs e)
        {
            Control control = (Control)sender;

            CreateHeaderLabels();

            //Set the position and size of the main panel in which the items are listed
            MainItemPanel.Location = new Point(10, 75);
            MainItemPanel.Height = control.Size.Height - mainPanelHeightOffset;
            MainItemPanel.Width = 700;

            CreateItemInformationPanel();
        }

        /// <summary>
        /// Creates the header labels at the top of the window
        /// </summary>
        private void CreateHeaderLabels()
        {
            Label ItemNameLabel = new Label()
            {
                Name = "ItemNameLabel",
                Text = "Item name",
                Font = headerFont,
                Location = new Point(10, 45),
                AutoSize = true
            };
            Controls.Add(ItemNameLabel);

            Label CraftedBPLabel = new Label()
            {
                Name = "CraftedBluePrintLabel",
                Text = "Crafted blueprint",
                Font = headerFont,
                Location = new Point(310, 45),
                AutoSize = true
            };
            Controls.Add(CraftedBPLabel);

            Label CraftedItemLabel = new Label()
            {
                Name = "CraftedItemLabel",
                Text = "Crafted item",
                Font = headerFont,
                Location = new Point(530, 45),
                AutoSize = true
            };
            Controls.Add(CraftedItemLabel);
        }

        /// <summary>
        /// Create the panel in which all the selected item's information will be shown
        /// </summary>
        private void CreateItemInformationPanel()
        {
            Panel ItemInformationPanel = new Panel()
            {
                Name = "ItemInformationPanel",
                BackColor = Color.Cyan,
                Size = new Size(300, 400),
                Location = new Point(725, 75)
            };
            Controls.Add(ItemInformationPanel);

            CreateIDLabel(ItemInformationPanel);
            CreateImageLabel(ItemInformationPanel);
        }
        /// <summary>
        /// Create the image panel in which the thumbnail of the selected item is shown
        /// </summary>
        private void CreateImageLabel(Control control)
        {
            Label imageLabel = new Label()
            {
                Name = "ImageLabel",
                BackColor = Color.Magenta,
                Size = new Size(128, 128),
                Location = new Point(162, 10),
            };

            itemImageLabel = imageLabel;
            control.Controls.Add(imageLabel);
        }

        private void CreateIDLabel(Control control)
        {
            Label itemIDHeader = new Label()
            {
                Name = "ItemIDHeaderLabel",
                //Font = new Font("Arial", 10, FontStyle.Bold),
                Font = itemEntryFont,
                Text = "ID:",
                AutoSize = true,
                Location = new Point(10, control.Size.Height - 30)
            };
            control.Controls.Add(itemIDHeader);

            Label itemID = new Label()
            {
                Name = "ItemID",
                Font = itemEntryFont,
                AutoSize = true,
                Location = new Point(35, control.Size.Height - 30)
            };
            control.Controls.Add(itemID);
            itemIDLabel = itemID;
        }
        /// <summary>
        /// Every time the window gets resized
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Inventory_Resize(object sender, EventArgs e)
        {
            Control control = (Control)sender;
            MainItemPanel.Height = control.Size.Height - mainPanelHeightOffset;

            if (control.Size.Width < MainItemPanel.Width)
            {
                MainItemPanel.Width = control.Size.Width - 25;
            }
            else
            {
                MainItemPanel.Width = 700;
            }
        }
    }

    internal class InventoryEntry
    {
        private string name;
        private bool hasCraftedBlueprint;
        private bool hasCraftedFinalItem;

        public InventoryEntry(string name, bool hasCraftedBlueprint, bool hasCraftedFinalItem)
        {
            this.name = name;
            this.hasCraftedBlueprint = hasCraftedBlueprint;
            this.hasCraftedFinalItem = hasCraftedFinalItem;
        }
    }
}
