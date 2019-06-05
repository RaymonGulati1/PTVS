// Python Tools for Visual Studio
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

using System;
using Microsoft.PythonTools.Infrastructure;

namespace Microsoft.PythonTools.Options {
    /// <summary>
    /// Stores options related to the language server
    /// </summary>
    public sealed class LanguageServerOptions {
        private readonly PythonToolsService _pyService;
        private const string Category = "LanguageServer";

        internal LanguageServerOptions(PythonToolsService pyService) {
            _pyService = pyService;
            Load();
        }

        public string TypeShedPath { get; set; }
        public bool SuppressTypeShed { get; set; }

        public void Load() {
            TypeShedPath = _pyService.LoadString(nameof(TypeShedPath), Category);
            SuppressTypeShed = _pyService.LoadBool(nameof(SuppressTypeShed), Category) ?? false;
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public void Save() {
            _pyService.SaveString(nameof(TypeShedPath), Category, TypeShedPath);
            _pyService.SaveBool(nameof(SuppressTypeShed), Category, SuppressTypeShed);
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public void Reset() {
            TypeShedPath = string.Empty;
            SuppressTypeShed = false;
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Changed;
    }
}
