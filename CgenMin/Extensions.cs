using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks; 

namespace CPPParser
{
    public static class Extensions
    {

        //------------------------------strings--------------------------------------------------------

        public static string SetEscapesOnString(this string strToEscapeEscapes)
        {
            List<char> cc = new List<char>();
            //create regex pattern by putting escapes
            string pat = "";
            foreach (char c in strToEscapeEscapes)
            {
                pat += c.ToString();
                if (c.ToString() == "\\")
                {
                    pat += "\\";
                }
            }

            return pat;
        }


        public static bool isStartsWithHashtagInclude(this string lineToCheckIfisInclude)
        {

            //if start of line starts with a // then it is not a include
            if (Regex.IsMatch(lineToCheckIfisInclude, @"^\s*//") )
            {
                return false;
            }
            //if line starts with this pattern, it is a inlcude
            if (Regex.IsMatch(lineToCheckIfisInclude, @"^\s*#include"))
            {
                return true;
            }

            return false;
        }

        public static bool IsAnEmptyLine(this string lineToCheck)
        {
            if (Regex.IsMatch(lineToCheck, @"^\s*$"))
            {
                return true;
            }

            return false;
        }

        public static bool IsPragmaOnce(this string lineToCheck)
        {
            //if start of line starts with a // then it is not a include
            if (Regex.IsMatch(lineToCheck, @"^\s*//"))
            {
                return false;
            }
            //if line starts with this pattern, it is a inlcude
            if (Regex.IsMatch(lineToCheck, @"^\s*#pragma once"))
            {
                return true;
            }

            return false;
        }

        public static bool IsComment(this string lineToCheck)
        {
            //if start of line starts with a // then it is not a include
            if (Regex.IsMatch(lineToCheck, @"^\s*//"))
            {
                return true;
            } 

            return false;
        }




        //----------------------------------------libclang stuff --------------------------------------------------------


        //----------------CXTranslationUnit
        //public static List<CXTranslationUnit> GetTranslationUnitsFromAllFiles(this IEnumerable<string> filePaths)
        //{
        //    List<CXTranslationUnit> resultsTU = new List<CXTranslationUnit>();

        //    //CXUnsavedFile cxf = new CXUnsavedFile();
        //    CXIndex index = clang.createIndex(0, 0); 
        //    CXUnsavedFile[] unsavedFile = new CXUnsavedFile[0];
        //    string[] arr = { "-x", "c++" };
             
        //    foreach (var filePath in filePaths)
        //    {
        //        var translationUnit = clang.parseTranslationUnit(index, filePath, arr, 2, new CXUnsavedFile[0], 0, 0);
        //        resultsTU.Add(translationUnit);
        //    }  
              
        //    return resultsTU;
        //}


        ////----------------Cursors 
        //public static CXSourceLocation getCursorLocation(this CXCursor cursor)
        //{
        //    CXSourceLocation sloc = clang.getCursorLocation(cursor);
        //    uint line;
        //    uint column;
        //    uint offset;
        //    CXFile cxfile;
        //    clang.getFileLocation(sloc, out cxfile, out line, out column, out offset);
        //    //----get file location
        //    string fileasStr = clang.getFileName(cxfile).ToString();

        //    return sloc;
        //}

    }
}
