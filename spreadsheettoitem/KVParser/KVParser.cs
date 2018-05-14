using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace spreadsheettoitem.KVParser
{

    enum CommentState {
        NOT_COMMENT,
        LINE_COMMENT,
        BLOCK_COMMENT,
        SAFETY_KEY
    }

    enum ParserState {
        BEFORE_KEY,
        KEY,
        AFTER_KEY,
        VALUE,
        AFTER_VALUE,
        BEFORE_FIRST_ELEMENT
    }

    public static class KVParser
    {
        const char SafetyKey = '\\';

        public static string SafeSubstring(string original, int pos, int length) {
            if (pos + length < original.Length) {

                return original.Substring(pos, length);
            }return "";
        }

        public static List<KVFile> Compile(string FolderPath, string FileName, out string ErrorMessage)
        {
            Console.WriteLine(FolderPath);
            Console.WriteLine(FileName);
            
            List<KVFile> rv = new List<KVFile>();

            string FullPath = FolderPath + FileName;

            string RawKV = File.ReadAllText(FullPath);

            string DependencyParseError = "";
            KVFile thiskv = Parse(RawKV, out ErrorMessage);
            thiskv.Path = FullPath;

            if (ErrorMessage != "") { return null; }

            rv.Add(thiskv);
            thiskv.dependencies.ForEach(obj => {
                if (FolderPath[FolderPath.Length - 1] != '/' &&
                FolderPath[FolderPath.Length - 1] != '\\') {
                    FolderPath += '\\';
                }

                string NewFolderPath = FolderPath + obj.Value;

                int index = NewFolderPath.LastIndexOf('/');
                int backslashIndex = NewFolderPath.LastIndexOf('\\');
                if (backslashIndex > index) {
                    index = backslashIndex;
                }
                if (index == -1) {
                    index = 0;
                }

                string dependencyError;

                List<KVFile> dependency = Compile(
                    NewFolderPath.Substring(0, index),
                    NewFolderPath.Substring(index),
                    out dependencyError);
                rv.AddRange(dependency);

                if (DependencyParseError == "")
                {
                    DependencyParseError = dependencyError;
                }
                else {
                    rv.AddRange(dependency);
                }
            });
            ErrorMessage = DependencyParseError;
            if (ErrorMessage != "") { return null; }
            return rv;
        }

        public static KVFile Parse(string Raw, out string ErrorMessage) {
            List<KVPair> includePaths = new List<KVPair>();

            KVFile rv = new KVFile();
            KVPair rvBase = null;
            KVPair ParentKV = null;
            KVPair CurrentKV = null;
            int Line = 0;
            ParserState parserState = ParserState.BEFORE_KEY;
            CommentState commentState = CommentState.NOT_COMMENT;

            for (int x = 0; x < Raw.Length; x++) {
                if (commentState == CommentState.NOT_COMMENT) {

                    switch (Raw[x])
                    {
                        case SafetyKey:
                            {
                                commentState = CommentState.SAFETY_KEY;
                            }
                            break;
                        case '/':
                            {
                                if (parserState != ParserState.KEY && parserState != ParserState.VALUE)
                                {
                                    string symbolCheck = SafeSubstring(Raw, x, 2);
                                    switch (symbolCheck)
                                    {
                                        case "//":

                                            commentState = CommentState.LINE_COMMENT;
                                            break;
                                        /*case "/*":
                                            commentState = CommentState.BLOCK_COMMENT;
                                            break;*/
                                    }
                                }
                            }
                            break;
                        case '#':
                            {
                                switch (parserState) {
                                    case ParserState.BEFORE_KEY:
                                    case ParserState.AFTER_VALUE:
                                        CurrentKV = new KVPair();
                                        parserState = ParserState.AFTER_KEY;
                                        includePaths.Add(CurrentKV);
                                        break;
                                }
                            }
                            break;
                        case '"':
                            {
                                switch (parserState) {
                                    case ParserState.BEFORE_KEY:
                                    case ParserState.AFTER_VALUE:
                                    case ParserState.BEFORE_FIRST_ELEMENT:
                                        CurrentKV = new KVPair();
                                        if (ParentKV != null)
                                        {
                                            CurrentKV.Parent = ParentKV;
                                            ParentKV.ChildKVs.Add(CurrentKV);
                                        }
                                        if (rvBase == null)
                                        {
                                            rvBase = CurrentKV;
                                        }
                                        parserState = ParserState.KEY;
                                        break;
                                    default:
                                        parserState++;
                                        break;
                                }
                            }
                            break;
                        case '{':
                            {
                                //parserState = ParserState.BEFORE_KEY;
                                if (parserState != ParserState.AFTER_KEY) {
                                    ErrorMessage = "Attempting to create table when value of key is already assigned. Line " + Line;
                                    return null;
                                }
                                parserState = ParserState.BEFORE_FIRST_ELEMENT;
                                ParentKV = CurrentKV;
                            }
                            break;
                        case '}':
                            {

                                parserState = ParserState.AFTER_VALUE;
                                if (ParentKV == null) {
                                    ErrorMessage = "Item KV Curly Braces are not balanced";
                                    return null;
                                }
                                if (ParentKV.ChildKVs.Count > 0)
                                {
                                    Tabs(ref CurrentKV.ValueComment, ref ParentKV.ValueComment);
                                }
                                else { Tabs(ref ParentKV.KeyComment, ref ParentKV.ValueComment); }

                                CurrentKV = ParentKV;
                                ParentKV = ParentKV.Parent;
                            }
                            break;
                    }
                }

                if (Raw[x] != '"' || commentState != CommentState.NOT_COMMENT) {

                    WriteToKV(rv, CurrentKV, parserState, Raw[x]);
                }
                if (SafeSubstring(Raw, x, Environment.NewLine.Length) == Environment.NewLine) {
                    Line++;
                }
                if ((SafeSubstring(Raw, x, Environment.NewLine.Length) == Environment.NewLine && commentState == CommentState.LINE_COMMENT))
                   // || (SafeSubstring(Raw, x, 2) == "*/" && commentState == CommentState.BLOCK_COMMENT))
                {
                    commentState = CommentState.NOT_COMMENT;
                }

            }
            ErrorMessage = "";
            rv.root = rvBase; 
            rv.dependencies = includePaths;
            return rv;
        }

        //split s1 into s1 and s2. s2 will only contain tabs and spaces.
        static void Tabs(ref string s1, ref string s2) {
            for (int y = s1.Length - 1; y >= 0; y--)
            {
                switch (s1[y])
                {
                    case '\t':
                    case ' ':
                        break;
                    default:
                        {
                            if (y != s1.Length - 1)
                            {
                                s2 = s1.Substring(y + 1);
                                s1 = s1.Substring(0, y + 1);
                            }
                            y = -1;
                        }
                        break;
                }
            }
        }

        static void WriteToKV(KVFile FileToWrite, KVPair PairToWrite, ParserState parserState, char newCharToWrite)
        {
            switch (parserState) {
                case ParserState.BEFORE_KEY:
                    FileToWrite.PreKey += newCharToWrite;
                    break;
                case ParserState.KEY:
                    PairToWrite.Key += newCharToWrite;
                    break;
                case ParserState.BEFORE_FIRST_ELEMENT:
                case ParserState.AFTER_KEY:
                    PairToWrite.KeyComment += newCharToWrite;
                    break;
                case ParserState.VALUE:
                    PairToWrite.Value += newCharToWrite;
                    break;
                case ParserState.AFTER_VALUE:
                    PairToWrite.ValueComment += newCharToWrite;
                    break;
            }
        }
    }
}
