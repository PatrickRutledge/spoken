# Versions Packaging (Developer Notes)

Input formats supported
- USFM (preferred): one .usfm file per book, zipped per translation (e.g., eng-kjv_usfm.zip)
- OSIS XML: single XML per translation
- JSON/CSV: simple fallback packs if USFM/OSIS not available

Folder layout (repo)
- versions/  # development copies only (don’t commit licensed content)
  - eng-kjv2006_usfm.zip
  - eng-asv_usfm.zip
  - eng-web-c_usfm.zip
  - engwebp_usfm.zip
  - deu1912_usfm.zip
  - frajnd_usfm.zip
  - sparvg_usfm.zip

Bundling strategy
- Bundle KJV, ASV, WEB within the app package for offline-first.
- All others via catalog downloads. Keep the app size lean.

USFM ingestion notes
- We honor paragraph/poetry markers but render prose:
  - Remove chapter/verse numbers and editorial headings
  - Paragraphs: justified, first-line indent, one blank line spacing
  - Poetry: keep internal line breaks within paragraph blocks for flow
  - Small caps for the divine name (e.g., LORD)
  - Capitalize pronouns for Deity (He, Him, His, etc.) in English
  - Smart quotes and em-dash normalization

Attribution
- Provide a short distribution/attribution line per translation (displayed in HTML/PDF footer).

Catalog manifest
- See docs/catalog-manifest.md for the JSON schema used by the download catalog.

Do we commit the versions/ folder?
- Public-domain development copies can be kept locally. Avoid committing large binary zip files to keep the repo light.
- Preferred: Host in release assets or a CDN/Git LFS; include only sample/small test packs in the repo.
