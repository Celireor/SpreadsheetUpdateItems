using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using spreadsheettoitem.KVParser;

namespace spreadsheettoitem.ExcelParser
{
    enum StatColumns
    {
        Strength = 9,
        Agility = 10,
        Intelligence = 11,
        Damage = 12,
        Atkspd = 13,
        MvSpd = 14,
        Health = 15,
        Mana = 16,
        Armor = 17,
        HPRegen = 18,
        MPRegen = 19,
        MagicResist = 20
    }
    public class ItemStats
    {

        /*
        public float Strength;
        public float Agility;
        public float Intelligence;
        public float Damage;
        public float Atkspd;
        public float MvSpd;
        public float Health;
        public float Mana;
        public float Armor;
        public float HPRegen;
        public float MPRegen;
        public float MagicResist;*/

        public static float[] Values = new float[12] {
            129,
            132,
            122,
            32,
            13,
            45,
            21,
            124,
            23,
            125,
            123,
            54
        };

        public static void UpdateSheet(KVPair sheet)
        {

            KVPair abilitySpecial = sheet.ChildKVs.Find(obj => obj.Key == "AbilitySpecial");
            int abilitySpecialCount = abilitySpecial.ChildKVs.Count();
            //StatPrintMaster.statPrints[0].Print(abilitySpecial, null, 50);

            KVPair modifiersSheet = sheet.ChildKVs.Find(obj => obj.Key == "Modifiers");
            if (modifiersSheet == null) {
                modifiersSheet = new KVPair("Modifiers", 2);
                sheet.ChildKVs.Add(modifiersSheet);
            }

            KVPair statModifierSheet = modifiersSheet.ChildKVs.Find(obj => obj.Key == ProgramData.Settings.StatModifier);
            if (statModifierSheet == null)
            {
                statModifierSheet = new KVPair(ProgramData.Settings.StatModifier, 3);
                statModifierSheet.Key = ProgramData.Settings.StatModifier;
                modifiersSheet.ChildKVs.Add(statModifierSheet);

                statModifierSheet.ChildKVs.Add(new KVPair("Passive", "1", 4));
                statModifierSheet.ChildKVs.Add(new KVPair("IsHidden", "1", 4));
                statModifierSheet.ChildKVs.Add(new KVPair("Attributes", "MODIFIER_ATTRIBUTE_MULTIPLE", 4));
            }
            KVPair PropertiesSheet = statModifierSheet.ChildKVs.Find(obj => obj.Key == "Properties"); ;
            if (PropertiesSheet == null) {
                PropertiesSheet = new KVPair("Properties", 4);
                statModifierSheet.ChildKVs.Add(PropertiesSheet);
            }

            for (int x = 0; x < 12; x++) {
                ProgramData.Settings.StatPrints[x].Print(abilitySpecial, PropertiesSheet, Values[x]);
            }
            //new StatPrint("FIELD_INTEGER", "agility").Print(abilitySpecial, null, 50);
        }
    }

    public static class s {
        public static StatPrint[] StatPrints = new StatPrint[12];
        public static string StatModifier;
    }

    [Serializable()]
    public class StatPrint {
        string var_type;
        string name;
        string modifier_name;
        public StatPrint(string var_type, string namePrompt, string modifier_name)
        {
            this.var_type = var_type;
            Console.WriteLine(namePrompt);
            this.name = Console.ReadLine();
            this.modifier_name = modifier_name;
        }
        public void Print(KVPair abilitySpecial, KVPair Modifiers, float Value) {

            KVPair bonusStatKV = null;
            KVPair thisValue = abilitySpecial.ChildKVs.Find(obj => {
                bonusStatKV = obj.ChildKVs.Find(obj2 => obj2.Key != "var_type");
                return bonusStatKV.Key == name;
            });
            if (thisValue == null)
            {
                if (Value != 0)
                {
                    KVPair newAbilitySpecial = new KVPair((abilitySpecial.ChildKVs.Count).ToString(), 3);
                    abilitySpecial.ChildKVs.Add(newAbilitySpecial);
                    newAbilitySpecial.Parent = abilitySpecial;
                    newAbilitySpecial.ChildKVs.Add(new KVPair("var_type", var_type, 4));
                    newAbilitySpecial.ChildKVs.Add(new KVPair(name, Value.ToString(), 4));
                }
            }
            else
            {
                bonusStatKV.Value = Value.ToString();
            }

            KVPair PropertyKV = Modifiers.ChildKVs.Find(obj => obj.Key == modifier_name);
            if (PropertyKV == null && Value != 0)
            {

                PropertyKV = new KVPair(modifier_name, "%" + name, 4);
                Modifiers.ChildKVs.Add(PropertyKV);
            }
        }
    }
}
