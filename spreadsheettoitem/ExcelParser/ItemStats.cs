using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using spreadsheettoitem.KVParser;

namespace spreadsheettoitem.ExcelParser
{
    /*enum StatColumns
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
    }*/
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
        
        public string Name;
        KVPair sheet;
        public float[] Values = new float[12] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        public KVPair FindSheet(KVPair RootSheet, int Index) {
            //Try finding based on this DisplayToInternal index (fastest if it works)
            if (ProgramData.DisplayToInternal.Length > Index && SearchSheet(RootSheet, Index) == true) { return null; }
            //Try finding based on the entire index lookup
            for (int x = 0; x < ProgramData.DisplayToInternal.Length; x++)
            {
                if (SearchSheet(RootSheet, x) == true) { return null; }
            }
            //Try using the name as item ID
            sheet = RootSheet.ChildKVs.Find(obj => obj.Key == Name.ToLower());
            if (LogSheet()) { return null; };
            //Try adding item_ to the start of the name
            sheet = RootSheet.ChildKVs.Find(obj => obj.Key == ("item_" + Name.ToLower()));
            //If false, give up. Ask the user what the item id is.
            if (LogSheet()) { return null; };
            Console.WriteLine("Cannot find internal item name for \"" + Name + "\". Please key in the internal item name or the internal item name you want the item to have.");
            string internal_name = Console.ReadLine();
            sheet = RootSheet.ChildKVs.Find(obj => obj.Key == (internal_name));
            if (LogSheet()) { return null; };
            Console.WriteLine("Item name is not found. Generating item.");
            KVPair newItem = new KVPair(internal_name, 1);
            sheet = newItem;
            LogSheet();
            return newItem;
        }

        bool SearchSheet(KVPair RootSheet, int Index) {
            
            if (ProgramData.DisplayToInternal[Index].Item1 == Name)
            {
                sheet = RootSheet.ChildKVs.Find(obj => obj.Key == ProgramData.DisplayToInternal[Index].Item2);
                return LogSheet();
            }
            return false;
        }

        bool LogSheet() {

            if (sheet != null)
            {
                Console.WriteLine("Logged internal item name for \"" + Name + "\" as \"" + sheet.Key + "\"");
                ProgramData.DisplayToInternal_Preconversion.Add(new Tuple<string, string>(Name, sheet.Key));
                return true;
            }
            return false;
        }

        public void UpdateSheet()
        {
            KVPair abilitySpecial = sheet.ChildKVs.Find(obj => obj.Key == "AbilitySpecial");
            if (abilitySpecial == null) {
                abilitySpecial = new KVPair("AbilitySpecial", 2);
                sheet.ChildKVs.Add(abilitySpecial);
            }
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

                PropertyKV = new KVPair(modifier_name, "%" + name, 5);
                Modifiers.ChildKVs.Add(PropertyKV);
            }
        }
    }
}
