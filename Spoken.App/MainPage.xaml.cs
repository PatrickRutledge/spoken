using Spoken.Core;

namespace Spoken.App;

public partial class MainPage : ContentPage
{
	private readonly ITextSource _source;
    private readonly CatalogService _catalogService;
    private SessionState _session = new();
    private TabSession? ActiveTab => _session.Tabs.FirstOrDefault(t => t.Id == _session.ActiveTabId);
    private bool _loadingTab = false; // guard to avoid recursive events

	public MainPage()
	{
		InitializeComponent();
        _catalogService = new CatalogService();
        _source = new UsfmZipTextSource(
            Path.Combine(AppContext.BaseDirectory, "versions"),
            Path.Combine(FileSystem.AppDataDirectory, "translations"));
		TranslationPicker.SelectedIndex = 0;
        TitleEntry.TextChanged += (_, __) => { if (!_loadingTab && ActiveTab != null) { ActiveTab.Title = TitleEntry.Text; _ = PersistAsync(); RebuildTabBar(); } };
        PassageEntry.TextChanged += (_, __) => { if (!_loadingTab && ActiveTab != null) { ActiveTab.Passage = PassageEntry.Text; } };
        TranslationPicker.SelectedIndexChanged += (_, __) => { if (!_loadingTab && ActiveTab != null && TranslationPicker.SelectedItem is string tx) { ActiveTab.Translation = tx; } };
        _ = InitializeSessionAsync();
        LoadAvailableTranslationsAsync();
	}

    private void LoadAvailableTranslationsAsync()
    {
        try
        {
            // Start with bundled translations
            var availableTranslations = new List<string> { "KJV", "ASV", "WEB" };
            
            // Add installed catalog translations
            var installedCodes = _catalogService.GetInstalledTranslations();
            availableTranslations.AddRange(installedCodes.Where(code => !availableTranslations.Contains(code, StringComparer.OrdinalIgnoreCase)));
            
            // Update picker
            TranslationPicker.Items.Clear();
            foreach (var translation in availableTranslations)
            {
                TranslationPicker.Items.Add(translation);
            }
            
            // Restore selected index
            if (TranslationPicker.Items.Count > 0)
                TranslationPicker.SelectedIndex = 0;
        }
        catch
        {
            // Fallback to bundled translations if catalog fails
            TranslationPicker.Items.Clear();
            TranslationPicker.Items.Add("KJV");
            TranslationPicker.Items.Add("ASV");
            TranslationPicker.Items.Add("WEB");
            TranslationPicker.SelectedIndex = 0;
        }
    }

	private async Task InitializeSessionAsync()
	{
		_session = await SessionStore.LoadAsync();
		LoadActiveTabIntoUi();
		RebuildTabBar();
	}

	private void LoadActiveTabIntoUi()
	{
		var tab = ActiveTab;
		if (tab == null) return;
		_loadingTab = true;
		try
		{
			PassageEntry.Text = tab.Passage;
			TitleEntry.Text = tab.Title;
			var idx = Array.IndexOf(new[] { "KJV", "ASV", "WEB" }, tab.Translation?.ToUpperInvariant());
			TranslationPicker.SelectedIndex = idx >= 0 ? idx : 0;
			if (!string.IsNullOrWhiteSpace(tab.Html))
				Preview.Source = new HtmlWebViewSource { Html = tab.Html };
		}
		finally
		{
			_loadingTab = false;
		}
	}

	private async Task PersistAsync() => await SessionStore.SaveAsync(_session);

	private void RebuildTabBar()
	{
		TabBar.Children.Clear();
		foreach (var tab in _session.Tabs)
		{
			var isActive = tab.Id == _session.ActiveTabId;
			var border = new Border
			{
				BackgroundColor = isActive ? (Color)Application.Current!.Resources["Primary"] : Colors.White,
				Padding = new Thickness(8,4),
				Stroke = isActive ? Colors.Transparent : Colors.LightGray,
				StrokeThickness = 1,
				Content = BuildTabContent(tab, isActive)
			};
			var tap = new TapGestureRecognizer();
			tap.Tapped += (_, __) => { _session.ActiveTabId = tab.Id; LoadActiveTabIntoUi(); RebuildTabBar(); _ = PersistAsync(); };
			border.GestureRecognizers.Add(tap);
			TabBar.Children.Add(border);
		}
		// Add button
		var addBtn = new Button { Text = "+", WidthRequest = 32, HeightRequest = 32, Padding = 0 };
		addBtn.Clicked += OnAddTabClicked;
		TabBar.Children.Add(addBtn);
	}

	private View BuildTabContent(TabSession tab, bool isActive)
	{
		var label = new Label { Text = Truncate(tab.DisplayLabel, 24), TextColor = isActive ? Colors.White : Colors.Black, VerticalTextAlignment = TextAlignment.Center };
		var close = new Button { Text = "×", Padding = new Thickness(0), WidthRequest = 24, HeightRequest = 24, BackgroundColor = Colors.Transparent, TextColor = isActive ? Colors.White : Colors.Black, FontSize = 14 };
		close.Clicked += (s, e) => { CloseTab(tab.Id); };
		return new HorizontalStackLayout { Spacing = 4, Children = { label, close } };
	}

	private void CloseTab(Guid id)
	{
		var idx = _session.Tabs.FindIndex(t => t.Id == id);
		if (idx < 0) return;
		_session.Tabs.RemoveAt(idx);
		if (_session.Tabs.Count == 0)
		{
			var t = new TabSession();
			_session.Tabs.Add(t);
			_session.ActiveTabId = t.Id;
		}
		else if (_session.ActiveTabId == id)
		{
			var newIdx = Math.Max(0, idx - 1);
			_session.ActiveTabId = _session.Tabs[newIdx].Id;
		}
		LoadActiveTabIntoUi();
		RebuildTabBar();
		_ = PersistAsync();
	}

	private void OnAddTabClicked(object? sender, EventArgs e)
	{
		var tab = new TabSession();
		_session.Tabs.Add(tab);
		_session.ActiveTabId = tab.Id;
		LoadActiveTabIntoUi();
		RebuildTabBar();
		_ = PersistAsync();
	}

	private static string Truncate(string? s, int len)
		=> string.IsNullOrEmpty(s) ? string.Empty : (s.Length <= len ? s : s.Substring(0, len - 1) + '…');

	private async void OnFormatClicked(object sender, EventArgs e)
	{
		try
		{
			var parseResult = PassageParser.Parse(PassageEntry.Text?.Trim() ?? "");
			if (!parseResult.IsSuccess)
			{
				await DisplayAlert("Invalid Passage", parseResult.ErrorMessage, "OK");
				return;
			}

			var trans = (TranslationPicker.SelectedItem?.ToString() ?? "KJV").ToUpperInvariant();
			var verses = new List<Verse>();
			await foreach (var v in _source.GetVersesAsync(trans, parseResult.UsfmCode!, 
				parseResult.ChapterStart, parseResult.VerseStart, parseResult.ChapterEnd, parseResult.VerseEnd))
				verses.Add(v);

			if (verses.Count == 0)
			{
				await DisplayAlert("No Results", $"No verses found for {parseResult} in {trans}. The passage may not exist in this translation.", "OK");
				return;
			}

			var html = ProseFormatter.FormatToParagraphs(verses);
            if (ActiveTab != null)
            {
                ActiveTab.Html = html;
                ActiveTab.Passage = parseResult.ToString(); // Use normalized passage reference
                ActiveTab.Translation = trans;
                ActiveTab.Title = TitleEntry.Text;
                _ = PersistAsync();
                RebuildTabBar();
            }
			Preview.Source = new HtmlWebViewSource { Html = html };
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", ex.Message, "OK");
		}
	}	private async void OnExportPdfClicked(object sender, EventArgs e)
	{
		try
		{
			if (ActiveTab == null || string.IsNullOrWhiteSpace(ActiveTab.Html))
				OnFormatClicked(sender, e);
			if (ActiveTab == null || string.IsNullOrWhiteSpace(ActiveTab.Html))
				return;

			var passageLabel = ActiveTab.Passage ?? "";
			var bytes = PdfExporter.ExportLetterPdf(ActiveTab.Html, passageLabel, TitleEntry.Text);
			var file = Path.Combine(FileSystem.CacheDirectory, "spoken.pdf");
			File.WriteAllBytes(file, bytes);
			await Launcher.Default.OpenAsync(new OpenFileRequest("spoken.pdf", new ReadOnlyFile(file)));
		}
		catch (Exception ex)
		{
			await DisplayAlert("Export Error", ex.Message, "OK");
		}
	}
}

