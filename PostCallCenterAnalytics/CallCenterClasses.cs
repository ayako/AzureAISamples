namespace PostCallCenterAnalytics
{
    public class AppSettings
    {
        public string? AzureAISpeechRegion { get; set; }
        public string? AzureAISpeechKey { get; set; }
        public string? AzureAISpeechModel { get; set; }
        public string? AzureAILanguageRegion { get; set; }
        public string? AzureAILanguageKey { get; set; }

    }

    public class TranscriptionResponse
    {
        public string? self { get; set; }
        public Model? model { get; set; }
        public Links? links { get; set; }
        public Properties? properties { get; set; }
        public DateTime? lastActionDateTime { get; set; }
        public string? status { get; set; }
        public DateTime? createdDateTime { get; set; }
        public string? locale { get; set; }
        public string? displayName { get; set; }

        public class Model
        {
            public string? self { get; set; }
        }

        public class Links
        {
            public string? files { get; set; }
        }

        public class Properties
        {
            public bool? diarizationEnabled { get; set; }
            public bool? wordLevelTimestampsEnabled { get; set; }
            public bool? displayFormWordLevelTimestampsEnabled { get; set; }
            public int[]? channels { get; set; }
            public string? punctuationMode { get; set; }
            public string? profanityFilterMode { get; set; }
        }

    }

    public class TranscriptionFiles
    {
        public Value[]? values { get; set; }

        public class Value
        {
            public string? self { get; set; }
            public string? name { get; set; }
            public string? kind { get; set; }
            public Properties? properties { get; set; }
            public DateTime? createdDateTime { get; set; }
            public Links? links { get; set; }
        }

        public class Properties
        {
            public int? size { get; set; }
        }

        public class Links
        {
            public string? contentUrl { get; set; }
        }

    }

    public class TranscriptionResult
    {
        public string? source { get; set; }
        public DateTime? timestamp { get; set; }
        public int? durationInTicks { get; set; }
        public string? duration { get; set; }
        public CombinedRecognizedPhrases[]? combinedRecognizedPhrases { get; set; }
        public RecognizedPhrases[]? recognizedPhrases { get; set; }

        public class CombinedRecognizedPhrases
        {
            public int? channel { get; set; }
            public string? lexical { get; set; }
            public string? itn { get; set; }
            public string? maskedITN { get; set; }
            public string? display { get; set; }
        }

        public class RecognizedPhrases
        {
            public string? recognitionStatus { get; set; }
            public int? channel { get; set; }
            public int? speaker { get; set; }
            public string? offset { get; set; }
            public string? duration { get; set; }
            public float? offsetInTicks { get; set; }
            public float? durationInTicks { get; set; }
            public Nbest[]? nBest { get; set; }
        }

        public class Nbest
        {
            public float? confidence { get; set; }
            public string? lexical { get; set; }
            public string? itn { get; set; }
            public string? maskedITN { get; set; }
            public string? display { get; set; }
        }

    }

    public class FormattedTranscription
    {
        public int? ConversationId { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public string? Duration { get; set; }
        public int? SpeakerId { get; set; }
        public string? Phrase { get; set; }
        public string? RedactedPhrase { get; set; }
        public string? Sentiment { get; set; }
        public Scores? ConfidenceScores { get; set; }
        public class Scores
        {
            public double? Positive { get; set; }
            public double? Neutral { get; set; }
            public double? Negative { get; set; }
        }
    }

    public class SummarizationResult
    {
        public string? jobId { get; set; }
        public DateTime? lastUpdatedDateTime { get; set; }
        public DateTime? createdDateTime { get; set; }
        public DateTime? expirationDateTime { get; set; }
        public string? status { get; set; }
        public object[]? errors { get; set; }
        public Tasks? tasks { get; set; }

        public class Tasks
        {
            public int? completed { get; set; }
            public int? failed { get; set; }
            public int? inProgress { get; set; }
            public int? total { get; set; }
            public Item[]? items { get; set; }
        }

        public class Item
        {
            public string? kind { get; set; }
            public string? taskName { get; set; }
            public DateTime? lastUpdateDateTime { get; set; }
            public string? status { get; set; }
            public Results? results { get; set; }
        }

        public class Results
        {
            public Conversation[]? conversations { get; set; }
            public object[]? errors { get; set; }
            public string? modelVersion { get; set; }
        }

        public class Conversation
        {
            public Summary[]? summaries { get; set; }
            public string? id { get; set; }
            public object[]? warnings { get; set; }
        }

        public class Summary
        {
            public string? aspect { get; set; }
            public string? text { get; set; }
        }

    }
}
