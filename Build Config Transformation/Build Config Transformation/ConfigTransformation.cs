//------------------------------------------------------------------------------
// <copyright file="ConfigTransformation.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using BuildConfigTransformation.Services;
using EnvDTE;
using System.Diagnostics;
using System.IO;

namespace BuildConfigTransformation
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ConfigTransformation
    {
        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        private ProjectItem _selectedProjectItem;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigTransformation"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private ConfigTransformation(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                OleMenuCommand olePreviewTransformMenu = new OleMenuCommand(this.PreviewMenuItemCallback, null, this.PreviewBeforeQueryStatus, CommandList.PrewiewTransform);
                commandService.AddCommand(olePreviewTransformMenu);

                OleMenuCommand oleAddTransformMenu = new OleMenuCommand(this.AddMenuItemCallback, null, this.AddBeforeQueryStatus, CommandList.AddTransform);
                commandService.AddCommand(oleAddTransformMenu);

                OleMenuCommand oleUpdateTransformMenu = new OleMenuCommand(this.UpdateMenuItemCallback, null, this.UpdateOrRemoveBeforeQueryStatus, CommandList.UpdateTransform);
                commandService.AddCommand(oleUpdateTransformMenu);

                OleMenuCommand oleRemoveTransformMenu = new OleMenuCommand(this.RemoveMenuItemCallback, null, this.UpdateOrRemoveBeforeQueryStatus, CommandList.RemoveTransform);
                commandService.AddCommand(oleRemoveTransformMenu);
            }
        }

        #region Preview

        private void PreviewMenuItemCallback(object sender, EventArgs eventArgs)
        {
            try
            {
                ProjectItem source = (ProjectItem)this._selectedProjectItem.Collection.Parent;
                if (source == null)
                {
                    UiService.ShowMessageBox("Build Config Transformation", "Cannot find source config", OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGICON.OLEMSGICON_INFO);
                }
                else
                {
                    UiService.ShowTransformDifference(source.FullPath(), this._selectedProjectItem.FullPath());
                }
            }
            catch (Exception ex)
            {
                UiService.ShowMessageBox("Build Config Transformation", ex.Message, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGICON.OLEMSGICON_INFO);
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Exception in PreviewMenuItemCallback() of: {0}. Exception message: {1}", new object[2] { this, ex.Message }));
            }
        }

        private void PreviewBeforeQueryStatus(object sender, EventArgs eventArgs)
        {
            try
            {
                OleMenuCommand oleMenuCommand = sender as OleMenuCommand;

                if (oleMenuCommand != null)
                {
                    oleMenuCommand.Visible = false;
                    oleMenuCommand.Enabled = false;

                    this._selectedProjectItem = VSItemsService.GetCurrentProjectItem();

                    if (TransformService.IsTransformConfig(this._selectedProjectItem))
                    {
                        oleMenuCommand.Visible = true;
                        oleMenuCommand.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Exception in PreviewBeforeQueryStatus() of: {0}. Exception message: {1}", new object[] { this, ex.Message }));
            }
        }

        #endregion

        #region Add

        private void AddMenuItemCallback(object sender, EventArgs eventArgs)
        {
            try
            {
                Project project = VSItemsService.GetCurrentProject();
                if (project == null)
                {
                    UiService.ShowMessageBox("Build Config Transformation", "Cannot find progect", OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGICON.OLEMSGICON_INFO);
                    return;
                }

                if (this._selectedProjectItem == null)
                {
                    UiService.ShowMessageBox("Build Config Transformation", "Cannot find source config", OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGICON.OLEMSGICON_INFO);
                    return;
                }

                string selectedFile = this._selectedProjectItem.FullPath();
                string directoryName = Path.GetDirectoryName(selectedFile);
                string fileName = Path.GetFileNameWithoutExtension(selectedFile);
                string fileExtension = Path.GetExtension(selectedFile);

                foreach (Configuration conf in project.ConfigurationManager)
                {
                    string destFile = Path.Combine(directoryName, string.Format("{0}.{1}{2}", fileName, conf.ConfigurationName, fileExtension));
                    TransformService.CreateTransformConfigFile(selectedFile, destFile);
                    this._selectedProjectItem.ProjectItems.AddFromFile(destFile);
                }

                project.Save();
            }
            catch (Exception ex)
            {
                UiService.ShowMessageBox("Build Config Transformation", ex.Message, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGICON.OLEMSGICON_INFO);
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Exception in AddMenuItemCallback() of: {0}. Exception message: {1}", new object[2] { this, ex.Message }));
            }
        }

        private void AddBeforeQueryStatus(object sender, EventArgs eventArgs)
        {
            try
            {
                OleMenuCommand oleMenuCommand = sender as OleMenuCommand;

                if (oleMenuCommand != null)
                {
                    oleMenuCommand.Visible = false;
                    oleMenuCommand.Enabled = false;

                    this._selectedProjectItem = VSItemsService.GetCurrentProjectItem();

                    if (TransformService.IsProjectConfig(this._selectedProjectItem) && !TransformService.HaveTransformConfigs(this._selectedProjectItem))
                    {
                        oleMenuCommand.Visible = true;
                        oleMenuCommand.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Exception in AddBeforeQueryStatus() of: {0}. Exception message: {1}", new object[] { this, ex.Message }));
            }
        }

        #endregion

        #region Update and Remove

        private void RemoveMenuItemCallback(object sender, EventArgs eventArgs)
        {
            try
            {

                Project project = VSItemsService.GetCurrentProject();
                if (project == null)
                {
                    UiService.ShowMessageBox("Build Config Transformation", "Cannot find progect", OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGICON.OLEMSGICON_INFO);
                    return;
                }

                if (this._selectedProjectItem == null)
                {
                    UiService.ShowMessageBox("Build Config Transformation", "Cannot find source config", OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGICON.OLEMSGICON_INFO);
                    return;
                }

                if (UiService.ShowMessageBox("Build Config Transformation", "Are you sure? That you want to remove all transformation for file: " + this._selectedProjectItem.Name,
                    OLEMSGBUTTON.OLEMSGBUTTON_OKCANCEL, OLEMSGICON.OLEMSGICON_WARNING) == UiService.IDOK)
                {
                    foreach (ProjectItem projectItem in this._selectedProjectItem.ProjectItems)
                    {
                        File.Delete(projectItem.FullPath());
                        projectItem.Delete();
                    }

                    project.Save();
                }
            }
            catch (Exception ex)
            {
                UiService.ShowMessageBox("Build Config Transformation", ex.Message, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGICON.OLEMSGICON_INFO);
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Exception in UpdateMenuItemCallback() of: {0}. Exception message: {1}", new object[2] { this, ex.Message }));
            }
        }

        private void UpdateMenuItemCallback(object sender, EventArgs eventArgs)
        {
            try
            {
                Project project = VSItemsService.GetCurrentProject();
                if (project == null)
                {
                    UiService.ShowMessageBox("Build Config Transformation", "Cannot find progect", OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGICON.OLEMSGICON_INFO);
                    return;
                }

                if (this._selectedProjectItem == null)
                {
                    UiService.ShowMessageBox("Build Config Transformation", "Cannot find source config", OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGICON.OLEMSGICON_INFO);
                    return;
                }

                string selectedFile = this._selectedProjectItem.FullPath();
                string directoryName = Path.GetDirectoryName(selectedFile);
                string fileName = Path.GetFileNameWithoutExtension(selectedFile);
                string fileExtension = Path.GetExtension(selectedFile);

                foreach (Configuration conf in project.ConfigurationManager)
                {
                    string destFile = Path.Combine(directoryName, string.Format("{0}.{1}{2}", fileName, conf.ConfigurationName, fileExtension));

                    if (!File.Exists(destFile))
                    {
                        TransformService.CreateTransformConfigFile(selectedFile, destFile);
                        this._selectedProjectItem.ProjectItems.AddFromFile(destFile);
                    }
                }

                project.Save();
            }
            catch (Exception ex)
            {
                UiService.ShowMessageBox("Build Config Transformation", ex.Message, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGICON.OLEMSGICON_INFO);
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Exception in UpdateMenuItemCallback() of: {0}. Exception message: {1}", new object[2] { this, ex.Message }));
            }
        }

        private void UpdateOrRemoveBeforeQueryStatus(object sender, EventArgs eventArgs)
        {
            try
            {
                OleMenuCommand oleMenuCommand = sender as OleMenuCommand;

                if (oleMenuCommand != null)
                {
                    oleMenuCommand.Visible = false;
                    oleMenuCommand.Enabled = false;

                    this._selectedProjectItem = VSItemsService.GetCurrentProjectItem();

                    if (TransformService.IsProjectConfig(this._selectedProjectItem) && TransformService.HaveTransformConfigs(this._selectedProjectItem))
                    {
                        oleMenuCommand.Visible = true;
                        oleMenuCommand.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Exception in UpdateBeforeQueryStatus() of: {0}. Exception message: {1}", new object[] { this, ex.Message }));
            }
        }

        #endregion

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ConfigTransformation Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new ConfigTransformation(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            UiService.ShowMessageBox("Build Config Transformation", "unknowen menu item");
        }
    }
}
