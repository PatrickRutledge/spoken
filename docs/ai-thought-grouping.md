# AI Thought Grouping Feature

## Overview
The AI Thought Grouping feature provides an alternative to traditional USFM scholarly paragraph formatting by using artificial intelligence to identify coherent thought units for enhanced reading flow and comprehension.

## How It Works

### Dual Formatting Modes
The app now supports two paragraph formatting approaches:

1. **Scholarly (USFM)** - Traditional manuscript-based paragraph breaks using USFM markers (\p, \m)
2. **AI Thought Groups** - Intelligent grouping based on thematic coherence and narrative flow

### AI Services

#### OpenAI Integration (Primary)
- Uses GPT-4o-mini for cost-effective biblical text analysis
- Analyzes thematic continuity, logical flow, speaker changes, and narrative shifts
- Considers Hebrew parallelism and English reading comprehension
- Requires `OPENAI_API_KEY` environment variable

#### Heuristic Fallback (Secondary)
- Rule-based analysis when OpenAI is unavailable
- Detects dialogue markers, temporal transitions, location changes, topic shifts
- Prevents overly long paragraphs with 5-verse maximum fallback

### User Interface
- **Formatting dropdown**: Switch between "Scholarly (USFM)" and "AI Thought Groups"
- **AI Service indicator**: Shows which service is active (OpenAI GPT-4 or Heuristic Grouper)
- **Real-time switching**: Instantly re-formats current passage when mode changes

## Technical Implementation

### New Classes
- `IThoughtGrouper` - Interface for AI thought grouping services
- `OpenAIThoughtGrouper` - GPT-4 powered analysis with biblical context
- `HeuristicThoughtGrouper` - Rule-based fallback for offline use

### Enhanced Data Model
```csharp
public class Verse
{
    // Existing properties...
    public int ThoughtGroupId { get; set; } = 0;
    public bool IsThoughtStart { get; set; } = false;
}

public enum FormattingMode
{
    Scholarly,
    AiThoughtGroups
}
```

### ProseFormatter Enhancement
- Updated to support both formatting modes
- Respects `IsThoughtStart` for AI-generated breaks
- Maintains `IsNewParagraph` for USFM scholarly breaks

## Usage

### Setting Up OpenAI (Optional)
1. Get an OpenAI API key from https://platform.openai.com/
2. Set environment variable: `OPENAI_API_KEY=your_key_here`
3. Restart the application

### Using the Feature
1. Select any biblical passage
2. Choose "AI Thought Groups" from the Formatting dropdown
3. The passage will be re-formatted with AI-generated paragraph breaks
4. Switch back to "Scholarly (USFM)" for traditional formatting

## Benefits

### AI Thought Groups
- **Enhanced Readability**: Natural thought flow for modern readers
- **Contextual Understanding**: AI considers narrative structure and themes
- **Flexible Grouping**: Adapts to different literary styles within Scripture

### Scholarly USFM
- **Academic Integrity**: Preserves manuscript traditions
- **Historical Accuracy**: Reflects original Hebrew/Greek structure
- **Scholarly Standards**: Matches academic study Bibles

## Future Enhancements
- Local AI models (Phi-3, Llama) for complete offline capability
- User feedback system to improve AI grouping quality
- Preset grouping styles (devotional, study, liturgical)
- Integration with additional biblical resources and commentaries

## Architecture Notes
- Graceful degradation: Falls back to heuristic grouping if OpenAI fails
- Cost optimization: Uses GPT-4o-mini instead of full GPT-4
- Caching: Future enhancement to cache AI results for performance
- Privacy: No verse text stored externally, only analyzed in real-time