//
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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using OpenFolderExtension;

namespace OpenFolderExtension
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(OpenFolderPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class OpenFolderPackage : AsyncPackage
    {
        public const string PackageGuidString = "9a00504e-156b-4250-81b1-565c17b14a57";

        public OpenFolderPackage()
        {

        }

        protected override System.Threading.Tasks.Task InitializeAsync(
            CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            Commands.OpenContainingFolderItemNode.Initialize(this);
            Commands.OpenContainingFolderProjectNode.Initialize(this);
            Commands.OpenContainingFolderSolutionNode.Initialize(this);
            Commands.OpenOutDirectoryProjectNode.Initialize(this);

            CommandsPowershell.OpenContainingFolderItemNode.Initialize(this);
            CommandsPowershell.OpenContainingFolderProjectNode.Initialize(this);
            CommandsPowershell.OpenContainingFolderSolutionNode.Initialize(this);
            CommandsPowershell.OpenOutDirectoryProjectNode.Initialize(this);

            CommandsCmd.OpenContainingFolderItemNode.Initialize(this);
            CommandsCmd.OpenContainingFolderProjectNode.Initialize(this);
            CommandsCmd.OpenContainingFolderSolutionNode.Initialize(this);
            CommandsCmd.OpenOutDirectoryProjectNode.Initialize(this);

            base.Initialize();

            return System.Threading.Tasks.Task.CompletedTask;
        }
    }
}
