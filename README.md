# RoslynFaqLinkMaker
This small exe makes "FAQ(n)" symbol that is in FAQ.md to link to FAQ.cs.

Usage: 
```cmd
-c, --CSharpFaqFileRawUrl    Required. Raw content URL with SHA-1 for FAQ.cs.
                             e.g.
                             https://raw.githubusercontent.com/dotnet/roslyn/8856ab99946b9c6b587835c2b9d34daf06ca808c/src/Samples/CSharp/APISampleUnitTests/FAQ.cs

-m, --MarkdownFaqFilePath    Required. Markdown file path for FAQ.md.
                             e.g.
                             C:\Users\foo\roslyn.wiki\FAQ.md

-o, --OutputPath             Required. Output file path for link generated FAQ.md.
                             e.g.
                             C:\Users\foo\roslyn.wiki\FAQ.md

--help                       Display this help screen.

```

Sample result is [here](https://github.com/urasandesu/RoslynFaqLinkMaker/wiki/FAQ).
