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
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.IO;

namespace OpenFolderExtension
{
    internal static class ProjectSettings
    {
        private static bool HasProperty(Properties properties, string propertyName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (properties != null)
            {
                foreach (Property item in properties)
                {
                    if (item != null && item.Name == propertyName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static FileInfo GetSolutionPath(Solution sln)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var slnFile = new FileInfo(sln.FullName);
            if (slnFile.Directory == null || slnFile.Directory.Exists == false)
            {
                throw new FileNotFoundException("Unable to find the path for the selected item");
            }

            return new FileInfo(slnFile.Directory.FullName);
        }

        public static FileInfo GetSelectedItemPath(SelectedItem selectedItem)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (selectedItem.Project != null)
            {
                return GetProjectPath(selectedItem.Project);
            }

            if (selectedItem.ProjectItem != null)
            {
                return GetProjectItemPath(selectedItem.ProjectItem);
            }

            throw new FileNotFoundException("Unable to find the path for the selected item");
        }

        public static FileInfo GetTargetFile(Project proj)
        {
            try
            {
                return GetPrimaryOutput(proj);
            }
            catch (NullReferenceException) { }

            try
            {
                return GetOutputPath(proj);
            }
            catch(NullReferenceException) { }

            throw new FileNotFoundException("Unable to find the target file path");
        }

        private static Properties GetActiveConfigurationProperties(Project proj)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (proj.ConfigurationManager.ActiveConfiguration.Properties != null)
            {
                return proj.Properties.Item("ActiveConfiguration").Value as Properties;
            }

            if (HasProperty(proj.Properties, "ActiveConfiguration") == false)
            {
                throw new NullReferenceException("Unable to find the active configuration");
            }

            return proj.Properties.Item("ActiveConfiguration").Value as Properties;
        }

        public static FileInfo GetOutputPath(Project proj)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var prop = GetActiveConfigurationProperties(proj);
            if (HasProperty(prop, "OutputPath") == false)
            {
                throw new FileNotFoundException("Unable to find the project output path");
            }

            var filePath = prop?.Item("OutputPath").Value.ToString();
            try
            {
                if (Path.IsPathRooted(filePath))
                {
                    return new FileInfo(filePath);
                }

                var projectPath = GetProjectPath(proj);
                return new FileInfo(Path.Combine(projectPath.Directory.FullName, filePath ?? string.Empty));
            }
            catch (Exception) { }

            return new FileInfo(filePath);
        }

        public static FileInfo GetPrimaryOutput(Project proj)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var prop = GetActiveConfigurationProperties(proj);
            if (HasProperty(prop, "PrimaryOutput") == false)
            {
                throw new FileNotFoundException("Unable to find the project primary output");
            }

            var filePath = prop?.Item("PrimaryOutput").Value.ToString();
            try
            {
                if (Path.IsPathRooted(filePath))
                {
                    return new FileInfo(filePath);
                }

                var projectPath = GetProjectPath(proj);
                return new FileInfo(Path.Combine(projectPath.Directory.FullName, filePath ?? string.Empty));
            }
            catch (Exception) { }

            return new FileInfo(filePath);
        }

        public static FileInfo GetProjectItemPath(ProjectItem item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (HasProperty(item.Properties, "FullPath") == false)
            {
                throw new FileNotFoundException("Unable to find the project item full path");
            }

            var fullPathProperty = item.Properties.Item("FullPath");
            if (fullPathProperty == null)
            {
                throw new FileNotFoundException("Unable to find the project item full path");
            }

            return new FileInfo(fullPathProperty.Value.ToString());
        }

        public static FileInfo GetProjectPath(Project proj)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // C# Project has the FullPath property
            if (HasProperty(proj.Properties, "FullPath"))
            {
                var filePath = proj.Properties.Item("FullPath").Value.ToString();
                return new FileInfo(filePath);
            }

            // C++ Project has the ProjectFile property
            if (HasProperty(proj.Properties, "ProjectFile"))
            {
                var filePath = proj.Properties.Item("ProjectFile").Value.ToString();
                return new FileInfo(filePath);
            }

            throw new FileNotFoundException("Unable to find the project path");
        }
    }
}
