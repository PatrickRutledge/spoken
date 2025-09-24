using Spoken.Core;

namespace Spoken.App;

public partial class MainPage : ContentPage
{
	private readonly ITextSource _source;
    private readonly CatalogService _catalogService;
    private SessionState _session = new();
    private TabSession? ActiveTab => _session.Tabs.FirstOrDefault(t => t.Id == _session.ActiveTabId);
    private bool _loadingTab = false; // guard to avoid recursive events
    private bool _updatingSelectors = false; // guard to avoid recursive picker updates
    private List<BiblicalBook> _books = new();

	public MainPage()
	{
		InitializeComponent();
        _catalogService = new CatalogService();
        _source = new UsfmZipTextSource(
            Path.Combine(AppContext.BaseDirectory, "versions"),
            Path.Combine(FileSystem.AppDataDirectory, "translations"));
		
        InitializeBooks();
        InitializePickers();
        SetDefaultPassage();
        BuildBookList();
        _ = InitializeSessionAsync();
        LoadAvailableTranslationsAsync();
	}

    private void InitializeBooks()
    {
        _books = PassageParser.GetAllBooks().ToList();
    }

    private void InitializePickers()
    {
        // Populate book pickers
        foreach (var book in _books)
        {
            StartBookPicker.Items.Add(book.Name);
            EndBookPicker.Items.Add(book.Name);
        }

        // Set default selection
        TranslationPicker.SelectedIndex = 0;
        StartBookPicker.SelectedIndex = 0; // Genesis
        EndBookPicker.SelectedIndex = 0;   // Genesis
    }

    private void SetDefaultPassage()
    {
        _updatingSelectors = true;
        
        // Set to Genesis 1:31
        StartBookPicker.SelectedIndex = 0; // Genesis
        EndBookPicker.SelectedIndex = 0;   // Genesis
        
        // Update chapter pickers to populate them
        UpdateChapterPicker(StartBookPicker, StartChapterPicker);
        UpdateChapterPicker(EndBookPicker, EndChapterPicker);
        
        // Set chapter 1 (index 0)
        StartChapterPicker.SelectedIndex = 0;
        EndChapterPicker.SelectedIndex = 0;
        
        // Update verse pickers to populate them
        UpdateVersePicker(StartBookPicker, StartChapterPicker, StartVersePicker);
        UpdateVersePicker(EndBookPicker, EndChapterPicker, EndVersePicker);
        
        // Set verse 31 (index 30) for both start and end
        if (StartVersePicker.Items.Count >= 31)
            StartVersePicker.SelectedIndex = 30; // Verse 31
        if (EndVersePicker.Items.Count >= 31)
            EndVersePicker.SelectedIndex = 30; // Verse 31
        
        _updatingSelectors = false;
        
        // Update the preview with the new selection
        _ = UpdatePreviewAsync();
    }

    private void BuildBookList()
    {
        BookListPanel.Children.Clear();
        
        foreach (var book in _books)
        {
            var bookButton = new Button
            {
                Text = $"{book.Name} ({book.ChapterCount} chapters)",
                BackgroundColor = Colors.Transparent,
                TextColor = Color.FromArgb("#0A5BCB"),
                FontSize = 12,
                Padding = new Thickness(8, 4),
                HorizontalOptions = LayoutOptions.Fill
            };
            
            bookButton.Clicked += (sender, e) => OnBookListItemClicked(book);
            BookListPanel.Children.Add(bookButton);
        }
    }

    private void OnBookListItemClicked(BiblicalBook book)
    {
        // Set the start book picker to this book
        var bookIndex = _books.IndexOf(book);
        if (bookIndex >= 0)
        {
            StartBookPicker.SelectedIndex = bookIndex;
            EndBookPicker.SelectedIndex = bookIndex;
        }
    }

    private void OnStartBookChanged(object sender, EventArgs e)
    {
        if (_updatingSelectors) return;
        UpdateChapterPicker(StartBookPicker, StartChapterPicker);
        OnPassageSelectionChanged(sender, e);
    }

    private void OnEndBookChanged(object sender, EventArgs e)
    {
        if (_updatingSelectors) return;
        UpdateChapterPicker(EndBookPicker, EndChapterPicker);
        OnPassageSelectionChanged(sender, e);
    }

    private void OnStartChapterChanged(object sender, EventArgs e)
    {
        if (_updatingSelectors) return;
        UpdateVersePicker(StartBookPicker, StartChapterPicker, StartVersePicker);
        OnPassageSelectionChanged(sender, e);
    }

    private void OnEndChapterChanged(object sender, EventArgs e)
    {
        if (_updatingSelectors) return;
        UpdateVersePicker(EndBookPicker, EndChapterPicker, EndVersePicker);
        OnPassageSelectionChanged(sender, e);
    }

    private void OnPassageSelectionChanged(object sender, EventArgs e)
    {
        if (_updatingSelectors) return;
        _ = UpdatePreviewAsync();
        UpdateFooterCitation();
        
        // Update active tab
        if (!_loadingTab && ActiveTab != null)
        {
            ActiveTab.Passage = GetCurrentPassageReference();
            if (TranslationPicker.SelectedItem is string tx)
                ActiveTab.Translation = tx;
            _ = PersistAsync();
        }
    }

    private void UpdateChapterPicker(Picker bookPicker, Picker chapterPicker)
    {
        chapterPicker.Items.Clear();
        
        if (bookPicker.SelectedIndex >= 0 && bookPicker.SelectedIndex < _books.Count)
        {
            var book = _books[bookPicker.SelectedIndex];
            for (int i = 1; i <= book.ChapterCount; i++)
            {
                chapterPicker.Items.Add(i.ToString());
            }
            if (chapterPicker.Items.Count > 0)
                chapterPicker.SelectedIndex = 0;
        }
    }

    private void UpdateVersePicker(Picker bookPicker, Picker chapterPicker, Picker versePicker)
    {
        versePicker.Items.Clear();
        
        // Get the actual verse count for the selected book and chapter
        if (bookPicker.SelectedIndex >= 0 && bookPicker.SelectedIndex < _books.Count &&
            chapterPicker.SelectedIndex >= 0)
        {
            var book = _books[bookPicker.SelectedIndex];
            var chapter = chapterPicker.SelectedIndex + 1;
            var verseCount = PassageParser.GetVerseCount(book.Name, chapter);
            
            for (int i = 1; i <= verseCount; i++)
            {
                versePicker.Items.Add(i.ToString());
            }
        }
        else
        {
            // Fallback to 50 verses if we can't determine the book/chapter
            for (int i = 1; i <= 50; i++)
            {
                versePicker.Items.Add(i.ToString());
            }
        }
        
        if (versePicker.Items.Count > 0)
            versePicker.SelectedIndex = 0;
    }

    private string GetCurrentPassageReference()
    {
        if (StartBookPicker.SelectedIndex < 0 || StartBookPicker.SelectedIndex >= _books.Count)
            return "";

        var startBook = _books[StartBookPicker.SelectedIndex];
        var startChapter = StartChapterPicker.SelectedIndex + 1;
        var startVerse = StartVersePicker.SelectedIndex + 1;

        // Only use end values if they're actually selected (not -1)
        var endBook = EndBookPicker.SelectedIndex >= 0 && EndBookPicker.SelectedIndex < _books.Count 
            ? _books[EndBookPicker.SelectedIndex] : null;
        var endChapter = EndChapterPicker.SelectedIndex >= 0 ? EndChapterPicker.SelectedIndex + 1 : (int?)null;
        var endVerse = EndVersePicker.SelectedIndex >= 0 ? EndVersePicker.SelectedIndex + 1 : (int?)null;
        


        // Format the reference based on range type
        // If no end book is selected, it's a single verse
        if (endBook == null)
        {
            return $"{startBook.Name} {startChapter}:{startVerse}";
        }
        
        // If end book is selected but no chapter/verse, default to same chapter/verse as start
        if (endChapter == null) endChapter = startChapter;
        if (endVerse == null) endVerse = startVerse;
        
        if (startBook.UsfmCode == endBook.UsfmCode)
        {
            if (startChapter == endChapter)
            {
                if (startVerse == endVerse)
                    return $"{startBook.Name} {startChapter}:{startVerse}";
                else
                    return $"{startBook.Name} {startChapter}:{startVerse}-{endVerse}";
            }
            else
            {
                return $"{startBook.Name} {startChapter}:{startVerse}-{endChapter}:{endVerse}";
            }
        }
        else
        {
            return $"{startBook.Name} {startChapter}:{startVerse} - {endBook.Name} {endChapter}:{endVerse}";
        }
    }

    private void UpdateFooterCitation()
    {
        var passage = GetCurrentPassageReference();
        var translation = TranslationPicker.SelectedItem?.ToString() ?? "KJV";
        
        if (!string.IsNullOrEmpty(passage))
        {
            FooterCitation.Text = $"{passage} ({translation})";
        }
        else
        {
            FooterCitation.Text = "Select a passage to view citation";
        }
    }

    private async Task UpdatePreviewAsync()
    {
        try
        {
            var passage = GetCurrentPassageReference();
            PassageReference.Text = passage;
            
            if (string.IsNullOrEmpty(passage))
            {
                Preview.Source = new HtmlWebViewSource { Html = "<html><body><p>Select a passage to preview</p></body></html>" };
                return;
            }

            var parseResult = PassageParser.Parse(passage);
            if (!parseResult.IsSuccess)
            {
                Preview.Source = new HtmlWebViewSource { Html = $"<html><body><p>Error: {parseResult.ErrorMessage}</p></body></html>" };
                return;
            }

            // Debug output - show what we're parsing and what gets extracted
            var debugInfo = $"Raw: '{passage}' -> Parsed: {parseResult.UsfmCode} {parseResult.ChapterStart}:{parseResult.VerseStart}-{parseResult.ChapterEnd}:{parseResult.VerseEnd}";
            
            var trans = TranslationPicker.SelectedItem?.ToString() ?? "KJV";
            var verses = new List<Verse>();
            

            
            await foreach (var v in _source.GetVersesAsync(trans, parseResult.UsfmCode!, 
                parseResult.ChapterStart, parseResult.VerseStart, parseResult.ChapterEnd, parseResult.VerseEnd))
            {
                verses.Add(v);
            }

            // Show debug info including verse count
            PassageReference.Text = $"{debugInfo} -> Retrieved {verses.Count} verses";
            
            // Add first and last verse info to debug display
            if (verses.Count > 0)
            {
                var first = verses.First();
                var last = verses.Last();
                PassageReference.Text += $" (First: {first.Chapter}:{first.Number}, Last: {last.Chapter}:{last.Number})";
            }

            if (verses.Count == 0)
            {
                Preview.Source = new HtmlWebViewSource { Html = $"<html><body><p>No verses found for {passage} in {trans}</p></body></html>" };
                return;
            }

            var html = ProseFormatter.FormatToParagraphs(verses);
            Preview.Source = new HtmlWebViewSource { Html = html };
        }
        catch (Exception ex)
        {
            Preview.Source = new HtmlWebViewSource { Html = $"<html><body><p>Error: {ex.Message}</p></body></html>" };
        }
    }

    private async void OnGetPassageClicked(object sender, EventArgs e)
    {
        await UpdatePreviewAsync();
    }

    private void OnImmersiveReaderClicked(object sender, EventArgs e)
    {
        // TODO: Implement immersive reader mode
        DisplayAlert("Immersive Reader", "Immersive reader mode coming soon!", "OK");
    }

    private void OnTitleChanged(object sender, TextChangedEventArgs e)
    {
        if (!_loadingTab && ActiveTab != null)
        {
            ActiveTab.Title = e.NewTextValue ?? "";
            _ = PersistAsync();
        }
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
		// RebuildTabBar();
	}

	private void LoadActiveTabIntoUi()
	{
		var tab = ActiveTab;
		if (tab == null) return;
		_loadingTab = true;
		try
		{
			SetPassageReference(tab.Passage ?? "");
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

	private void SetPassageReference(string passage)
	{
		_updatingSelectors = true;
		
		if (string.IsNullOrEmpty(passage))
		{
			StartBookPicker.SelectedIndex = -1;
			EndBookPicker.SelectedIndex = -1;
			_updatingSelectors = false;
			return;
		}

		var parseResult = PassageParser.Parse(passage);
		if (!parseResult.IsSuccess)
		{
			_updatingSelectors = false;
			return;
		}

		// Find and set start book
		var startBook = _books.FirstOrDefault(b => b.UsfmCode == parseResult.UsfmCode);
		if (startBook != null)
		{
			StartBookPicker.SelectedIndex = _books.IndexOf(startBook);
			UpdateChapterPicker(StartBookPicker, StartChapterPicker);
			
			// Set start chapter and verse
			if (parseResult.ChapterStart.HasValue && parseResult.ChapterStart > 0 && parseResult.ChapterStart <= StartChapterPicker.Items.Count)
			{
				StartChapterPicker.SelectedIndex = parseResult.ChapterStart.Value - 1;
				UpdateVersePicker(StartBookPicker, StartChapterPicker, StartVersePicker);
				
				if (parseResult.VerseStart.HasValue && parseResult.VerseStart > 0 && parseResult.VerseStart <= StartVersePicker.Items.Count)
					StartVersePicker.SelectedIndex = parseResult.VerseStart.Value - 1;
			}

			// Set end selectors for ranges
			EndBookPicker.SelectedIndex = StartBookPicker.SelectedIndex;
			UpdateChapterPicker(EndBookPicker, EndChapterPicker);
			
			var endChapter = parseResult.ChapterEnd ?? parseResult.ChapterStart;
			if (endChapter.HasValue && endChapter > 0 && endChapter <= EndChapterPicker.Items.Count)
			{
				EndChapterPicker.SelectedIndex = endChapter.Value - 1;
				UpdateVersePicker(EndBookPicker, EndChapterPicker, EndVersePicker);
				
				var endVerse = parseResult.VerseEnd ?? parseResult.VerseStart;
				if (endVerse.HasValue && endVerse > 0 && endVerse <= EndVersePicker.Items.Count)
					EndVersePicker.SelectedIndex = endVerse.Value - 1;
			}
		}
		
		_updatingSelectors = false;
	}

	private async Task PersistAsync() => await SessionStore.SaveAsync(_session);

	// private void RebuildTabBar()
	// {
	// 	TabBar.Children.Clear();
	// 	foreach (var tab in _session.Tabs)
	// 	{
	// 		var isActive = tab.Id == _session.ActiveTabId;
	// 		var border = new Border
	// 		{
	// 			BackgroundColor = isActive ? (Color)Application.Current!.Resources["Primary"] : Colors.White,
	// 			Padding = new Thickness(8,4),
	// 			Stroke = isActive ? Colors.Transparent : Colors.LightGray,
	// 			StrokeThickness = 1,
	// 			Content = BuildTabContent(tab, isActive)
	// 		};
	// 		var tap = new TapGestureRecognizer();
	// 		tap.Tapped += (_, __) => { _session.ActiveTabId = tab.Id; LoadActiveTabIntoUi(); RebuildTabBar(); _ = PersistAsync(); };
	// 		border.GestureRecognizers.Add(tap);
	// 		TabBar.Children.Add(border);
	// 	}
	// 	// Add button
	// 	var addBtn = new Button { Text = "+", WidthRequest = 32, HeightRequest = 32, Padding = 0 };
	// 	addBtn.Clicked += OnAddTabClicked;
	// 	TabBar.Children.Add(addBtn);
	// }

	// private View BuildTabContent(TabSession tab, bool isActive)
	// {
	// 	var label = new Label { Text = Truncate(tab.DisplayLabel, 24), TextColor = isActive ? Colors.White : Colors.Black, VerticalTextAlignment = TextAlignment.Center };
	// 	var close = new Button { Text = "×", Padding = new Thickness(0), WidthRequest = 24, HeightRequest = 24, BackgroundColor = Colors.Transparent, TextColor = isActive ? Colors.White : Colors.Black, FontSize = 14 };
	// 	close.Clicked += (s, e) => { CloseTab(tab.Id); };
	// 	return new HorizontalStackLayout { Spacing = 4, Children = { label, close } };
	// }

	// private void CloseTab(Guid id)
	// {
	// 	var idx = _session.Tabs.FindIndex(t => t.Id == id);
	// 	if (idx < 0) return;
	// 	_session.Tabs.RemoveAt(idx);
	// 	if (_session.Tabs.Count == 0)
	// 	{
	// 		var t = new TabSession();
	// 		_session.Tabs.Add(t);
	// 		_session.ActiveTabId = t.Id;
	// 	}
	// 	else if (_session.ActiveTabId == id)
	// 	{
	// 		var newIdx = Math.Max(0, idx - 1);
	// 		_session.ActiveTabId = _session.Tabs[newIdx].Id;
	// 	}
	// 	LoadActiveTabIntoUi();
	// 	// RebuildTabBar();
	// 	_ = PersistAsync();
	// }

	// private void OnAddTabClicked(object? sender, EventArgs e)
	// {
	// 	var tab = new TabSession();
	// 	_session.Tabs.Add(tab);
	// 	_session.ActiveTabId = tab.Id;
	// 	LoadActiveTabIntoUi();
	// 	// RebuildTabBar();
	// 	_ = PersistAsync();
	// }

	private static string Truncate(string? s, int len)
		=> string.IsNullOrEmpty(s) ? string.Empty : (s.Length <= len ? s : s.Substring(0, len - 1) + '…');

	private async void OnFormatClicked(object sender, EventArgs e)
	{
		try
		{
			var parseResult = PassageParser.Parse(GetCurrentPassageReference());
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
                // RebuildTabBar();
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

