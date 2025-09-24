using Spoken.Core;

namespace Spoken.App;

public partial class TranslationsPage : ContentPage
{
    private readonly CatalogService _catalogService;
    private readonly UsfmZipTextSource _textSource;

    public TranslationsPage()
    {
        InitializeComponent();
        _catalogService = new CatalogService();
        _textSource = new UsfmZipTextSource(
            Path.Combine(AppContext.BaseDirectory, "versions"),
            Path.Combine(FileSystem.AppDataDirectory, "translations")
        );
        _ = LoadTranslationsAsync();
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        await LoadTranslationsAsync();
    }

    private async Task LoadTranslationsAsync()
    {
        LoadingIndicator.IsVisible = true;
        TranslationsContainer.Children.Clear();

        try
        {
            // Get available translations from catalog
            var availableTranslations = await _catalogService.GetAvailableTranslationsAsync();
            var installedCodes = _catalogService.GetInstalledTranslations();

            // Add bundled translations first
            var bundledTranslations = new[]
            {
                new { Code = "KJV", Name = "King James Version", IsBundled = true, IsInstalled = true },
                new { Code = "ASV", Name = "American Standard Version", IsBundled = true, IsInstalled = true },
                new { Code = "WEB", Name = "World English Bible", IsBundled = true, IsInstalled = true }
            };

            foreach (var bundled in bundledTranslations)
            {
                TranslationsContainer.Children.Add(CreateTranslationCard(
                    bundled.Code, bundled.Name, "Bundled", null, true, true, false));
            }

            // Add a separator
            if (availableTranslations.Count > 0)
            {
                var separator = new BoxView
                {
                    Color = Colors.LightGray,
                    HeightRequest = 1,
                    Margin = new Thickness(0, 8)
                };
                TranslationsContainer.Children.Add(separator);
            }

            // Add catalog translations
            foreach (var translation in availableTranslations)
            {
                var isInstalled = installedCodes.Contains(translation.Code.ToUpperInvariant());
                TranslationsContainer.Children.Add(CreateTranslationCard(
                    translation.Code, translation.Name, translation.Language, translation.Description, 
                    isInstalled, false, true));
            }

            if (availableTranslations.Count == 0 && bundledTranslations.Length > 0)
            {
                var noNetworkLabel = new Label
                {
                    Text = "No additional translations available (check network connection)",
                    FontSize = 14,
                    TextColor = Colors.Gray,
                    HorizontalTextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 16)
                };
                TranslationsContainer.Children.Add(noNetworkLabel);
            }
        }
        catch (Exception ex)
        {
            var errorLabel = new Label
            {
                Text = $"Error loading catalog: {ex.Message}",
                FontSize = 14,
                TextColor = Colors.Red,
                HorizontalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 16)
            };
            TranslationsContainer.Children.Add(errorLabel);
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
        }
    }

    private View CreateTranslationCard(string code, string name, string? language, string? description, 
        bool isInstalled, bool isBundled, bool canDownload)
    {
        var card = new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Colors.LightGray,
            StrokeThickness = 1,
            Padding = new Thickness(12),
            Margin = new Thickness(0, 4)
        };

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        var infoStack = new VerticalStackLayout
        {
            Spacing = 4
        };

        var titleStack = new HorizontalStackLayout
        {
            Spacing = 8
        };

        titleStack.Children.Add(new Label
        {
            Text = $"{code} - {name}",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold
        });

        if (isBundled)
        {
            titleStack.Children.Add(new Label
            {
                Text = "BUNDLED",
                FontSize = 10,
                TextColor = Colors.White,
                BackgroundColor = Color.FromArgb("#0A5BCB"),
                Padding = new Thickness(4, 2)
            });
        }
        else if (isInstalled)
        {
            titleStack.Children.Add(new Label
            {
                Text = "INSTALLED",
                FontSize = 10,
                TextColor = Colors.White,
                BackgroundColor = Colors.Green,
                Padding = new Thickness(4, 2)
            });
        }

        infoStack.Children.Add(titleStack);

        if (!string.IsNullOrWhiteSpace(language))
        {
            infoStack.Children.Add(new Label
            {
                Text = language,
                FontSize = 14,
                TextColor = Colors.Gray
            });
        }

        if (!string.IsNullOrWhiteSpace(description))
        {
            infoStack.Children.Add(new Label
            {
                Text = description,
                FontSize = 12,
                TextColor = Colors.DarkGray
            });
        }

        grid.Children.Add(infoStack);
        Grid.SetColumn(infoStack, 0);

        // Action button
        if (canDownload && !isBundled)
        {
            var button = new Button
            {
                Text = isInstalled ? "Remove" : "Download",
                BackgroundColor = isInstalled ? Colors.Red : Color.FromArgb("#0A5BCB"),
                TextColor = Colors.White,
                FontSize = 12,
                Padding = new Thickness(12, 6),
                VerticalOptions = LayoutOptions.Center
            };

            button.Clicked += async (s, e) => await HandleActionAsync(code, name, isInstalled, button);
            grid.Children.Add(button);
            Grid.SetColumn(button, 1);
        }

        card.Content = grid;
        return card;
    }

    private async Task HandleActionAsync(string code, string name, bool isCurrentlyInstalled, Button button)
    {
        button.IsEnabled = false;
        
        try
        {
            if (isCurrentlyInstalled)
            {
                // Remove translation
                var success = await _catalogService.DeleteTranslationAsync(code);
                if (success)
                {
                    await DisplayAlert("Success", $"{name} has been removed.", "OK");
                    await LoadTranslationsAsync(); // Refresh the list
                }
                else
                {
                    await DisplayAlert("Error", $"Failed to remove {name}.", "OK");
                }
            }
            else
            {
                // Download translation
                var availableTranslations = await _catalogService.GetAvailableTranslationsAsync();
                var translation = availableTranslations.FirstOrDefault(t => 
                    string.Equals(t.Code, code, StringComparison.OrdinalIgnoreCase));
                
                if (translation == null)
                {
                    await DisplayAlert("Error", "Translation not found in catalog.", "OK");
                    return;
                }

                button.Text = "Downloading...";
                
                var progress = new Progress<double>(p => 
                {
                    MainThread.BeginInvokeOnMainThread(() => 
                    {
                        button.Text = $"Downloading {p:P0}";
                    });
                });

                var success = await _catalogService.DownloadTranslationAsync(translation, progress);
                
                if (success)
                {
                    await DisplayAlert("Success", $"{name} has been downloaded successfully.", "OK");
                    await LoadTranslationsAsync(); // Refresh the list
                }
                else
                {
                    await DisplayAlert("Error", $"Failed to download {name}. Please check your connection and try again.", "OK");
                    button.Text = "Download";
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            button.IsEnabled = true;
        }
    }
}