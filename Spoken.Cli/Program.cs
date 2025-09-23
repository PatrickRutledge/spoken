using Spoken.Core;

// Minimal CLI harness: dotnet run -- "John 1:1-18" KJV "Prologue"
var argsList = args.ToList();
string passage = argsList.ElementAtOrDefault(0) ?? "Genesis 1:1-5";
string translation = argsList.ElementAtOrDefault(1) ?? "KJV";
string title = argsList.ElementAtOrDefault(2) ?? "";

Console.WriteLine($"Formatting '{passage}' ({translation})...");

// Very naive passage parse: assumes "Book chapter:verse-verse"
string book;
int chapterStart = 1, verseStart = 1, chapterEnd = 1, verseEnd = 5;
{
	passage = passage.Trim().Trim('\"', '\'');
	int idx = passage.LastIndexOf(' ');
	if (idx > 0)
	{
		book = passage.Substring(0, idx);
		var cv = passage.Substring(idx + 1);
		var cvParts = cv.Split('-', 2);
		var start = cvParts[0];
		if (start.Contains(':'))
		{
			var s = start.Split(':');
			chapterStart = int.Parse(s[0]);
			verseStart = int.Parse(s[1]);
		}
		if (cvParts.Length > 1)
		{
			var end = cvParts[1];
			if (end.Contains(':'))
			{
				var e = end.Split(':');
				chapterEnd = int.Parse(e[0]);
				verseEnd = int.Parse(e[1]);
			}
			else
			{
				chapterEnd = chapterStart;
				verseEnd = int.Parse(end);
			}
		}
		else
		{
			chapterEnd = chapterStart; verseEnd = verseStart;
		}
	}
	else
	{
		book = passage; chapterStart = 1; verseStart = 1; chapterEnd = 1; verseEnd = 25;
	}
}

var source = new UsfmZipTextSource();
var verses = new List<Verse>();
await foreach (var v in source.GetVersesAsync(translation, book, chapterStart, verseStart, chapterEnd, verseEnd))
{
	verses.Add(v);
}

var htmlBody = ProseFormatter.FormatToParagraphs(verses);
// Inject optional title
if (!string.IsNullOrWhiteSpace(title))
{
	htmlBody = htmlBody.Replace("<div class=\"container\">", $"<div class=\"container\"><h1 class=\"title\">{System.Net.WebUtility.HtmlEncode(title)}</h1>");
}

var outDir = Path.Combine(Directory.GetCurrentDirectory(), "out");
Directory.CreateDirectory(outDir);
var htmlPath = Path.Combine(outDir, "sample.html");
await File.WriteAllTextAsync(htmlPath, htmlBody);
Console.WriteLine($"Wrote HTML: {htmlPath}");

var pdfBytes = PdfExporter.ExportLetterPdf(htmlBody, $"{book} {chapterStart}:{verseStart}-{verseEnd}", title);
var pdfPath = Path.Combine(outDir, "sample.pdf");
await File.WriteAllBytesAsync(pdfPath, pdfBytes);
Console.WriteLine($"Wrote PDF: {pdfPath}");
