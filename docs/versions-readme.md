# Input formats supported
- USFM (preferred): one .usfm file per book, zipped per translation (e.g., eng-kjv_usfm.zip)
- Currently focusing on high-quality USFM sources for scholarly accuracy

Currently bundled translations
- versions/
  - engkjvcpb_usfm.zip (KJV - Cambridge Paragraph Bible edition)
  - eng-asv_usfm.zip (ASV - American Standard Version)

Bundling strategy
- Bundle only KJV and ASV within the app package for offline-first operation
- Focus on quality over quantity - each translation is carefully vetted
- Future translations will be added based on user requests and technical feasibility (Developer Notes)

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

USFM ingestion notes - Scholarly Formatting Standards
- **Paragraph Structure**: We strictly honor USFM paragraph markers (\p, \m) to preserve biblical manuscript traditions
  - Paragraph breaks may occur mid-sentence to maintain theological thought units
  - Each paragraph represents a complete literary or thematic unit as determined by biblical scholars
  - This matches the formatting found in academic study Bibles and scholarly editions
- **Text Processing**: 
  - Remove chapter/verse numbers and editorial headings
  - Paragraphs: justified, first-line indent, one blank line spacing
  - Poetry: preserve USFM poetry markers (\q, \q1, \q2) with appropriate indentation
  - Small caps for the divine name (e.g., LORD)
  - Capitalize pronouns for Deity (He, Him, His, etc.) in English
  - Smart quotes and em-dash normalization
- **Why Mid-Sentence Breaks**: USFM preserves Hebrew and Greek manuscript structure where thematic shifts, speakers, or narrative actions create natural divisions that don't always align with English sentence structure

Attribution
- Provide a short distribution/attribution line per translation (displayed in HTML/PDF footer).

Catalog manifest
- See docs/catalog-manifest.md for the JSON schema used by the download catalog.

Do we commit the versions/ folder?
- Public-domain development copies can be kept locally. Avoid committing large binary zip files to keep the repo light.
- Preferred: Host in release assets or a CDN/Git LFS; include only sample/small test packs in the repo.
