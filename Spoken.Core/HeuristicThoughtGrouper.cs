using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spoken.Core
{
    /// <summary>
    /// Fallback thought grouper that uses simple heuristics when AI services are unavailable
    /// </summary>
    public class HeuristicThoughtGrouper : IThoughtGrouper
    {
        public bool IsAvailable => true;
        public string ServiceName => "Heuristic Grouper";
        
        public Task<List<Verse>> AnalyzeThoughtUnitsAsync(List<Verse> verses, string bookName, int chapterNumber)
        {
            if (!verses.Any()) return Task.FromResult(verses);
            
            var result = verses.ToList();
            int currentGroupId = 1;
            
            // Simple heuristic: look for natural thought breaks based on content patterns
            for (int i = 0; i < result.Count; i++)
            {
                var verse = result[i];
                verse.ThoughtGroupId = currentGroupId;
                
                // Determine if this verse starts a new thought group
                verse.IsThoughtStart = ShouldStartNewThought(verse, i == 0 ? null : result[i - 1], verses, i);
                
                if (verse.IsThoughtStart && i > 0)
                {
                    currentGroupId++;
                }
            }
            
            return Task.FromResult(result);
        }
        
        private bool ShouldStartNewThought(Verse current, Verse? previous, List<Verse> allVerses, int index)
        {
            if (index == 0) return true; // First verse always starts a group
            if (previous == null) return true;
            
            var currentText = current.Text.ToLower();
            var previousText = previous.Text.ToLower();
            
            // Heuristics for new thought units:
            
            // 1. Dialogue markers - someone starts speaking
            if (ContainsDialogueStart(currentText))
                return true;
                
            // 2. Temporal transitions
            if (ContainsTimeTransition(currentText))
                return true;
                
            // 3. Location changes
            if (ContainsLocationChange(currentText, previousText))
                return true;
                
            // 4. Topic shift indicators
            if (ContainsTopicShift(currentText))
                return true;
                
            // 5. Every 4-6 verses as a fallback to prevent overly long paragraphs
            var versesSinceLastStart = 0;
            for (int j = index - 1; j >= 0; j--)
            {
                if (allVerses[j].IsThoughtStart) break;
                versesSinceLastStart++;
            }
            
            if (versesSinceLastStart >= 5) return true;
            
            return false;
        }
        
        private bool ContainsDialogueStart(string text)
        {
            return text.Contains("said") || text.Contains("spoke") || text.Contains("answered") ||
                   text.Contains("replied") || text.Contains("declared") || text.Contains("proclaimed");
        }
        
        private bool ContainsTimeTransition(string text)
        {
            return text.Contains("then") || text.Contains("now") || text.Contains("when") ||
                   text.Contains("after") || text.Contains("before") || text.Contains("meanwhile") ||
                   text.Contains("suddenly") || text.Contains("immediately");
        }
        
        private bool ContainsLocationChange(string current, string previous)
        {
            var locationWords = new[] { "went", "came", "returned", "departed", "entered", "left" };
            return locationWords.Any(word => current.Contains(word) && !previous.Contains(word));
        }
        
        private bool ContainsTopicShift(string text)
        {
            return text.Contains("but") || text.Contains("however") || text.Contains("therefore") ||
                   text.Contains("moreover") || text.Contains("furthermore") || text.Contains("nevertheless");
        }
    }
}