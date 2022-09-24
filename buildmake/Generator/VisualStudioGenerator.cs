using System;
using System.Net.Http.Headers;
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

        public void SetShortName(String shortName) {
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
            foreach(String line in this.generateHeader())
            {
                lines.Add(line);
            }
            foreach (String line in this.generateWorkspaceProject(xDocument)) {
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

            this.generateProjectFile(xDocument, buildPath);
        }

        private List<String> generateHeader()
        {
            List<String> header = new List<String>();

            switch (this.GetYearVersion())
            {
                case "2202":
                case "2019":
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
            
            switch(iEnumerable.Cast<XAttribute>().First().Value) {
                case "CSharp":
                    projects.Add("Project(\"{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\") = \"" + projectName + "\", \"" + projectName + "/" + projectName + ".csproj\", \"{" + guid.ToString().ToUpper() + "}\"");

                    projects.Add("EndProject");
                    break;
                default:
                    throw new Exception("Project type not implemented");
            }



 
            projects.Add("Global");

           
            if(xDocument.XPathSelectElements("workspace/project/configuration")  == null)
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
                foreach(XElement xElement in xDocument.XPathSelectElements("workspace/project/configuration"))
                {
                    projects.Add("\t\t" + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/name").Value + "|" + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/platform").Value + " = " + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/name").Value + "|" + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/platform").Value);
                    counter++;  
                }
                projects.Add("\tEndGlobalSection");

                projects.Add("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");

                counter = 1;
                foreach (XElement xElement in xDocument.XPathSelectElements("workspace/project/configuration"))
                {
                    projects.Add("\t\t{" + guid.ToString().ToUpper() + "}."+ xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/name").Value + "|" + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/platform").Value + ".ActiveCfg = "+ xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/name").Value + "|" + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/platform").Value);
                    projects.Add("\t\t{" + guid.ToString().ToUpper() + "}." + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/name").Value + "|" + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/platform").Value + ".Build.0 = " + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/name").Value + "|" + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/platform").Value);
                    counter++;
                }

                projects.Add("\tEndGlobalSection");
            }
            projects.Add("EndGlobal");
            return projects;
        }

        
        private void generateProjectFile(XDocument xDocument, String buildPath)
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
            lines.Add("\t\t<AssemblyName>"+ projectName + "</AssemblyName>");

            lines.Add("\t\t<TargetFrameworkVersion>" + xDocument.XPathSelectElement("workspace /project/dotnet_framework").Value + "</TargetFrameworkVersion>");
            lines.Add("\t</PropertyGroup>");



            int counter = 1;
            foreach (XElement xElement in xDocument.XPathSelectElements("workspace/project/configuration"))
            {

                lines.Add("\t<PropertyGroup Condition=\"'$(Configuration)|$(Platform)' == '" + xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/name").Value + "|"+ xDocument.XPathSelectElement("workspace/project/configuration[" + counter + "]/platform").Value + "' \">");
                lines.Add("\t\t<OutputPath>build\\$(Configuration)\\$(Platform)\\</OutputPath>");
                lines.Add("\t\t<EmitDebugInformation>" + xDocument.XPathSelectElement("workspace/project/configuration["+counter+ "]/options/debug").Value + "</EmitDebugInformation>"); ;
                lines.Add("\t</PropertyGroup>");

                counter++;
            }

            

            XPathNavigator xPathNavigator = xDocument.Root.CreateNavigator();
            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xPathNavigator.NameTable);
            xmlNamespaceManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");

            IEnumerable<Object> iEnumerable = (IEnumerable<Object>)xDocument.XPathEvaluate("workspace/project/@xsi:type", xmlNamespaceManager);

            switch (iEnumerable.Cast<XAttribute>().First().Value)
            {
                case "CSharp":
                    lines.Add("<ItemGroup>\r\n    <Compile Include=\"*.cs\" />\r\n  </ItemGroup>");
                    lines.Add("<Target Name=\"Build\">\r\n    <MakeDir Directories=\"$(OutputPath)\" Condition=\"!Exists('$(OutputPath)')\" />\r\n    <Csc Sources=\"@(Compile)\" Platform=\"x86\" EmitDebugInformation=\"$(EmitDebugInformation)\" OutputAssembly=\"$(OutputPath)$(AssemblyName).exe\" />\r\n  </Target>");
                    break;
                default:
                    throw new Exception("Project type not implemented");

            }



            lines.Add("</Project>");

            if (!Directory.Exists(buildPath + "/" + projectName))
            {
                Directory.CreateDirectory(buildPath + "/" + projectName);
            }

            switch (iEnumerable.Cast<XAttribute>().First().Value)
            {
                case "CSharp":

                    File.WriteAllLines(buildPath + "/" + projectName + "/" + projectName + ".csproj", lines.ToArray());
                    break;
                default:
                    throw new Exception("Project type not implemented");
            }
        }
    }
}
