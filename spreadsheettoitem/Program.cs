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
        public static string DataPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        public static ProgramSettings Settings;
        public static string PathToModify;

        static ProgramData()
        {
            if (File.Exists(DataPath + pathFile))
            {
                Stream stream = File.Open(DataPath + pathFile, FileMode.Open);
                BinaryFormatter formatter = new BinaryFormatter();
                PathToModify = (string)formatter.Deserialize(stream);
                stream.Close();
            }
            else {

                Console.WriteLine("Please type the full path of the Dota custom items file you wish to modify.");
                PathToModify = Console.ReadLine();
            }

            if (File.Exists(DataPath + dataFile))
            {
                Stream stream = File.Open(DataPath + dataFile, FileMode.Open);
                BinaryFormatter formatter = new BinaryFormatter();
                Settings = (ProgramSettings)formatter.Deserialize(stream);
                stream.Close();
            }
            else { Settings = new ProgramSettings(); }
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

            Open(ProgramData.PathToModify);
            //Save data
            Stream stream = File.Open(ProgramData.DataPath + ProgramData.dataFile, FileMode.OpenOrCreate);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, ProgramData.Settings);
            stream.Close();
            stream = File.Open(ProgramData.DataPath + ProgramData.pathFile, FileMode.OpenOrCreate);
            formatter.Serialize(stream, ProgramData.PathToModify);
            stream.Close();
        }

        static void Open(string Path) {
            string RawKV = File.ReadAllText(Path);
            string ErrorMessage;
            KVPair thiskv = KVParser.KVParser.Parse(RawKV, out ErrorMessage);
            if (thiskv == null)
            {
                Console.WriteLine(ErrorMessage);
                Console.ReadKey();
            }
            else
            {
                ItemStats.UpdateSheet(thiskv.ChildKVs[0]);
                File.WriteAllText(@Path, thiskv.Print());
            }
        }
    }   
}