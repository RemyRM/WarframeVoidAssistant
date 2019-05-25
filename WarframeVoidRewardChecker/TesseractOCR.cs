using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

namespace WarframeVoidRewardChecker
{
    static class TesseractOCR
    {
        struct CharCombi
        {
            public readonly char endChar;
            public readonly char startChar;

            internal CharCombi(char a, char b)
            {
                endChar = a;
                startChar = b;
            }
        }
        static readonly CharCombi[] charSets = new CharCombi[]
        {
            new CharCombi('U', 'R'),
            new CharCombi('u', 'r'),
            new CharCombi('O', 'R'),
            new CharCombi('u', 'r')
        };

        /// <summary>
        /// Read the text contents from a screenshot
        /// </summary>
        /// <param name="testImagePath"></param>
        internal static void Tesseract(string testImagePath)
        {
            try
            {

                using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
                {
                    engine.SetVariable("tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
                    using (var img = Pix.LoadFromFile(testImagePath))
                    {
                        using (var page = engine.Process(img))
                        {
                            var text = page.GetText();

                            Console.WriteLine("Mean confidence: {0}", page.GetMeanConfidence());
                            #region checkIfNeeded

                            //Console.WriteLine("Text (GetText): \r\n{0}", text);
                            //Console.WriteLine("Text (iterator):");
                            //using (var iter = page.GetIterator())
                            //{
                            //    iter.Begin();

                            //    do
                            //    {
                            //        do
                            //        {
                            //            do
                            //            {
                            //                do
                            //                {
                            //                    if (iter.IsAtBeginningOf(PageIteratorLevel.Block))
                            //                    {
                            //                        //Console.WriteLine("<BLOCK>");
                            //                    }

                            //                    //Console.Write(iter.GetText(PageIteratorLevel.Word));
                            //                    //Console.Write(" ");

                            //                    if (iter.IsAtFinalOf(PageIteratorLevel.TextLine, PageIteratorLevel.Word))
                            //                    {
                            //                        //Console.WriteLine();
                            //                    }
                            //                } while (iter.Next(PageIteratorLevel.TextLine, PageIteratorLevel.Word));

                            //                if (iter.IsAtFinalOf(PageIteratorLevel.Para, PageIteratorLevel.TextLine))
                            //                {
                            //                    //Console.WriteLine();
                            //                }
                            //            } while (iter.Next(PageIteratorLevel.Para, PageIteratorLevel.TextLine));
                            //        } while (iter.Next(PageIteratorLevel.Block, PageIteratorLevel.Para));
                            //    } while (iter.Next(PageIteratorLevel.Block));
                            //}
                            #endregion

                            ValidateResults(ref text);

                            VoidChecker.results.Add(text);
                        }
                    }
                }
                Console.WriteLine("");
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                Debug.WriteLine("Unexpected Error: " + e.Message);
                Debug.WriteLine("Details: ");
                Debug.WriteLine(e.ToString());
            }
            //Console.Write("Press any key to continue . . . ");
            //Console.ReadKey(true);
        }

        /// <summary>
        /// Validate if the text obtained by tesseract is valid
        /// </summary>
        /// <param name="input"></param>
        static void ValidateResults(ref string input)
        {
            //Some character combinations end up getting recognised as whitespace, remove those whitespaces.
            foreach (CharCombi cc in charSets)
            {
                //if (input.Contains("BARREL"))
                //{
                if (input.Contains($"{cc.endChar} {cc.startChar}"))
                {
                    input = input.Replace($"{cc.endChar} {cc.startChar}", $"{cc.endChar}{cc.startChar}");
                }
                //}
            }

            //Remove any newlines Tesseract finds
            while (input.Contains("\n"))
            {
                input = input.Replace("\n", "");
            }

            if (CompareInputAgainstDictionary(input))
            {
                Console.WriteLine("{0} was found in the dictionary", input);
                return;
            }
            else
            {
                Console.WriteLine("{0} was NOT found in the dictionary", input);
                input = FindClosestMatch(input);
                Console.WriteLine("Found {0} after closest match", input);
            }
        }

        /// <summary>
        /// Check if the AllPrimeItems list has an entry that matches input
        /// </summary>
        /// <param name="input">Item name</param>
        /// <returns>Wether input is a valid warframe item</returns>
        static bool CompareInputAgainstDictionary(string input)
        {
            List<WarframeItem> itemClass = WarframeMarketApi.GetAllPrimeItems();

            return itemClass.Exists(o => o.item_name.Equals(input));
        }

        /// <summary>
        /// Sees if the first word in combination with any other words appears in the total items list, if this is the case we know it's that item
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        static string FindClosestMatch(string input)
        {
            //Formas arn't in the item list, but should still be recognised.
            if (input.Contains("FORMA"))
            {
                return "FORMA BLUEPRINT";
            }

            List<WarframeItem> itemClass = WarframeMarketApi.GetAllPrimeItems();

            string[] splitInput = input.Split(null);
            WarframeItem matchFirstWord = itemClass.FirstOrDefault(o => o.item_name.Contains(splitInput[0]));

            if (matchFirstWord != null)
            {
                Console.WriteLine("First word matched any word in total word list. Continueing");
            }
            else
            {
                Console.WriteLine("Error, no word matched {0}", splitInput[0]);
                //TODO: implement something that searches for the nearest word
                return "No match found";
            }

            //Make a list of lists containing the items, this so we can sort them by the index given by split input. This way we can see if any item is found twice, meaning we got a match.
            List<List<WarframeItem>> occuranceOfWordList = new List<List<WarframeItem>>();

            //Loop through all the items found with the keywords found in SplitInput. Add all the results found to matchesWord
            for (int i = 0; i < splitInput.Length; i++)
            {
                List<WarframeItem> matchesWord = itemClass.Where(o => o.item_name.Contains(splitInput[i])).ToList();
                occuranceOfWordList.Add(matchesWord);
            }

            //If any of the results found with the first keyword is also found in the second or third list we got a match, and we know it's that item.
            WarframeItem result = null;
            for (int i = 1; i <= splitInput.Length; i++)
            {
                try
                {
                    if (result == null)
                    {
                        result = occuranceOfWordList[0].Intersect(occuranceOfWordList[i]).FirstOrDefault();
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return result.item_name;
        }
    }
}

