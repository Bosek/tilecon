using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TiledataConverter.Tiledata;

namespace TiledataConverter
{
    class Program
    {
        static void PrintHelp()
        {
            Console.WriteLine("https://github.com/Bosek/tilecon");
            Console.WriteLine();
            Console.WriteLine("USAGE: tilecon <VERB>");
            Console.WriteLine("VERBS:");
            Console.WriteLine("json, tojson\tconverts MUL file to JSON");
            Console.WriteLine("mul, tomul\tconverts JSON files to MUL");
        }
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp();
                return;
            }

            args = args.Select(arg => arg.ToLower()).ToArray();
            switch (args[0])
            {
                case "json":
                case "tojson":
                    {
                        if (!File.Exists("tiledata.mul"))
                        {
                            Console.WriteLine("tiledata.mul not found");
                            break;
                        }

                        var tiledataManager = new TiledataManager("tiledata.mul");
                        Console.WriteLine("Loading tiledata.mul");
                        var landTiledata = tiledataManager.LoadLandTiledata();
                        var staticTiledata = tiledataManager.LoadStaticTiledata();

                        Console.WriteLine("Creating landTiledata.json");
                        Json.SerializeToFile("landTiledata.json", TiledataManager.GetDict(landTiledata));

                        Console.WriteLine("Creating staticTiledata.json");
                        Json.SerializeToFile("staticTileData.json", TiledataManager.GetDict(staticTiledata));
                        break;
                    }
                case "mul":
                case "tomul":
                    {
                        if (!File.Exists("landTiledata.json"))
                        {
                            Console.WriteLine("landTiledata.json not found");
                            break;
                        }
                        if (!File.Exists("staticTiledata.json"))
                        {
                            Console.WriteLine("staticTiledata.json not found");
                            break;
                        }

                        Console.WriteLine("Loading landTiledata.json");
                        var landTiledataDict = (Dictionary<string, LandTiledata>)JsonConvert.DeserializeObject(
                            File.ReadAllText("landTiledata.json"), typeof(Dictionary<string, LandTiledata>));
                        var landTiledata = TiledataManager.GetList(landTiledataDict).ToArray();

                        Console.WriteLine("Loading staticTiledata.json");
                        var staticTiledataDict = (Dictionary<string, StaticTiledata>)JsonConvert.DeserializeObject(
                            File.ReadAllText("staticTiledata.json"), typeof(Dictionary<string, StaticTiledata>));
                        var staticTiledata = TiledataManager.GetList(staticTiledataDict).ToArray();

                        Console.WriteLine("Creating tiledata.mul");
                        TiledataManager.SaveTileData("tiledata.mul", landTiledata, staticTiledata);
                        break;
                    }
                default:
                    PrintHelp();
                    break;
            }
        }


    }
}
