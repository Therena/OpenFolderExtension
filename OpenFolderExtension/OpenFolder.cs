//
// Copyright 2016 David Roller
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE80;
using EnvDTE;
using System.IO;

namespace OpenFolderExtension
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class OpenFolder
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("7f67f2cb-b6b3-478a-8dc4-7dbd77df5c6e");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenFolder"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private OpenFolder(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static OpenFolder Instance
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
            Instance = new OpenFolder(package);
        }

        private void FolderOfProjectItem(DTE2 dte)
        {
            foreach (SelectedItem selectedItem in dte.SelectedItems)
            {
                if (selectedItem.ProjectItem == null)
                {
                    return;
                }

                var projectItem = selectedItem.ProjectItem;
                var fullPathProperty = projectItem.Properties.Item("FullPath");

                if (fullPathProperty == null)
                {
                    return;
                }

                var fullPath = new FileInfo(fullPathProperty.Value.ToString());
                System.Diagnostics.Process.Start("explorer.exe", "\"" + fullPath.Directory.FullName + "\"");
            }
        }

        private void FolderOfProject(DTE2 dte)
        {
            foreach (SelectedItem selectedItem in dte.SelectedItems)
            {
                if (selectedItem.Project == null)
                {
                    return;
                }

                var project = selectedItem.Project;
                var fullPathProperty = project.Properties.Item("FullPath");

                if (fullPathProperty == null)
                {
                    return;
                }

                var fullPath = new FileInfo(fullPathProperty.Value.ToString());
                System.Diagnostics.Process.Start("explorer.exe", "\"" + fullPath.Directory.FullName + "\"");
            }
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
            var dte = this.ServiceProvider.GetService(typeof(SDTE)) as DTE2;
            if (dte.SelectedItems.Count <= 0)
            {
                return;
            }

            foreach (SelectedItem selectedItem in dte.SelectedItems)
            {
                if (selectedItem.Project != null)
                {
                    FolderOfProject(dte);
                    return;
                }

                if (selectedItem.ProjectItem != null)
                {
                    FolderOfProjectItem(dte);
                    return;
                }
            }
        }
    }
}
