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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Threading;
using TestUtilities.Mocks;
using MefV1 = System.ComponentModel.Composition;
using ComposablePartDefinition = Microsoft.VisualStudio.Composition.ComposablePartDefinition;

namespace TestUtilities {
    public static class MefCatalogFactory {
        private static readonly Resolver StandardResolver = Resolver.DefaultInstance;
        private static readonly PartDiscovery Discovery = PartDiscovery.Combine(new AttributedPartDiscoveryV1(StandardResolver), new AttributedPartDiscovery(StandardResolver, true));

        public static ComposableCatalog AddJoinableTaskContext(this ComposableCatalog composableCatalog)
            => composableCatalog.AddInstance(() => new JoinableTaskContext(TestMainThreadService.Instance.Thread, TestMainThreadService.Instance.SyncContext));

        public static ComposableCatalog WithServiceProvider(this ComposableCatalog composableCatalog) 
            => composableCatalog.AddType<ServiceProvider>();

        public static ComposableCatalog AddInstance<T>(this ComposableCatalog composableCatalog, Func<T> factory) 
            => composableCatalog.AddType(MefFactoryGenerator.GetExportType(factory));

        public static ComposableCatalog AddInstance<T, TResult>(this ComposableCatalog composableCatalog, Func<T, TResult> factory) 
            => composableCatalog.AddType(MefFactoryGenerator.GetExportType(factory));

        public static ComposableCatalog AddInstance<T1, T2, TResult>(this ComposableCatalog composableCatalog, Func<T1, T2, TResult> factory) 
            => composableCatalog.AddType(MefFactoryGenerator.GetExportType(factory));

        public static ComposableCatalog AddType<T>(this ComposableCatalog composableCatalog)
            => composableCatalog.AddType(typeof(T));

        public static ComposableCatalog AddType(this ComposableCatalog composableCatalog, Type type)
            => composableCatalog.AddPart(Discovery.CreatePart(type));

        public static ComposableCatalog AddTypesFromAssembly(this ComposableCatalog composableCatalog, string assemblyName, params string[] typeNames) {
            var assembly = GetLoadedAssembly(assemblyName, AppDomain.CurrentDomain.GetAssemblies());
            foreach (var typeName in typeNames) {
                var type = assembly.GetType(typeName) ?? throw new AssertFailedException($@"Type {typeName} can't be found in assembly {assemblyName}.");
                composableCatalog = composableCatalog.AddType(type);
            }
            return composableCatalog;
        }

        public static ComposableCatalog CreateAssembliesCatalog(params string[] assemblyNames) {
            var appDomainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var loadedAssemblies = new Assembly[assemblyNames.Length];

            for (var i = 0; i < assemblyNames.Length; i++) {
                loadedAssemblies[i] = GetLoadedAssembly(assemblyNames[i], appDomainAssemblies);
            }

            var types = new List<Type>();
            foreach (var assembly in loadedAssemblies) {
                types.AddRange(assembly.GetTypes());
            }

            var parts = new ComposablePartDefinition[types.Count];
            Parallel.For(0, types.Count, i => parts[i] = Discovery.CreatePart(types[i]));
            return ComposableCatalog.Create(StandardResolver).AddParts(parts.Where(p => p != null));
        }

        private static Assembly GetLoadedAssembly(string assemblyName, Assembly[] appDomainAssemblies) {
            var loadedAssembly = appDomainAssemblies.FirstOrDefault(a => string.Equals(a.GetName().Name, assemblyName, StringComparison.OrdinalIgnoreCase));
            return loadedAssembly ?? throw new AssertFailedException($@"Assembly {assemblyName} isn't loaded.
Please use {nameof(AssemblyLoader)}.{nameof(AssemblyLoader.EnsureLoaded)} to preload assemblies.");
        }

        [MefV1.Export(typeof(MockServiceProvider))]
        [MefV1.Export(typeof(SVsServiceProvider))]
        private class ServiceProvider : MockServiceProvider, SVsServiceProvider {
            [MefV1.ImportingConstructor]
            private ServiceProvider([MefV1.Import] ExportProvider exportProvider, [MefV1.Import] MefV1.ICompositionService compositionService)
                : base(new ComponentModel(exportProvider, compositionService)) {}
        }

        private class ComponentModel : IComponentModel, SComponentModel {
            private readonly ExportProvider _exportProvider;

            public MefV1.Primitives.ComposablePartCatalog DefaultCatalog => throw new NotSupportedException();
            public MefV1.Hosting.ExportProvider DefaultExportProvider { get; }
            public MefV1.ICompositionService DefaultCompositionService { get; }

            public ComponentModel(ExportProvider exportProvider, MefV1.ICompositionService compositionService) {
                _exportProvider = exportProvider;
                DefaultExportProvider = exportProvider.AsExportProvider();
                DefaultCompositionService = compositionService;
            }

            public MefV1.Primitives.ComposablePartCatalog GetCatalog(string catalogName) => throw new NotSupportedException();

            public T GetService<T>() where T : class => _exportProvider.GetExportedValue<T>();
            public IEnumerable<T> GetExtensions<T>() where T : class => _exportProvider.GetExportedValues<T>();
        }
    }
}