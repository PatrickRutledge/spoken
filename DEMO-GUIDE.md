# Spoken Bible App - Demo Guide

## 🎯 How to Demo the App

### Option 1: Visual Studio (Recommended)
1. **Open the solution in Visual Studio 2022**
   ```
   Open: D:\spoken\Spoken.sln
   ```

2. **Set Startup Project**
   - Right-click `Spoken.App` in Solution Explorer
   - Select "Set as Startup Project"

3. **Select Windows Target**
   - In the toolbar, ensure "Windows Machine" is selected as the target
   - Framework should be `net8.0-windows10.0.19041.0`

4. **Run the App**
   - Press F5 or click the "Play" button
   - The app will build and launch

### Option 2: Command Line Build & Run
1. **Build the app**
   ```powershell
   cd D:\spoken
   dotnet build -c Debug
   ```

2. **Run the built executable directly**
   ```powershell
   cd "Spoken.App\bin\Debug\net8.0-windows10.0.19041.0\win10-x64"
   .\Spoken.App.exe
   ```

### Option 3: Install MSIX Package (Advanced)
1. **Enable Developer Mode** (Windows Settings > Update & Security > For developers)
2. **Install the package**
   ```powershell
   cd "D:\spoken\Spoken.App\bin\Release\net8.0-windows10.0.19041.0\win10-x64\AppPackages\Spoken.App_1.0.0.0_Test"
   # Right-click on Spoken.App_1.0.0.0_x64.msix and select "Install"
   ```

## 🎪 Demo Script

### What to Show

#### 1. **Main Interface**
- Modern flyout navigation menu
- Clean, professional UI
- Passage entry field with smart validation

#### 2. **Enhanced Passage Parsing** 
Try these examples:
- `John 3:16` (Single verse)
- `Romans 8:28-39` (Verse range)
- `Psalm 23` (Whole chapter)
- `Genesis 1:1-2:3` (Cross-chapter range)
- `Matt 5-7` (Multiple chapters)
- `Revelation` (Whole book)

**Demonstrate error handling:**
- `John 50:16` (Invalid chapter)
- `Romans 8:100` (Invalid verse)
- `Fake 1:1` (Invalid book name)

#### 3. **Tabbed Sessions**
- Open multiple passages in different tabs
- Show session persistence (close and reopen app)
- Demonstrate tab management

#### 4. **Translation Management**
- Navigate to "Translations" in flyout menu
- Show available translations catalog
- Demonstrate download progress
- Install/remove translations

#### 5. **PDF Export**
- Format a passage
- Click "Export PDF" 
- Show generated PDF with beautiful formatting

#### 6. **About & Privacy**
- Navigate to "About and Privacy"
- Show privacy policy
- Demonstrate app information

### Key Features to Highlight

✅ **Smart Passage Parsing**
- 66 biblical books with abbreviations
- 6 different reference formats supported
- Intelligent error messages with suggestions

✅ **Multiple Translations**
- Downloadable catalog system
- Local caching for offline use
- SHA-256 integrity verification

✅ **Session Management**
- Persistent tabbed interface
- JSON-based state storage
- Automatic session restoration

✅ **Beautiful PDF Export**
- Professional document formatting
- Custom typography and layout
- Integrated file sharing

✅ **Privacy First**
- No data collection or tracking
- Local-only storage
- Complete privacy policy

✅ **Store Ready**
- MSIX packaging configured
- Proper app identity and metadata
- Professional UI and UX

## 🐛 Troubleshooting

### If the app won't start:
1. Ensure you have .NET 8 SDK installed
2. Make sure Windows 10 version 19041 or later
3. Try running Visual Studio as Administrator
4. Clear bin/obj folders and rebuild

### If packages are missing:
```powershell
dotnet restore
dotnet build
```

### If MSIX installation fails:
- Enable Developer Mode in Windows Settings
- The package is unsigned (normal for development)
- Use Visual Studio debugging instead

## 🎯 Demo Tips

1. **Start with Visual Studio** - Most reliable for demos
2. **Prepare sample passages** - Have John 3:16, Psalm 23, Romans 8:28-39 ready
3. **Show error handling** - Try invalid references to show smart validation
4. **Highlight unique features** - Focus on parsing intelligence and session management
5. **End with PDF export** - Great visual finale

The app is production-ready with professional features and Microsoft Store packaging!