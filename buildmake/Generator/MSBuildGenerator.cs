using System.Numerics;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace buildmake.Generator
{
    class MSBuildGenerator : Generator
    {
        private String longName;
        private String shortName;

        private int intVersion;

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

        public void generate(XDocument xDocument, String buildPath)
        {

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
            lines.Add("\t\t<RootNamespace>" + projectName + "</RootNamespace>");


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


            lines.Add("\t<ItemGroup>");
            lines.Add("\t\t<Compile Include =\"*.cs\" />");
            lines.Add("\t</ItemGroup>");

            lines.Add("\t<Import Project=\"$(MSBuildToolsPath)\\Microsoft.CSharp.targets\" />");

            lines.Add("</Project>");

            if (!Directory.Exists(buildPath + "/" + projectName))
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

                switch (this.GetIntVersion())
                {
                    case 17:
                        lines.Add("\t\t<PlatformToolset>v143</PlatformToolset>");
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
                lines.Add("\t<ItemDefinitionGroup Condition=\"'$(Configuration)|$(Platform)'=='" + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/name").Value + "|" + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/platform").Value + "'\">");

                lines.Add("\t\t<ClCompile>");

                XElement xElementSpecification = xDocument.XPathSelectElement("workspace/project/specification");
                if (xElementSpecification != null)
                {
                    switch (xElementSpecification.Value)
                    {
                        case "C++20":
                            lines.Add("\t\t\t<LanguageStandard>stdcpp20</LanguageStandard>");
                            break;
                        case "C++17":
                            lines.Add("\t\t\t<LanguageStandard>stdcpp17</LanguageStandard>");
                            break;
                        case "C++14":
                            lines.Add("\t\t\t<LanguageStandard>stdcp14</LanguageStandard>");
                            break;
                        default:
                            lines.Add("\t\t\t<LanguageStandard>stdcpp14</LanguageStandard>");
                            break;
                    }
                }
                else
                {
                    lines.Add("\t\t\t<LanguageStandard>stdcpp14</LanguageStandard>");
                }


                lines.Add("\t\t</ClCompile>");


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
