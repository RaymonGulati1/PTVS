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

using System.Collections.Generic;
using Microsoft.CookiecutterTools.Infrastructure;

namespace CookiecutterTests {
    class MockRedirector : Redirector {
        private List<string> ErrorLines { get; } = new List<string>();
        private List<string> Lines { get; } = new List<string>();

        public override void WriteErrorLine(string line) {
            ErrorLines.Add(line);
        }

        public override void WriteLine(string line) {
            Lines.Add(line);
        }
    }
}
