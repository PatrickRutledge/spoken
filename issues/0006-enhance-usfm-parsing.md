Title: Enhance USFM Parsing & Formatting
Status: Done
Labels: usfm, parsing, formatting

## Summary
Improve fidelity of USFM ingestion: multi-line poetry, footnotes, cross-references, paragraph heuristics, and divine name/small caps accuracy.

## Requirements
- Support multi-level poetry markers (\q, \q1, \q2) with indentation in HTML & PDF.
- Strip or optionally display footnotes (\f ... \f*), cross-refs (\x ... \x*), Word list/glossary markers.
- Improved paragraph detection: treat \p, \m, \q# transitions appropriately.
- Handle chapter introductions (\is, \ip) by excluding or optional future flag.
- Normalize whitespace across wrapped lines.

## Acceptance Criteria
- [ ] Complex Psalms with nested \q# markers render visually indented.
- [ ] Footnotes removed by default (no leftover markers or brackets).
- [ ] No orphan markers appear in output HTML.
- [ ] Performance does not degrade (>2x) on large books (e.g., Psalms) vs. current parser.

## Implementation Notes
- Expand tokenizer to recognize begin/end paired markers.
- Consider constructing an intermediate AST before formatting.
