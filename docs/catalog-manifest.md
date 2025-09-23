# Catalog Manifest (Public-Domain Translations)

Purpose: Define a simple JSON manifest hosted on a CDN/GitHub Pages that the app downloads to discover available translations. Each entry points to a downloadable package (USFM zip, OSIS XML, or JSON pack) and includes attribution and checksum.

Example manifest (catalog.json):

[
  {
    "code": "eng-kjv",
    "name": "King James Version",
    "language": "en",
    "license": "Public Domain",
    "format": "usfm-zip",
    "url": "https://examplecdn.com/translations/eng-kjv_usfm.zip",
    "sizeBytes": 5800000,
    "checksum": {
      "algo": "sha256",
      "value": "<hex>"
    },
    "attribution": "KJV text is public domain.",
    "books": 66
  },
  {
    "code": "eng-web",
    "name": "World English Bible",
    "language": "en",
    "license": "Public Domain",
    "format": "usfm-zip",
    "url": "https://examplecdn.com/translations/eng-web_usfm.zip",
    "sizeBytes": 6200000,
    "checksum": { "algo": "sha256", "value": "<hex>" },
    "attribution": "WEB is public domain.",
    "books": 66
  }
]

Format values:
- usfm-zip: Zip contains .usfm files per book
- osis-xml: Single OSIS XML file
- json-pack: Our JSON/JSONL format

Notes:
- Host the manifest at a stable URL. The app caches it and supports a refresh action.
- Each package should be immutable by versioned URL or include a version field.
- Provide license/attribution text for display in the footer.
