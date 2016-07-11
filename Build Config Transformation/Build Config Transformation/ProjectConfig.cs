using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BuildConfigTransformation
{
    public class ProjectConfig
    {
        private string _projectFile;
        private XElement _projectRoot;
        private const string _namespace = "http://schemas.microsoft.com/developer/msbuild/2003";

        public ProjectConfig(string projectFile)
        {
            this._projectFile = projectFile;
            this._projectRoot = XElement.Load(projectFile);
        }

        public void Save()
        {
            this._projectRoot.Save(this._projectFile);
        }

        private XElement NewXElement(string name, params object[] content)
        {
            return new XElement((XNamespace)_namespace + name, content);
        }

        private bool TaskExist(string sourceConfig, string target)
        {
            return true;
        }

        public void AddAfterBuildTarget(string sourceConfig)
        {
            if (this.TaskExist(sourceConfig, "AfterBuild"))
                return;
            XElement transformXml = this.NewXElement("TransformXml", new XAttribute("Source", str5), new XAttribute("Destination", str6), new XAttribute("Transform", str4));
            XElement target = this.NewXElement("Target", new XAttribute("Name", "AfterBuild"), new XAttribute("Condition", conditionConfig), new XComment("Generate transformed config in the output directory"), transformXml);
            this._projectRoot.Add(target);
        }
    }
}
