/* 
 * File: Program.cs
 * 
 * Author: Akira Sugiura (urasandesu@gmail.com)
 * 
 * 
 * Copyright (c) 2017 Akira Sugiura
 *  
 *  This software is MIT License.
 *  
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *  
 *  The above copyright notice and this permission notice shall be included in
 *  all copies or substantial portions of the Software.
 *  
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 *  THE SOFTWARE.
 */



using CommandLine;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynFaqLinkMaker.Mixins.CommandLine;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace RoslynFaqLinkMaker
{
    class Program
    {
        static int Main(string[] args)
        {
            var options = new Options();
            if (!Parser.Default.ParseArguments(args, options))
                return -1089286372;

            if (!Parser.Default.ParseAdditionalSettings(options))
                return 1369264027;


            Console.WriteLine("Download '{0}'...", options.CSharpFaqFileRawUrl);
            var csFaqSource = new WebClient().DownloadString(options.CSharpFaqFileRawUri);
            var csFaqFileName = Path.GetFileName(options.CSharpFaqFileRawUri.LocalPath);


            Console.WriteLine("Parse '{0}'...", csFaqFileName);
            var projectId = ProjectId.CreateNewId();
            var documentId = DocumentId.CreateNewId(projectId);

            var solution = new AdhocWorkspace().CurrentSolution.
                                                AddProject(projectId, "Project", "Project", LanguageNames.CSharp).
                                                AddMetadataReference(projectId, Mscorlib).
                                                AddDocument(documentId, csFaqFileName, csFaqSource).
                                                WithProjectCompilationOptions(projectId, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var project = solution.GetProject(projectId);
            var document = project.GetDocument(documentId);

            var root = document.GetSyntaxRootAsync().Result;
            var model = document.GetSemanticModelAsync().Result;
            var faqAttrSym = model.GetDeclaredSymbol(root.DescendantNodes().OfType<TypeDeclarationSyntax>().Single(_ => _.Identifier.ValueText == "FAQAttribute"));
            var query = from methodDecl in root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                        let methodSym = model.GetDeclaredSymbol(methodDecl)
                        where methodSym.GetAttributes().Any(_ => _.AttributeClass == faqAttrSym)
                        let appliedFaqAttr = methodSym.GetAttributes().First(_ => _.AttributeClass == faqAttrSym)
                        select new { Id = (int)appliedFaqAttr.ConstructorArguments[0].Value, MethodSymbol = methodSym };
            var faqAppliedMethodSyms = query.ToDictionary(_ => _.Id, _ => _.MethodSymbol);


            Console.WriteLine("Arrange '{0}'...", options.MarkdownFaqFilePath);
            var mdFaqSource = File.ReadAllText(options.MarkdownFaqFilePath);
            mdFaqSource = FaqRegex.Replace(mdFaqSource, m =>
            {
                var idStr = m.Groups["id"].Value;
                var id = int.Parse(idStr);
                var faqAppliedMethodSym = faqAppliedMethodSyms[id];
                var location = faqAppliedMethodSym.Locations.First();
                var lineSpan = location.GetLineSpan();
                return string.Format("[{0}]({1}#L{2})", m.Value, options.CSharpFaqFileUri, lineSpan.StartLinePosition.Line + 1);
            });
            mdFaqSource = UnneededTagRegex.Replace(mdFaqSource, "");


            Console.WriteLine("Output '{0}'...", options.OutputPath);
            File.WriteAllText(options.OutputPath, mdFaqSource);
            return 0;
        }

        static MetadataReference ms_mscorlib;

        static MetadataReference Mscorlib
        {
            get
            {
                if (ms_mscorlib == null)
                    ms_mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
                return ms_mscorlib;
            }
        }

        static Regex ms_faqRegex;
        static Regex FaqRegex
        {
            get
            {
                if (ms_faqRegex == null)
                    ms_faqRegex = new Regex(@"[“""]FAQ\((?<id>\d+)\)[""”]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                return ms_faqRegex;
            }
        }

        static Regex ms_unneededTagRegex;
        static Regex UnneededTagRegex
        {
            get
            {
                if (ms_unneededTagRegex == null)
                    ms_unneededTagRegex = new Regex(@"\(\[installed location information\|faq#codefiles\]\)\s*", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                return ms_unneededTagRegex;
            }
        }
    }
}
