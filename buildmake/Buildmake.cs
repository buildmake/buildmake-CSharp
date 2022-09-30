using buildmake.Command;
using buildmake.Generator;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;

namespace buildmake
{
    class Buildmake
    {
        private String sourceDir;
        private String buildDir;
        private String generator;

        private static Boolean xmlError = false;

        private Dictionary<String, Generator.Generator> generators = new Dictionary<String, Generator.Generator>();

        public Buildmake()
        {
            this.initGenerators();

            List<Command.Command> commands = new Command.Command().GetCommands();
            
            foreach(Command.Command command in commands)
            {
                if(command is ShowHelpCommand)
                {
                    this.ShowHelp();
                    return;
                }

                if(command is SourceCommand)
                {
                    this.sourceDir = ((SourceCommand)command).GetDir();
                }

                if (command is BuildCommand)
                {
                    this.buildDir = ((BuildCommand)command).GetDir();
                }

                if (command is GeneratorCommand)
                {
                    this.generator = ((GeneratorCommand)command).GetGenerator();
                }
            }

            if(this.sourceDir == null || this.buildDir == null || this.generator == null)
            {
                this.ShowHelp();
                return;
            }
            
            XmlSchemaSet xmlSchemaSet = new XmlSchemaSet();
            xmlSchemaSet.XmlResolver = new System.Xml.XmlUrlResolver();

            xmlSchemaSet.Add("", "." + "/validation/buildmake_validate.xsd");

            XmlReader XmlReader = XmlReader.Create("." + "/buildmake.xml");
            XDocument xDocument = XDocument.Load(XmlReader);
            xDocument.Validate(xmlSchemaSet, ValidationEventHandler);

            if(Buildmake.xmlError != true) {
                if(!Directory.Exists(Directory.GetCurrentDirectory() + "/" + this.buildDir))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/" + this.buildDir);
                }

                Generator.Generator outGenerator = new Generator.Generator();
                this.generators.TryGetValue(this.generator, out outGenerator);

                if (outGenerator != null)
                {
                    if (outGenerator is VisualStudioGenerator)
                    {
                        ((VisualStudioGenerator)outGenerator).generate(xDocument, Directory.GetCurrentDirectory() + "/" + this.buildDir);
                    }

                    if(outGenerator is MSBuildGenerator)
                    {
                        ((MSBuildGenerator)outGenerator).generate(xDocument, Directory.GetCurrentDirectory() + "/" + this.buildDir);
                    }
                }
            }

            static void ValidationEventHandler(object sender, ValidationEventArgs e)
            {
                Console.WriteLine(e.Message);
                Buildmake.xmlError = true;
            }

        }

        private void initGenerators()
        {
            VisualStudioGenerator visualStudioGenerator = new VisualStudioGenerator();
            visualStudioGenerator.SetLongName("Visual Studio 17 2022");
            visualStudioGenerator.SetShortName("vs2022");
            visualStudioGenerator.SetIntVersion(17);
            visualStudioGenerator.SetYearVersion("2022");

            generators.Add(visualStudioGenerator.GetLongName(), visualStudioGenerator);
            generators.Add(visualStudioGenerator.GetShortName(), visualStudioGenerator);



            visualStudioGenerator = new VisualStudioGenerator();
            visualStudioGenerator.SetLongName("Visual Studio 16 2019");
            visualStudioGenerator.SetShortName("vs2019");
            visualStudioGenerator.SetIntVersion(16);
            visualStudioGenerator.SetYearVersion("2019");

            generators.Add(visualStudioGenerator.GetLongName(), visualStudioGenerator);
            generators.Add(visualStudioGenerator.GetShortName(), visualStudioGenerator);



            visualStudioGenerator = new VisualStudioGenerator();
            visualStudioGenerator.SetLongName("Visual Studio 15 2017");
            visualStudioGenerator.SetShortName("vs2017");
            visualStudioGenerator.SetIntVersion(15);
            visualStudioGenerator.SetYearVersion("2017");

            generators.Add(visualStudioGenerator.GetLongName(), visualStudioGenerator);
            generators.Add(visualStudioGenerator.GetShortName(), visualStudioGenerator);


            visualStudioGenerator = new VisualStudioGenerator();
            visualStudioGenerator.SetLongName("Visual Studio 14 2015");
            visualStudioGenerator.SetShortName("vs2015");
            visualStudioGenerator.SetIntVersion(14);
            visualStudioGenerator.SetYearVersion("2015");

            generators.Add(visualStudioGenerator.GetLongName(), visualStudioGenerator);
            generators.Add(visualStudioGenerator.GetShortName(), visualStudioGenerator);



            MSBuildGenerator msBuildGenerator = new MSBuildGenerator();
            msBuildGenerator.SetLongName("MSBuild 17");
            msBuildGenerator.SetIntVersion(17);
            msBuildGenerator.SetShortName("msb17");

            generators.Add(msBuildGenerator.GetLongName(), msBuildGenerator);
            generators.Add(msBuildGenerator.GetShortName(), msBuildGenerator);
        }

        private void ShowHelp()
        {
            Console.WriteLine("buildmake.exe [options] [generators]");

            Console.WriteLine("");
            Console.WriteLine("options (where --source|-S, --build|-B options are required)");
            Console.WriteLine("");
            Console.WriteLine("--help|-H           This help message");
            Console.WriteLine("--source|-S         The source directory");
            Console.WriteLine("--build|-B          The build directory");
            Console.WriteLine("");
            Console.WriteLine("generators");
            Console.WriteLine("");
            Console.WriteLine("Visual Studio");
            Console.WriteLine("");
            Console.WriteLine("Visual Studio 17 2022 | vs2022");
            Console.WriteLine("Visual Studio 16 2019 | vs2019");
            Console.WriteLine("Visual Studio 15 2017 | vs2017");
            Console.WriteLine("Visual Studio 14 2015 | vs2015");

            Console.WriteLine("");
            Console.WriteLine("");

            Console.WriteLine("MSBuild");
            Console.WriteLine("");

            Console.WriteLine("MSBuild 17 | msb17");
            Console.WriteLine("");
        }
    }
}
