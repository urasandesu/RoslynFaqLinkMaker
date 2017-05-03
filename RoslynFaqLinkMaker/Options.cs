/* 
 * File: Options.cs
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
using CommandLine.Text;
using System;
using System.IO;
using System.Linq;

namespace RoslynFaqLinkMaker
{
    class Options
    {
        [Option('c', "CSharpFaqFileRawUrl", Required = true, HelpText = "Raw content URL with SHA-1 for FAQ.cs. e.g. https://raw.githubusercontent.com/dotnet/roslyn/8856ab99946b9c6b587835c2b9d34daf06ca808c/src/Samples/CSharp/APISampleUnitTests/FAQ.cs")]
        public string CSharpFaqFileRawUrl { get; set; }
        public Uri CSharpFaqFileRawUri { get; private set; }
        public Uri CSharpFaqFileUri { get; private set; }

        [Option('m', "MarkdownFaqFilePath", Required = true, HelpText = "Markdown file path for FAQ.md. e.g. C:\\Users\\foo\\roslyn.wiki\\FAQ.md")]
        public string MarkdownFaqFilePath { get; set; }

        [Option('o', "OutputPath", Required = true, HelpText = "Output file path for link generated FAQ.md. e.g. C:\\Users\\foo\\roslyn.wiki\\FAQ.md")]
        public string OutputPath { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this);
        }

        public bool ParseAdditionalSettings(out string errorMessage)
        {
            errorMessage = default(string);

            {
                var csFaqFileRawUri = default(Uri);
                if (!Uri.TryCreate(CSharpFaqFileRawUrl, UriKind.Absolute, out csFaqFileRawUri))
                {
                    errorMessage = "'CSharpFaqFileRawUrl' must be absolute URI form.";
                    return false;
                }
                CSharpFaqFileRawUri = csFaqFileRawUri;
            }
            {
                var segmentList = CSharpFaqFileRawUri.Segments.ToList();
                segmentList.Insert(3, "blob/");
                CSharpFaqFileUri = new UriBuilder(CSharpFaqFileRawUri.Scheme, "github.com", CSharpFaqFileRawUri.Port, string.Join("", segmentList)).Uri;
            }
            {
                if (!File.Exists(MarkdownFaqFilePath))
                {
                    errorMessage = string.Format("'{0}' is not found.", MarkdownFaqFilePath);
                    return false;
                }
            }

            return true;
        }
    }
}
