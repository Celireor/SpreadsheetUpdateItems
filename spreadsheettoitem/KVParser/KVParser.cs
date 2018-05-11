using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static KVPair Parse(string Raw, out string ErrorMessage) {
            KVPair rv = null;
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
                        case '"':
                            {
                                if (parserState == ParserState.BEFORE_KEY) {
                                    CurrentKV = new KVPair();
                                    if (ParentKV != null)
                                    {
                                        CurrentKV.Parent = ParentKV;
                                        ParentKV.ChildKVs.Add(CurrentKV);
                                    }
                                    if (rv == null) {
                                        rv = CurrentKV;
                                    }
                                }
                                if (parserState == ParserState.AFTER_VALUE || parserState == ParserState.BEFORE_FIRST_ELEMENT)
                                {
                                    parserState = ParserState.KEY;
                                    CurrentKV = new KVPair();
                                    if (ParentKV != null)
                                    {
                                        CurrentKV.Parent = ParentKV;
                                        ParentKV.ChildKVs.Add(CurrentKV);
                                    }
                                }
                                else { parserState++; }
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

                                //parserState = ParserState.BEFORE_KEY;
                                if (ParentKV == null) {
                                    ErrorMessage = "Item KV Curly Braces are not balanced";
                                    return null;
                                }
                                for (int y = CurrentKV.ValueComment.Length - 1; y >= 0; y--) {
                                    switch (CurrentKV.ValueComment[y])
                                    {
                                        case '\t':
                                        case ' ':
                                            break;
                                        default:
                                            {
                                                if (y != CurrentKV.ValueComment.Length - 1) {
                                                    ParentKV.ValueComment = CurrentKV.ValueComment.Substring(y - 1);
                                                    CurrentKV.ValueComment = CurrentKV.ValueComment.Substring(0, y + 1);
                                                }
                                                y = -1;
                                            }
                                            break;
                                    }
                                }
                                //ParentKV.ValueComment = CurrentKV.ValueComment;
                               // CurrentKV.ValueComment = "";
                                CurrentKV = ParentKV;
                                ParentKV = ParentKV.Parent;
                            }
                            break;
                    }
                }

                if (Raw[x] != '"' || commentState != CommentState.NOT_COMMENT) {

                    WriteToKV(CurrentKV, parserState, Raw[x]);
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
            return rv;
        }

        static void WriteToKV(KVPair PairToWrite, ParserState parserState, char newCharToWrite) {
            switch (parserState) {
                case ParserState.BEFORE_KEY:
                    PairToWrite.PreKeyComment += newCharToWrite;
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
