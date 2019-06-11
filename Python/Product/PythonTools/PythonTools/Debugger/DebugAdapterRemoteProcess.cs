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

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.PythonTools.Infrastructure;
using Microsoft.PythonTools.Logging;
using Microsoft.VisualStudio.Debugger.DebugAdapterHost.Interfaces;
using Newtonsoft.Json.Linq;

namespace Microsoft.PythonTools.Debugger {
    sealed class DebugAdapterRemoteProcess : ITargetHostProcess, IDisposable {
        private const int _debuggerConnectionTimeout = 20000; // 20 seconds
        private DebugAdapterProcessStream _stream;
        private bool _debuggerConnected = false;

        private DebugAdapterRemoteProcess() {}

        public static ITargetHostProcess Attach(string attachJson) {
            var process = new DebugAdapterRemoteProcess();
            var attached = process.AttachProcess(attachJson);
            return attached ? process : null;
        }

        private bool AttachProcess(string attachJson) {
            var json = JObject.Parse(attachJson);
            var uri = new Uri(json["remote"].Value<string>());
            return ConnectSocket(uri);
        }

        private bool ConnectSocket(Uri uri) {
            _debuggerConnected = false;
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            EndPoint endpoint;
            if (uri.IsLoopback) {
                endpoint = new IPEndPoint(IPAddress.Loopback, uri.Port);
            } else {
                endpoint = new DnsEndPoint(uri.Host, uri.Port);
            }

            var logger = (IPythonToolsLogger)VisualStudio.Shell.ServiceProvider.GlobalProvider.GetService(typeof(IPythonToolsLogger));

            Debug.WriteLine("Connecting to remote debugger at {0}", uri.ToString());
            Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(() => Task.WhenAny(
                    Task.Factory.FromAsync(socket.BeginConnect, socket.EndConnect, endpoint, null),
                    Task.Delay(_debuggerConnectionTimeout)));
            try {
                if (socket.Connected) {
                    _debuggerConnected = true;
                    _stream = new DebugAdapterProcessStream(new NetworkStream(socket, ownsSocket: true));
                    _stream.Disconnected += OnDisconnected;
                    _stream.Initialized += OnInitialized;
                    _stream.LegacyDebugger += OnLegacyDebugger;
                } else {
                    Debug.WriteLine("Timed out waiting for debugger to connect.", nameof(DebugAdapterRemoteProcess));
                    logger?.LogEvent(PythonLogEvent.DebugAdapterConnectionTimeout, "Attach");
                }
            } catch (AggregateException ex) {
                Debug.WriteLine("Error waiting for debugger to connect {0}".FormatInvariant(ex.InnerException ?? ex), nameof(DebugAdapterRemoteProcess));
            }

            return _debuggerConnected;
        }

        private void OnDisconnected(object sender, EventArgs e) {
            if (_stream != null) {
                _stream.Dispose();
            }
            Exited?.Invoke(this, null);
        }

        private void OnInitialized(object sender, EventArgs e) {
            CustomDebugAdapterProtocolExtension.SendRequest(
                new PtvsdVersionRequest(),
                PtvsdVersionHelper.VerifyPtvsdVersion,
                PtvsdVersionHelper.VerifyPtvsdVersionError);
        }

        private void OnLegacyDebugger(object sender, EventArgs e) {
            PtvsdVersionHelper.VerifyPtvsdVersionLegacy();
        }

        public IntPtr Handle => IntPtr.Zero;

        public Stream StandardInput => _stream;

        public Stream StandardOutput => _stream;

        public bool HasExited => _debuggerConnected;

        public event EventHandler Exited;
        public event DataReceivedEventHandler ErrorDataReceived;

        public void Dispose() {
            if(_stream != null) {
                _stream.Dispose();
            }
        }

        public void Terminate() {
            if (_stream != null) {
                _stream.Dispose();
            }
            ErrorDataReceived?.Invoke(this, null);
        }
    }
}
