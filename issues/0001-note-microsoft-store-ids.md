Title: Record Microsoft Store IDs
Status: Done
Labels: docs, store, completed

## Summary
Capture Microsoft Store identity (Publisher ID, Name, reserved app name) in repository documentation for future packaging and submission.

## Context
Store identity information is required later for MSIX packaging, Partner Center submission, and automated CI metadata. Having it documented early prevents lookup delays.

## Work Completed
- Added `store-identity.md` documenting Publisher and reserved app details.
- Referenced within `prd.md` for traceability.

## Acceptance Criteria
- [x] A markdown doc exists with all required store identifiers.
- [x] PRD references where to find store IDs.
- [x] Build/packaging scripts (future) can read from a single canonical file.

## Follow Ups
- Integrate IDs into future `CI/CD` pipeline for automated manifest stamping.
