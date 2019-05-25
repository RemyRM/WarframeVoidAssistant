using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace WarframeVoidRewardChecker
{
    public static class WarframeMarketApi
    {
        //API docs: https://docs.google.com/document/d/1121cjBNN4BeZdMBGil6Qbuqse-sWpEXPpitQH5fb_Fo/edit#heading=h.irwashnbboeo
        readonly static string apiCallAllItems = @"https://api.warframe.market/v1/items";
        readonly static string JSONpath = @"D:\Dev\C#\WarframeVoidRewardChecker\WarframeVoidRewardChecker\Resources\Files\";
        readonly static string primeJsonFileName = "PrimeItems.json";

        readonly static string warframeMarketBaseUrl = @"https://warframe.market/static/assets/";

        static List<WarframeItem> allPrimeItems = new List<WarframeItem>();
        static List<List<WarframeItem>> allPrimesBySet = new List<List<WarframeItem>>();

        internal static List<WarframeItem> GetAllPrimeItems()
        {
            return allPrimeItems;
        }

        internal static List<List<WarframeItem>> GetAllPrimesBySet()
        {
            return allPrimesBySet;
        }

        /// <summary>
        /// Reads in a json containing *all* items from the warframe.market api and filters it down to just the prime items
        /// </summary>
        internal static void ReadWarframeMarketAllItemsJson()
        {
            if (CheckForPrimedItemsJson())
                return;

            //Create a call to the warframe market API to get a list of all the items
            WebRequest apiItemRequest = WebRequest.Create(apiCallAllItems);
            apiItemRequest.Credentials = CredentialCache.DefaultCredentials;

            //Get the response from the API
            HttpWebResponse response = (HttpWebResponse)apiItemRequest.GetResponse();

            Console.WriteLine("Response status: " + response.StatusCode);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine("Error in getting all items, code {0}", response.StatusCode);
                return;
            }

            string rawItemJson = "";
            //Read the actual data from the response
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                rawItemJson = reader.ReadToEnd();
            }

            SavePrimeItemsJson(rawItemJson);
        }

        /// <summary>
        /// Save the rawJson string containing the prime items list to json
        /// </summary>
        /// <param name="rawItemJson"></param>
        private static void SavePrimeItemsJson(string rawItemJson)
        {
            JObject jsonResults = JObject.Parse(rawItemJson);
            List<JToken> filteredResults = jsonResults["payload"]["items"]["en"].Children().ToList();

            foreach (JToken token in filteredResults)
            {
                WarframeItem item = token.ToObject<WarframeItem>();
                //Get rid of any non-prime items (like mods) and items sets.
                if (item.item_name.Contains("Prime") && !item.item_name.Contains("Primed"))
                {
                    item.SetThumb(warframeMarketBaseUrl + item.thumb);
                    item.SetItemName(item.item_name.ToUpper());
                    allPrimeItems.Add(item);
                }

            }
            //Order the items alphabetically by name
            allPrimeItems = allPrimeItems.OrderBy(o => o.item_name).ToList();
            SavePrimesBySet();

            SavePrimesToJson();
        }

        /// <summary>
        /// Creates a list of all set items, subdivided into lists of items with the set item on top
        /// </summary>
        private static void SavePrimesBySet()
        {
            for (int i = 0; i < allPrimeItems.Count; i++)
            {
                if (allPrimeItems[i].item_name.Contains("SET"))
                {
                    string setItemName = allPrimeItems[i].item_name;
                    List<WarframeItem> itemSet = new List<WarframeItem>
                    {
                        allPrimeItems[i]
                    };
                    for (int j = i; j >= 0; j--)
                    {
                        string sub = setItemName.Substring(0, setItemName.IndexOf(' '));
                        if (allPrimeItems[j].item_name.Contains(sub) && !allPrimeItems[j].item_name.Contains("SET"))
                        {
                            itemSet.Add(allPrimeItems[j]);
                        }
                        else if (!allPrimeItems[j].item_name.Contains(sub))
                        {
                            break;
                        }
                    }

                    allPrimesBySet.Add(itemSet);
                }
            }
        }

        /// <summary>
        /// Checks the local storage for a "PrimeItems.json"
        /// If it exists then load in this file
        /// </summary>
        /// <returns>wether "PrimeItems.json" exists</returns>
        internal static bool CheckForPrimedItemsJson()
        {
            if (File.Exists(JSONpath + primeJsonFileName))
            {
                ReadInPrimeItemsJson();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Read in the existing PrimeItems.json into "allPrimeItems" List
        /// </summary>
        internal static void ReadInPrimeItemsJson()
        {
            string rawJson = "";
            using (StreamReader reader = File.OpenText(JSONpath + primeJsonFileName))
            {
                rawJson = reader.ReadToEnd();
            }

            JArray jsonResults = JArray.Parse(rawJson);
            List<JToken> filteredResults = jsonResults.Children().ToList();
            foreach (JToken token in filteredResults)
            {
                WarframeItem item = token.ToObject<WarframeItem>();
                allPrimeItems.Add(item);
            }
            Console.WriteLine("PrimeItems.json was loaded in");
            SavePrimesBySet();
        }

        /// <summary>
        /// Saves the filtered list of prime items to a new json file called "PrimeItems.json"
        /// </summary>
        internal static void SavePrimesToJson()
        {
            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    OverrideSpecifiedNames = false
                }
            };

            string jsonNoFormatting = JsonConvert.SerializeObject(allPrimeItems, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented
            });

            using (StreamWriter sw = new StreamWriter(JSONpath + primeJsonFileName))
            {
                sw.Write(jsonNoFormatting);
            }
            Console.WriteLine("PrimeItems.json was sucesfully created");
        }
    }

    internal class WarframeItem
    {
        public string item_name;
        public string thumb;
        public string id;
        public string url_name;

        public void SetItemName(string value)
        {
            item_name = value;
        }

        public void SetThumb(string value)
        {
            thumb = value;
        }

        public void SetId(string value)
        {
            id = value;
        }

        public void SetUrlName(string value)
        {
            url_name = value;
        }
    }
}
