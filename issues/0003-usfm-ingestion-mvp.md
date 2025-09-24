Title: USFM Ingestion MVP
Status: Done
Labels: usfm, ingestion, completed

## Summary
Initial loader reads zipped USFM sources (KJV, ASV, WEB) and streams verses for a passage range.

## Context
Foundation for all formatting/export. Currently supports basic markers (\id, \c, \v, \p, \q).

## Work Completed
- Implemented `UsfmZipTextSource`.
- Added basic poetry hint via \q.
- Range filtering for chapters/verses.

## Acceptance Criteria
- [x] Can request discrete passages (single chapter or chapter:verse ranges) across included translations.
- [x] Handles missing verses gracefully (skips, no crash).
- [x] Performance acceptable (<1s typical passage load on desktop).

## Follow Ups
- See Issue 0006 for advanced parsing.
