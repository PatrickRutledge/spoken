using Spoken.Core;

namespace Spoken.App;

public partial class MainPage : ContentPage
{
	private readonly ITextSource _source = new UsfmZipTextSource(Path.Combine(AppContext.BaseDirectory, "versions"));
	private string _lastHtml = "";
	private PassageRef? _lastRef;

	public MainPage()
	{
		InitializeComponent();
		TranslationPicker.SelectedIndex = 0;
		PassageEntry.Text = "John 3:16-18";
	}

	private async void OnFormatClicked(object sender, EventArgs e)
	{
		try
		{
			var (book, cs, vs, ce, ve) = ParsePassage(PassageEntry.Text?.Trim() ?? "");
			var trans = (TranslationPicker.SelectedItem?.ToString() ?? "KJV").ToUpperInvariant();
			var verses = new List<Verse>();
			await foreach (var v in _source.GetVersesAsync(trans, book, cs, vs, ce, ve))
				verses.Add(v);

			var html = ProseFormatter.FormatToParagraphs(verses);
			_lastHtml = html;
			_lastRef = new PassageRef(trans, book, cs, vs, ce, ve);
			Preview.Source = new HtmlWebViewSource { Html = html };
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", ex.Message, "OK");
		}
	}

	private async void OnExportPdfClicked(object sender, EventArgs e)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(_lastHtml) || _lastRef is null)
				OnFormatClicked(sender, e);
			if (string.IsNullOrWhiteSpace(_lastHtml) || _lastRef is null)
				return;

			var bytes = PdfExporter.ExportLetterPdf(_lastHtml, _lastRef.ToString(), TitleEntry.Text);
			var file = Path.Combine(FileSystem.CacheDirectory, "spoken.pdf");
			File.WriteAllBytes(file, bytes);
			await Launcher.Default.OpenAsync(new OpenFileRequest("spoken.pdf", new ReadOnlyFile(file)));
		}
		catch (Exception ex)
		{
			await DisplayAlert("Export Error", ex.Message, "OK");
		}
	}

	private static (string book, int? cs, int? vs, int? ce, int? ve) ParsePassage(string input)
	{
		// Very naive parser supporting "Book C:V-V" or "Book C" or "Book"
		if (string.IsNullOrWhiteSpace(input)) return ("John", 3, 16, 3, 18);
		var m = System.Text.RegularExpressions.Regex.Match(input, @"^\s*(.+?)\s+(\d+)(?::(\d+)(?:-(\d+)(?::(\d+))?)?)?\s*$");
		if (!m.Success) return (input.Trim(), null, null, null, null);
		var book = m.Groups[1].Value.Trim();
		int cs = int.Parse(m.Groups[2].Value);
		int? vs = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : null;
		int? ce = m.Groups[5].Success ? int.Parse(m.Groups[4].Value) : (vs.HasValue ? cs : (int?)null);
		int? ve = m.Groups[5].Success ? int.Parse(m.Groups[5].Value) : (vs.HasValue ? vs : (int?)null);
		return (book, cs, vs, ce, ve);
	}
}

