using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spreadsheettoitem.ExcelParser
{
    static class CsvParser
    {
        public static List<ItemStats> Parse(string Raw) {
            List<ItemStats> rv = new List<ItemStats>();
            ItemStats currentItemStat = null;
            int Line = 0;
            int Column = 0;
            int NewLineCooldown = 0;
            for (int x = 0; x < Raw.Length; x++) {
                if (Raw[x] == Environment.NewLine[0] && x + Environment.NewLine.Length <= Raw.Length)
                {
                    string testNewline = Raw.Substring(x, Environment.NewLine.Length);
                    if (testNewline == Environment.NewLine)
                    {
                        Line++;
                        Column = 0;
                        if (Line > 1) {
                            currentItemStat = new ItemStats();
                            rv.Add(currentItemStat);
                        }
                        NewLineCooldown = Environment.NewLine.Length;
                    }
                }
                else if (Raw[x] == ',')
                {
                    Column++;
                }
                else {
                    if (Line > 1 && NewLineCooldown <= 0)
                    {
                        if (Column >= 9 && Column <= 20)
                        {
                            currentItemStat.Values[Column - 9] *= 10;
                            currentItemStat.Values[Column - 9] += (float)char.GetNumericValue(Raw[x]);
                        }
                        else if (Column == 0)
                        {
                            currentItemStat.Name += Raw[x];
                        }
                    }
                }
                NewLineCooldown--;
            }

            return rv;
        }
    }
}
