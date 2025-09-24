# Product Requirements Document (PRD)

Title: Spoken Bible — Windows (Microsoft Store) first; iOS later
Owner: You (Product), Copilot (initial draft)
Last updated: 2025-09-23
Status: Draft for review

---

## 1) Summary
A modern, fast, and accessible Bible reading app for Windows distributed via the Microsoft Store. Focus on distraction-free reading, fast navigation, robust offline support, and a clean Fluent UI that feels native on Windows 11. The app initially ships with two carefully curated public-domain translations (KJV and ASV) using scholarly USFM formatting standards. Additional translations will be added in future versions based on user requests and technical feasibility.

Primary platform now: Windows (Microsoft Store)
Secondary platform later: iOS
Primary tech: .NET MAUI (Windows via WinUI 3), .NET 8, MSIX packaging, SQLite (with FTS5) for offline content and search, QuestPDF for PDF export

---

## 2) Goals and Non-Goals
### Goals
- Deliver a polished Windows Bible app (Microsoft Store) with:
  - Fast book/chapter/verse navigation
  - Offline reading (downloaded translations)
  - Full-text search with highlighting
  - Bookmarks, highlights, and notes
  - Reading themes (light/dark, font size, font family)
  - Keyboard-first and screen-reader-friendly UX
- Ship in 8–10 weeks with a shippable MVP and clear roadmap.
- Utilize an architecture that enables future iOS delivery with minimal rework.

### Non-Goals (for MVP)
- Cloud sync (user accounts) — planned for a later phase
- Licensed, paywalled translations (e.g., NIV, ESV) — later with publisher agreements
- Complex social features (sharing notes, groups)
- Advanced reading plans — MVP includes a simple verse-of-the-day with a basic schedule

---

## 3) Target Users and Personas
- Reader: Wants a clean, reliable Bible app for study and devotional reading on Windows.
- Student/Teacher: Needs fast search, bookmarking, and notes for lessons/sermons.
- Accessibility-first users: Requires keyboard navigation, high contrast, and Narrator support.

---

## 4) Problem Statement
Windows users lack a deeply native-feeling Bible app in the Microsoft Store that balances performance, offline support, accessibility, and a modern Fluent UI.

---

## 5) Scope
MVP scope (Windows) includes:
- Translations: Bundle KJV (Cambridge Paragraph Bible) and ASV inside the app for immediate offline use. These versions use scholarly USFM paragraph structure that preserves biblical manuscript traditions. Additional translations will be considered for future versions based on user feedback and requests.
- Passage Selection & Formatting: User selects a version and a passage (supports whole book and range Book Chapter:Verse – Chapter:Verse). On click, app generates a prose-formatted view: removes chapter/verse/headings, outputs justified paragraphs with one blank line between paragraphs, indented first-line. Poetry rendered as paragraph blocks with preserved internal line breaks for flow.
- HTML Output Tabs: Each formatted output opens in a new in-app HTML tab. Tabs persist across app restarts.
- Export: Export to PDF with footer containing the passage reference and page numbering as "Page n of X"; numbering restarts for each export. Default to Letter size. If an e-reader export is included later (EPUB/MOBI), use device defaults; out-of-scope for MVP but design kept compatible.
- Fonts & Theme: Minimal monochromatic blue theme (#0A5BCB). Optionally allow serif choices (Georgia, EB Garamond, Merriweather, Lora, Crimson Pro) and Inter; not required for MVP.
- Search (optional for MVP): Full-text search across downloaded translations (SQLite FTS5) — can be deferred if schedule requires; formatting workflow is the core.
- Offline: Translation packages stored locally; first-run shows available built-in version and offers download of others from the catalog.
- Accessibility: Keyboard shortcuts, high-contrast, Narrator labels, large text.
- Stability: Crash-free, smooth formatting and rendering.

Out of scope for MVP (future):
- Cross-device sync; sign-in
- Licensed translations and audio
- Parallel translations and interlinear views
- Advanced reading plans/trackers and reminders
- Rich footnotes/cross-references rendering

---

## 6) User Stories (MVP)
- As a reader, I can quickly navigate to John 3:16 by keyboard so I can reference a verse in class.
- As a student, I can search for "love" and see ranked results with snippets to find relevant passages.
- As a reader, I can change font size and theme so night reading is comfortable.
- As a teacher, I can bookmark and highlight verses across multiple sessions offline.
- As an accessibility user, I can navigate the app fully via keyboard and Narrator.
- As a presenter, I can select a passage, click Format as Spoken, and get a prose-formatted HTML tab that persists and can be exported to PDF with a footer and page numbers.

---

## 7) Functional Requirements (MVP)
Reading & Navigation
- Book/chapter/verse browsing with back/forward navigation
- Quick jump (Ctrl+L) to enter references like "Jn 3:16" with fuzzy parsing
- Whole-book selection and verse-range selection (e.g., John 3:1–21)
- Recent passages list

Formatting (Spoken/Prose)
- Remove chapter numbers, verse numbers, and editorial headings
- Paragraph formatting: justified text, first-line indent, one blank line between paragraphs
- Poetry handling: keep as paragraph sections but preserve internal line breaks for flow
- Smart punctuation: smart quotes and em-dash normalization
- Small caps rendering for the divine name (e.g., LORD) where applicable
- Capitalize pronouns for Deity (He, Him, His, etc.) in English content
- Optional title/note field inserted as heading at top
- Paste-your-own-text input supported; treat as raw text (no reference parsing required)

HTML Output Tabs
- Each output opens in a new tab
- Tabs persist across app restarts (saved tab metadata and content cache)

Export
- PDF export with footer: "{Passage Reference} · Page n of X"
- Default page size: Letter; margins configurable (sane defaults)
- Page numbering restarts per export
- Future-friendly for e-reader export (e.g., EPUB); not required for MVP

Theming & Settings
- Minimal blue theme (#0A5BCB)
- Font size, line height; optional font family choices (Georgia, EB Garamond, Merriweather, Lora, Crimson Pro, Inter)
- Default translation selection; manage translation downloads

- Bundled PD translations in app: KJV (Cambridge Paragraph Bible), ASV
- **USFM Scholarly Formatting**: Paragraph breaks follow biblical manuscript traditions using the Unified Standard Format Markers (USFM) standard, preserving authentic theological thought units
- **Future Translation Strategy**: Additional translations will be added based on user requests. Users can contact us to request specific versions they'd like to see included
- Include short attribution line per translation in HTML/PDF footer

Accessibility
- Full keyboard navigation, focus states, and shortcuts
- Narrator/Screen reader labels and semantic headings
- High contrast support and scalable text

Performance
- Format and render typical chapter under 300 ms on a mid-range device
- Smooth scrolling/interaction in HTML tab

---

## 8) Non-Functional Requirements
- Reliability: Crash-free sessions ≥ 99.9% post-launch (30-day)
- Performance: Search returns first page under 800 ms for common queries
- Security: Local data encrypted at rest where feasible; safe file I/O
- Privacy: No telemetry by default; clear disclosures for any data collected
- Compatibility: Windows 10 21H2+ and Windows 11; x64 and ARM64

---

## 9) Windows-Specific Requirements (Microsoft Store)
Packaging & Distribution
- .NET 8, .NET MAUI (WinUI 3) packaged as MSIX
- App identity via Partner Center; flighting support for beta

Store Submission
- Category: Books & Reference
- IARC rating questionnaire completion (content is generally suitable for all ages)
- Privacy policy URL (even if minimal data)
- Store listing assets (screenshots, logo, description, keywords)
- Pricing: Free initial; optional IAP planned later

Design
- Fluent design system; native title bar; acrylic/mica backgrounds respectful of contrast
- NavigationView, CommandBar, teaching tips where helpful
- Respect system theme and accessibility settings

Quality & Validation
- Windows App Certification Kit (WACK) pass prior to submission
- Installer verifies MSIX signing; updates via Store handled automatically

Store Identity (for packaging & Partner Center)
- Package/Identity/Name: AetherForge.Spoken
- Package/Identity/Publisher: CN=80415444-5392-4904-8AC7-7511A51DFC7C
- Package/Properties/PublisherDisplayName: Aether Forge
- Package Family Name (PFN): AetherForge.Spoken_qrby07m9ype14
- Package SID: S-1-15-2-2736691906-1834129564-1175683458-3516803888-2464970110-1830282950-3729152391
- Store ID: 9PFMKD32PJ0R
- Store deep link: Available after live
- Web Store URL: Available after live

---

## 10) Architecture and Tech Choices
- Framework: .NET MAUI targeting Windows via WinUI 3; paves path to iOS
- Language: C#/.NET 8
- Data: SQLite with FTS5 (if search is included) for scripture and search; local user data tables for tabs cache and settings
- Content ingestion: Prefer USFM (zipped per translation) or OSIS; support simple JSON/CSV fallback. Normalize into internal model suitable for prose formatting. Respect poetry hints if available.
- Dependency injection: .NET built-in DI; MVVM for presentation
- Packaging: MSIX with app installer
- Export: QuestPDF (PDF with reliable footers and page X of Y)
- Testing: xUnit/MSTest for logic; Playwright or MAUI UITest for UI smoke

PDF Generation (QuestPDF)
- Template-driven document with:
  - Header (optional): user-provided title/note when present
  - Body: justified paragraphs with first-line indent and poetry line breaks
  - Footer: "{Passage Reference} · Page {n} of {X}" centered; page number restarts per export
- Defaults: Letter size; margins 0.75"; serif font stack (system or embedded) with small caps rendering for divine name
- Hyphenation: enabled where available; widows/orphans control to minimize single-line stragglers
- Integration: Export action transforms the same HTML/DOM model used for tabs into a QuestPDF layout model to ensure visual consistency

Alternative considered: WinUI 3 (Windows-only) for maximum native control now; rejected to avoid rework for iOS later. MAUI provides shared UI + platform renderers.

---

## 11) Data Model (MVP sketch)
- Table Translations(id, code, name, license, version)
- Table Books(id, translationId, canonicalOrder, name, abbrev)
- Table Verses(id, translationId, bookId, chapter, verse, text, isPoetryHint)
- FTS index on Verses(text, bookId, chapter, verse) (if search enabled in MVP)
- Table Tabs(id, createdAt, title, passageRef, translationCode, htmlCachePath)
- Table Settings(id, key, value)

---

## 12) Content & Licensing Plan
 Bundled: Include KJV, ASV, and WEB packaged with the app to ensure offline-first operation on first run.
 Downloadable: Additional PD versions (especially foreign-language) discoverable via curated catalog endpoint (e.g., ebible.org sources) and cached locally.
- Format sources: Accept USFM zip, OSIS XML, or JSON/CSV packs. Normalize at import and store localized attribution metadata.
- Later: Negotiate licenses for NIV (Biblica), ESV (Crossway), NKJV (HarperCollins Christian), etc.
- Audio (later): Explore PD recordings and commercial licenses with clear redistribution rights.
- Attribution: Include a short distribution/attribution line per translation in HTML/PDF footer.

---

## 12a) UI/UX Overview (MVP)
- Top bar: Passage range picker (supports whole book), Translation selector (KJV/ASV/WEB initially, plus downloaded items), Title/Note field
- Body: Paste-your-own-text area (optional), Format as Spoken button
- Tabs bar: Each formatted HTML output appears as a persistent tab with close controls (tabs survive app restarts)
- Export: Per-tab Export to PDF action with footer "{Passage} · Page n of X"; default Letter size
- Theme: Minimal monochromatic blue (#0A5BCB), high-contrast compatible; serif body font by default; optional font choices (Georgia, EB Garamond, Merriweather, Lora, Crimson Pro, Inter) when enabled

---

## 12b) UI/UX Essentials (Controls & Flows)
Main window
- Top bar
  - Translation selector: KJV, ASV, WEB (bundled) + downloaded items
  - Passage picker: supports book dropdown, chapter/verse fields, and range; whole-book toggle
  - Title/Note field: optional heading for output
  - Format as Spoken button
- Body
  - Paste-your-own-text area (toggleable panel) for raw text formatting
- Tabs bar
  - Each formatted output appears as a tab with title (Title/Note or Passage Ref). Tabs persist across restarts.
  - Per-tab actions: Export PDF, Copy HTML, Close tab

Keyboard
- Ctrl+L: Quick jump to passage
- Ctrl+Enter: Format as Spoken
- Ctrl+P: Export PDF (focused tab)

Settings
- Fonts (optional selection list), line height, margins for PDF, theme
- Manage translations: view bundled, downloaded, and available catalog items

---

## 13) Accessibility (A11y)
- Keyboard shortcuts for navigation and search
- Focus order and visible focus states
- Semantic UI elements for Narrator; verse numbers as landmarks where appropriate
- High contrast and UI scaling; minimum 4.5:1 contrast

---

## 14) Localization & RTL
- App UI initially in English; architected for localization
- Scripture content supports multiple languages via translation packages
- Plan for RTL UI support (Arabic/Hebrew) in a later phase; ensure layout containers support mirroring

---

## 15) Privacy, Compliance, and Legal
- Collect minimal data; default: no telemetry
- If using crash reporting/analytics (later), add consent dialog and update privacy policy
- Provide a clear Privacy Policy URL and Support contact in Store listing
- Honor user data export/import; local-only by default

---

## 16) Monetization
- MVP: Free app; no ads
- Future: In-app purchases for premium translations; optional one-time “Support the project” donation
- Keep product catalog abstracted to map to Microsoft Store IAP now and Apple IAP later

---

## 17) Analytics & Metrics
- Store analytics (installs, ratings, crash rate) via Microsoft Partner Center
- Optional events later (if enabled with consent): search count, session length

Success metrics (Post-launch 90 days)
- ≥ 10,000 installs
- ≥ 4.6 average rating
- Crash-free sessions ≥ 99.9%
- D1 retention ≥ 30%, D30 ≥ 10%

---

## 18) Milestones & Timeline (Indicative)
Week 1–2: Design + content pipeline
- Finalize UX wireframes, typography, color
- Build translation import to SQLite and FTS indexing

Week 3–5: Core app features
- Navigation, reader, settings, offline downloads
- Bookmarks/highlights/notes; search with FTS

Week 6–7: Polish & a11y
- Keyboard shortcuts, Narrator labels, perf tuning
- WACK pre-check; store assets prep

Week 8–9: Store submission
- Partner Center setup, MSIX signing, submission, review

Post-launch: Patch & roadmap
- Bugs, minor improvements, feedback triage

---

## 19) Acceptance Criteria (MVP – Windows)
- App launches and can format a chosen passage into prose HTML in ≤ 1s
- Navigate to any verse in ≤ 4 clicks or a single quick-jump input; support whole-book and range selection
- Generated HTML tabs persist after restart (at least last 5 tabs)
- PDF export renders justified paragraphs with footer "{Passage} · Page n of X", default Letter, and restarts numbering per export
- If search is included, "love" returns results < 800 ms for first page on KJV; otherwise, search is deferred without blocking MVP
- Passes WACK; approved in Microsoft Store

---

## 20) Risks & Mitigations
- Licensing complexity for popular translations → Start PD translations; design entitlement system early
- Performance with large FTS indexes → Use incremental indexing and pragmatic pagination; prebuilt indexes in packages
- A11y gaps → Engage Narrator testing early; include keyboard-only QA
- App review delays → Submit early to beta flight; maintain checklists

---

## 21) iOS Path (Future)
- Reuse .NET MAUI shared UI and data layer
- Replace Windows-specific services (file paths, window chrome) with platform services
- Use Apple IAP for premium translations; port entitlement layer
- iOS-specific a11y (VoiceOver), HIG alignment, TestFlight rollout

---

## 22) Open Questions
- Confirm initial PD translations bundled vs downloadable (KJV bundled; ASV, WEB downloadable at v1?)
- Deuterocanonical books policy per translation
- EPUB/e-reader export priority and device defaults (post-MVP)
- Donation/IAP timing for v1 vs v1.1
- Privacy policy domain and any org-specific legal/EULA text

---

## 23) Appendix
- Keyboard shortcuts (proposed):
  - Ctrl+L: Quick jump
  - Ctrl+F: Search
  - Ctrl+B: Bookmark
  - Ctrl+H: Highlight toggle
  - Ctrl+N: Note on verse
  - Ctrl+ +/-: Font size
- Reference: Confirm image sizes and counts for Microsoft Store assets in Partner Center at submission time.
