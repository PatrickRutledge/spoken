Title: Passage Range Parser Enhancements
Status: Done
Labels: parsing, passages

## Summary
Extend passage parsing to handle multi-chapter ranges, whole-book selections, and edge cases (intro chapters, trailing punctuation).

## Requirements
- Support syntax like: `John 3-4`, `John 3:16-18`, `John 3:16-4:2`, `Obadiah`, `Psalm 1-3`.
- Validate out-of-order or invalid ranges with user-friendly error messages.
- Normalize canonical book names & common abbreviations (Gen, Ge, Gn -> Genesis).
- Provide structured `PassageRef` model with explicit start/end (book, chapter, verse?).

## Acceptance Criteria
- [ ] Unit tests cover representative range patterns (≥12 cases including invalid input).
- [ ] Parser rejects impossible references (e.g., John 22:999) gracefully.
- [ ] Whole-book load does not exceed acceptable time (<2s) on large books (Psalms) with current architecture.

## Implementation Notes
- Introduce a lightweight grammar or regex-based tokenizer feeding a validator referencing a canonical book metadata table.
