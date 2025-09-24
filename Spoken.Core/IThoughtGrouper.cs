using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spoken.Core
{
    public interface IThoughtGrouper
    {
        /// <summary>
        /// Analyzes a sequence of verses and identifies coherent thought units
        /// </summary>
        /// <param name="verses">The verses to analyze</param>
        /// <param name="bookName">The book name for context</param>
        /// <param name="chapterNumber">The chapter number for context</param>
        /// <returns>Verses with thought group information populated</returns>
        Task<List<Verse>> AnalyzeThoughtUnitsAsync(List<Verse> verses, string bookName, int chapterNumber);
        
        /// <summary>
        /// Gets whether the AI service is available
        /// </summary>
        bool IsAvailable { get; }
        
        /// <summary>
        /// Gets the name of the AI service being used
        /// </summary>
        string ServiceName { get; }
    }
}