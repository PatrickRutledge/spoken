Title: Create MAUI App Skeleton (Windows)
Status: Done (Phase 1)
Labels: maui, ui, completed

## Summary
Bootstrap a Windows-only MAUI shell integrating core library: passage input, translation picker, preview, PDF export.

## Context
Early interactive validation of formatting & export before investing in multi-platform complexity.

## Work Completed
- Added `Spoken.App` MAUI project limited to Windows target.
- Implemented `MainPage` UI with basic controls and WebView preview.
- Integrated PDF export button.

## Acceptance Criteria
- [x] App builds & runs on Windows (.NET 8) with live formatting.
- [x] PDF export triggers and writes file to user-selected location / default path.
- [x] Styling uses primary color #0A5BCB.

## Follow Ups
- Tabs & session persistence (Issue 0005).
- MVVM refactor (future separate issue).
