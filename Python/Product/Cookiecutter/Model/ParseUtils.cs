﻿// Python Tools for Visual Studio
// Copyright(c) Microsoft Corporation
// All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the License); you may not use
// this file except in compliance with the License. You may obtain a copy of the
// License at http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED ON AN  *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS
// OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY
// IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE,
// MERCHANTABILITY OR NON-INFRINGEMENT.
//
// See the Apache Version 2.0 License for specific language governing
// permissions and limitations under the License.

using System.Text.RegularExpressions;

namespace Microsoft.CookiecutterTools.Model {
    class ParseUtils {
        public static bool ParseGitHubRepoOwnerAndName(string repoUrl, out string owner, out string name) {
            var m = Regex.Match(repoUrl, @"http(s)?://github\.com/(?<owner>.+?)/(?<name>.+?)(/|#|\?|$)");
            owner = m.Groups["owner"].Value;
            name = m.Groups["name"].Value;
            return m.Groups["owner"].Success && m.Groups["name"].Success;
        }
    }
}
