using System;
using System.Reflection.Emit;
using System.Reflection.PortableExecutable;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace buildmake.Generator
{
    class VisualStudioGenerator : Generator
    {
        private String longName;
        private String shortName;

        private int intVersion;
        private String yearVersion;

        public void SetLongName(String longName)
        {
            this.longName = longName;
        }

        public void SetShortName(String shortName)
        {
            this.shortName = shortName;
        }

        public void SetIntVersion(int intVersion)
        {
            this.intVersion = intVersion;
        }

        public void SetYearVersion(String yearVersion)
        {
            this.yearVersion = yearVersion;
        }

        public String GetLongName()
        {
            return this.longName;
        }

        public String GetShortName()
        {
            return this.shortName;
        }

        public int GetIntVersion()
        {
            return this.intVersion;
        }

        public String GetYearVersion()
        {
            return this.yearVersion;
        }

        public void generate(XDocument xDocument, String buildPath)
        {
            List<String> lines = new List<String>();
            foreach (String line in this.generateHeader())
            {
                lines.Add(line);
            }
            foreach (String line in this.generateWorkspaceProject(xDocument))
            {
                lines.Add(line);
            }

            XElement workspacetNameXElement = xDocument.XPathSelectElement("workspace/name");
            String workspaceName = null;
            if (workspacetNameXElement == null)
            {
                workspaceName = "workspace";
            }
            else
            {
                workspaceName = workspacetNameXElement.Value;
            }


            File.WriteAllLines(buildPath + "/" + workspaceName + ".sln", lines.ToArray());

            XPathNavigator xPathNavigator = xDocument.Root.CreateNavigator();
            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xPathNavigator.NameTable);
            xmlNamespaceManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            IEnumerable<Object> iEnumerable = (IEnumerable<Object>)xDocument.XPathEvaluate("workspace/project/@xsi:type", xmlNamespaceManager);


            switch (iEnumerable.Cast<XAttribute>().First().Value)
            {
                case "CSharp":
                    this.generateCSharpProjectFile(xDocument, buildPath);
                    break;
                case "Cplusplus":
                    this.generateCPlusPlusProjectFile(xDocument, buildPath);
                    break;
                default:
                    throw new Exception("Project type not implemented");
                    break;
            }
        }

        private List<String> generateHeader()
        {
            List<String> header = new List<String>();

            switch (this.GetYearVersion())
            {
                case "2022":
                case "2019":
                case "2017":
                    header.Add("Microsoft Visual Studio Solution File, Format Version 12.00");
                    break;
                default:
                    throw new Exception("Version is not supported");
            }

            header.Add("# Visual Studio Version " + this.GetIntVersion());
            header.Add("MinimumVisualStudioVersion = " + this.GetIntVersion());
            return header;
        }

        private List<String> generateWorkspaceProject(XDocument xDocument)
        {
            List<String> projects = new List<String>();

            Guid guid = Guid.NewGuid();

            XElement projectNameXElement = xDocument.XPathSelectElement("workspace/project/name");
            String projectName = null;
            if (projectNameXElement == null)
            {
                projectName = "project";
            }
            else
            {
                projectName = projectNameXElement.Value;
            }

            XPathNavigator xPathNavigator = xDocument.Root.CreateNavigator();
            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xPathNavigator.NameTable);
            xmlNamespaceManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");


            IEnumerable<Object> iEnumerable = (IEnumerable<Object>)xDocument.XPathEvaluate("workspace/project/@xsi:type", xmlNamespaceManager);

            switch (iEnumerable.Cast<XAttribute>().First().Value)
            {
                case "CSharp":
                    projects.Add("Project(\"{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\") = \"" + projectName + "\", \"" + projectName + "/" + projectName + ".csproj\", \"{" + guid.ToString().ToUpper() + "}\"");

                    projects.Add("EndProject");
                    break;
                case "Cplusplus":
                    projects.Add("Project(\"{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}\") = \"" + projectName + "\", \"" + projectName + "/" + projectName + ".vcxproj\", \"{" + guid.ToString().ToUpper() + "}\"");

                    projects.Add("EndProject");
                    break;
                default:
                    throw new Exception("Project type not implemented");
            }

            projects.Add("Global");


            if (xDocument.XPathSelectElements("workspace/project/configuration") == null)
            {
                projects.Add("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
                projects.Add("\t\tDebug|Any CPU = Debug|Any CPU");
                projects.Add("\t\tRelease|Any CPU = Release|Any CPU");
                projects.Add("\tEndGlobalSection");

                projects.Add("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");

                projects.Add("\t\t{" + guid.ToString().ToUpper() + "}.Debug|Any CPU.ActiveCfg = Debug|Any CP");
                projects.Add("\t\t{" + guid.ToString().ToUpper() + "}.Debug |Any CPU.Build.0 = Debug|Any CP");

                projects.Add("\t\t{" + guid.ToString().ToUpper() + "}.Release|Any CPU.ActiveCfg = Debug|Any CP");
                projects.Add("\t\t{" + guid.ToString().ToUpper() + "}.Release|Any CPU.Build.0 = Debug|Any CP");

                projects.Add("EndGlobalSection");
            }
            else
            {
                projects.Add("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");

                int counter = 1;
                foreach (XElement xElement in xDocument.XPathSelectElements("workspace/project/configuration"))
                {
                    projects.Add("\t\t" + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/name").Value + "|" + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/platform").Value + " = " + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/name").Value + "|" + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/platform").Value);
                    counter++;
                }
                projects.Add("\tEndGlobalSection");

                projects.Add("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");

                counter = 1;
                foreach (XElement xElement in xDocument.XPathSelectElements("workspace/project/configuration"))
                {
                    projects.Add("\t\t{" + guid.ToString().ToUpper() + "}." + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/name").Value + "|" + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/platform").Value + ".ActiveCfg = " + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/name").Value + "|" + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/platform").Value);
                    projects.Add("\t\t{" + guid.ToString().ToUpper() + "}." + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/name").Value + "|" + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/platform").Value + ".Build.0 = " + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/name").Value + "|" + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/platform").Value);
                    counter++;
                }

                projects.Add("\tEndGlobalSection");
            }
            projects.Add("EndGlobal");
            return projects;
        }


        private void generateCSharpProjectFile(XDocument xDocument, String buildPath)
        {
            List<String> lines = new List<String>();

            XElement projectNameXElement = xDocument.XPathSelectElement("workspace/project/name");
            String projectName = null;
            if (projectNameXElement == null)
            {
                projectName = "project";
            }
            else
            {
                projectName = projectNameXElement.Value;
            }

            lines.Add("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            lines.Add("<Project xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\" ToolsVersion=\"Current\">");

            lines.Add("\t<PropertyGroup>");
            lines.Add("\t\t<AssemblyName>" + projectName + "</AssemblyName>");

            lines.Add("\t\t<TargetFrameworkVersion>" + xDocument.XPathSelectElement("workspace /project/dotnet_framework").Value + "</TargetFrameworkVersion>");
            lines.Add("\t</PropertyGroup>");

            int counter = 1;
            foreach (XElement xElement in xDocument.XPathSelectElements("workspace/project/configuration"))
            {

                lines.Add("\t<PropertyGroup Condition=\"'$(Configuration)|$(Platform)' == '" + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/name").Value + "|" + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/platform").Value + "' \">");
                lines.Add("\t\t<OutputPath>build\\$(Configuration)\\$(Platform)\\</OutputPath>");
                lines.Add("\t\t<EmitDebugInformation>" + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/options/debug").Value + "</EmitDebugInformation>"); ;
                lines.Add("\t\t<Optimize>" + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/options/optimization").Value + "</Optimize>");

                lines.Add("\t</PropertyGroup>");

                counter++;
            }

            lines.Add("\t<Import Project=\"$(MSBuildToolsPath)\\Microsoft.CSharp.targets\" />");

            lines.Add("</Project>");

            if(!Directory.Exists(buildPath + "/" + projectName))
            {
                Directory.CreateDirectory(buildPath + "/" + projectName);
            }
            File.WriteAllLines(buildPath + "/" + projectName + "/" + projectName + ".csproj", lines.ToArray());
        }


        private void generateCPlusPlusProjectFile(XDocument xDocument, String buildPath)
        {
            List<String> lines = new List<String>();

            XElement projectNameXElement = xDocument.XPathSelectElement("workspace/project/name");
            String projectName = null;
            if (projectNameXElement == null)
            {
                projectName = "project";
            }
            else
            {
                projectName = projectNameXElement.Value;
            }


            lines.Add("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            lines.Add("<Project DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\" ToolsVersion=\"Current\">");

            lines.Add("\t<ItemGroup  Label=\"ProjectConfigurations\">");


            int counter = 1;
            foreach (XElement xElement in xDocument.XPathSelectElements("workspace/project/configuration"))
            {
                lines.Add("\t\t<ProjectConfiguration Include=\"" + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/name").Value + "|" + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/platform").Value + "\">");
                lines.Add("\t\t\t<Configuration>" + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/name").Value + "</Configuration>");
                lines.Add("\t\t\t<Platform>" + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/platform").Value + "</Platform>");
                lines.Add("\t\t</ProjectConfiguration>");
                counter++;
            }

            lines.Add("\t</ItemGroup>");

            lines.Add("\t<Import Project=\"$(VCTargetsPath)\\Microsoft.Cpp.default.props\" />");

            counter = 1;
            foreach (XElement xElement in xDocument.XPathSelectElements("workspace/project/configuration"))
            {
                lines.Add("\t<PropertyGroup Label=\"Configuration\" Condition=\"'$(Configuration)|$(Platform)'=='" + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/name").Value + "|" + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/platform").Value + "'\">");


                lines.Add("\t\t<WholeProgramOptimization>" + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/options/optimization").Value + "</WholeProgramOptimization>");
               
                
                switch (this.GetYearVersion())
                {
                    case "2022":
                        lines.Add("\t\t<PlatformToolset>v143</PlatformToolset>");
                        break;
                    case "2019":
                        lines.Add("\t\t<PlatformToolset>v142</PlatformToolset>");
                        break;
                    case "2017":
                        lines.Add("\t\t<PlatformToolset>v141</PlatformToolset>");
                        break;
                    default:
                        throw new Exception("Version is not supported");
                }


                lines.Add("\t</PropertyGroup>");

                counter++;
            }

            lines.Add("\t<Import Project=\"$(VCTargetsPath)\\Microsoft.Cpp.props\" />");

            counter = 1;
            foreach (XElement xElement in xDocument.XPathSelectElements("workspace/project/configuration"))
            {
                lines.Add("\t<ItemDefinitionGroup Condition=\"'$(Configuration)|$(Platform)'=='"+ xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/name").Value + "|" + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/platform").Value + "'\">");

                 lines.Add("\t\t<Link>");
                 lines.Add("\t\t\t<GenerateDebugInformation>" + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/options/debug").Value + "</GenerateDebugInformation>"); 
            

                 lines.Add("\t\t</Link>");
                lines.Add("\t</ItemDefinitionGroup>");

                counter++;


                
            }


            lines.Add("\t<ItemGroup>");
            lines.Add("\t\t<ClCompile Include=\"*.cpp\" />");
            lines.Add("\t</ItemGroup>");


            lines.Add("\t<Import Project=\"$(VCTargetsPath)\\Microsoft.Cpp.Targets\" />");
            lines.Add("</Project>");

            if (!Directory.Exists(buildPath + "/" + projectName))
            {
                Directory.CreateDirectory(buildPath + "/" + projectName);
            }
            File.WriteAllLines(buildPath + "/" + projectName + "/" + projectName + ".vcxproj", lines.ToArray());
        }
    }
}