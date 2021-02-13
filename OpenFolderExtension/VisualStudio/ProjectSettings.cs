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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace OpenFolderExtension
{
    internal static class ProjectSettings
    {
        private static Dictionary<string, object> GetAllProperty(Properties properties)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var propertyList = new Dictionary<string, object>();

            if (properties == null)
            {
                return propertyList;
            }

            foreach (Property item in properties)
            {
                try
                {
                    if (item == null)
                    {
                        continue;
                    }

                    propertyList.Add(item.Name, item.Value);
                }
                catch (COMException) { }
                catch (TargetParameterCountException) { }
            }

            return propertyList;
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

        private static Properties GetActiveConfigurationProperties(Project proj)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (proj.ConfigurationManager.ActiveConfiguration.Properties != null)
            {
                return proj.ConfigurationManager.ActiveConfiguration.Properties;
            }

            var propertyList = GetAllProperty(proj.Properties);
            if (propertyList.ContainsKey("ActiveConfiguration") == false)
            {
                throw new NullReferenceException("Unable to find the active configuration");
            }

            return proj.Properties.Item("ActiveConfiguration").Value as Properties;
        }

        public static FileInfo GetOutputFileName(Project proj)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var prop = GetActiveConfigurationProperties(proj);

            var propertyList = GetAllProperty(prop);

            string propertyName;
            if (propertyList.ContainsKey("PrimaryOutput"))
            {
                propertyName = "PrimaryOutput";
            }
            else if (propertyList.ContainsKey("CodeAnalysisInputAssembly"))
            {
                propertyName = "CodeAnalysisInputAssembly";
            }
            else
            {
                throw new FileNotFoundException("Unable to find the project output path");
            }

            var filePath = propertyList[propertyName].ToString();
            try
            {
                if (Path.IsPathRooted(filePath))
                {
                    return new FileInfo(filePath);
                }

                var projectPath = GetProjectPath(proj);
                return new FileInfo(Path.Combine(projectPath.Directory.FullName, filePath ?? string.Empty));
            }
            catch (IOException) { }

            return new FileInfo(filePath);
        }

        public static FileInfo GetProjectItemPath(ProjectItem item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var propertyList = GetAllProperty(item.Properties);

            if (propertyList.ContainsKey("FullPath") == false)
            {
                throw new FileNotFoundException("Unable to find the project item full path");
            }
            return new FileInfo(propertyList["FullPath"].ToString());
        }

        public static FileInfo GetProjectPath(Project proj)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var propertyList = GetAllProperty(proj.Properties);

            // C# Project has the FullPath property
            if (propertyList.ContainsKey("FullPath"))
            {
                return new FileInfo(propertyList["FullPath"].ToString());
            }

            // C++ Project has the ProjectFile property
            if (propertyList.ContainsKey("ProjectFile"))
            {
                return new FileInfo(propertyList["ProjectFile"].ToString());
            }

            throw new FileNotFoundException("Unable to find the project path");
        }
    }
}
