Title: Catalog Download & Version Management
Status: Done
Labels: catalog, networking, translations

## Summary
Implement remote catalog fetch for additional public-domain translations with attribution metadata and local caching.

## Requirements
- Catalog manifest (JSON) fetched from configurable endpoint.
- Display available translations (name, language, short description, license/attribution snippet).
- Download zip on demand; verify checksum; store under a managed directory structure.
- Provide uninstall (remove translation) and refresh catalog.
- Handle offline mode gracefully (show cached translations, message if catalog unavailable).

## Acceptance Criteria
- [ ] New translation appears after download without app restart.
- [ ] Checksum mismatch triggers retry & user-visible error.
- [ ] Attribution text accessible from an About / Info panel.
- [ ] Network failures do not crash app; fallback path works.

## Implementation Notes
- Use HttpClient with ETag caching.
- Maintain local index file mapping translation code -> version/checksum.
