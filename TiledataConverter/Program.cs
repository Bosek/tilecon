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
            Console.WriteLine("USAGE: tilecon <VERB> <FILENAME>");
            Console.WriteLine("VERBS:");
            Console.WriteLine("\tjson, tojson\tconverts MUL file to JSON, <FILENAME> is input here");
            Console.WriteLine("\tmul, tomul\tconverts JSON files to MUL, <FILENAME> is output here");
            Console.WriteLine("FILENAME defaults to tiledata.mul");
        }
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp();
                return;
            }

            args = args.Select(arg => arg.ToLower()).ToArray();
            var tiledataFilename = "tiledata.mul";
            if (args.Length == 2)
                tiledataFilename = args[1];

            switch (args[0])
            {
                case "json":
                case "tojson":
                    {
                        if (!File.Exists(tiledataFilename))
                        {
                            Console.WriteLine($"{tiledataFilename} not found");
                            break;
                        }

                        var tiledataManager = new Tiledata.Tiledata(tiledataFilename);
                        Console.WriteLine($"Loading {tiledataFilename}");
                        var landGroups = tiledataManager.Load<LandTiledata>((progress, maximum) =>
                        {
                            Console.Write($"\rLoading land tiledata {Math.Floor(((float)progress / maximum) * 100)}%");
                            if (progress == maximum)
                                Console.Write("\n");
                        });
                        var staticTiledata = tiledataManager.Load<StaticTiledata>((progress, maximum) =>
                        {
                            Console.Write($"\rLoading static tiledata {Math.Floor(((float)progress / maximum) * 100)}%");
                            if (progress == maximum)
                                Console.Write("\n");
                        });

                        Console.WriteLine("Creating landTiledata.json");
                        Json.SerializeToFile("landTiledata.json", Tiledata.Tiledata.GetDict(landGroups));

                        Console.WriteLine("Creating staticTiledata.json");
                        Json.SerializeToFile("staticTileData.json", Tiledata.Tiledata.GetDict(staticTiledata));
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
                        var landTiledataGroupsDict = (Dictionary<string, TileGroup>)JsonConvert.DeserializeObject(
                            File.ReadAllText("landTiledata.json"), typeof(Dictionary<string, TileGroup>));

                        Console.WriteLine("Loading staticTiledata.json");
                        var staticTiledataGroupsDict = (Dictionary<string, TileGroup>)JsonConvert.DeserializeObject(
                            File.ReadAllText("staticTiledata.json"), typeof(Dictionary<string, TileGroup>));

                        Console.WriteLine($"Creating {tiledataFilename}");
                        Tiledata.Tiledata.Save(tiledataFilename, landTiledataGroupsDict, staticTiledataGroupsDict);
                        break;
                    }
                default:
                    PrintHelp();
                    break;
            }
        }


    }
}
