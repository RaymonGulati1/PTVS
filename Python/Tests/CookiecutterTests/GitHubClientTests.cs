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

using System.Threading.Tasks;
using Microsoft.CookiecutterTools.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;

namespace CookiecutterTests {
    [TestClass]
    public class GitHubClientTests {
       [TestMethod]
        public async Task CheckEncoding() {
            // Use a repository with a description in Chinese to check UTF-8 decoding
            var client = new GitHubClient();
            var details = await client.GetRepositoryDetails("chenyinxin", "cookiecutter-bitadmin-core");
            AssertUtil.Contains(details.Description, "BitAdminCore是基于net core的管理应用快速开发框架");
        }
    }
}
