using System.Text.RegularExpressions;

namespace Spoken.Core;

public class PassageParser
{
    private static readonly Dictionary<string, (string FullName, string UsfmCode, int MaxChapters)> BookDatabase = new(StringComparer.OrdinalIgnoreCase)
    {
        // Old Testament
        {"Genesis", ("Genesis", "GEN", 50)}, {"Gen", ("Genesis", "GEN", 50)}, {"Ge", ("Genesis", "GEN", 50)}, {"Gn", ("Genesis", "GEN", 50)},
        {"Exodus", ("Exodus", "EXO", 40)}, {"Exo", ("Exodus", "EXO", 40)}, {"Ex", ("Exodus", "EXO", 40)},
        {"Leviticus", ("Leviticus", "LEV", 27)}, {"Lev", ("Leviticus", "LEV", 27)}, {"Le", ("Leviticus", "LEV", 27)}, {"Lv", ("Leviticus", "LEV", 27)},
        {"Numbers", ("Numbers", "NUM", 36)}, {"Num", ("Numbers", "NUM", 36)}, {"Nu", ("Numbers", "NUM", 36)}, {"Nm", ("Numbers", "NUM", 36)},
        {"Deuteronomy", ("Deuteronomy", "DEU", 34)}, {"Deut", ("Deuteronomy", "DEU", 34)}, {"Dt", ("Deuteronomy", "DEU", 34)},
        {"Joshua", ("Joshua", "JOS", 24)}, {"Josh", ("Joshua", "JOS", 24)}, {"Jos", ("Joshua", "JOS", 24)},
        {"Judges", ("Judges", "JDG", 21)}, {"Judg", ("Judges", "JDG", 21)}, {"Jg", ("Judges", "JDG", 21)},
        {"Ruth", ("Ruth", "RUT", 4)}, {"Ru", ("Ruth", "RUT", 4)},
        {"1 Samuel", ("1 Samuel", "1SA", 31)}, {"1Sam", ("1 Samuel", "1SA", 31)}, {"1Sa", ("1 Samuel", "1SA", 31)}, {"1 S", ("1 Samuel", "1SA", 31)},
        {"2 Samuel", ("2 Samuel", "2SA", 24)}, {"2Sam", ("2 Samuel", "2SA", 24)}, {"2Sa", ("2 Samuel", "2SA", 24)}, {"2 S", ("2 Samuel", "2SA", 24)},
        {"1 Kings", ("1 Kings", "1KI", 22)}, {"1Kgs", ("1 Kings", "1KI", 22)}, {"1Ki", ("1 Kings", "1KI", 22)}, {"1 K", ("1 Kings", "1KI", 22)},
        {"2 Kings", ("2 Kings", "2KI", 25)}, {"2Kgs", ("2 Kings", "2KI", 25)}, {"2Ki", ("2 Kings", "2KI", 25)}, {"2 K", ("2 Kings", "2KI", 25)},
        {"1 Chronicles", ("1 Chronicles", "1CH", 29)}, {"1Chr", ("1 Chronicles", "1CH", 29)}, {"1Ch", ("1 Chronicles", "1CH", 29)},
        {"2 Chronicles", ("2 Chronicles", "2CH", 36)}, {"2Chr", ("2 Chronicles", "2CH", 36)}, {"2Ch", ("2 Chronicles", "2CH", 36)},
        {"Ezra", ("Ezra", "EZR", 10)}, {"Ezr", ("Ezra", "EZR", 10)},
        {"Nehemiah", ("Nehemiah", "NEH", 13)}, {"Neh", ("Nehemiah", "NEH", 13)}, {"Ne", ("Nehemiah", "NEH", 13)},
        {"Esther", ("Esther", "EST", 10)}, {"Est", ("Esther", "EST", 10)}, {"Es", ("Esther", "EST", 10)},
        {"Job", ("Job", "JOB", 42)},
        {"Psalms", ("Psalms", "PSA", 150)}, {"Psalm", ("Psalms", "PSA", 150)}, {"Ps", ("Psalms", "PSA", 150)}, {"Psa", ("Psalms", "PSA", 150)},
        {"Proverbs", ("Proverbs", "PRO", 31)}, {"Prov", ("Proverbs", "PRO", 31)}, {"Pr", ("Proverbs", "PRO", 31)}, {"Pro", ("Proverbs", "PRO", 31)},
        {"Ecclesiastes", ("Ecclesiastes", "ECC", 12)}, {"Eccl", ("Ecclesiastes", "ECC", 12)}, {"Ecc", ("Ecclesiastes", "ECC", 12)}, {"Ec", ("Ecclesiastes", "ECC", 12)},
        {"Song of Solomon", ("Song of Solomon", "SNG", 8)}, {"Song", ("Song of Solomon", "SNG", 8)}, {"SS", ("Song of Solomon", "SNG", 8)}, {"SOS", ("Song of Solomon", "SNG", 8)},
        {"Isaiah", ("Isaiah", "ISA", 66)}, {"Isa", ("Isaiah", "ISA", 66)}, {"Is", ("Isaiah", "ISA", 66)},
        {"Jeremiah", ("Jeremiah", "JER", 52)}, {"Jer", ("Jeremiah", "JER", 52)}, {"Je", ("Jeremiah", "JER", 52)},
        {"Lamentations", ("Lamentations", "LAM", 5)}, {"Lam", ("Lamentations", "LAM", 5)}, {"La", ("Lamentations", "LAM", 5)},
        {"Ezekiel", ("Ezekiel", "EZK", 48)}, {"Ezek", ("Ezekiel", "EZK", 48)}, {"Eze", ("Ezekiel", "EZK", 48)}, {"Ez", ("Ezekiel", "EZK", 48)},
        {"Daniel", ("Daniel", "DAN", 12)}, {"Dan", ("Daniel", "DAN", 12)}, {"Da", ("Daniel", "DAN", 12)},
        {"Hosea", ("Hosea", "HOS", 14)}, {"Hos", ("Hosea", "HOS", 14)}, {"Ho", ("Hosea", "HOS", 14)},
        {"Joel", ("Joel", "JOL", 3)}, {"Joe", ("Joel", "JOL", 3)}, {"Jl", ("Joel", "JOL", 3)},
        {"Amos", ("Amos", "AMO", 9)}, {"Am", ("Amos", "AMO", 9)},
        {"Obadiah", ("Obadiah", "OBA", 1)}, {"Obad", ("Obadiah", "OBA", 1)}, {"Ob", ("Obadiah", "OBA", 1)},
        {"Jonah", ("Jonah", "JON", 4)}, {"Jon", ("Jonah", "JON", 4)}, {"Jnh", ("Jonah", "JON", 4)},
        {"Micah", ("Micah", "MIC", 7)}, {"Mic", ("Micah", "MIC", 7)}, {"Mi", ("Micah", "MIC", 7)},
        {"Nahum", ("Nahum", "NAM", 3)}, {"Nah", ("Nahum", "NAM", 3)}, {"Na", ("Nahum", "NAM", 3)},
        {"Habakkuk", ("Habakkuk", "HAB", 3)}, {"Hab", ("Habakkuk", "HAB", 3)}, {"Hb", ("Habakkuk", "HAB", 3)},
        {"Zephaniah", ("Zephaniah", "ZEP", 3)}, {"Zeph", ("Zephaniah", "ZEP", 3)}, {"Zp", ("Zephaniah", "ZEP", 3)},
        {"Haggai", ("Haggai", "HAG", 2)}, {"Hag", ("Haggai", "HAG", 2)}, {"Hg", ("Haggai", "HAG", 2)},
        {"Zechariah", ("Zechariah", "ZEC", 14)}, {"Zech", ("Zechariah", "ZEC", 14)}, {"Zc", ("Zechariah", "ZEC", 14)},
        {"Malachi", ("Malachi", "MAL", 4)}, {"Mal", ("Malachi", "MAL", 4)}, {"Ml", ("Malachi", "MAL", 4)},
        
        // New Testament
        {"Matthew", ("Matthew", "MAT", 28)}, {"Matt", ("Matthew", "MAT", 28)}, {"Mt", ("Matthew", "MAT", 28)},
        {"Mark", ("Mark", "MRK", 16)}, {"Mk", ("Mark", "MRK", 16)}, {"Mr", ("Mark", "MRK", 16)},
        {"Luke", ("Luke", "LUK", 24)}, {"Lk", ("Luke", "LUK", 24)}, {"Lu", ("Luke", "LUK", 24)},
        {"John", ("John", "JHN", 21)}, {"Jn", ("John", "JHN", 21)}, {"Jhn", ("John", "JHN", 21)},
        {"Acts", ("Acts", "ACT", 28)}, {"Ac", ("Acts", "ACT", 28)},
        {"Romans", ("Romans", "ROM", 16)}, {"Rom", ("Romans", "ROM", 16)}, {"Ro", ("Romans", "ROM", 16)},
        {"1 Corinthians", ("1 Corinthians", "1CO", 16)}, {"1Cor", ("1 Corinthians", "1CO", 16)}, {"1Co", ("1 Corinthians", "1CO", 16)}, {"1 C", ("1 Corinthians", "1CO", 16)},
        {"2 Corinthians", ("2 Corinthians", "2CO", 13)}, {"2Cor", ("2 Corinthians", "2CO", 13)}, {"2Co", ("2 Corinthians", "2CO", 13)}, {"2 C", ("2 Corinthians", "2CO", 13)},
        {"Galatians", ("Galatians", "GAL", 6)}, {"Gal", ("Galatians", "GAL", 6)}, {"Ga", ("Galatians", "GAL", 6)},
        {"Ephesians", ("Ephesians", "EPH", 6)}, {"Eph", ("Ephesians", "EPH", 6)}, {"Ep", ("Ephesians", "EPH", 6)},
        {"Philippians", ("Philippians", "PHP", 4)}, {"Phil", ("Philippians", "PHP", 4)}, {"Php", ("Philippians", "PHP", 4)}, {"Pp", ("Philippians", "PHP", 4)},
        {"Colossians", ("Colossians", "COL", 4)}, {"Col", ("Colossians", "COL", 4)},
        {"1 Thessalonians", ("1 Thessalonians", "1TH", 5)}, {"1Thess", ("1 Thessalonians", "1TH", 5)}, {"1Th", ("1 Thessalonians", "1TH", 5)}, {"1 T", ("1 Thessalonians", "1TH", 5)},
        {"2 Thessalonians", ("2 Thessalonians", "2TH", 3)}, {"2Thess", ("2 Thessalonians", "2TH", 3)}, {"2Th", ("2 Thessalonians", "2TH", 3)}, {"2 T", ("2 Thessalonians", "2TH", 3)},
        {"1 Timothy", ("1 Timothy", "1TI", 6)}, {"1Tim", ("1 Timothy", "1TI", 6)}, {"1Ti", ("1 Timothy", "1TI", 6)},
        {"2 Timothy", ("2 Timothy", "2TI", 4)}, {"2Tim", ("2 Timothy", "2TI", 4)}, {"2Ti", ("2 Timothy", "2TI", 4)},
        {"Titus", ("Titus", "TIT", 3)}, {"Tit", ("Titus", "TIT", 3)}, {"Ti", ("Titus", "TIT", 3)},
        {"Philemon", ("Philemon", "PHM", 1)}, {"Phlm", ("Philemon", "PHM", 1)}, {"Phm", ("Philemon", "PHM", 1)},
        {"Hebrews", ("Hebrews", "HEB", 13)}, {"Heb", ("Hebrews", "HEB", 13)}, {"He", ("Hebrews", "HEB", 13)},
        {"James", ("James", "JAS", 5)}, {"Jas", ("James", "JAS", 5)}, {"Jm", ("James", "JAS", 5)},
        {"1 Peter", ("1 Peter", "1PE", 5)}, {"1Pet", ("1 Peter", "1PE", 5)}, {"1Pe", ("1 Peter", "1PE", 5)}, {"1 P", ("1 Peter", "1PE", 5)},
        {"2 Peter", ("2 Peter", "2PE", 3)}, {"2Pet", ("2 Peter", "2PE", 3)}, {"2Pe", ("2 Peter", "2PE", 3)}, {"2 P", ("2 Peter", "2PE", 3)},
        {"1 John", ("1 John", "1JN", 5)}, {"1Jn", ("1 John", "1JN", 5)}, {"1J", ("1 John", "1JN", 5)},
        {"2 John", ("2 John", "2JN", 1)}, {"2Jn", ("2 John", "2JN", 1)}, {"2J", ("2 John", "2JN", 1)},
        {"3 John", ("3 John", "3JN", 1)}, {"3Jn", ("3 John", "3JN", 1)}, {"3J", ("3 John", "3JN", 1)},
        {"Jude", ("Jude", "JUD", 1)}, {"Jd", ("Jude", "JUD", 1)},
        {"Revelation", ("Revelation", "REV", 22)}, {"Rev", ("Revelation", "REV", 22)}, {"Re", ("Revelation", "REV", 22)}
    };

    // Patterns for different passage formats
    private static readonly Regex WholeBookPattern = new(@"^(.+?)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex SingleChapterPattern = new(@"^(.+?)\s+(\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex SingleVersePattern = new(@"^(.+?)\s+(\d+):(\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex VerseRangePattern = new(@"^(.+?)\s+(\d+):(\d+)-(\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ChapterRangePattern = new(@"^(.+?)\s+(\d+)-(\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ChapterVerseRangePattern = new(@"^(.+?)\s+(\d+):(\d+)-(\d+):(\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static ParseResult Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return ParseResult.Error("Please enter a passage reference.");

        input = input.Trim().TrimEnd(',', '.', ';', '!', '?');

        // Try different patterns in order of specificity
        
        // Pattern: "Book Chapter:Verse-Chapter:Verse" (e.g., "John 3:16-4:2")
        var match = ChapterVerseRangePattern.Match(input);
        if (match.Success)
        {
            return TryParseChapterVerseRange(match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value, match.Groups[4].Value, match.Groups[5].Value);
        }

        // Pattern: "Book Chapter:Verse-Verse" (e.g., "John 3:16-18")
        match = VerseRangePattern.Match(input);
        if (match.Success)
        {
            return TryParseVerseRange(match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value, match.Groups[4].Value);
        }

        // Pattern: "Book Chapter-Chapter" (e.g., "John 3-4")
        match = ChapterRangePattern.Match(input);
        if (match.Success)
        {
            return TryParseChapterRange(match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
        }

        // Pattern: "Book Chapter:Verse" (e.g., "John 3:16")
        match = SingleVersePattern.Match(input);
        if (match.Success)
        {
            return TryParseSingleVerse(match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
        }

        // Pattern: "Book Chapter" (e.g., "John 3")
        match = SingleChapterPattern.Match(input);
        if (match.Success)
        {
            return TryParseSingleChapter(match.Groups[1].Value, match.Groups[2].Value);
        }

        // Pattern: "Book" (whole book)
        return TryParseWholeBook(input);
    }

    private static ParseResult TryParseChapterVerseRange(string bookStr, string startChapterStr, string startVerseStr, string endChapterStr, string endVerseStr)
    {
        if (!TryResolveBook(bookStr, out var book, out var error))
            return ParseResult.Error(error);

        if (!int.TryParse(startChapterStr, out var startChapter) || !int.TryParse(startVerseStr, out var startVerse) ||
            !int.TryParse(endChapterStr, out var endChapter) || !int.TryParse(endVerseStr, out var endVerse))
            return ParseResult.Error("Invalid chapter or verse numbers.");

        if (startChapter < 1 || endChapter < 1 || startVerse < 1 || endVerse < 1)
            return ParseResult.Error("Chapter and verse numbers must be positive.");

        if (startChapter > book.MaxChapters || endChapter > book.MaxChapters)
            return ParseResult.Error($"{book.FullName} only has {book.MaxChapters} chapters.");

        if (startChapter > endChapter || (startChapter == endChapter && startVerse > endVerse))
            return ParseResult.Error("Start reference must come before end reference.");

        return ParseResult.Success(book.FullName, book.UsfmCode, startChapter, startVerse, endChapter, endVerse);
    }

    private static ParseResult TryParseVerseRange(string bookStr, string chapterStr, string startVerseStr, string endVerseStr)
    {
        if (!TryResolveBook(bookStr, out var book, out var error))
            return ParseResult.Error(error);

        if (!int.TryParse(chapterStr, out var chapter) || !int.TryParse(startVerseStr, out var startVerse) || !int.TryParse(endVerseStr, out var endVerse))
            return ParseResult.Error("Invalid chapter or verse numbers.");

        if (chapter < 1 || startVerse < 1 || endVerse < 1)
            return ParseResult.Error("Chapter and verse numbers must be positive.");

        if (chapter > book.MaxChapters)
            return ParseResult.Error($"{book.FullName} only has {book.MaxChapters} chapters.");

        if (startVerse > endVerse)
            return ParseResult.Error("Start verse must come before end verse.");

        return ParseResult.Success(book.FullName, book.UsfmCode, chapter, startVerse, chapter, endVerse);
    }

    private static ParseResult TryParseChapterRange(string bookStr, string startChapterStr, string endChapterStr)
    {
        if (!TryResolveBook(bookStr, out var book, out var error))
            return ParseResult.Error(error);

        if (!int.TryParse(startChapterStr, out var startChapter) || !int.TryParse(endChapterStr, out var endChapter))
            return ParseResult.Error("Invalid chapter numbers.");

        if (startChapter < 1 || endChapter < 1)
            return ParseResult.Error("Chapter numbers must be positive.");

        if (startChapter > book.MaxChapters || endChapter > book.MaxChapters)
            return ParseResult.Error($"{book.FullName} only has {book.MaxChapters} chapters.");

        if (startChapter > endChapter)
            return ParseResult.Error("Start chapter must come before end chapter.");

        return ParseResult.Success(book.FullName, book.UsfmCode, startChapter, null, endChapter, null);
    }

    private static ParseResult TryParseSingleVerse(string bookStr, string chapterStr, string verseStr)
    {
        if (!TryResolveBook(bookStr, out var book, out var error))
            return ParseResult.Error(error);

        if (!int.TryParse(chapterStr, out var chapter) || !int.TryParse(verseStr, out var verse))
            return ParseResult.Error("Invalid chapter or verse number.");

        if (chapter < 1 || verse < 1)
            return ParseResult.Error("Chapter and verse numbers must be positive.");

        if (chapter > book.MaxChapters)
            return ParseResult.Error($"{book.FullName} only has {book.MaxChapters} chapters.");

        return ParseResult.Success(book.FullName, book.UsfmCode, chapter, verse, chapter, verse);
    }

    private static ParseResult TryParseSingleChapter(string bookStr, string chapterStr)
    {
        if (!TryResolveBook(bookStr, out var book, out var error))
            return ParseResult.Error(error);

        if (!int.TryParse(chapterStr, out var chapter))
            return ParseResult.Error("Invalid chapter number.");

        if (chapter < 1)
            return ParseResult.Error("Chapter number must be positive.");

        if (chapter > book.MaxChapters)
            return ParseResult.Error($"{book.FullName} only has {book.MaxChapters} chapters.");

        return ParseResult.Success(book.FullName, book.UsfmCode, chapter, null, chapter, null);
    }

    private static ParseResult TryParseWholeBook(string bookStr)
    {
        if (!TryResolveBook(bookStr, out var book, out var error))
            return ParseResult.Error(error);

        return ParseResult.Success(book.FullName, book.UsfmCode, null, null, null, null);
    }

    private static bool TryResolveBook(string bookStr, out (string FullName, string UsfmCode, int MaxChapters) book, out string error)
    {
        bookStr = bookStr.Trim();
        
        if (BookDatabase.TryGetValue(bookStr, out book))
        {
            error = "";
            return true;
        }

        // Try partial matching for cases like "1cor" -> "1 Corinthians"
        var candidates = BookDatabase.Where(kvp => 
            kvp.Key.Replace(" ", "").StartsWith(bookStr.Replace(" ", ""), StringComparison.OrdinalIgnoreCase) ||
            kvp.Value.FullName.StartsWith(bookStr, StringComparison.OrdinalIgnoreCase)
        ).ToList();

        if (candidates.Count == 1)
        {
            book = candidates[0].Value;
            error = "";
            return true;
        }

        if (candidates.Count > 1)
        {
            var suggestions = string.Join(", ", candidates.Take(5).Select(c => c.Value.FullName));
            error = $"Ambiguous book name '{bookStr}'. Did you mean: {suggestions}?";
        }
        else
        {
            error = $"Unknown book '{bookStr}'. Please check the spelling and try again.";
        }

        book = default;
        return false;
    }
}

public class ParseResult
{
    public bool IsSuccess { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? Book { get; private set; }
    public string? UsfmCode { get; private set; }
    public int? ChapterStart { get; private set; }
    public int? VerseStart { get; private set; }
    public int? ChapterEnd { get; private set; }
    public int? VerseEnd { get; private set; }

    private ParseResult() { }

    public static ParseResult Success(string book, string usfmCode, int? chapterStart, int? verseStart, int? chapterEnd, int? verseEnd)
    {
        return new ParseResult
        {
            IsSuccess = true,
            Book = book,
            UsfmCode = usfmCode,
            ChapterStart = chapterStart,
            VerseStart = verseStart,
            ChapterEnd = chapterEnd,
            VerseEnd = verseEnd
        };
    }

    public static ParseResult Error(string message)
    {
        return new ParseResult
        {
            IsSuccess = false,
            ErrorMessage = message
        };
    }

    public override string ToString()
    {
        if (!IsSuccess) return ErrorMessage ?? "Parse failed";

        if (ChapterStart == null) return Book ?? "";
        if (VerseStart == null) 
        {
            if (ChapterStart == ChapterEnd) return $"{Book} {ChapterStart}";
            return $"{Book} {ChapterStart}-{ChapterEnd}";
        }
        if (ChapterStart == ChapterEnd)
        {
            if (VerseStart == VerseEnd) return $"{Book} {ChapterStart}:{VerseStart}";
            return $"{Book} {ChapterStart}:{VerseStart}-{VerseEnd}";
        }
        return $"{Book} {ChapterStart}:{VerseStart}-{ChapterEnd}:{VerseEnd}";
    }
}