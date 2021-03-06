﻿//
// Copyright 2021 David Roller 
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

namespace OpenFolderExtension.Commands
{
    internal sealed class OpenContainingFolderItemNode
    {
        public const int CommandId = 0x3001;
        private readonly AsyncPackage m_Package;
        public static readonly Guid CommandSet = new Guid("3D94678C-412C-47E9-A4B9-DEF3BE3AAD1E");

        private OpenContainingFolderItemNode(AsyncPackage package)
        {
            this.m_Package = package ?? throw new ArgumentNullException(nameof(package));

            if (ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
            {
                var menuCommandId = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandId);
                commandService.AddCommand(menuItem);
            }
        }

        public static OpenContainingFolderItemNode Instance
        {
            get;
            private set;
        }

        private IServiceProvider ServiceProvider => m_Package;

        public static void Initialize(AsyncPackage package)
        {
            Instance = new OpenContainingFolderItemNode(package);
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var selectedItems = (ServiceProvider.GetService(typeof(SDTE)) as DTE2)?.SelectedItems;
            if (selectedItems == null || selectedItems.Count == 0)
            {
                return;
            }

            FileInfo path = null;
            DirectoryInfo solutionPath = new DirectoryInfo(Path.GetPathRoot(Environment.SystemDirectory));
            try
            {
                var solutionFile = ProjectSettings.GetSolutionPath((ServiceProvider.GetService(typeof(SDTE)) as DTE2)?.Solution);
                solutionPath = solutionFile.Directory;

                foreach (SelectedItem selectedItem in selectedItems)
                {
                    path = ProjectSettings.GetSelectedItemPath(selectedItem);
                }
            }
            catch (FileNotFoundException)
            {
                path = ProjectSettings.LookingForSelectedItem(this.ServiceProvider);
            }

            Explorer.Show(path, solutionPath);
        }
    }
}
