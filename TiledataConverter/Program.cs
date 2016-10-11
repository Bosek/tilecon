using Newtonsoft.Json;
using System;
using System.IO;

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
            }
            else
            {
                switch (args[0])
                {
                    case "json":
                    case "tojson":
                        if (!File.Exists("tiledata.mul"))
                        {
                            Console.WriteLine("tiledata.mul not found");
                        }
                        else
                        {
                            var tiledataManager = new TiledataManager("tiledata.mul");
                            Console.WriteLine("Loading land data");
                            var landTiledata = tiledataManager.LoadLandTiledata();
                            Console.WriteLine("Land data loaded");

                            Console.WriteLine("Loading static data");
                            var staticTiledata = tiledataManager.LoadStaticTiledata();
                            Console.WriteLine("Static data loaded");

                            Console.WriteLine("Creating landTiledata.json");
                            File.WriteAllText("landTiledata.json", JsonConvert.SerializeObject(landTiledata, Formatting.Indented));

                            Console.WriteLine("Creating staticTiledata.json");
                            File.WriteAllText("staticTiledata.json", JsonConvert.SerializeObject(staticTiledata, Formatting.Indented));
                        }
                        break;
                    case "mul":
                    case "tomul":
                        var landTiledataExists = File.Exists("landTiledata.json");
                        var staticTiledataExists = File.Exists("staticTiledata.json");
                        if (!landTiledataExists || !staticTiledataExists)
                        {
                            if (!landTiledataExists)
                            {
                                Console.WriteLine("landTiledata.json not found");
                            }
                            if (!staticTiledataExists)
                            {
                                Console.WriteLine("staticTiledata.json not found");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Loading land data");
                            var landTiledata = (LandTiledata[])JsonConvert.DeserializeObject(
                                File.ReadAllText("landTiledata.json"), typeof(LandTiledata[]));
                            Console.WriteLine("Land data loaded");

                            Console.WriteLine("Loading static data");
                            var staticTiledata = (StaticTiledata[])JsonConvert.DeserializeObject(
                                File.ReadAllText("staticTiledata.json"), typeof(StaticTiledata[]));
                            Console.WriteLine("Static data loaded");

                            Console.WriteLine("Creating tiledata.mul");
                            TiledataManager.SaveTileData("tiledata.mul", landTiledata, staticTiledata);
                        }
                        break;
                    default:
                        PrintHelp();
                        break;
                }
            }
        }


    }
}
