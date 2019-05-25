using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace WarframeVoidRewardChecker
{
    public partial class Inventory : Form
    {
        readonly static string JSONpath = @"D:\Dev\C#\WarframeVoidRewardChecker\WarframeVoidRewardChecker\Resources\Files\";
        readonly static string inventorySaveFile = @"InventorySaveData.json";

        static List<WarframeItem> allPrimeItems;
        static List<List<WarframeItem>> allPrimeSets;
        static List<InventoryEntry> inventory = new List<InventoryEntry>();

        static Font itemEntryFont = new Font("Arial", 11);
        static Font headerFont = new Font("Arial", 12, FontStyle.Bold);

        static int startY = 0;
        static int itemBoxHeight = 30;

        static int mainPanelHeightOffset = 125;
        static int itemPanelWidth = 660;

        static Label itemImageLabel;
        static Label itemIDLabel;

        /// <summary>
        /// Inventory constructor, gets called before the load
        /// </summary>
        public Inventory()
        {
            InitializeComponent();
            MainItemPanel.AutoScroll = true;

            allPrimeItems = WarframeMarketApi.GetAllPrimeItems();
            allPrimeSets = WarframeMarketApi.GetAllPrimesBySet();

            if (File.Exists(JSONpath + inventorySaveFile))
            {
                LoadInventoryData();
            }
            else
            {
                SaveInventoryData(true);
            }
        }

        /// <summary>
        /// If no inventory save file exists we create a new one, and fill it with the base value of having everything false
        /// </summary>
        private void SaveInventoryData(bool createNewFile)
        {
            if (createNewFile)
            {
                //Create an inventory entry for each prime item
                for (int i = 0; i < allPrimeItems.Count; i++)
                {
                    inventory.Add(new InventoryEntry(allPrimeItems[i].item_name, false, false));
                }
            }

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    OverrideSpecifiedNames = false
                }
            };

            string jsonNoFormatting = JsonConvert.SerializeObject(inventory, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented
            });

            //Write the inventory to a JSON file
            using (StreamWriter sw = new StreamWriter(JSONpath + inventorySaveFile))
            {
                sw.Write(jsonNoFormatting);
            }
            if (!File.Exists(JSONpath + inventorySaveFile))
            {
                Console.WriteLine("Error creating file. File not found");
            }

            //FillInventoryPanel();
        }

        /// <summary>
        /// Load in the inventory save file
        /// </summary>
        private void LoadInventoryData()
        {
            string rawJson = "";
            using (StreamReader reader = new StreamReader(JSONpath + inventorySaveFile))
            {
                rawJson = reader.ReadToEnd();
            }

            JArray jsonResult = JArray.Parse(rawJson);
            List<JToken> filteredResults = jsonResult.Children().ToList();
            foreach (JToken token in filteredResults)
            {
                InventoryEntry item = token.ToObject<InventoryEntry>();
                inventory.Add(item);
            }

            FillInventoryPanel();
        }

        /// <summary>
        /// Fill the main panel with the item entries
        /// </summary>
        private void FillInventoryPanel(string filter = null)
        {
            Console.WriteLine("started filling of inventory with {0} sets", allPrimeSets.Count);
            int nextYOffset = -30;
            for (int i = 0; i < allPrimeSets.Count; i++)
            {
                //If the item name doesn't match the filter we don't include it
                //if (filter != null && !allPrimeSets[i][0].item_name.Contains(filter))
                //{
                //    Console.WriteLine("Not found in filter, continue.");
                //    continue;
                //}

                Panel itemSetPanel = new Panel()
                {
                    Name = allPrimeSets[i][0].item_name,
                    Width = itemPanelWidth,
                    Height = itemBoxHeight,
                    Location = new Point(0, nextYOffset),
                    //BackColor = Color.Magenta
                };
                MainItemPanel.Controls.Add(itemSetPanel);
                Console.WriteLine("Added main set panel {0} at {1}", itemSetPanel.Name, itemSetPanel.Location);

                int itemYOffset = 30;
                int itemXOffset = 15;
                for (int j = 0; j < allPrimeSets[i].Count; j++)
                {
                    itemXOffset = (j == 0) ? 0 : 15;

                    Panel itemPanel = new Panel()
                    {
                        Name = allPrimeSets[i][j].item_name,
                        Width = itemPanelWidth,
                        Height = itemBoxHeight,
                        Location = new Point(0, itemYOffset),
                    };
                    itemSetPanel.Controls.Add(itemPanel);

                    //if (i % 2 == 0)
                    //{
                    //    itemPanel.BackColor = Color.White;
                    //}
                    //else
                    //{
                    //    itemPanel.BackColor = Color.WhiteSmoke;
                    //}

                    //Create a label containing the item name
                    LinkLabel itemLabel = new LinkLabel()
                    {
                        AutoSize = true,
                        Location = new Point(itemXOffset, 5),
                        Text = allPrimeSets[i][j].item_name,
                        Font = itemEntryFont,
                        ActiveLinkColor = Color.Black,
                        LinkColor = Color.Black,
                        LinkBehavior = LinkBehavior.NeverUnderline
                    };
                    itemLabel.Click += ItemLabel_Click;
                    itemPanel.Controls.Add(itemLabel);

                    //create a checkbox for wether or not the bp has been crafted
                    if (j != 0)
                    {
                        CheckBox craftedBPCheckbox = new CheckBox()
                        {
                            Location = new Point(305, 5),
                            Checked = inventory[i].HasCraftedBlueprint
                        };
                        craftedBPCheckbox.CheckedChanged += CraftedBPCheckbox_CheckedChanged;
                        itemPanel.Controls.Add(craftedBPCheckbox);
                    }

                    //create a checkbox for wether or not the item itself was crafted
                    CheckBox craftedFinalItemCheckbox = new CheckBox()
                    {
                        Location = new Point(525, 5),
                        Checked = inventory[i].HasCraftedFinalItem
                    };
                    craftedFinalItemCheckbox.CheckedChanged += CraftedFinalItemCheckbox_CheckedChanged;
                    itemPanel.Controls.Add(craftedFinalItemCheckbox);

                    itemSetPanel.Height += itemBoxHeight;

                    itemYOffset += itemBoxHeight;
                    nextYOffset += itemBoxHeight;

                    Console.WriteLine("Added subpanel {0} at {1}", itemPanel.Name, itemPanel.Location);
                }
                nextYOffset += 10;
            }

            //for (int i = 0; i < inventory.Count; i++)
            //{
            //    int yOffset = startY + (i * itemBoxHeight);

            //    //Create a sub panel that contains the name and checkboxes
            //    Panel SubItemPanel = new Panel()
            //    {
            //        Location = new Point(0, yOffset),
            //        Width = 660,
            //        Height = itemBoxHeight,
            //        Name = inventory[i].Name
            //    };

            //    if (i % 2 == 0)
            //    {
            //        SubItemPanel.BackColor = Color.WhiteSmoke;
            //    }

            //    //Create a label containing the item name
            //    LinkLabel itemLabel = new LinkLabel()
            //    {
            //        AutoSize = true,
            //        Location = new Point(0, 5),
            //        Text = allPrimeItems[i].item_name,
            //        Font = itemEntryFont,
            //        ActiveLinkColor = Color.Black,
            //        LinkColor = Color.Black,
            //        LinkBehavior = LinkBehavior.NeverUnderline
            //    };
            //    itemLabel.Click += ItemLabel_Click;

            //    //create a checkbox for wether or not the bp has been crafted
            //    CheckBox craftedBPCheckbox = new CheckBox()
            //    {
            //        Location = new Point(305, 5),
            //        Checked = inventory[i].HasCraftedBlueprint
            //    };
            //    craftedBPCheckbox.CheckedChanged += CraftedBPCheckbox_CheckedChanged;

            //    //create a checkbox for wether or not the item itself was crafted
            //    CheckBox craftedFinalItemCheckbox = new CheckBox()
            //    {
            //        Location = new Point(525, 5),
            //        Checked = inventory[i].HasCraftedFinalItem
            //    };
            //    craftedFinalItemCheckbox.CheckedChanged += CraftedFinalItemCheckbox_CheckedChanged;

            //    //add the name and checkboxes to the sub panel 
            //    SubItemPanel.Controls.Add(itemLabel);
            //    SubItemPanel.Controls.Add(craftedBPCheckbox);
            //    SubItemPanel.Controls.Add(craftedFinalItemCheckbox);

            //    //add the panel to the main form
            //    MainItemPanel.Controls.Add(SubItemPanel);
            //}
        }

        /// <summary>
        /// When the value of the item crafted checkbox changed we update the inventoryEntry accordingly
        /// </summary>
        private void CraftedFinalItemCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cBox = (CheckBox)sender;
            int index = inventory.IndexOf(inventory.FirstOrDefault(o => o.Name.Equals(cBox.Parent.Name)));
            inventory[index].SetHasCraftedItem(cBox.Checked);
        }

        /// <summary>
        /// When the value of the BP crafted checkbox changed we update the inventoryEntry accordingly
        /// </summary>
        private void CraftedBPCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cBox = (CheckBox)sender;
            int index = inventory.IndexOf(inventory.FirstOrDefault(o => o.Name.Equals(cBox.Parent.Name)));
            inventory[index].SetHasCraftedBP(cBox.Checked);
        }

        /// <summary>
        /// Fills the selected information panel with the clicked item's information
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemLabel_Click(object sender, EventArgs e)
        {
            LinkLabel s = (LinkLabel)sender;

            WarframeItem item = allPrimeItems.FirstOrDefault(o => o.item_name.Equals(s.Text));

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

        #region inventory_listeners
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


        private void Inventory_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveInventoryData(false);
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
        #endregion
    }

    internal class InventoryEntry
    {
        public readonly string Name;
        public bool HasCraftedBlueprint { get; private set; }
        public bool HasCraftedFinalItem { get; private set; }

        public InventoryEntry(string name, bool hasCraftedBlueprint, bool hasCraftedFinalItem)
        {
            this.Name = name;
            this.HasCraftedBlueprint = hasCraftedBlueprint;
            this.HasCraftedFinalItem = hasCraftedFinalItem;
        }

        public string GetName()
        {
            return Name;
        }

        public void SetHasCraftedBP(bool value)
        {
            HasCraftedBlueprint = value;
        }

        public void SetHasCraftedItem(bool value)
        {
            HasCraftedFinalItem = value;
        }
    }
}
