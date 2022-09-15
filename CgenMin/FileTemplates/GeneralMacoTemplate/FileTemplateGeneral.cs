using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CodeGenerator.FileTemplates.Files
{
     

    public class FileTemplateGeneral : FileTemplateBase
    {
        private List<int> LoopIncrement = new List<int>()
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0

        };
        private List<string> generalMacros = new List<string>()
        {
            "", "", "", "", "", "", "", "", "", "",
            "", "", "", "", "", "", "", "", "", "",
            "", "", "", "", "", "", "", "", "", "",
            "", "", "", "", "", "", "", "", "", "",
            "", "", "", "", "", "", "", "", "", "",
            "", "", "", "", "", "", "", "", "", "",
            "", "", "", "", "", "", "", "", "", "",
            "", "", "", "", "", "", "", "", "", "",
            "", "", "", "", "", "", "", "", "", "",
            "", "", "", "", "", "", "", "", "", "",
            "", "", "", "", "", "", "", "", "", "",
            "", "", "", "", "", "", "", "", "", ""

        };


        public FileTemplateGeneral(string templateOutputDestination,string nameOfcGenMacroFile, string nameOfOutputTemplateFile) : base(templateOutputDestination, "General.txt", nameOfOutputTemplateFile)
        {

            string pathTofile = Path.Combine(templateOutputDestination, nameOfcGenMacroFile);
            string contents = File.ReadAllText(pathTofile);

            //remove any comment lines that are ` 
            while (contents.Contains('`'))
            {
                int indexOfComment = contents.IndexOf('`');
                while (contents[indexOfComment] != '\n')
                {
                    contents = contents.Remove(indexOfComment, 1);
                }
            }

            //get all ##LoopIncrement1 values if any
            for (int i = 1; i <= 120; i++)
            {
                string patternForLoopIncrement = @"##LoopIncrement"+i+@"\s+(.*)";
                Match mmm = Regex.Match(contents, patternForLoopIncrement, RegexOptions.Multiline);
                if (mmm.Success)
                {
                    try
                    {
                        int valueOfInc = Convert.ToInt32(mmm.Groups[1].Value);
                        LoopIncrement[i-1] = valueOfInc;
                    }
                    catch (Exception e)
                    { 
                    }  
                }
            }

            //get all ##macrogroup values if any
            for (int i = 1; i <= 120; i++)
            {
                using (StringReader reader = new StringReader(contents))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string patternForLoopIncrement = @"##Macro" + i + @"\s+(.*)$";
                        Match mmm = Regex.Match(line, patternForLoopIncrement, RegexOptions.Multiline);
                        if (mmm.Success)
                        {
                            try
                            {
                                generalMacros[i - 1] = mmm.Groups[1].Value.Trim();
                            }
                            catch (Exception e)
                            {
                            }
                        }

                    }
                }
                /*
                string patternForLoopIncrement = @"##Macro" + i + @"\s+(.*)$";
                Match mmm = Regex.Match(contents, patternForLoopIncrement, RegexOptions.Multiline);
                if (mmm.Success)
                {
                    try
                    {
                        generalMacros[i - 1] = mmm.Groups[1].Value.Trim(); 
                    }
                    catch (Exception e)
                    {
                    }
                }*/
            }

            /* 
##ToFile testForGeneral.h 
 
hi, macro 2 is <#Macro2#>
<#Macro3#>*/

            /*
            //get all contents after the ##ToFile stuff
            string patternForValuesDone = @"##ToFile\s+(.*)";//@"##EndValues##";//@"[.|\s]*(##############################)";
            Match mm = Regex.Match(contents, patternForValuesDone, RegexOptions.Multiline);
            string macroContents = contents.Substring(mm.Index + mm.Length, contents.Length - (mm.Index + mm.Length));
           
            //write that in General.txt  
            string pathToGeneraltxt = Path.Combine(PathTOFileTemplate, NameOfTemplateFile);
            macroContents = "//generated file: " + NameOfOutputTemplateFile + "\n" + macroContents;
            File.WriteAllText(pathToGeneraltxt, macroContents);
            */


            var macroLoopGroup1 = new MacroGroupLoop("1", "i");
            var macroLoopGroup2 = new MacroGroupLoop("2", "i");
            var macroLoopGroup3 = new MacroGroupLoop("3", "i");
            var macroLoopGroup4 = new MacroGroupLoop("4", "i");
            var macroLoopGroup5 = new MacroGroupLoop("5", "i");
            var macroLoopGroup6 = new MacroGroupLoop("6", "i");
            var macroLoopGroup7 = new MacroGroupLoop("7", "i");
            var macroLoopGroup8 = new MacroGroupLoop("8", "i");
            var macroLoopGroup9 = new MacroGroupLoop("9", "i");
            var macroLoopGroup10 = new MacroGroupLoop("10", "i");
            var macroLoopGroup11 = new MacroGroupLoop("11", "i");
            var macroLoopGroup12 = new MacroGroupLoop("12", "i");
            var macroLoopGroup13 = new MacroGroupLoop("13", "i");
            var macroLoopGroup14 = new MacroGroupLoop("14", "i");
            var macroLoopGroup15 = new MacroGroupLoop("15", "i");
            var macroLoopGroup16 = new MacroGroupLoop("16", "i");
            var macroLoopGroup17 = new MacroGroupLoop("17", "i");
            var macroLoopGroup18 = new MacroGroupLoop("18", "i");
            var macroLoopGroup19 = new MacroGroupLoop("19", "i");
            var macroLoopGroup20 = new MacroGroupLoop("20", "i");
            var macroLoopGroup21 = new MacroGroupLoop("21", "i");
            var macroLoopGroup22 = new MacroGroupLoop("22", "i");
            var macroLoopGroup23 = new MacroGroupLoop("23", "i");
            var macroLoopGroup24 = new MacroGroupLoop("24", "i");
            var macroLoopGroup25 = new MacroGroupLoop("25", "i");
            var macroLoopGroup26 = new MacroGroupLoop("26", "i");
            var macroLoopGroup27 = new MacroGroupLoop("27", "i");
            var macroLoopGroup28 = new MacroGroupLoop("28", "i");
            var macroLoopGroup29 = new MacroGroupLoop("29", "i");
            var macroLoopGroup30 = new MacroGroupLoop("30", "i");
            var macroLoopGroup31 = new MacroGroupLoop("31", "i");
            var macroLoopGroup32 = new MacroGroupLoop("32", "i");
            var macroLoopGroup33 = new MacroGroupLoop("33", "i");
            var macroLoopGroup34 = new MacroGroupLoop("34", "i");
            var macroLoopGroup35 = new MacroGroupLoop("35", "i");
            var macroLoopGroup36 = new MacroGroupLoop("36", "i");
            var macroLoopGroup37 = new MacroGroupLoop("37", "i");
            var macroLoopGroup38 = new MacroGroupLoop("38", "i");
            var macroLoopGroup39 = new MacroGroupLoop("39", "i");
            var macroLoopGroup40 = new MacroGroupLoop("40", "i");
            var macroLoopGroup41 = new MacroGroupLoop("41", "i");
            var macroLoopGroup42 = new MacroGroupLoop("42", "i");
            var macroLoopGroup43 = new MacroGroupLoop("43", "i");
            var macroLoopGroup44 = new MacroGroupLoop("44", "i");
            var macroLoopGroup45 = new MacroGroupLoop("45", "i");
            var macroLoopGroup46 = new MacroGroupLoop("46", "i");
            var macroLoopGroup47 = new MacroGroupLoop("47", "i");
            var macroLoopGroup48 = new MacroGroupLoop("48", "i");
            var macroLoopGroup49 = new MacroGroupLoop("49", "i");
            var macroLoopGroup50 = new MacroGroupLoop("50", "i");
            var macroLoopGroup51 = new MacroGroupLoop("51", "i");
            var macroLoopGroup52 = new MacroGroupLoop("52", "i");
            var macroLoopGroup53 = new MacroGroupLoop("53", "i");
            var macroLoopGroup54 = new MacroGroupLoop("54", "i");
            var macroLoopGroup55 = new MacroGroupLoop("55", "i");
            var macroLoopGroup56 = new MacroGroupLoop("56", "i");
            var macroLoopGroup57 = new MacroGroupLoop("57", "i");
            var macroLoopGroup58 = new MacroGroupLoop("58", "i");
            var macroLoopGroup59 = new MacroGroupLoop("59", "i");
            var macroLoopGroup60 = new MacroGroupLoop("60", "i");
            var macroLoopGroup61 = new MacroGroupLoop("61", "i");
            var macroLoopGroup62 = new MacroGroupLoop("62", "i");
            var macroLoopGroup63 = new MacroGroupLoop("63", "i");
            var macroLoopGroup64 = new MacroGroupLoop("64", "i");
            var macroLoopGroup65 = new MacroGroupLoop("65", "i");
            var macroLoopGroup66 = new MacroGroupLoop("66", "i");
            var macroLoopGroup67 = new MacroGroupLoop("67", "i");
            var macroLoopGroup68 = new MacroGroupLoop("68", "i");
            var macroLoopGroup69 = new MacroGroupLoop("69", "i");
            var macroLoopGroup70 = new MacroGroupLoop("70", "i");
            var macroLoopGroup71 = new MacroGroupLoop("71", "i");
            var macroLoopGroup72 = new MacroGroupLoop("72", "i");
            var macroLoopGroup73 = new MacroGroupLoop("73", "i");
            var macroLoopGroup74 = new MacroGroupLoop("74", "i");
            var macroLoopGroup75 = new MacroGroupLoop("75", "i");
            var macroLoopGroup76 = new MacroGroupLoop("76", "i");
            var macroLoopGroup77 = new MacroGroupLoop("77", "i");
            var macroLoopGroup78 = new MacroGroupLoop("78", "i");
            var macroLoopGroup79 = new MacroGroupLoop("79", "i");
            var macroLoopGroup80 = new MacroGroupLoop("80", "i");
            var macroLoopGroup81 = new MacroGroupLoop("81", "i");
            var macroLoopGroup82 = new MacroGroupLoop("82", "i");
            var macroLoopGroup83 = new MacroGroupLoop("83", "i");
            var macroLoopGroup84 = new MacroGroupLoop("84", "i");
            var macroLoopGroup85 = new MacroGroupLoop("85", "i");
            var macroLoopGroup86 = new MacroGroupLoop("86", "i");
            var macroLoopGroup87 = new MacroGroupLoop("87", "i");
            var macroLoopGroup88 = new MacroGroupLoop("88", "i");
            var macroLoopGroup89 = new MacroGroupLoop("89", "i");
            var macroLoopGroup90 = new MacroGroupLoop("90", "i");
            var macroLoopGroup91 = new MacroGroupLoop("91", "i");
            var macroLoopGroup92 = new MacroGroupLoop("92", "i");
            var macroLoopGroup93 = new MacroGroupLoop("93", "i");
            var macroLoopGroup94 = new MacroGroupLoop("94", "i");
            var macroLoopGroup95 = new MacroGroupLoop("95", "i");
            var macroLoopGroup96 = new MacroGroupLoop("96", "i");
            var macroLoopGroup97 = new MacroGroupLoop("97", "i");
            var macroLoopGroup98 = new MacroGroupLoop("98", "i");
            var macroLoopGroup99 = new MacroGroupLoop("99", "i");
            var macroLoopGroup100 = new MacroGroupLoop("100", "i");
            var macroLoopGroup101 = new MacroGroupLoop("101", "i");
            var macroLoopGroup102 = new MacroGroupLoop("102", "i");
            var macroLoopGroup103 = new MacroGroupLoop("103", "i");
            var macroLoopGroup104 = new MacroGroupLoop("104", "i");
            var macroLoopGroup105 = new MacroGroupLoop("105", "i");
            var macroLoopGroup106 = new MacroGroupLoop("106", "i");
            var macroLoopGroup107 = new MacroGroupLoop("107", "i");
            var macroLoopGroup108 = new MacroGroupLoop("108", "i");
            var macroLoopGroup109 = new MacroGroupLoop("109", "i");
            var macroLoopGroup110 = new MacroGroupLoop("110", "i");
            var macroLoopGroup111 = new MacroGroupLoop("111", "i");
            var macroLoopGroup112 = new MacroGroupLoop("112", "i");
            var macroLoopGroup113 = new MacroGroupLoop("113", "i");
            var macroLoopGroup114 = new MacroGroupLoop("114", "i");
            var macroLoopGroup115 = new MacroGroupLoop("115", "i");
            var macroLoopGroup116 = new MacroGroupLoop("116", "i");
            var macroLoopGroup117 = new MacroGroupLoop("117", "i");
            var macroLoopGroup118 = new MacroGroupLoop("118", "i");
            var macroLoopGroup119 = new MacroGroupLoop("119", "i");
            var macroLoopGroup120 = new MacroGroupLoop("120", "i");

            var Macro1 = new Macro("Macro1", generalMacros[0]);
            var Macro2 = new Macro("Macro2", generalMacros[1]);
            var Macro3 = new Macro("Macro3", generalMacros[2]);
            var Macro4 = new Macro("Macro4", generalMacros[3]);
            var Macro5 = new Macro("Macro5", generalMacros[4]);
            var Macro6 = new Macro("Macro6", generalMacros[5]);
            var Macro7 = new Macro("Macro7", generalMacros[6]);
            var Macro8 = new Macro("Macro8", generalMacros[7]);
            var Macro9 = new Macro("Macro9", generalMacros[8]);
            var Macro10 = new Macro("Macro10", generalMacros[9]);
            var Macro11 = new Macro("Macro11", generalMacros[10]);
            var Macro12 = new Macro("Macro12", generalMacros[11]);
            var Macro13 = new Macro("Macro13", generalMacros[12]);
            var Macro14 = new Macro("Macro14", generalMacros[13]);
            var Macro15 = new Macro("Macro15", generalMacros[14]);
            var Macro16 = new Macro("Macro16", generalMacros[15]);
            var Macro17 = new Macro("Macro17", generalMacros[16]);
            var Macro18 = new Macro("Macro18", generalMacros[17]);
            var Macro19 = new Macro("Macro19", generalMacros[18]);
            var Macro20 = new Macro("Macro20", generalMacros[19]);
            var Macro21 = new Macro("Macro21", generalMacros[20]);
            var Macro22 = new Macro("Macro22", generalMacros[21]);
            var Macro23 = new Macro("Macro23", generalMacros[22]);
            var Macro24 = new Macro("Macro24", generalMacros[23]);
            var Macro25 = new Macro("Macro25", generalMacros[24]);
            var Macro26 = new Macro("Macro26", generalMacros[25]);
            var Macro27 = new Macro("Macro27", generalMacros[26]);
            var Macro28 = new Macro("Macro28", generalMacros[27]);
            var Macro29 = new Macro("Macro29", generalMacros[28]);
            var Macro30 = new Macro("Macro30", generalMacros[29]);
            var Macro31 = new Macro("Macro31", generalMacros[30]);
            var Macro32 = new Macro("Macro32", generalMacros[31]);
            var Macro33 = new Macro("Macro33", generalMacros[32]);
            var Macro34 = new Macro("Macro34", generalMacros[33]);
            var Macro35 = new Macro("Macro35", generalMacros[34]);
            var Macro36 = new Macro("Macro36", generalMacros[35]);
            var Macro37 = new Macro("Macro37", generalMacros[36]);
            var Macro38 = new Macro("Macro38", generalMacros[37]);
            var Macro39 = new Macro("Macro39", generalMacros[38]);
            var Macro40 = new Macro("Macro40", generalMacros[39]);
            var Macro41 = new Macro("Macro41", generalMacros[40]);
            var Macro42 = new Macro("Macro42", generalMacros[41]);
            var Macro43 = new Macro("Macro43", generalMacros[42]);
            var Macro44 = new Macro("Macro44", generalMacros[43]);
            var Macro45 = new Macro("Macro45", generalMacros[44]);
            var Macro46 = new Macro("Macro46", generalMacros[45]);
            var Macro47 = new Macro("Macro47", generalMacros[46]);
            var Macro48 = new Macro("Macro48", generalMacros[47]);
            var Macro49 = new Macro("Macro49", generalMacros[48]);
            var Macro50 = new Macro("Macro50", generalMacros[49]);
            var Macro51 = new Macro("Macro51", generalMacros[50]);
            var Macro52 = new Macro("Macro52", generalMacros[51]);
            var Macro53 = new Macro("Macro53", generalMacros[52]);
            var Macro54 = new Macro("Macro54", generalMacros[53]);
            var Macro55 = new Macro("Macro55", generalMacros[54]);
            var Macro56 = new Macro("Macro56", generalMacros[55]);
            var Macro57 = new Macro("Macro57", generalMacros[56]);
            var Macro58 = new Macro("Macro58", generalMacros[57]);
            var Macro59 = new Macro("Macro59", generalMacros[58]);
            var Macro60 = new Macro("Macro60", generalMacros[59]);
            var Macro61 = new Macro("Macro61", generalMacros[60]);
            var Macro62 = new Macro("Macro62", generalMacros[61]);
            var Macro63 = new Macro("Macro63", generalMacros[62]);
            var Macro64 = new Macro("Macro64", generalMacros[63]);
            var Macro65 = new Macro("Macro65", generalMacros[64]);
            var Macro66 = new Macro("Macro66", generalMacros[65]);
            var Macro67 = new Macro("Macro67", generalMacros[66]);
            var Macro68 = new Macro("Macro68", generalMacros[67]);
            var Macro69 = new Macro("Macro69", generalMacros[68]);
            var Macro70 = new Macro("Macro70", generalMacros[69]);
            var Macro71 = new Macro("Macro71", generalMacros[70]);
            var Macro72 = new Macro("Macro72", generalMacros[71]);
            var Macro73 = new Macro("Macro73", generalMacros[72]);
            var Macro74 = new Macro("Macro74", generalMacros[73]);
            var Macro75 = new Macro("Macro75", generalMacros[74]);
            var Macro76 = new Macro("Macro76", generalMacros[75]);
            var Macro77 = new Macro("Macro77", generalMacros[76]);
            var Macro78 = new Macro("Macro78", generalMacros[77]);
            var Macro79 = new Macro("Macro79", generalMacros[78]);
            var Macro80 = new Macro("Macro80", generalMacros[79]);
            var Macro81 = new Macro("Macro81", generalMacros[80]);
            var Macro82 = new Macro("Macro82", generalMacros[81]);
            var Macro83 = new Macro("Macro83", generalMacros[82]);
            var Macro84 = new Macro("Macro84", generalMacros[83]);
            var Macro85 = new Macro("Macro85", generalMacros[84]);
            var Macro86 = new Macro("Macro86", generalMacros[85]);
            var Macro87 = new Macro("Macro87", generalMacros[86]);
            var Macro88 = new Macro("Macro88", generalMacros[87]);
            var Macro89 = new Macro("Macro89", generalMacros[88]);
            var Macro90 = new Macro("Macro90", generalMacros[89]);
            var Macro91 = new Macro("Macro91", generalMacros[90]);
            var Macro92 = new Macro("Macro92", generalMacros[91]);
            var Macro93 = new Macro("Macro93", generalMacros[92]);
            var Macro94 = new Macro("Macro94", generalMacros[93]);
            var Macro95 = new Macro("Macro95", generalMacros[94]);
            var Macro96 = new Macro("Macro96", generalMacros[95]);
            var Macro97 = new Macro("Macro97", generalMacros[96]);
            var Macro98 = new Macro("Macro98", generalMacros[97]);
            var Macro99 = new Macro("Macro99", generalMacros[98]);
            var Macro100 = new Macro("Macro100", generalMacros[99]);
            var Macro101 = new Macro("Macro101", generalMacros[100]);
            var Macro102 = new Macro("Macro102", generalMacros[101]);
            var Macro103 = new Macro("Macro103", generalMacros[102]);
            var Macro104 = new Macro("Macro104", generalMacros[103]);
            var Macro105 = new Macro("Macro105", generalMacros[104]);
            var Macro106 = new Macro("Macro106", generalMacros[105]);
            var Macro107 = new Macro("Macro107", generalMacros[106]);
            var Macro108 = new Macro("Macro108", generalMacros[107]);
            var Macro109 = new Macro("Macro109", generalMacros[108]);
            var Macro110 = new Macro("Macro110", generalMacros[109]);
            var Macro111 = new Macro("Macro111", generalMacros[110]);
            var Macro112 = new Macro("Macro112", generalMacros[111]);
            var Macro113 = new Macro("Macro113", generalMacros[112]);
            var Macro114 = new Macro("Macro114", generalMacros[113]);
            var Macro115 = new Macro("Macro115", generalMacros[114]);
            var Macro116 = new Macro("Macro116", generalMacros[115]);
            var Macro117 = new Macro("Macro117", generalMacros[116]);
            var Macro118 = new Macro("Macro118", generalMacros[117]);
            var Macro119 = new Macro("Macro119", generalMacros[118]);
            var Macro120 = new Macro("Macro120", generalMacros[119]);




            Macros = new List<Macro>();
            Macros.Add(Macro1);
            Macros.Add(Macro2);
            Macros.Add(Macro3);
            Macros.Add(Macro4);
            Macros.Add(Macro5);
            Macros.Add(Macro6);
            Macros.Add(Macro7);
            Macros.Add(Macro8);
            Macros.Add(Macro9);
            Macros.Add(Macro10);
            Macros.Add(Macro11);
            Macros.Add(Macro12);
            Macros.Add(Macro13);
            Macros.Add(Macro14);
            Macros.Add(Macro15);
            Macros.Add(Macro16);
            Macros.Add(Macro17);
            Macros.Add(Macro18);
            Macros.Add(Macro19);
            Macros.Add(Macro20);
            Macros.Add(Macro21);
            Macros.Add(Macro22);
            Macros.Add(Macro23);
            Macros.Add(Macro24);
            Macros.Add(Macro25);
            Macros.Add(Macro26);
            Macros.Add(Macro27);
            Macros.Add(Macro28);
            Macros.Add(Macro29);
            Macros.Add(Macro30);
            Macros.Add(Macro31);
            Macros.Add(Macro32);
            Macros.Add(Macro33);
            Macros.Add(Macro34);
            Macros.Add(Macro35);
            Macros.Add(Macro36);
            Macros.Add(Macro37);
            Macros.Add(Macro38);
            Macros.Add(Macro39);
            Macros.Add(Macro40);
            Macros.Add(Macro41);
            Macros.Add(Macro42);
            Macros.Add(Macro43);
            Macros.Add(Macro44);
            Macros.Add(Macro45);
            Macros.Add(Macro46);
            Macros.Add(Macro47);
            Macros.Add(Macro48);
            Macros.Add(Macro49);
            Macros.Add(Macro50);
            Macros.Add(Macro51);
            Macros.Add(Macro52);
            Macros.Add(Macro53);
            Macros.Add(Macro54);
            Macros.Add(Macro55);
            Macros.Add(Macro56);
            Macros.Add(Macro57);
            Macros.Add(Macro58);
            Macros.Add(Macro59);
            Macros.Add(Macro60); 
            Macros.Add(Macro61);
            Macros.Add(Macro62);
            Macros.Add(Macro63);
            Macros.Add(Macro64);
            Macros.Add(Macro65);
            Macros.Add(Macro66);
            Macros.Add(Macro67);
            Macros.Add(Macro68);
            Macros.Add(Macro69);
            Macros.Add(Macro70);
            Macros.Add(Macro71);
            Macros.Add(Macro72);
            Macros.Add(Macro73);
            Macros.Add(Macro74);
            Macros.Add(Macro75);
            Macros.Add(Macro76);
            Macros.Add(Macro77);
            Macros.Add(Macro78);
            Macros.Add(Macro79);
            Macros.Add(Macro80);
            Macros.Add(Macro81);
            Macros.Add(Macro82);
            Macros.Add(Macro83);
            Macros.Add(Macro84);
            Macros.Add(Macro85);
            Macros.Add(Macro86);
            Macros.Add(Macro87);
            Macros.Add(Macro88);
            Macros.Add(Macro89);
            Macros.Add(Macro90);
            Macros.Add(Macro91);
            Macros.Add(Macro92);
            Macros.Add(Macro93);
            Macros.Add(Macro94);
            Macros.Add(Macro95);
            Macros.Add(Macro96);
            Macros.Add(Macro97);
            Macros.Add(Macro98);
            Macros.Add(Macro99);
            Macros.Add(Macro100);
            Macros.Add(Macro101);
            Macros.Add(Macro102);
            Macros.Add(Macro103);
            Macros.Add(Macro104);
            Macros.Add(Macro105);
            Macros.Add(Macro106);
            Macros.Add(Macro107);
            Macros.Add(Macro108);
            Macros.Add(Macro109);
            Macros.Add(Macro110); 
            Macros.Add(Macro111);
            Macros.Add(Macro112);
            Macros.Add(Macro113);
            Macros.Add(Macro114);
            Macros.Add(Macro115);
            Macros.Add(Macro116);
            Macros.Add(Macro117);
            Macros.Add(Macro118);
            Macros.Add(Macro119);
            Macros.Add(Macro120); 




            MacroLoopGroups.Add(macroLoopGroup1);
            MacroLoopGroups.Add(macroLoopGroup2);
            MacroLoopGroups.Add(macroLoopGroup3);
            MacroLoopGroups.Add(macroLoopGroup4);
            MacroLoopGroups.Add(macroLoopGroup5);
            MacroLoopGroups.Add(macroLoopGroup6);
            MacroLoopGroups.Add(macroLoopGroup7);
            MacroLoopGroups.Add(macroLoopGroup8);
            MacroLoopGroups.Add(macroLoopGroup9);
            MacroLoopGroups.Add(macroLoopGroup10);
            MacroLoopGroups.Add(macroLoopGroup11);
            MacroLoopGroups.Add(macroLoopGroup12);
            MacroLoopGroups.Add(macroLoopGroup13);
            MacroLoopGroups.Add(macroLoopGroup14);
            MacroLoopGroups.Add(macroLoopGroup15);
            MacroLoopGroups.Add(macroLoopGroup16);
            MacroLoopGroups.Add(macroLoopGroup17);
            MacroLoopGroups.Add(macroLoopGroup18);
            MacroLoopGroups.Add(macroLoopGroup19);
            MacroLoopGroups.Add(macroLoopGroup20);
            MacroLoopGroups.Add(macroLoopGroup21);
            MacroLoopGroups.Add(macroLoopGroup22);
            MacroLoopGroups.Add(macroLoopGroup23);
            MacroLoopGroups.Add(macroLoopGroup24);
            MacroLoopGroups.Add(macroLoopGroup25);
            MacroLoopGroups.Add(macroLoopGroup26);
            MacroLoopGroups.Add(macroLoopGroup27);
            MacroLoopGroups.Add(macroLoopGroup28);
            MacroLoopGroups.Add(macroLoopGroup29);
            MacroLoopGroups.Add(macroLoopGroup30);
            MacroLoopGroups.Add(macroLoopGroup31);
            MacroLoopGroups.Add(macroLoopGroup32);
            MacroLoopGroups.Add(macroLoopGroup33);
            MacroLoopGroups.Add(macroLoopGroup34);
            MacroLoopGroups.Add(macroLoopGroup35);
            MacroLoopGroups.Add(macroLoopGroup36);
            MacroLoopGroups.Add(macroLoopGroup37);
            MacroLoopGroups.Add(macroLoopGroup38);
            MacroLoopGroups.Add(macroLoopGroup39);
            MacroLoopGroups.Add(macroLoopGroup40);
            MacroLoopGroups.Add(macroLoopGroup41);
            MacroLoopGroups.Add(macroLoopGroup42);
            MacroLoopGroups.Add(macroLoopGroup43);
            MacroLoopGroups.Add(macroLoopGroup44);
            MacroLoopGroups.Add(macroLoopGroup45);
            MacroLoopGroups.Add(macroLoopGroup46);
            MacroLoopGroups.Add(macroLoopGroup47);
            MacroLoopGroups.Add(macroLoopGroup48);
            MacroLoopGroups.Add(macroLoopGroup49);
            MacroLoopGroups.Add(macroLoopGroup50);
            MacroLoopGroups.Add(macroLoopGroup51);
            MacroLoopGroups.Add(macroLoopGroup52);
            MacroLoopGroups.Add(macroLoopGroup53);
            MacroLoopGroups.Add(macroLoopGroup54);
            MacroLoopGroups.Add(macroLoopGroup55);
            MacroLoopGroups.Add(macroLoopGroup56);
            MacroLoopGroups.Add(macroLoopGroup57);
            MacroLoopGroups.Add(macroLoopGroup58);
            MacroLoopGroups.Add(macroLoopGroup59);
            MacroLoopGroups.Add(macroLoopGroup60);
            MacroLoopGroups.Add(macroLoopGroup61);
            MacroLoopGroups.Add(macroLoopGroup62);
            MacroLoopGroups.Add(macroLoopGroup63);
            MacroLoopGroups.Add(macroLoopGroup64);
            MacroLoopGroups.Add(macroLoopGroup65);
            MacroLoopGroups.Add(macroLoopGroup66);
            MacroLoopGroups.Add(macroLoopGroup67);
            MacroLoopGroups.Add(macroLoopGroup68);
            MacroLoopGroups.Add(macroLoopGroup69);
            MacroLoopGroups.Add(macroLoopGroup70);
            MacroLoopGroups.Add(macroLoopGroup71);
            MacroLoopGroups.Add(macroLoopGroup72);
            MacroLoopGroups.Add(macroLoopGroup73);
            MacroLoopGroups.Add(macroLoopGroup74);
            MacroLoopGroups.Add(macroLoopGroup75);
            MacroLoopGroups.Add(macroLoopGroup76);
            MacroLoopGroups.Add(macroLoopGroup77);
            MacroLoopGroups.Add(macroLoopGroup78);
            MacroLoopGroups.Add(macroLoopGroup79);
            MacroLoopGroups.Add(macroLoopGroup80);
            MacroLoopGroups.Add(macroLoopGroup81);
            MacroLoopGroups.Add(macroLoopGroup82);
            MacroLoopGroups.Add(macroLoopGroup83);
            MacroLoopGroups.Add(macroLoopGroup84);
            MacroLoopGroups.Add(macroLoopGroup85);
            MacroLoopGroups.Add(macroLoopGroup86);
            MacroLoopGroups.Add(macroLoopGroup87);
            MacroLoopGroups.Add(macroLoopGroup88);
            MacroLoopGroups.Add(macroLoopGroup89);
            MacroLoopGroups.Add(macroLoopGroup90);
            MacroLoopGroups.Add(macroLoopGroup91);
            MacroLoopGroups.Add(macroLoopGroup92);
            MacroLoopGroups.Add(macroLoopGroup93);
            MacroLoopGroups.Add(macroLoopGroup94);
            MacroLoopGroups.Add(macroLoopGroup95);
            MacroLoopGroups.Add(macroLoopGroup96);
            MacroLoopGroups.Add(macroLoopGroup97);
            MacroLoopGroups.Add(macroLoopGroup98);
            MacroLoopGroups.Add(macroLoopGroup99);
            MacroLoopGroups.Add(macroLoopGroup100);
            MacroLoopGroups.Add(macroLoopGroup101);
            MacroLoopGroups.Add(macroLoopGroup102);
            MacroLoopGroups.Add(macroLoopGroup103);
            MacroLoopGroups.Add(macroLoopGroup104);
            MacroLoopGroups.Add(macroLoopGroup105);
            MacroLoopGroups.Add(macroLoopGroup106);
            MacroLoopGroups.Add(macroLoopGroup107);
            MacroLoopGroups.Add(macroLoopGroup108);
            MacroLoopGroups.Add(macroLoopGroup109);
            MacroLoopGroups.Add(macroLoopGroup110);
            MacroLoopGroups.Add(macroLoopGroup111);
            MacroLoopGroups.Add(macroLoopGroup112);
            MacroLoopGroups.Add(macroLoopGroup113);
            MacroLoopGroups.Add(macroLoopGroup114);
            MacroLoopGroups.Add(macroLoopGroup115);
            MacroLoopGroups.Add(macroLoopGroup116);
            MacroLoopGroups.Add(macroLoopGroup117);
            MacroLoopGroups.Add(macroLoopGroup118);
            MacroLoopGroups.Add(macroLoopGroup119);
            MacroLoopGroups.Add(macroLoopGroup120);


            for (int j = 0; j < 120; j++)
            {
                for (int i = LoopIncrement[j]; i > 0; i--)
                {
                    MacroLoopGroups[j].AddNewGroup(i.ToString());
                } 
            }

            //now define the macro groups


            /*
                       //first get the information in UserCode1 and put that in the General.txt
                           string pathTofile = Path.Combine(templateOutputDestination, nameOfOutputTemplateFile);
                       string contents = File.ReadAllText(pathTofile);
           
                        
                       string patternForFirstComment = @"##UserCode_main##((.|\n)*?)##END##";
                       Match m = Regex.Match(contents, patternForFirstComment, RegexOptions.Multiline);
                       if (m.Success)
                       {
                           string contentsOfUserCode = m.Groups[1].Value;
                           //write that in General.txt but only AFTER the UserCode_main
                           string pathToGeneraltxt = Path.Combine(PathTOFileTemplate,NameOfTemplateFile);
           
                           string patternForWholeUserCodemain = @"\/\*\n*.*##UserCode_main##.*##END##.*\n*\*\/";
                           Match mm = Regex.Match(contents, patternForWholeUserCodemain, RegexOptions.Multiline);
                           if (mm.Success)
                           {
           
                               File.WriteAllText(pathToGeneraltxt, contentsOfUserCode+ mm.Length);
                           }
                           else
                           {
                               Debug.Assert((this.MacroLoopGroups.Count != 0), "You need a ##UserCode_main## comment in your file.");
                           }
                           
                       }
                       else
                       {
                           Debug.Assert((this.MacroLoopGroups.Count != 0), "You need a ##UserCode_main## comment in your file.");
                       }
                       */


        }
    }
}
