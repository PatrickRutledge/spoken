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
    private static readonly Regex Chapter = new(@"^\\c\s+(\d+)", RegexOptions.Compiled);
    private static readonly Regex Verse = new(@"^\\v\s+(\d+)\s+(.*)$", RegexOptions.Compiled);
    private static readonly Regex Id = new(@"^\\id\s+(\S+)", RegexOptions.Compiled);

    public UsfmZipTextSource(string? versionsDir = null)
    {
        _versionsDir = versionsDir ?? Path.Combine(Directory.GetCurrentDirectory(), "versions");
    }

    public async IAsyncEnumerable<Verse> GetVersesAsync(string translationCode, string book, int? chapterStart, int? verseStart, int? chapterEnd, int? verseEnd, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        var zipPath = FindZipForTranslation(translationCode);
        if (zipPath is null)
        {
            // Fallback single verse to avoid hard failure
            yield return new Verse { Book = book, Chapter = chapterStart ?? 1, Number = verseStart ?? 1, Text = $"[Missing translation: {translationCode}]", IsPoetryHint = false };
            yield break;
        }

        var targetUsfm = BookMaps.NameToUsfm.TryGetValue(book, out var usfmCode) ? usfmCode : null;
        if (targetUsfm is null)
        {
            yield return new Verse { Book = book, Chapter = chapterStart ?? 1, Number = verseStart ?? 1, Text = $"[Unknown book: {book}]", IsPoetryHint = false };
            yield break;
        }

        using var zip = ZipFile.OpenRead(zipPath);
        foreach (var entry in zip.Entries)
        {
            if (!entry.FullName.EndsWith(".usfm", StringComparison.OrdinalIgnoreCase)) continue;
            using var reader = new StreamReader(entry.Open(), Encoding.UTF8, true);
            string? line;
            string? currentId = null;
            int currentChapter = 0;
            bool poetry = false;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (ct.IsCancellationRequested) yield break;
                if (line.Length == 0) continue;
                var mId = Id.Match(line);
                if (mId.Success)
                {
                    currentId = mId.Groups[1].Value.Trim();
                    poetry = false;
                    continue;
                }
                if (currentId is null) continue;

                // only process target book
                if (!currentId.StartsWith(targetUsfm, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (line.StartsWith("\\q")) { poetry = true; continue; }
                if (line.StartsWith("\\p")) { poetry = false; continue; }

                var mC = Chapter.Match(line);
                if (mC.Success)
                {
                    currentChapter = int.Parse(mC.Groups[1].Value);
                    continue;
                }
                var mV = Verse.Match(line);
                if (mV.Success)
                {
                    int vnum = int.Parse(mV.Groups[1].Value);
                    string vtext = mV.Groups[2].Value.Trim();
                    if (WithinRange(currentChapter, vnum, chapterStart, verseStart, chapterEnd, verseEnd))
                    {
                        yield return new Verse { Book = book, Chapter = currentChapter, Number = vnum, Text = vtext, IsPoetryHint = poetry };
                    }
                }
            }
        }
    }

    private string? FindZipForTranslation(string translationCode)
    {
        if (!Directory.Exists(_versionsDir)) return null;
        var files = Directory.GetFiles(_versionsDir, "*.zip", SearchOption.TopDirectoryOnly);
        if (files.Length == 0) return null;
        string code = translationCode.ToLowerInvariant();
        // simple heuristics
        if (code.Contains("kjv") || code == "kjv")
            return files.FirstOrDefault(f => Path.GetFileName(f).ToLowerInvariant().Contains("kjv"));
        if (code.Contains("asv") || code == "asv")
            return files.FirstOrDefault(f => Path.GetFileName(f).ToLowerInvariant().Contains("asv"));
        if (code.Contains("web") || code == "web")
            return files.FirstOrDefault(f => Path.GetFileName(f).ToLowerInvariant().Contains("web"));
        // fallback any
        return files.FirstOrDefault();
    }

    private static bool WithinRange(int chapter, int verse, int? cStart, int? vStart, int? cEnd, int? vEnd)
    {
        if (cStart is null) return true;
        int cs = cStart ?? 1, vs = vStart ?? 1, ce = cEnd ?? cs, ve = vEnd ?? vs;
        if (chapter < cs || chapter > ce) return false;
        if (cs == ce)
        {
            return verse >= vs && verse <= ve;
        }
        if (chapter == cs) return verse >= vs;
        if (chapter == ce) return verse <= ve;
        return true;
    }
}
