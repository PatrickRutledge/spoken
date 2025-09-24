Title: Integrate QuestPDF for PDF Export
Status: Done
Labels: pdf, formatting, completed

## Summary
Add QuestPDF to generate paginated Letter-sized PDFs with footer (Passage · Page n of X) per PRD.

## Context
High-fidelity export is a core differentiator vs. simple copy/paste. QuestPDF chosen for .NET 8 compatibility and fluent API.

## Work Completed
- Added QuestPDF dependency to `Spoken.Core`.
- Implemented `PdfExporter.ExportLetterPdf` with dynamic footer.
- Resolved community license initialization.

## Acceptance Criteria
- [x] Export produces a multi-page PDF for long passages.
- [x] Footer includes passage reference and page numbering.
- [x] No license exceptions at runtime.

## Follow Ups
- Add styling enhancements (poetry indentation, small caps in PDF) after richer HTML pipeline is finalized.
