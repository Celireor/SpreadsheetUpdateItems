using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using spreadsheettoitem.KVParser;
using spreadsheettoitem.ExcelParser;


using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace spreadsheettoitem
{
    public static class ProgramData
    {
        public const string dataFile = "/data.xml";
        public const string pathFile = "/defaultPath.xml";
        public const string itemDefinitionsFile = "/itemDefinitions.xml";
        public static string DataPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        public static ProgramSettings Settings;

        public static string PathToModify;

        public static List<Tuple<string, string>> DisplayToInternal_Preconversion = new List<Tuple<string, string>>();
        public static Tuple<string, string>[] DisplayToInternal;

        static ProgramData()
        {
            DisplayToInternal = (Tuple<string, string>[])LoadData(itemDefinitionsFile);
            if (DisplayToInternal == null)
            {
                DisplayToInternal = new Tuple<string, string>[0];
            }
            PathToModify = (string)LoadData(pathFile);
            if (PathToModify == null)
            {
                Console.WriteLine("Please type the full path of the Dota custom items file you wish to modify.");
                PathToModify = Console.ReadLine();
            }
            Settings = (ProgramSettings)LoadData(dataFile);
            if (Settings == null) {
                Settings = new ProgramSettings();
            }
        }

        static object LoadData(string fileName) {
            if (File.Exists(DataPath + fileName))
            {
                Stream stream = File.Open(DataPath + fileName, FileMode.Open);
                BinaryFormatter formatter = new BinaryFormatter();
                object rv = formatter.Deserialize(stream);
                stream.Close();
                return rv;
            }
            return null;
        }
    }

    [Serializable()]
    public class ProgramSettings {

        public StatPrint[] StatPrints = new StatPrint[12];
        public string StatModifier;

        public ProgramSettings()
        {
            Console.WriteLine("As this is your first time using Le Spreadsheet To Dota Item KV, please fill in the following information:");
            Console.WriteLine("Modifier used to store stats:");
            StatModifier = Console.ReadLine();
            StatPrints[0] = new StatPrint("FIELD_INTEGER", "MODIFIER_PROPERTY_STATS_STRENGTH_BONUS Variable:", "MODIFIER_PROPERTY_STATS_STRENGTH_BONUS");
            StatPrints[1] = new StatPrint("FIELD_INTEGER", "MODIFIER_PROPERTY_STATS_AGILITY_BONUS Variable:", "MODIFIER_PROPERTY_STATS_AGILITY_BONUS");
            StatPrints[2] = new StatPrint("FIELD_INTEGER", "MODIFIER_PROPERTY_STATS_INTELLECT_BONUS Variable:", "MODIFIER_PROPERTY_STATS_INTELLECT_BONUS");
            StatPrints[3] = new StatPrint("FIELD_INTEGER", "MODIFIER_PROPERTY_PREATTACK_BONUS_DAMAGE Variable:", "MODIFIER_PROPERTY_PREATTACK_BONUS_DAMAGE");
            StatPrints[4] = new StatPrint("FIELD_INTEGER", "MODIFIER_PROPERTY_ATTACKSPEED_BONUS_CONSTANT Variable:", "MODIFIER_PROPERTY_ATTACKSPEED_BONUS_CONSTANT");
            StatPrints[5] = new StatPrint("FIELD_INTEGER", "MODIFIER_PROPERTY_MOVESPEED_BONUS_CONSTANT Variable:", "MODIFIER_PROPERTY_MOVESPEED_BONUS_CONSTANT");
            StatPrints[6] = new StatPrint("FIELD_INTEGER", "MODIFIER_PROPERTY_HEALTH_BONUS Variable:", "MODIFIER_PROPERTY_HEALTH_BONUS");
            StatPrints[7] = new StatPrint("FIELD_FLOAT", "MODIFIER_PROPERTY_MANA_BONUS Variable:", "MODIFIER_PROPERTY_MANA_BONUS");
            StatPrints[8] = new StatPrint("FIELD_FLOAT", "MODIFIER_PROPERTY_PHYSICAL_ARMOR_BONUS Variable:", "MODIFIER_PROPERTY_PHYSICAL_ARMOR_BONUS");
            StatPrints[9] = new StatPrint("FIELD_FLOAT", "MODIFIER_PROPERTY_HEALTH_REGEN_CONSTANT Variable:", "MODIFIER_PROPERTY_HEALTH_REGEN_CONSTANT");
            StatPrints[10] = new StatPrint("FIELD_FLOAT", "MODIFIER_PROPERTY_MANA_REGEN_CONSTANT Variable:", "MODIFIER_PROPERTY_MANA_REGEN_CONSTANT");
            StatPrints[11] = new StatPrint("FIELD_FLOAT", "MODIFIER_PROPERTY_MAGICAL_RESISTANCE_BONUS Variable:", "MODIFIER_PROPERTY_MAGICAL_RESISTANCE_BONUS");
        }
    }

    class Program
    {

        static void Main(string[] args)
        {
            if (args.Length > 0) {

                Open(ProgramData.PathToModify, args[0]);
                //Save data
                SaveSetting(ProgramData.dataFile, ProgramData.Settings);
                SaveSetting(ProgramData.pathFile, ProgramData.PathToModify);
                SaveSetting(ProgramData.itemDefinitionsFile, ProgramData.DisplayToInternal_Preconversion.ToArray());
            }
        }

        static void SaveSetting(string FileName, object ObjectToSerialize) {


            Stream stream = File.Open(ProgramData.DataPath + FileName, FileMode.OpenOrCreate);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, ObjectToSerialize);
            stream.Close();
        }

        static void Open(string LuaPath, string CsvPath) {
            string RawKV = File.ReadAllText(LuaPath);
            string RawCSV = File.ReadAllText(CsvPath);
            string ErrorMessage;
            KVPair thiskv = KVParser.KVParser.Parse(RawKV, out ErrorMessage);
            if (thiskv == null)
            {
                Console.WriteLine(ErrorMessage);
                Console.ReadKey();
            }
            else
            {
                List<ItemStats> itemStats = CsvParser.Parse(RawCSV);
                int index = 0;
                itemStats.ForEach(obj => {
                    obj.FindSheet(thiskv, index);
                    obj.UpdateSheet();
                    index++;
                });
                File.WriteAllText(LuaPath, thiskv.Print());
            }
        }
    }   
}