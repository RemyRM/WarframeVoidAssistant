using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
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

        static List<InventorySetEntry> setsInventory = new List<InventorySetEntry>();
        //static List<InventoryItemEntry> inventory = new List<InventoryItemEntry>();

        static Font itemEntryFont = new Font("Arial", 11);
        static Font headerFont = new Font("Arial", 12, FontStyle.Bold);

        static int itemBoxHeight = 30;

        static int mainPanelHeightOffset = 125;
        static int itemPanelWidth = 660;

        static Label itemImageLabel;
        static Label itemIDLabel;
        static Label itemRelicLabel;
        static Label itemDucatsLabel;

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
        private void SaveInventoryData(bool createNewFile, bool isClosing = false)
        {
            if (createNewFile)
            {
                for (int i = 0; i < allPrimeSets.Count; i++)
                {
                    InventoryItemEntry[] itemEntry = new InventoryItemEntry[allPrimeSets[i].Count - 1];

                    for (int j = 1; j < allPrimeSets[i].Count; j++)
                    {
                        itemEntry[j - 1] = new InventoryItemEntry(allPrimeSets[i][j].item_name, false);
                    }
                    InventorySetEntry setEntry = new InventorySetEntry(allPrimeSets[i][0].item_name, false, itemEntry);
                    setsInventory.Add(setEntry);
                }
            }

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    OverrideSpecifiedNames = false
                }
            };

            string jsonNoFormatting = JsonConvert.SerializeObject(setsInventory, new JsonSerializerSettings
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

            if (!isClosing)
            {
                FillInventoryPanel();
            }
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
            List<JToken> filteredResults = jsonResult.ToList();

            foreach (JToken token in filteredResults)
            {
                InventorySetEntry item = token.ToObject<InventorySetEntry>();
                setsInventory.Add(item);
            }

            FillInventoryPanel();
        }

        /// <summary>
        /// Fill the main panel with the item entries
        /// </summary>
        private void FillInventoryPanel(string filter = null)
        {
            Console.WriteLine("started filling of inventory with {0} sets", setsInventory.Count);
            int nextYOffset = 0;
            for (int i = 0; i < setsInventory.Count; i++)
            {
                int indexI = i;
                ////If the item name doesn't match the filter we don't include it
                //if (filter != null && !allPrimeSets[i][0].item_name.Contains(filter))
                //{
                //    //Console.WriteLine("Not found in filter, continue.");
                //    continue;
                //}
                //else if (filter != null && allPrimeSets[i][0].item_name.Contains(filter))
                //{
                //    MainItemPanel.Controls.Clear();
                //    Console.WriteLine("Found {0} using filter {1}", allPrimeSets[i][0].item_name, filter);
                //}

                Panel itemSetPanel = new Panel()
                {
                    Name = setsInventory[i].Name,
                    Width = itemPanelWidth,
                    Height = itemBoxHeight,
                    Location = new Point(0, nextYOffset),
                    BackColor = Color.Magenta
                };
                MainItemPanel.Controls.Add(itemSetPanel);

                LinkLabel setName = new LinkLabel()
                {
                    //AutoSize = true,
                    Location = new Point(0, 5),
                    Text = setsInventory[i].Name,
                    Font = itemEntryFont,
                    ActiveLinkColor = Color.Black,
                    LinkColor = Color.Black,
                    LinkBehavior = LinkBehavior.NeverUnderline,
                    AutoSize = true,
                    Height = itemBoxHeight
                };
                itemSetPanel.Controls.Add(setName);
                setName.Click += ItemLabel_Click;

                //create a checkbox for wether or not the item itself was crafted
                CheckBox craftedSet = new CheckBox()
                {
                    Location = new Point(525, 5),
                    AutoSize = true,
                    Checked = setsInventory[i].HasCraftedSet
                };

                craftedSet.CheckedChanged += (sender, EventArgs) => { CraftedSet_CheckedChanged(sender, EventArgs, indexI); };
                itemSetPanel.Controls.Add(craftedSet);

                nextYOffset += 30;

                int itemYOffset = 30;
                int itemXOffset = 15;
                for (int j = 0; j < setsInventory[i].ItemsInSet.Length; j++)
                {
                    int indexJ = j;
                    Panel itemPanel = new Panel()
                    {
                        Name = setsInventory[i].ItemsInSet[j].Name,
                        Width = itemPanelWidth,
                        Height = itemBoxHeight,
                        Location = new Point(0, itemYOffset),
                    };
                    itemSetPanel.Controls.Add(itemPanel);

                    //Create a label containing the item name
                    LinkLabel itemLabel = new LinkLabel()
                    {
                        AutoSize = true,
                        Location = new Point(itemXOffset, 5),
                        Text = setsInventory[i].ItemsInSet[j].Name,
                        Font = itemEntryFont,
                        ActiveLinkColor = Color.Black,
                        LinkColor = Color.Black,
                        LinkBehavior = LinkBehavior.NeverUnderline
                    };
                    itemPanel.Controls.Add(itemLabel);
                    itemLabel.Click += ItemLabel_Click;

                    //create a checkbox for wether or not the bp has been crafted
                    CheckBox craftedBPCheckbox = new CheckBox()
                    {
                        Location = new Point(305, 5),
                        Checked = setsInventory[i].ItemsInSet[j].HasCraftedBlueprint
                    };

                    craftedBPCheckbox.CheckedChanged += (sender, EventArgs) => { CraftedBPCheckbox_CheckedChanged(sender, EventArgs, indexI, indexJ); };

                    itemPanel.Controls.Add(craftedBPCheckbox);

                    //Add this box's height to the total height, so the next entry will be underneath this one
                    itemSetPanel.Height += itemBoxHeight;

                    itemYOffset += itemBoxHeight;
                    nextYOffset += itemBoxHeight;

                }
                nextYOffset += 10;
            }
        }

        /// <summary>
        /// When the value of the item crafted checkbox changed we update the inventoryEntry accordingly
        /// </summary>
        private void CraftedSet_CheckedChanged(object sender, EventArgs e, int index)
        {
            CheckBox cBox = (CheckBox)sender;
            setsInventory[index].SetHasCraftedSet(cBox.Checked);
        }

        /// <summary>
        /// When the value of the BP crafted checkbox changed we update the inventoryEntry accordingly
        /// </summary>
        private void CraftedBPCheckbox_CheckedChanged(object sender, EventArgs e, int indexI, int indexJ)
        {
            CheckBox cBox = (CheckBox)sender;
            setsInventory[indexI].ItemsInSet[indexJ].SetHasCraftedBP(cBox.Checked);
        }

        /// <summary>
        /// Fills the selected information panel with the clicked item's information
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemLabel_Click(object sender, EventArgs e)
        {
            LinkLabel s = (LinkLabel)sender;

            WarframeItem wfItem = allPrimeItems.FirstOrDefault(o => o.item_name.Equals(s.Text));

            itemIDLabel.Text = wfItem.id;

            //Get the thumbnail of the item and display it in the image label
            Image thumbnail = null;
            using (var client = new WebClient())
            {
                byte[] buffer = client.DownloadData(wfItem.thumb);
                using (var stream = new MemoryStream(buffer))
                {
                    thumbnail = Image.FromStream(stream);
                }
            }

            itemImageLabel.Image = thumbnail;

            //string rawJson = WarframeMarketApi.RequestItemInfo(wfItem.url_name);

            //JObject jsonResults = JObject.Parse(rawJson);
            //List<JToken> filteredResults = jsonResults["payload"]["item"]["items_in_set"][0].ToList();
            //string ducats = "";
            //foreach (JToken item in filteredResults)
            //{
            //    if (item.Path.Contains("ducats"))
            //    {
            //        Console.WriteLine(item.Path);
            //        ducats = new string(item.ToString().Where(char.IsDigit).ToArray());
            //    }
            //}
            //Console.WriteLine(ducats);
        }

        #region layout

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
                Text = "Crafted set",
                Font = headerFont,
                Location = new Point(530, 45),
                AutoSize = true
            };
            Controls.Add(CraftedItemLabel);
        }

        /// <summary>
        /// Creates a searchbar to filter the item list
        /// </summary>
        private void CreateSearchBar()
        {
            TextBox searchField = new TextBox()
            {
                Name = "ItemSearchBox",
                Size = new Size(250, 15),
                Text = "Type to search",
                Font = itemEntryFont,
                Location = new Point(10, 15)
            };
            Controls.Add(searchField);
            searchField.GotFocus += SearchField_Click;
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
                Size = new Size(450, 450),
                Location = new Point(725, 75)
            };
            Controls.Add(ItemInformationPanel);

            CreateImageLabel(ItemInformationPanel);
            CreateIDLabel(ItemInformationPanel);
            CreateRelicLabel(ItemInformationPanel);
            CreateDucatsLabel(ItemInformationPanel);
        }

        private void CreateLaunchAssistantButton()
        {
            Button launchButton = new Button()
            {
                Name = "LaunchButton",
                Location = new Point(750, 550),
                Size = new Size(400, 100),
                Font = new Font("Arial", 20, FontStyle.Bold),
                Text = "Launch",
            };
            launchButton.Click += LaunchButton_Click;

            Controls.Add(launchButton);
        }



        /// <summary>
        /// Create the image panel in which the thumbnail of the selected item is shown
        /// </summary>
        private void CreateImageLabel(Control control)
        {
            int imageDimension = 128;
            int borderPadding = 10;
            Label imageLabel = new Label()
            {
                Name = "ImageLabel",
                BackColor = Color.Magenta,
                Size = new Size(imageDimension, imageDimension),
                Location = new Point(control.Width - imageDimension - borderPadding, borderPadding),
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
                Location = new Point(10, 10)
            };
            control.Controls.Add(itemIDHeader);

            Label itemID = new Label()
            {
                Name = "ItemID",
                Font = itemEntryFont,
                AutoSize = true,
                Location = new Point(35, 10)
            };
            control.Controls.Add(itemID);
            itemIDLabel = itemID;
        }

        private void CreateRelicLabel(Control control)
        {
            Label itemRelicHeader = new Label()
            {
                Name = "ItemRelicHeader",
                Text = "Relic:",
                Font = itemEntryFont,
                AutoSize = true,
                Location = new Point(10, 35),
            };
            control.Controls.Add(itemRelicHeader);

            Label itemRelic = new Label()
            {
                Name = "ItemRelic",
                Font = itemEntryFont,
                Text = "",
                AutoSize = true,
                Location = new Point(55, 35),
            };
            control.Controls.Add(itemRelic);
            itemRelicLabel = itemRelic;
        }

        private void CreateDucatsLabel(Control control)
        {
            Label itemDucatHeader = new Label()
            {
                Name = "ItemDucatsHeader",
                Font = itemEntryFont,
                Text = "Ducats:",
                AutoSize = true,
                Location = new Point(10, 60),
            };
            control.Controls.Add(itemDucatHeader);

            Label itemDucats = new Label()
            {
                Name = "ItemDucats",
                Font = itemEntryFont,
                Text = "100",
                AutoSize = true,
                Location = new Point(65, 60),
            };
            control.Controls.Add(itemDucats);
            itemDucatsLabel = itemDucats;
        }
        #endregion

        #region Listeners

        /// <summary>
        /// Gets called when the form gets loaded in
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Inventory_Load(object sender, EventArgs e)
        {
            Control control = (Control)sender;

            //Set the position and size of the main panel in which the items are listed
            MainItemPanel.Location = new Point(10, 75);
            MainItemPanel.Height = control.Size.Height - mainPanelHeightOffset;
            MainItemPanel.Width = 700;

            CreateHeaderLabels();
            CreateItemInformationPanel();
            CreateSearchBar();
            CreateLaunchAssistantButton();
        }

        private void Inventory_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveInventoryData(false, true);
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

        private void SearchField_Click(object sender, EventArgs e)
        {
            TextBox searchBox = (TextBox)sender;
            searchBox.Text = "";
        }

        VoidChecker voidChecker = null;
        /// <summary>
        /// Clicking the launch button will start the VoidChecker, if it is already running show a messagebox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LaunchButton_Click(object sender, EventArgs e)
        {
            if (voidChecker == null)
            {
                //StartChecker();

                //Uncomment this together with voidChecker = new VoidChecker in startChecker
                Thread t = new Thread(new ThreadStart(StartChecker));
                t.Start();
            }
            else
            {
                string caption = "Void checker already running";
                string message = "The void checker is already running...";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result;

                result = MessageBox.Show(caption, message, buttons);
            }
        }

        private void StartChecker()
        {
            //Use this to hide the program from the taskbar
            voidChecker = new VoidChecker();

            //Use this to show it on the taskbar
            //Form F = new VoidChecker();
            //F.ShowDialog();
        }
        #endregion
    }

    #region Inventory classes

    internal class InventorySetEntry
    {
        public string Name { get; private set; }
        public bool HasCraftedSet { get; private set; }
        public InventoryItemEntry[] ItemsInSet { get; private set; }

        public InventorySetEntry(string name, bool hasCraftedSet, InventoryItemEntry[] itemsInSet)
        {
            this.Name = name;
            this.HasCraftedSet = hasCraftedSet;
            this.ItemsInSet = itemsInSet;
        }

        public void SetHasCraftedSet(bool value)
        {
            HasCraftedSet = value;
        }
    }

    internal class InventoryItemEntry
    {
        public readonly string Name;
        public bool HasCraftedBlueprint { get; private set; }

        public InventoryItemEntry(string name, bool hasCraftedBlueprint)
        {
            this.Name = name;
            this.HasCraftedBlueprint = hasCraftedBlueprint;
        }

        public void SetHasCraftedBP(bool value)
        {
            HasCraftedBlueprint = value;
        }
    }
    #endregion
}
