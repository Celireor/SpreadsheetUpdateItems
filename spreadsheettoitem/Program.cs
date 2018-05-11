using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using spreadsheettoitem.KVParser;
using spreadsheettoitem.ExcelParser;

namespace spreadsheettoitem
{
    

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0) {
                Open(args[0]);
            }
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