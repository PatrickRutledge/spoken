# Spoken

A Windows-first Bible app that formats Scripture into readable, justified prose with poetry preserved as line-broken blocks, small caps for the divine name, smart quotes, em dashes, and export to PDF with footer (Passage · Page n of X). Currently includes KJV and ASV translations, with plans to add more translations based on user requests.

- **Tech:** .NET 8, .NET MAUI (Windows via WinUI 3), QuestPDF
- **Core:** USFM ingestion with scholarly paragraph structure, prose formatter, PDF export
- **App:** Minimal blue theme (#0A5BCB), passage input, translation picker, preview, export

## USFM Formatting & Scholarly Structure

Spoken uses the **Unified Standard Format Markers (USFM)** standard for biblical text formatting. This means:

### Paragraph Structure
- **Paragraph breaks follow biblical manuscript traditions** rather than modern English conventions
- Breaks may occur mid-sentence to preserve **theological thought units** and **narrative structure**
- Each paragraph represents a complete **literary or thematic unit** as determined by biblical scholars
- This matches the formatting found in academic study Bibles and scholarly editions

### Why Paragraphs Break Mid-Sentence
The USFM standard preserves the structure of ancient Hebrew and Greek manuscripts:
- **Hebrew text structure** differs from modern English paragraph conventions
- Breaks mark **thematic shifts**, **speakers**, or **narrative actions** rather than grammatical sentences
- Original biblical texts didn't use modern punctuation - colons and periods were added by translators
- This scholarly approach helps readers understand the **original literary flow** of Scripture

### Translation Versions
- **KJV (King James Version):** Uses the Cambridge Paragraph Bible edition for clean, accurate text
- **ASV (American Standard Version):** Public domain translation with excellent accuracy
- **Future versions:** We plan to add more translations based on user requests and technical feasibility

## Documentation

See `docs/prd.md` for the product requirements and `docs/versions-readme.md` for translation packaging.
