Title: Packaging & Microsoft Store Submission Prep
Status: Done
Labels: packaging, store, release

## Summary
Prepare application for Microsoft Store submission: MSIX packaging, assets, privacy policy, telemetry stance, and automated build pipeline.

## Requirements
- MSIX packaging configuration (identity matches stored IDs).
- Provide required image assets (Square44, 50, 150, 310 etc.) and high-quality logo integration.
- Privacy policy markdown + link in Settings/About.
- App capability review: ensure minimal capabilities; removal of unneeded declarations.
- Optional pipeline (GitHub Actions) producing signed (if certificate provided) MSIX artifact.

## Acceptance Criteria
- [ ] Local MSIX build installs & launches successfully.
- [ ] WACK test passes (or documented justifications for warnings).
- [ ] Store checklist doc updated with asset SHA hashes.
- [ ] Privacy policy accessible within app UI.

## Implementation Notes
- Use `dotnet publish -f net8.0-windows10.0.22621.0 -c Release -p:WindowsPackageType=MSIX`.
- Consider separate issue later for code signing automation.
