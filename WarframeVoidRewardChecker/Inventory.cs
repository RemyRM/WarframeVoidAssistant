using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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
        static int boxHeight = 30;

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
                int yOffset = startY + (i * boxHeight);
                inventory.Add(new InventoryEntry(allPrimeItems[i].item_name, false, false));

                Panel SubItemPanel = new Panel()
                {
                    Location = new Point(0, yOffset),
                    Width = 660,
                    Height = boxHeight
                };

                if (i % 2 == 0)
                {
                    SubItemPanel.BackColor = Color.WhiteSmoke;
                }

                Label itemLabel = new Label()
                {
                    AutoSize = true,
                    Location = new Point(0, 5),
                    Text = allPrimeItems[i].item_name,
                    Font = itemEntryFont
                };

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
        /// Gets called when the form gets loaded in
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Inventory_Load(object sender, EventArgs e)
        {
            //Set the position of the panel in which everything exists
            MainItemPanel.Location = new Point(10, 75);

            Label ItemNameLabel = new Label()
            {
                Text = "Item name",
                Font = headerFont,
                Location = new Point(10, 45),
                AutoSize = true
            };
            Controls.Add(ItemNameLabel);

            Label CraftedBPLabel = new Label()
            {
                Text = "Crafted blueprint",
                Font = headerFont,
                Location = new Point(310, 45),
                AutoSize = true
            };
            Controls.Add(CraftedBPLabel);

            Label CraftedItemLabel = new Label()
            {
                Text = "Crafted item",
                Font = headerFont,
                Location = new Point(530, 45),
                AutoSize = true
            };
            Controls.Add(CraftedItemLabel);
        }

        /// <summary>
        /// Every time the window gets resized
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Inventory_Resize(object sender, EventArgs e)
        {
            Control control = (Control)sender;
            MainItemPanel.Height = control.Size.Height - 75;

            if (control.Size.Width < MainItemPanel.Width)
            {
                MainItemPanel.Width = control.Size.Width - 25;
            }
            else
            {
                MainItemPanel.Width = 660;
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
