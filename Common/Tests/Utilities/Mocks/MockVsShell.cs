﻿// Visual Studio Shared Project
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
using System.Collections.Generic;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace TestUtilities.Mocks {
    public class MockVsShell : IVsShell {
        public readonly Dictionary<int, object> Properties = new Dictionary<int, object>();
        public readonly object ReadOnlyPropertyValue = new object();
        private readonly List<IVsShellPropertyEvents> _listeners = new List<IVsShellPropertyEvents>();
        
        public int GetProperty(int propid, out object pvar) {
            if (Properties.TryGetValue(propid, out pvar)) {
                Console.WriteLine("MockVsShell.GetProperty(propid={0}) -> {1}", propid, pvar);
                return VSConstants.S_OK;
            }
            Console.WriteLine("MockVsShell.GetProperty(propid={0}) -> <unknown value>", propid);
            return VSConstants.E_INVALIDARG;
        }

        public int SetProperty(int propid, object var) {
            object value;
            if (Properties.TryGetValue(propid, out value)) {
                if (value == ReadOnlyPropertyValue) {
                    Console.WriteLine("MockVsShell.SetProperty(propid={0}, var={1}) -> E_INVALIDARG", propid, var);
                    return VSConstants.E_INVALIDARG;
                }
                Console.WriteLine("MockVsShell.SetProperty(propid={0}, var={1}) replacing {2}", propid, var, value);
            } else {
                Console.WriteLine("MockVsShell.SetProperty(propid={0}, var={1})", propid, var);
            }
            Properties[propid] = var;
            foreach (var l in _listeners) {
                l?.OnShellPropertyChange(propid, var);
            }
            return VSConstants.S_OK;
        }


        public int AdviseBroadcastMessages(IVsBroadcastMessageEvents pSink, out uint pdwCookie) {
            throw new NotImplementedException();
        }

        public int AdviseShellPropertyChanges(IVsShellPropertyEvents pSink, out uint pdwCookie) {
            _listeners.Add(pSink);
            pdwCookie = (uint)_listeners.Count - 1;
            return VSConstants.S_OK;
        }

        public int GetPackageEnum(out IEnumPackages ppenum) {
            throw new NotImplementedException();
        }

        public int IsPackageInstalled(ref Guid guidPackage, out int pfInstalled) {
            throw new NotImplementedException();
        }

        public int IsPackageLoaded(ref Guid guidPackage, out IVsPackage ppPackage) {
            throw new NotImplementedException();
        }

        public int LoadPackage(ref Guid guidPackage, out IVsPackage ppPackage) {
            throw new NotImplementedException();
        }

        public int LoadPackageString(ref Guid guidPackage, uint resid, out string pbstrOut) {
            throw new NotImplementedException();
        }

        public int LoadUILibrary(ref Guid guidPackage, uint dwExFlags, out uint phinstOut) {
            throw new NotImplementedException();
        }

        public int UnadviseBroadcastMessages(uint dwCookie) {
            throw new NotImplementedException();
        }

        public int UnadviseShellPropertyChanges(uint dwCookie) {
            _listeners[(int)dwCookie] = null;
            return VSConstants.S_OK;
        }
    }
}
