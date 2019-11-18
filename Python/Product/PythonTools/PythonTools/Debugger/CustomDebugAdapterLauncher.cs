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
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Debugger.DebugAdapterHost.Interfaces;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using Microsoft.PythonTools.Infrastructure;
using Newtonsoft.Json.Linq;
using System.Windows;
using System.Threading.Tasks;
using System.Threading;

namespace Microsoft.PythonTools.Debugger {
    [ComVisible(true)]
    [Guid(CustomAdapterLauncherCLSIDNNoBraces)]
    public sealed partial class CustomDebugAdapterLauncher : ICustomProtocolExtension, IAdapterLauncher {
        public const string CustomAdapterLauncherCLSIDNNoBraces = "A5E59A97-43B8-4B65-833A-5300076553E1";
        public const string CustomAdapterLauncherCLSID = "{" + CustomAdapterLauncherCLSIDNNoBraces + "}";
        public const string VSCodeDebugEngineId = "{86432F39-ADFD-4C56-AA8F-AF8FCDC66039}";
        public static Guid VSCodeDebugEngine = new Guid(VSCodeDebugEngineId);

        private IDebugAdapterHostContext _context;
        private IProtocolHostOperations _hostOperations;
        private CustomDebugAdapterLauncher _adapterLauncher;
        private IAdapterLaunchInfo _adapterLaunchInfo;

        public static IDictionary<int, CustomDebugAdapterLauncher> ActiveDebugAdapterLaunchers { get; set; } = new Dictionary<int, CustomDebugAdapterLauncher>();

        public void Initialize(IDebugAdapterHostContext context) {
            _context = context ?? throw new ArgumentException(nameof(context));
            _context.Events.DebuggingEnded += (sender, e) => ActiveDebugAdapterLaunchers.Remove(_adapterLaunchInfo.AttachProcessId);
            _adapterLauncher = this;
        }

        public void RegisterCustomMessages(ICustomMessageRegistry registry, IProtocolHostOperations hostOperations) {
            _hostOperations = hostOperations;
        }

        public ITargetHostProcess LaunchAdapter(IAdapterLaunchInfo launchInfo, ITargetHostInterop targetInterop) {
            // ITargetHostInterop provides a convenience wrapper to start the process
            // return targetInterop.ExecuteCommandAsync(path, "");
            _adapterLaunchInfo = launchInfo;

            ActiveDebugAdapterLaunchers[_adapterLaunchInfo.AttachProcessId] = this; 

            // If you need more control use the DebugAdapterProcess
            if (launchInfo.LaunchType == LaunchType.Attach) {
                return DebugAdapterRemoteProcess.Attach(this, launchInfo.LaunchJson);
            }

            return DebugAdapterProcess.Start(this, launchInfo.LaunchJson);
        }

        public void UpdateLaunchOptions(IAdapterLaunchInfo launchInfo) {
            if (launchInfo.LaunchType == LaunchType.Attach) {
                JObject launchJson = new JObject();
                launchInfo.DebugPort.GetPortName(out string uri);

                launchJson["remote"] = uri;
                AddDebugOptions(launchJson);
                AddRules(launchJson);

                launchInfo.LaunchJson = launchJson.ToString();
            }
        }

        public void SendRequest<TArgs, TResponse>(DebugRequestWithResponse<TArgs, TResponse> request, Action<TArgs, TResponse> completionFunc, Action<TArgs, ProtocolException> errorFunc = null)
            where TArgs : class, new()
            where TResponse : ResponseBody {
            _adapterLauncher._hostOperations.SendRequest(request, completionFunc, errorFunc);
        }

        public void SendRequest<TArgs>(DebugRequest<TArgs> request, Action<TArgs> completionFunc, Action<TArgs, ProtocolException> errorFunc = null)
            where TArgs : class, new() {
            _adapterLauncher._hostOperations.SendRequest(request, completionFunc, errorFunc);
        }

        public TResponse SendRequestSync<TArgs, TResponse>(DebugRequestWithResponse<TArgs, TResponse> request)
            where TArgs : class, new()
            where TResponse : ResponseBody {
            return _adapterLauncher._hostOperations.SendRequestSync(request);
        }

        public void SendRequestSync<TArgs>(DebugRequest<TArgs> request)
            where TArgs : class, new() {
            _adapterLauncher._hostOperations.SendRequestSync(request);
        }

        private void AddDebugOptions(JObject launchJson) {
            var debugService = (IPythonDebugOptionsService)Package.GetGlobalService(typeof(IPythonDebugOptionsService));

            JArray debugOptions = new JArray();
            if (debugService.ShowFunctionReturnValue) {
                debugOptions.Add("ShowReturnValue");
            }
            if (debugService.BreakOnSystemExitZero) {
                debugOptions.Add("BreakOnSystemExitZero");
            }

            if (debugOptions.Count > 0) {
                launchJson["debugOptions"] = debugOptions;
            }
        }

        private void AddRules(JObject launchJson) {
            string ptvsdDirectory = PathUtils.GetParent(typeof(CustomDebugAdapterLauncher).Assembly.Location);

            var rules = new JArray();
            var excludePTVSDirectory = new JObject() {
                ["path"] = Path.Combine(ptvsdDirectory, "**"),
                ["include"] = false,
            };

            rules.Add(excludePTVSDirectory);

            launchJson["rules"] = rules;
        }

        public bool CanUseExperimental() {
            return _adapterLauncher != null;
        }

        public string GetCurrentFrameFilename(int threadId) {
            var stackTraceResponse = _adapterLauncher?._hostOperations.SendRequestSync(new StackTraceRequest(threadId));
            return stackTraceResponse.StackFrames[0].Source.Path;
        }

        public (bool isSuccessful, string resultMessage) EvaluateReplRequest(string expression, int threadId) {
            var stackTraceResponse = _adapterLauncher?._hostOperations.SendRequestSync(new StackTraceRequest(threadId));
            var fid = stackTraceResponse.StackFrames[0].Id;
            
            AutoResetEvent adapterResponseEvent = new AutoResetEvent(false);
            bool isSuccessful = false;
            string requestResponse = "";

            _adapterLauncher?._hostOperations.SendRequest(
                new EvaluateRequest(expression.Replace("\n", "@LINE@")) {
                    FrameId = fid,
                    Context = EvaluateArguments.ContextValue.Repl
                },
                (EvaluateArguments e, EvaluateResponse r) => {
                    isSuccessful = true;
                    requestResponse = r.Result;
                    adapterResponseEvent.Set();
                },
                (EvaluateArguments e, ProtocolException p) => {
                    requestResponse = p.Message;
                    adapterResponseEvent.Set();
                }
            );

            adapterResponseEvent.WaitOne();

            return (isSuccessful, requestResponse);
        }
    }
}