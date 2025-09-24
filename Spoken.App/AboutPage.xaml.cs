namespace Spoken.App;

public partial class AboutPage : ContentPage
{
	public AboutPage()
	{
		InitializeComponent();
	}

	private async void OnPrivacyPolicyClicked(object sender, EventArgs e)
	{
		try
		{
			// Read the privacy policy from embedded resources or local file
			var privacyPolicyUrl = "https://github.com/AetherForge/spoken/blob/main/docs/privacy-policy.md";
			
			await DisplayAlert("Privacy Policy", 
				"Spoken is committed to protecting your privacy. " +
				"The app does not collect, store, or transmit any personal data. " +
				"All reading sessions and data are stored locally on your device only. " +
				"Network access is used solely for downloading biblical translations. " +
				"For the full privacy policy, please visit our GitHub repository.",
				"OK");
			
			// Optionally open the full privacy policy in browser
			var openFull = await DisplayAlert("Privacy Policy", 
				"Would you like to view the complete privacy policy online?", 
				"Yes", "No");
				
			if (openFull)
			{
				await Launcher.Default.OpenAsync(privacyPolicyUrl);
			}
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", "Unable to open privacy policy: " + ex.Message, "OK");
		}
	}
}