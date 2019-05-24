using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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

        static List<WarframeMarketItemClass> allPrimeItems = new List<WarframeMarketItemClass>();

        internal static List<WarframeMarketItemClass> GetAllPrimeItems()
        {
            return allPrimeItems;
        }

        /// <summary>
        /// Reads in a json containing *all* items from the warframe.market api and filters it down to just the prime items
        /// </summary>
        internal static void ReadWarframeMarketAllItemsJson()
        {
            if (CheckForPrimedItemsJson())
                return;

            WebRequest apiItemRequest = WebRequest.Create(apiCallAllItems);
            apiItemRequest.Credentials = CredentialCache.DefaultCredentials;
            HttpWebResponse response = (HttpWebResponse)apiItemRequest.GetResponse();
            Console.WriteLine("Response status: " + response.StatusCode);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine("Error in getting all items, code {0}", response.StatusCode);
                return;
            }

            string rawItemJson = "";
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                rawItemJson = reader.ReadToEnd();
            }

            JObject jsonResults = JObject.Parse(rawItemJson);
            List<JToken> filteredResults = jsonResults["payload"]["items"]["en"].Children().ToList();

            foreach (JToken token in filteredResults)
            {
                WarframeMarketItemClass item = token.ToObject<WarframeMarketItemClass>();
                //Get rid of any non-prime items (like mods) and items sets.
                if (item.item_name.Contains("Prime") && !item.item_name.Contains("Primed") &&
                    !item.item_name.Substring(item.item_name.Length - 3, 3).Equals("Set"))
                {
                    item.thumb = warframeMarketBaseUrl + item.thumb;
                    item.item_name = item.item_name.ToUpper();
                    allPrimeItems.Add(item);
                }

                //Order the items alphabetically by name
                allPrimeItems = allPrimeItems.OrderBy(o => o.item_name).ToList();
            }

            SavePrimesToJson();
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
                WarframeMarketItemClass item = token.ToObject<WarframeMarketItemClass>();
                allPrimeItems.Add(item);
            }
            Console.WriteLine("PrimeItems.json was loaded in");
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


            JsonSerializer serializer = new JsonSerializer
            {
                ContractResolver = contractResolver,
                NullValueHandling = NullValueHandling.Include
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

    internal class WarframeMarketItemClass
    {
        public string item_name;
        public string thumb;
        public string id;
        public string url_name;
    }
}
