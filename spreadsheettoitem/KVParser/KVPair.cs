using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spreadsheettoitem.KVParser
{
    public class KVPair
    {
        public KVPair Parent;

        public string PreKeyComment = ""; //Every value before the key. Will only have a value if it is the first key in the TableValue or is the root.
        public string Key = "";
        public string Value = "";
        public string KeyComment = ""; //every symbol after key before value
        public string ValueComment = ""; //every symbol after value before next key
        public List<KVPair> ChildKVs = new List<KVPair>(); //Represents all kvs inside { }. Will be read only if Value is empty.


        public KVPair() { }
        public KVPair(string Key, string Value, int TabCount)
        {
            this.Key = Key;
            this.Value = Value;
            FormatAsStringValue(TabCount);
        }
        public KVPair(string Key, int TabCount)
        {
            this.Key = Key;
            FormatAsFolderValue(TabCount);
        }
        /* Get all symbols this KV consist of, including comments. Keeps format. */
        public string Print() {
            string rv = PreKeyComment + "\"" + Key + "\"" + KeyComment;
            if (Value == "")
            {
                ChildKVs.ForEach(obj => { rv += obj.Print(); });
                rv += ValueComment;
            } else
            {
                rv += "\"" + Value + "\"" + ValueComment;
            }
            return rv;
        }
        
        void FormatAsStringValue(int TabCount) {
            PreKeyComment = new string('\t', TabCount);
            KeyComment = "\t";
            ValueComment = Environment.NewLine;
        }
        void FormatAsFolderValue(int TabCount) {

            PreKeyComment = Environment.NewLine + new string('\t', TabCount);
            KeyComment = Environment.NewLine + new string('\t', TabCount) + "{" + Environment.NewLine;
            //ValueComment = Environment.NewLine;
            ValueComment = new string('\t', TabCount) + "}" + Environment.NewLine;
        }
    }
}
