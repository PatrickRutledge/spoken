using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace Spoken.Core;

public static class BookMaps
{
    // Map human book names/abbrevs to USFM 3-letter codes
    public static readonly Dictionary<string, string> NameToUsfm = new(StringComparer.OrdinalIgnoreCase)
    {
        {"Genesis","GEN"},{"Gen","GEN"},
        {"Exodus","EXO"},{"Exo","EXO"},
        {"Leviticus","LEV"},{"Lev","LEV"},
        {"Numbers","NUM"},{"Num","NUM"},
        {"Deuteronomy","DEU"},{"Deut","DEU"},{"Deu","DEU"},
        {"Joshua","JOS"},{"Josh","JOS"},
        {"Judges","JDG"},{"Judg","JDG"},
        {"Ruth","RUT"},
        {"1 Samuel","1SA"},{"1Sam","1SA"},{"I Samuel","1SA"},{"1Sa","1SA"},
        {"2 Samuel","2SA"},{"2Sam","2SA"},{"II Samuel","2SA"},{"2Sa","2SA"},
        {"1 Kings","1KI"},{"1Kgs","1KI"},{"I Kings","1KI"},
        {"2 Kings","2KI"},{"2Kgs","2KI"},{"II Kings","2KI"},
        {"1 Chronicles","1CH"},{"1Chr","1CH"},
        {"2 Chronicles","2CH"},{"2Chr","2CH"},
        {"Ezra","EZR"},
        {"Nehemiah","NEH"},{"Neh","NEH"},
        {"Esther","EST"},{"Esth","EST"},
        {"Job","JOB"},
        {"Psalms","PSA"},{"Psalm","PSA"},{"Ps","PSA"},
        {"Proverbs","PRO"},{"Prov","PRO"},{"Pr","PRO"},
        {"Ecclesiastes","ECC"},{"Eccl","ECC"},{"Ecc","ECC"},
        {"Song of Solomon","SNG"},{"Song","SNG"},{"Song of Songs","SNG"},{"Canticles","SNG"},
        {"Isaiah","ISA"},{"Isa","ISA"},
        {"Jeremiah","JER"},{"Jer","JER"},
        {"Lamentations","LAM"},{"Lam","LAM"},
        {"Ezekiel","EZK"},{"Ezek","EZK"},
        {"Daniel","DAN"},{"Dan","DAN"},
        {"Hosea","HOS"},{"Hos","HOS"},
        {"Joel","JOL"},{"Joe","JOL"},
        {"Amos","AMO"},
        {"Obadiah","OBA"},{"Obad","OBA"},
        {"Jonah","JON"},
        {"Micah","MIC"},{"Mic","MIC"},
        {"Nahum","NAM"},{"Nah","NAM"},
        {"Habakkuk","HAB"},{"Hab","HAB"},
        {"Zephaniah","ZEP"},{"Zeph","ZEP"},
        {"Haggai","HAG"},{"Hag","HAG"},
        {"Zechariah","ZEC"},{"Zech","ZEC"},
        {"Malachi","MAL"},{"Mal","MAL"},
        {"Matthew","MAT"},{"Matt","MAT"},
        {"Mark","MRK"},{"Mk","MRK"},
        {"Luke","LUK"},{"Lk","LUK"},
        {"John","JHN"},{"Jn","JHN"},{"Jhn","JHN"},
        {"Acts","ACT"},
        {"Romans","ROM"},{"Rom","ROM"},
        {"1 Corinthians","1CO"},{"1Cor","1CO"},
        {"2 Corinthians","2CO"},{"2Cor","2CO"},
        {"Galatians","GAL"},{"Gal","GAL"},
        {"Ephesians","EPH"},{"Eph","EPH"},
        {"Philippians","PHP"},{"Phil","PHP"},
        {"Colossians","COL"},{"Col","COL"},
        {"1 Thessalonians","1TH"},{"1Thess","1TH"},
        {"2 Thessalonians","2TH"},{"2Thess","2TH"},
        {"1 Timothy","1TI"},{"1Tim","1TI"},
        {"2 Timothy","2TI"},{"2Tim","2TI"},
        {"Titus","TIT"},
        {"Philemon","PHM"},{"Phlm","PHM"},
        {"Hebrews","HEB"},{"Heb","HEB"},
        {"James","JAS"},{"Jas","JAS"},
        {"1 Peter","1PE"},{"1Pet","1PE"},
        {"2 Peter","2PE"},{"2Pet","2PE"},
        {"1 John","1JN"},{"1Jn","1JN"},
        {"2 John","2JN"},{"2Jn","2JN"},
        {"3 John","3JN"},{"3Jn","3JN"},
        {"Jude","JUD"},
        {"Revelation","REV"},{"Rev","REV"}
    };
}

public class UsfmZipTextSource : ITextSource
{
    private readonly string _versionsDir;
    private readonly string _catalogCacheDir;
    private static readonly Regex Chapter = new(@"^\\c\s+(\d+)", RegexOptions.Compiled);
    private static readonly Regex Verse = new(@"^\\v\s+(\d+)\s+(.*)$", RegexOptions.Compiled);
    private static readonly Regex Id = new(@"^\\id\s+(\S+)", RegexOptions.Compiled);
    private static readonly Regex PoetryMarker = new(@"^\\q(\d?)\s*(.*)", RegexOptions.Compiled);
    private static readonly Regex ParagraphMarker = new(@"^\\[pm]\s*(.*)", RegexOptions.Compiled);
    private static readonly Regex FootnotePattern = new(@"\\f\s+[^\\]*?\\f\*", RegexOptions.Compiled);
    private static readonly Regex CrossRefPattern = new(@"\\x\s+[^\\]*?\\x\*", RegexOptions.Compiled);
    private static readonly Regex StrongsPattern = new(@"\\w\s+([^\\|]*)\|[^\\]*?\\w\*", RegexOptions.Compiled);
    private static readonly Regex WordPattern = new(@"\\w\s+([^\\]*?)\\w\*", RegexOptions.Compiled);
    private static readonly Regex AddPattern = new(@"\\add\s+([^\\]*?)\\add\*", RegexOptions.Compiled);
    private static readonly Regex NdPattern = new(@"\\nd\s+([^\\]*?)\\nd\*", RegexOptions.Compiled);
    private static readonly Regex SectionHeadingPattern = new(@"^\\s\d?\s+.*", RegexOptions.Compiled);
    private static readonly Regex IntroPattern = new(@"^\\i[sp]\d?\s+.*", RegexOptions.Compiled);
    private static readonly Regex TitlePattern = new(@"^\\(mt|h|toc)\d?\s+.*", RegexOptions.Compiled);
    private static readonly Regex HeaderPattern = new(@"^\\(ide|rem|cp|ca|va|vp)\s+.*", RegexOptions.Compiled);

    public UsfmZipTextSource(string? versionsDir = null, string? catalogCacheDir = null)
    {
        _versionsDir = versionsDir ?? Path.Combine(Directory.GetCurrentDirectory(), "versions");
        _catalogCacheDir = catalogCacheDir ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Spoken", "translations");
    }

    public async IAsyncEnumerable<Verse> GetVersesAsync(string translationCode, string book, int? chapterStart, int? verseStart, int? chapterEnd, int? verseEnd, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        var zipPath = FindZipForTranslation(translationCode);
        if (zipPath is null)
        {
            // Fallback single verse to avoid hard failure
            yield return new Verse { Book = book, Chapter = chapterStart ?? 1, Number = verseStart ?? 1, Text = $"[Missing translation: {translationCode}]", IsPoetryHint = false, PoetryLevel = 0 };
            yield break;
        }

        // The book parameter is already a USFM code (e.g., "JOS", "1KI")
        // We don't need to look it up - just use it directly
        var targetUsfm = book;

        using var zip = ZipFile.OpenRead(zipPath);
        foreach (var entry in zip.Entries)
        {
            if (!entry.FullName.EndsWith(".usfm", StringComparison.OrdinalIgnoreCase)) continue;
            using var reader = new StreamReader(entry.Open(), Encoding.UTF8, true);
            string? line;
            string? currentId = null;
            int currentChapter = 0;
            int poetryLevel = 0;
            var versePendingLines = new List<string>();
            int? currentVerseNum = null;
            bool currentVerseIsNewParagraph = false;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (ct.IsCancellationRequested) yield break;
                line = line.Trim();
                if (line.Length == 0) continue;

                // Skip section headings, introductory material, titles, and headers
                if (SectionHeadingPattern.IsMatch(line) || IntroPattern.IsMatch(line) || 
                    TitlePattern.IsMatch(line) || HeaderPattern.IsMatch(line))
                    continue;

                var mId = Id.Match(line);
                if (mId.Success)
                {
                    // Flush any pending verse
                    if (currentVerseNum.HasValue && versePendingLines.Count > 0)
                    {
                        if (WithinRange(currentChapter, currentVerseNum.Value, chapterStart, verseStart, chapterEnd, verseEnd))
                        {
                            yield return CreateVerse(book, currentChapter, currentVerseNum.Value, versePendingLines, poetryLevel, currentVerseIsNewParagraph);
                        }
                        versePendingLines.Clear();
                        currentVerseNum = null;
                        currentVerseIsNewParagraph = false; // Reset paragraph flag
                    }
                    
                    currentId = mId.Groups[1].Value.Trim();
                    poetryLevel = 0;
                    continue;
                }
                if (currentId is null) continue;

                // only process target book
                if (!currentId.StartsWith(targetUsfm, StringComparison.OrdinalIgnoreCase))
                    continue;

                var poetryMatch = PoetryMarker.Match(line);
                if (poetryMatch.Success)
                {
                    poetryLevel = poetryMatch.Groups[1].Success && int.TryParse(poetryMatch.Groups[1].Value, out var level) ? level : 1;
                    var content = poetryMatch.Groups[2].Value.Trim();
                    if (!string.IsNullOrEmpty(content))
                        versePendingLines.Add(content);
                    continue;
                }

                var paragraphMatch = ParagraphMarker.Match(line);
                if (paragraphMatch.Success)
                {
                    poetryLevel = 0;
                    currentVerseIsNewParagraph = true; // Current verse being built starts a new paragraph
                    var content = paragraphMatch.Groups[1].Value.Trim();
                    if (!string.IsNullOrEmpty(content))
                        versePendingLines.Add(content);
                    continue;
                }

                var mC = Chapter.Match(line);
                if (mC.Success)
                {
                    // Flush any pending verse before chapter change
                    if (currentVerseNum.HasValue && versePendingLines.Count > 0)
                    {
                        if (WithinRange(currentChapter, currentVerseNum.Value, chapterStart, verseStart, chapterEnd, verseEnd))
                        {
                            yield return CreateVerse(book, currentChapter, currentVerseNum.Value, versePendingLines, poetryLevel, currentVerseIsNewParagraph);
                        }
                        versePendingLines.Clear();
                        currentVerseNum = null;
                        currentVerseIsNewParagraph = false; // Reset paragraph flag
                    }
                    
                    currentChapter = int.Parse(mC.Groups[1].Value);
                    continue;
                }

                var mV = Verse.Match(line);
                if (mV.Success)
                {
                    // Flush previous verse if any
                    if (currentVerseNum.HasValue && versePendingLines.Count > 0)
                    {
                        if (WithinRange(currentChapter, currentVerseNum.Value, chapterStart, verseStart, chapterEnd, verseEnd))
                        {
                            yield return CreateVerse(book, currentChapter, currentVerseNum.Value, versePendingLines, poetryLevel, currentVerseIsNewParagraph);
                        }
                        versePendingLines.Clear();
                        currentVerseIsNewParagraph = false; // Reset paragraph flag after using it
                    }

                    currentVerseNum = int.Parse(mV.Groups[1].Value);
                    string vtext = mV.Groups[2].Value.Trim();
                    if (!string.IsNullOrEmpty(vtext))
                        versePendingLines.Add(vtext);
                    continue;
                }

                // Continuation line - add to current verse if we have one
                if (currentVerseNum.HasValue && !string.IsNullOrEmpty(line))
                {
                    versePendingLines.Add(line);
                }
            }

            // Flush final verse
            if (currentVerseNum.HasValue && versePendingLines.Count > 0)
            {
                if (WithinRange(currentChapter, currentVerseNum.Value, chapterStart, verseStart, chapterEnd, verseEnd))
                {
                    yield return CreateVerse(book, currentChapter, currentVerseNum.Value, versePendingLines, poetryLevel, currentVerseIsNewParagraph);
                }
            }
        }
    }

    private static Verse CreateVerse(string book, int chapter, int verseNum, List<string> lines, int poetryLevel, bool isNewParagraph = false)
    {
        // Join all lines and clean up
        var text = string.Join(" ", lines).Trim();
        
        // Strip footnotes and cross-references first
        text = FootnotePattern.Replace(text, "");
        text = CrossRefPattern.Replace(text, "");
        
        // Handle Strong's numbers - extract the word but remove the reference
        text = StrongsPattern.Replace(text, "$1");
        
        // Handle simple word markers
        text = WordPattern.Replace(text, "$1");
        
        // Handle added text (italics in print)
        text = AddPattern.Replace(text, "$1");
        
        // Handle name of deity markers
        text = NdPattern.Replace(text, "$1");
        
        // Remove any remaining backslash markers
        text = Regex.Replace(text, @"\\[a-z]+\*?", "", RegexOptions.IgnoreCase);
        
        // Normalize whitespace
        text = Regex.Replace(text, @"\s+", " ").Trim();
        
        return new Verse 
        { 
            Book = book, 
            Chapter = chapter, 
            Number = verseNum, 
            Text = text, 
            IsPoetryHint = poetryLevel > 0,
            PoetryLevel = poetryLevel,
            IsNewParagraph = isNewParagraph
        };
    }

    private string? FindZipForTranslation(string translationCode)
    {
        var allDirs = new List<string>();
        
        // Check bundled versions directory first
        if (Directory.Exists(_versionsDir))
            allDirs.Add(_versionsDir);
            
        // Check catalog cache directory second
        if (Directory.Exists(_catalogCacheDir))
            allDirs.Add(_catalogCacheDir);
            
        if (allDirs.Count == 0) return null;
        
        foreach (var dir in allDirs)
        {
            var files = Directory.GetFiles(dir, "*.zip", SearchOption.TopDirectoryOnly);
            if (files.Length == 0) continue;
            
            string code = translationCode.ToLowerInvariant();
            
            // Try exact filename match first
            var exactMatch = files.FirstOrDefault(f => 
                string.Equals(Path.GetFileNameWithoutExtension(f), code, StringComparison.OrdinalIgnoreCase));
            if (exactMatch != null) return exactMatch;
            
            // Try partial matches
            if (code.Contains("kjv") || code == "kjv")
            {
                // Prioritize the cleanest KJV versions in order of preference
                var cleanestKjv = files.FirstOrDefault(f => Path.GetFileName(f).ToLowerInvariant().Contains("engkjvcpb"));
                if (cleanestKjv != null) return cleanestKjv;
                
                var kjv2006 = files.FirstOrDefault(f => Path.GetFileName(f).ToLowerInvariant().Contains("kjv2006"));
                if (kjv2006 != null) return kjv2006;
                
                // Fall back to any other KJV version as last resort
                var match = files.FirstOrDefault(f => Path.GetFileName(f).ToLowerInvariant().Contains("kjv"));
                if (match != null) return match;
            }
            if (code.Contains("asv") || code == "asv")
            {
                var match = files.FirstOrDefault(f => Path.GetFileName(f).ToLowerInvariant().Contains("asv"));
                if (match != null) return match;
            }
            // WEB and other translations removed - only support KJV and ASV
            
            // Fallback to first file in this directory
            if (files.Length > 0) return files[0];
        }
        
        return null;
    }

    private static int Cmp(int a1, int a2, int b1, int b2)
        => a1 != b1 ? a1.CompareTo(b1) : a2.CompareTo(b2);

    private static bool WithinRange(int chapter, int verse, int? cStart, int? vStart, int? cEnd, int? vEnd)
    {
        if (cStart is null || vStart is null) return false;

        int cs = cStart.Value, vs = vStart.Value;
        int ce = cEnd ?? cs,    ve = vEnd ?? vs;

        // Normalize if the inputs are reversed
        if (Cmp(cs, vs, ce, ve) > 0) { (cs, vs, ce, ve) = (ce, ve, cs, vs); }

        return Cmp(cs, vs, chapter, verse) <= 0 && Cmp(chapter, verse, ce, ve) <= 0;
    }
}
