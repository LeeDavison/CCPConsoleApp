using CCPConsoleApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CCPConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string[] CSVFiles = Directory.GetFiles(@"C:\CSVFiles\", "*.csv");

                if (CSVFiles.Length == 0)
                {
                    throw new Exception("No .csv files found");
                }

                foreach (var CSVFile in CSVFiles)
                {
                    // Import file
                    string errorLog = string.Empty;
                    string currentLine;
                    int currentLineCounter = 1;
                    bool firstlineFlag = true;
                    string textJSON = "[" + Environment.NewLine;
                    List<HeaderItem> headerList = new List<HeaderItem>();

                    using (FileStream fileStream = new FileStream(CSVFile, FileMode.Open, FileAccess.Read))
                    {
                        StreamReader streamReader = new StreamReader(fileStream, Encoding.UTF8);

                        while ((currentLine = streamReader.ReadLine()) != null)
                        {
                            if (firstlineFlag)
                            {
                                Console.WriteLine("Line " + currentLineCounter + " : Reading header . . .");

                                // Map headers
                                string firstLine = currentLine;
                                string[] firstLineArray = firstLine.Split(',');
                                int headerCounter = 0;

                                foreach (string header in firstLineArray)
                                {
                                    string headerType = string.Empty;
                                    string headerListName = string.Empty;

                                    if (String.IsNullOrEmpty(header))
                                        {
                                            throw new Exception("Empty header in file : " + CSVFile);
                                        }

                                    if (header.Contains("_"))
                                    {
                                        headerType = "List";
                                        headerListName = header.Split('_')[0];
                                    }
                                    else
                                    {
                                        headerType = "Single";
                                    }

                                    var headerItem = new HeaderItem()
                                    {
                                        Position = headerCounter,
                                        Type = headerType,
                                        Name = header,
                                        ListName = headerListName
                                    };

                                    headerList.Add(headerItem);
                                    headerCounter++;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Line " + currentLineCounter + " : Reading body . . .");

                                // Parse lines, map to JSON
                                List<string> valueList = new List<string>();
                                List<string> doneList = new List<string>();
                                string thisLine = currentLine;
                                string[] thisLineArray = thisLine.Split(',');
                                int currentCounter = 0;

                                if (textJSON.Length > 5)
                                {
                                    textJSON = textJSON + "," + Environment.NewLine;
                                }
                                textJSON = textJSON + "{";

                                foreach (string value in thisLineArray)
                                {
                                    if (String.IsNullOrEmpty(value))
                                    {
                                        throw new Exception("Empty value in file : " + CSVFile + ", line number : " + currentLineCounter);
                                    }

                                    HeaderItem thisItem = headerList.FirstOrDefault(h => h.Position == currentCounter);

                                    if (thisItem.Type == "Single")
                                    {
                                        textJSON = textJSON + Environment.NewLine + "\t\"" + thisItem.Name + "\": \"" + value + "\"";
                                        if (!headerList.Last().Equals(thisItem))
                                        {
                                            textJSON = textJSON + ",";
                                        }
                                    }
                                    else
                                    {
                                        string listType = thisItem.Name.Split('_')[0];
                                        if (!doneList.Contains(listType))
                                        {
                                            textJSON = textJSON + Environment.NewLine + "\t\"" + listType + "\": {";
                                            List<HeaderItem> headerMatchList = headerList.Where(h => h.ListName == listType).ToList();
                                            foreach (HeaderItem matchItem in headerMatchList)
                                            {
                                                textJSON = textJSON + Environment.NewLine + "\t\t\"" + matchItem.Name.Split('_')[1] + "\": \"" + thisLineArray[matchItem.Position] + "\"";
                                                if (!headerMatchList.Last().Equals(matchItem))
                                                {
                                                    textJSON = textJSON + ",";
                                                }
                                            }
                                            textJSON = textJSON + Environment.NewLine + "\t},";
                                            doneList.Add(listType);
                                        }
                                    }

                                    currentCounter++;
                                }
                                textJSON = textJSON + Environment.NewLine + "}";
                            }

                            firstlineFlag = false;
                            currentLineCounter++;
                        }

                        textJSON = textJSON + Environment.NewLine + "]";

                        File.WriteAllText(CSVFile.Replace(".csv", ".json"), textJSON);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Occured : " + Environment.NewLine + Environment.NewLine + e);
            }

            Console.ReadKey();
        }
    }
}
