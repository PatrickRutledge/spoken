using System;
using Spoken.Core;

class Program
{
    static void Main()
    {
        Console.WriteLine("Testing PassageParser with cross-book ranges:\n");
        
        // Test the problematic input from the user
        TestParse("Genesis 1:31 - Leviticus 5:10");
        
        // Test some other cross-book ranges
        TestParse("Matthew 1:1 - Mark 2:5");
        TestParse("Exodus 20:1 - Numbers 10:15");
        
        // Test regular ranges still work
        TestParse("John 3:16");
        TestParse("Genesis 1:1-10");
        TestParse("Psalms 23");
        
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }
    
    static void TestParse(string input)
    {
        Console.WriteLine($"Input: \"{input}\"");
        var result = PassageParser.Parse(input);
        
        if (result.IsSuccess)
        {
            Console.WriteLine($"✓ SUCCESS: {result.DisplayName}");
            Console.WriteLine($"  Book: {result.BookName} ({result.UsfmCode})");
            if (result.StartChapter.HasValue)
                Console.WriteLine($"  Range: {result.StartChapter}:{result.StartVerse} - {result.EndChapter}:{result.EndVerse}");
        }
        else
        {
            Console.WriteLine($"✗ ERROR: {result.ErrorMessage}");
        }
        Console.WriteLine();
    }
}