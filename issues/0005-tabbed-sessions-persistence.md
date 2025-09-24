Title: Tabbed Sessions with Persistence
Status: Done
Labels: ui, persistence, enhancement

## Summary
Allow multiple passages open concurrently as tabs; restore tabs and their state on application restart.

## Rationale
Users often study multiple passages; persistence improves continuity and perceived professionalism.

## Requirements
- Tab bar above content (add/remove/reorder not required initially; close + add new sufficient).
- Each tab stores: passage reference, translation, custom title (if provided), formatted HTML cache.
- On exit, serialize tab state to local app data directory.
- On start, load previous session; if empty, create a single blank tab.

## Acceptance Criteria
- [ ] Open >1 tab and switch without data loss.
- [ ] Close a tab; focus moves to nearest remaining tab logically.
- [ ] Restart app: previously open tabs reappear with correct content & active tab.
- [ ] Serialization tolerant of schema evolution (version field in JSON payload).
- [ ] Graceful handling if persisted file is corrupt (fallback to single blank tab).

## Implementation Notes
- Introduce `TabSession` model and `SessionStore` service.
- Use JSON file (e.g., `%LocalAppData%/Spoken/session.json`).
- Defer full MVVM: minimal event-driven code-behind acceptable initially, but design for ViewModel migration.

## Out of Scope (Future)
- Drag & drop reorder.
- Named workspaces.
