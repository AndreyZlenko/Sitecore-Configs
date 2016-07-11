using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;

namespace BuildConfigTransformation.Services
{
    public static class VSItemsService
    {

        public static Project GetCurrentProject()
        {
            DTE dte = (DTE)Package.GetGlobalService(typeof(DTE));

            Array activeSolutionProjects = dte.ActiveSolutionProjects as Array;
            if (activeSolutionProjects != null && activeSolutionProjects.Length == 1)
            {
                return activeSolutionProjects.GetValue(0) as Project;
            }

            return null;
        }

        public static ProjectItem GetCurrentProjectItem()
        {
            DTE dte = (DTE)Package.GetGlobalService(typeof(DTE));

            if (dte != null && dte.SelectedItems.Count == 1)
            {
                foreach (SelectedItem item in dte.SelectedItems)
                {
                    return item.ProjectItem;
                }
            }

            return null;
        }

        public static string FullPath(this ProjectItem source)
        {
            return (source.Document != null) ? source.Document.FullName : source.FileNames[1];
        }

        //public static TResult Value<TResult>(this Properties properties, string propertyName)
        //{
        //    foreach(Property prop in properties)
        //    {
        //        if (prop.Name == propertyName)
        //        {
        //            return (TResult)prop.Value;
        //        }
        //    }

        //    return default(TResult);
        //}
    }
}
