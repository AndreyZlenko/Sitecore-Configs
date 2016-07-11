using EnvDTE;
using Microsoft.Web.XmlTransform;
using System.IO;
using System.Xml.Linq;

namespace BuildConfigTransformation.Services
{
    public static class TransformService
    {
        public static bool IsProjectConfig(ProjectItem source)
        {
            return (source.Name.EndsWith(".config") && (!(source.Collection.Parent is ProjectItem) || !(source.Collection.Parent as ProjectItem).Name.EndsWith(".config")));
        }

        public static bool HaveTransformConfigs(ProjectItem source)
        {
            return (source.ProjectItems.Count != 0 && source.Name.EndsWith(".config"));
        }

        public static bool IsTransformConfig(ProjectItem source)
        {
            return (source.Name.EndsWith(".config") && (source.Collection.Parent is ProjectItem) && (source.Collection.Parent as ProjectItem).Name.EndsWith(".config"));
        }

        public static void CreateTransformConfigFile(string sourceFile, string destFile)
        {
            XDocument sourceConf = XDocument.Load(sourceFile);

            sourceConf.Root.RemoveNodes();
            sourceConf.Root.SetAttributeValue(XNamespace.Xmlns + "xdt", "http://schemas.microsoft.com/XML-Document-Transform");
            sourceConf.Root.Add(new XComment("Enter your transformation"));

            using (FileStream dest = File.Create(destFile))
            {
                sourceConf.Save(dest);
            }
        }

        public static string CreateTransformResultTempFile(string sourceFile, string transformFile)
        {
            string tmpFilePath = FileService.CreateTempFile(fileExtension: "config");

            XmlTransformableDocument transformableDocument = new XmlTransformableDocument();

            transformableDocument.PreserveWhitespace = true;
            transformableDocument.Load(sourceFile);

            new XmlTransformation(transformFile).Apply(transformableDocument);
            transformableDocument.Save(tmpFilePath);

            return tmpFilePath;
        }
    }
}
