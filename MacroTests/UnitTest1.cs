using CodeGenerator.cgenXMLSaves.SaveFiles;
using CodeGenerator.FileTemplates;
using CodeGenerator.FileTemplates.GeneralMacoTemplate;
using CodeGenerator.FileTemplatesMacros;
using System.Reflection;

namespace MacroTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }


        public static string DIRECTORYOFTHISCG = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string pathtoTemplateFileAndOutputFiles = DIRECTORYOFTHISCG;// + "\\bin\\debug";



        [Test]
        public void MacroFileTest()
        {
            FileTemplateMainCG maincgTemplate = new FileTemplateMainCG("", "moda1");
            maincgTemplate.CreateTemplate();
        }



        [Test]
        public void GeneralTemplateUserCodesTest()
        {
            //string pathtoTemplateFileAndOutputFiles = @"C:\Users\Hadi\OneDrive\Documents\VisualStudioprojects\Projects\cSharp\CodeGenerator\CodeGenerator\CodeGeneratorTest\bin\Debug";
            string nameOfcGenMacroFile = "testForUserCodes.cgenM";

            GeneralMacro generalMacro = new GeneralMacro(pathtoTemplateFileAndOutputFiles, nameOfcGenMacroFile);
            generalMacro.CreateTemplate();

        }


        [Test]
        public void GeneralTemplateTest()
        {
            //string pathtoTemplateFileAndOutputFiles = @"C:\Users\Hadi\OneDrive\Documents\VisualStudioprojects\Projects\cSharp\CodeGenerator\CodeGenerator\CodeGeneratorTest\bin\Debug";
            string nameOfcGenMacroFile = "testForGeneral.cgenM";

            GeneralMacro generalMacro = new GeneralMacro(pathtoTemplateFileAndOutputFiles, nameOfcGenMacroFile);
            generalMacro.CreateTemplate();

        }



        [Test]
        public void GeneralTemplateTest2()
        {
            //string pathtoTemplateFileAndOutputFiles = @"C:\Users\Hadi\OneDrive\Documents\VisualStudioprojects\Projects\cSharp\CodeGenerator\CodeGenerator\CodeGeneratorTest\bin\Debug";
            string nameOfcGenMacroFile = "AEObjectTest.cgenM";

            GeneralMacro generalMacro = new GeneralMacro(pathtoTemplateFileAndOutputFiles, nameOfcGenMacroFile);
            generalMacro.CreateTemplate();

        }


        [Test]
        public void MacroLoopSectionTest()
        {
            SaveFilecgenProjectGlobal saveFilecgenProjectGlobal = new SaveFilecgenProjectGlobal(DIRECTORYOFTHISCG + @"\CGensaveFiles");
            FileTemplateAllLibraryInlcudes faAllLibraryInlcudes = new FileTemplateAllLibraryInlcudes("", saveFilecgenProjectGlobal);
            FileTemplateCGKeywordDefine fileTemplateCgKeyword = new FileTemplateCGKeywordDefine("", saveFilecgenProjectGlobal);
            faAllLibraryInlcudes.CreateTemplate();
            fileTemplateCgKeyword.CreateTemplate();
        }


    }
}