﻿//
// Copyright 2020 David Roller 
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

namespace OpenFolderExtension
{
    internal sealed class OpenFolder
    {
        public const int CommandId = 0x0100;
        private readonly AsyncPackage package;
        public static readonly Guid CommandSet = new Guid("7f67f2cb-b6b3-478a-8dc4-7dbd77df5c6e");

        private OpenFolder(AsyncPackage package)
        {
            this.package = package ?? throw new ArgumentNullException("package");

            if (ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        public static OpenFolder Instance
        {
            get;
            private set;
        }

        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        public static void Initialize(AsyncPackage package)
        {
            Instance = new OpenFolder(package);
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if ((ServiceProvider.GetService(typeof(SDTE)) as DTE2).SelectedItems.Count <= 0)
            {
                return;
            }

            var folders = new Folders();
            foreach (SelectedItem selectedItem in (ServiceProvider.GetService(typeof(SDTE)) as DTE2).SelectedItems)
            {
                var path = "";
                if (selectedItem.Project != null)
                {
                    path = folders.GetProjectPath(selectedItem.Project);
                }

                if (selectedItem.ProjectItem != null)
                {
                    path = folders.GetProjectItemPath(selectedItem.ProjectItem);
                }

                if (string.IsNullOrWhiteSpace(path))
                {
                    return;
                }

                System.Diagnostics.Process.Start("explorer.exe", "\"" + path + "\"");
            }
        }
    }
}
