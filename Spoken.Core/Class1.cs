using System.Text;
using System.Text.RegularExpressions;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using QuestPDF;

namespace Spoken.Core;

public record PassageRef(string TranslationCode, string Book, int? ChapterStart, int? VerseStart, int? ChapterEnd, int? VerseEnd)
{
	public override string ToString()
	{
		if (ChapterStart is null) return $"{Book}";
		if (VerseStart is null) return $"{Book} {ChapterStart}";
		if (ChapterEnd is null || VerseEnd is null) return $"{Book} {ChapterStart}:{VerseStart}";
		var end = ChapterEnd == ChapterStart ? $"{VerseEnd}" : $"{ChapterEnd}:{VerseEnd}";
		return $"{Book} {ChapterStart}:{VerseStart}		– {end}";
	}
}

public class Verse
{
	public required string Book { get; init; }
	public required int Chapter { get; init; }
	public required int Number { get; init; }
	public required string Text { get; set; }
	public bool IsPoetryHint { get; set; }
	public int PoetryLevel { get; set; } = 0;
}

public class Passage
{
	public required PassageRef Reference { get; init; }
	public List<Verse> Verses { get; } = new();
}

public interface ITextSource
{
	IAsyncEnumerable<Verse> GetVersesAsync(string translationCode, string book, int? chapterStart, int? verseStart, int? chapterEnd, int? verseEnd, CancellationToken ct = default);
}

public static class ProseFormatter
{
	private static readonly Regex DivineName = new(@"\bLORD\b|\bGOD\b", RegexOptions.Compiled);
	private static readonly Regex DeityPronouns = new(@"\b(he|him|his|himself|thy|thee|thou)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
	private static readonly Regex StraightQuotes = new("\"|'", RegexOptions.Compiled);
	private static readonly Regex DoubleHyphen = new("--", RegexOptions.Compiled);

	public static string FormatToParagraphs(IEnumerable<Verse> verses, bool applySmallCapsDivineName = true, bool capitalizeDeityPronouns = true, bool smartQuotes = true, bool emDash = true)
	{
		var sb = new StringBuilder();
		// Group verses into paragraphs, respecting poetry structure
		var paragraph = new List<Verse>();
		void FlushParagraph()
		{
			if (paragraph.Count == 0) return;
			var text = string.Join(" ", paragraph.Select(v => v.Text.Trim()));
			text = Normalize(text, applySmallCapsDivineName, capitalizeDeityPronouns, smartQuotes, emDash);
			sb.AppendLine($"<p class=\"para\">{System.Net.WebUtility.HtmlEncode(text)}</p>");
			sb.AppendLine();
			paragraph.Clear();
		}

		foreach (var v in verses)
		{
			if (v.IsPoetryHint)
			{
				// Flush any pending prose paragraph first
				FlushParagraph();
				
				// Handle poetry with indentation based on level
				var text = Normalize(v.Text, applySmallCapsDivineName, capitalizeDeityPronouns, smartQuotes, emDash);
				var indentClass = v.PoetryLevel switch
				{
					0 or 1 => "poetry-1",
					2 => "poetry-2", 
					3 => "poetry-3",
					_ => "poetry-4"
				};
				sb.AppendLine($"<p class=\"para poetry {indentClass}\">{System.Net.WebUtility.HtmlEncode(text)}</p>");
				sb.AppendLine();
			}
			else
			{
				paragraph.Add(v);
				if (paragraph.Count >= 6)
					FlushParagraph();
			}
		}
		FlushParagraph();
		return WrapHtml(sb.ToString());
	}

	private static string Normalize(string text, bool smallCapsDivineName, bool capDeityPronouns, bool smartQuotes, bool emDash)
	{
		// Remove verse numbers, headings assumed removed upstream. Apply transformations.
		if (smallCapsDivineName)
		{
			text = DivineName.Replace(text, m => $"<span class=\"sc\">{m.Value}</span>");
		}
		if (capDeityPronouns)
		{
			text = DeityPronouns.Replace(text, m => m.Value switch
			{
				"he" or "He" => "He",
				"him" or "Him" => "Him",
				"his" or "His" => "His",
				"himself" or "Himself" => "Himself",
				"thy" or "Thy" => "Thy",
				"thee" or "Thee" => "Thee",
				"thou" or "Thou" => "Thou",
				_ => m.Value
			});
		}
		if (smartQuotes)
		{
			// Very naive: replace straight quotes with curly. Real impl should use a typographer lib.
			text = text.Replace("\"", "\u201c"); // opening double; simplification
			text = text.Replace("'", "\u2019");
		}
		if (emDash)
		{
			text = DoubleHyphen.Replace(text, "\u2014");
		}
		return text;
	}

	private static string WrapHtml(string body)
	{
		var css = @"<style>
html,body{margin:0;padding:0;background:#fff;color:#111;font-family:Georgia, ""Times New Roman"", serif;}
.container{padding:24px;}
.para{ text-align: justify; text-justify: inter-word; text-indent: 1.5em; margin: 0 0 1em 0; }
.para.poetry{ text-indent: 0; margin: 0.5em 0; }
.para.poetry-1{ margin-left: 0; }
.para.poetry-2{ margin-left: 1.5em; }
.para.poetry-3{ margin-left: 3em; }
.para.poetry-4{ margin-left: 4.5em; }
.sc{ font-variant-caps: small-caps; }
.title{ color:#0A5BCB; font-weight:600; margin:0 0 16px 0; }
</style>";
		return $"<html><head>{css}</head><body><div class=\"container\">{body}</div></body></html>";
	}
}

public static class PdfExporter
{
	public static byte[] ExportLetterPdf(string html, string passageRef, string? title = null)
	{
		// Configure QuestPDF license for development/community use
		Settings.License = LicenseType.Community;
		// Render the HTML body as plain paragraphs into QuestPDF for predictable pagination.
		// This is a simplified mapper that strips tags except <p> and extracts inner text.
	var doc = Document.Create(container =>
		{
			container.Page(page =>
			{
		page.Size(PageSizes.Letter);
				page.Margin(36); // 0.5 inch
				page.DefaultTextStyle(ts => ts.FontSize(12).FontFamily("Times New Roman").LineHeight(1.35f));

				page.Header().Element(e =>
				{
					if (!string.IsNullOrWhiteSpace(title))
						e.Text(title!).SemiBold().FontColor("#0A5BCB").FontSize(14);
				});

				page.Content().Element(e =>
				{
					// Extremely basic HTML to paragraph mapping: split on <p ...> ... </p>
					var paragraphs = ExtractParagraphsFromHtml(html);
					e.Column(col =>
					{
						col.Spacing(6);
						foreach (var p in paragraphs)
						{
							if (string.IsNullOrWhiteSpace(p)) continue;
							col.Item().Text(p); // left-aligned by default
						}
					});
				});

				page.Footer().AlignCenter().Text(text =>
				{
					text.Span(passageRef);
					text.Span(" · Page ");
					text.CurrentPageNumber();
					text.Span(" of ");
					text.TotalPages();
				});
			});
		});
		using var ms = new MemoryStream();
		doc.GeneratePdf(ms);
		return ms.ToArray();
	}

	private static List<string> ExtractParagraphsFromHtml(string html)
	{
		var list = new List<string>();
		var regex = new Regex(@"<p[^>]*>(.*?)</p>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
		foreach (Match m in regex.Matches(html))
		{
			var inner = Regex.Replace(m.Groups[1].Value, @"<[^>]+>", string.Empty); // strip remaining tags
			inner = System.Net.WebUtility.HtmlDecode(inner).Trim();
			list.Add(inner);
		}
		if (list.Count == 0)
		{
			var text = Regex.Replace(html, @"<[^>]+>", string.Empty);
			text = System.Net.WebUtility.HtmlDecode(text).Trim();
			if (!string.IsNullOrWhiteSpace(text)) list.Add(text);
		}
		return list;
	}
}
