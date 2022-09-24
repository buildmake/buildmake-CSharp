﻿using buildmake.Command;
using buildmake.Generator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Transactions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;

namespace buildmake
{
    class Buildmake
    {
        private String sourceDir = null;
        private String buildDir = null;
        private String generator = null;

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

                Generator.Generator outGenerator;
                this.generators.TryGetValue(this.generator, out outGenerator);

                if (outGenerator != null)
                {
                    if (outGenerator is VisualStudioGenerator)
                    {
                        ((VisualStudioGenerator)outGenerator).generate(xDocument, Directory.GetCurrentDirectory() + "/" + this.buildDir);
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
            visualStudioGenerator.SetYearVersion("2017");

            generators.Add("Visual Studio 17 2022", visualStudioGenerator);
            generators.Add("vs2022", visualStudioGenerator);
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
            Console.WriteLine("Visual Studio 17 2022 | vs2022");
            // Console.WriteLine("Visual Studio 2019 | vs2019");
            // Console.WriteLine("Visual Studio 2017 | vs2017");
            Console.WriteLine("");
            Console.WriteLine("");
        }
    }
}
