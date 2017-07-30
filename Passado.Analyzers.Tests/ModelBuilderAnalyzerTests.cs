﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Runtime.Loader;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Passado.Tests.Model;

namespace Passado.Analyzers.Tests
{
    public class ModelBuilderAnalyzerTests : ModelBuilderTestsBase
    {
        static readonly MetadataReference[] _references =
        {
            MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ExpressionType).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("mscorlib")).Location),
            MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Runtime")).Location),
            MetadataReference.CreateFromFile(typeof(IQueryBuilder<>).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Reflection")).Location)
        };

        public override async Task<List<(string ErrorId, string ErrorText)>> GetErrors(string source)
        {
            const string fileNamePrefix = "Source";
            const string projectName = "Project";

            var projectId = ProjectId.CreateNewId(debugName: projectName);

            var solution = new AdhocWorkspace()
                .CurrentSolution
                .AddProject(projectId, projectName, projectName, LanguageNames.CSharp)
                .AddMetadataReferences(projectId, _references);

            var newFileName = $"{fileNamePrefix}1.cs";
            var documentId = DocumentId.CreateNewId(projectId, debugName: newFileName);
            solution = solution.AddDocument(documentId, newFileName, SourceText.From(source));

            var project = solution.GetProject(projectId);
            project = project.WithCompilationOptions(((CSharpCompilationOptions)project.CompilationOptions).WithOutputKind(OutputKind.DynamicallyLinkedLibrary)
                                                                                                           .WithWarningLevel(0));
            var compilation = await project.GetCompilationAsync();

            var compilationDiagnostics = compilation.GetDiagnostics();

            var analyzer = new ModelAnalyzer();

            var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer));

            var diagnostics = await compilationWithAnalyzers.GetAllDiagnosticsAsync();

            return diagnostics.Select(d => (d.Id, d.GetMessage())).ToList();
        }
    }
}
