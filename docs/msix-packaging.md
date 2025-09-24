# MSIX Packaging Documentation

## Successfully Created MSIX Package

**Package Location:** 
`Spoken.App\bin\Release\net8.0-windows10.0.19041.0\win10-x64\AppPackages\Spoken.App_1.0.0.0_Test\`

**Package File:** `Spoken.App_1.0.0.0_x64.msix` (68.6 MB)

## Package Configuration

### Identity
- **Name:** AetherForge.Spoken
- **Publisher:** CN=80415444-5392-4904-8AC7-7511A51DFC7C
- **Publisher Display Name:** Aether Forge
- **Version:** 1.0.0.0

### Build Command
```powershell
# From Spoken.App directory:
dotnet publish -f net8.0-windows10.0.19041.0 -c Release -p:WindowsPackageType=MSIX --output ..\publish
```

### Capabilities
- `runFullTrust` (Required for .NET MAUI apps)
- `internetClient` (For downloading biblical translations)
- `removableStorage` (For file system access)

## Installation for Testing

### Install Package for Development Testing
```powershell
# Run from the AppPackages directory with admin privileges:
.\Add-AppDevPackage.ps1
```

### Uninstall Package
```powershell
# Through PowerShell:
Get-AppxPackage -Name "AetherForge.Spoken" | Remove-AppxPackage
```

## Assets Status

### Required Assets (Currently using placeholders)
- `Assets\Square44x44Logo.png` - App list icon
- `Assets\Square150x150Logo.png` - Start menu tile
- `Assets\Square71x71Logo.png` - Small tile
- `Assets\Wide310x150Logo.png` - Wide tile
- `Assets\Square310x310Logo.png` - Large tile
- `Assets\StoreLogo.png` - Store listing
- `Assets\SplashScreen.png` - App launch screen

**Next Step:** Replace placeholder assets with actual branded images.

## Store Submission Readiness

### ✅ Completed
- [x] MSIX package builds successfully
- [x] Store identity configuration complete
- [x] Package manifest properly configured
- [x] Privacy policy created and accessible in app
- [x] About page with app information
- [x] Minimal capability requirements

### 🔄 Next Steps
1. **Create branded image assets** (Square44, 150, 310, etc.)
2. **WACK testing** - Run Windows App Certification Kit
3. **Local installation testing** - Verify app installs and launches
4. **Store listing preparation** - Screenshots, descriptions, metadata

### 📋 Store Submission Checklist
- [ ] Run WACK test (Windows App Certification Kit)
- [ ] Create store listing screenshots
- [ ] Finalize store description and metadata
- [ ] Test installation from MSIX package
- [ ] Document any WACK warnings/exceptions

## Known Issues
- **Warning:** Path to `mspdbcmf.exe` not found (symbols package not generated) - This is acceptable for store submission
- **Assets:** Currently using placeholder images - need branded assets before store submission

## File Structure
```
Spoken.App_1.0.0.0_Test/
├── Spoken.App_1.0.0.0_x64.msix          # Main package file
├── Add-AppDevPackage.ps1                 # Development installation script
├── Install.ps1                          # Installation helper
└── Add-AppDevPackage.resources/         # Localized resources
```

The app is now successfully packaged and ready for final testing and store asset creation!