using System;
using Spoken.Core;

class Program
{
    static void Main()
    {
        Console.WriteLine("Testing PassageParser with single-book limitation:\n");
        
        // Test cross-book ranges (should be rejected)
        Console.WriteLine("=== Testing cross-book ranges (should be rejected) ===");
        TestParse("Genesis 1:31 - Leviticus 5:10");
        TestParse("Genesis 1:31-Leviticus 5:10");  // No spaces around dash
        TestParse("Gen 1:31 - Lev 5:10");          // Abbreviations
        TestParse("Matthew 1:1 - Mark 2:5");
        TestParse("Exodus 20:1 - Numbers 10:15");
        
        // Test single-book ranges (should work)
        Console.WriteLine("=== Testing single-book ranges (should work) ===");
        TestParse("John 3:16");                     // Single verse
        TestParse("Genesis 1:1-10");                // Verse range within chapter
        TestParse("John 3:16-4:2");                 // Chapter-verse range within book
        TestParse("Psalms 23");                     // Whole chapter
        TestParse("Genesis 1-3");                   // Chapter range
        TestParse("Romans");                        // Whole book
        
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }
    
    static void TestParse(string input)
    {
        Console.WriteLine($"Input: \"{input}\"");
        var result = PassageParser.Parse(input);
        
        if (result.IsSuccess)
        {
            Console.WriteLine($"✓ SUCCESS: {result.ToString()}");
            Console.WriteLine($"  Book: {result.Book} ({result.UsfmCode})");
            if (result.ChapterStart.HasValue)
                Console.WriteLine($"  Range: {result.ChapterStart}:{result.VerseStart} - {result.ChapterEnd}:{result.VerseEnd}");
        }
        else
        {
            Console.WriteLine($"✗ ERROR: {result.ErrorMessage}");
        }
        Console.WriteLine();
    }
}