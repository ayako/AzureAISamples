using Azure;
using Azure.AI.Language.Conversations;
using Azure.AI.TextAnalytics;
using Azure.Core;
using System.Data;
using System.Text;
using System.Text.Json;

namespace PostCallCenterAnalytics
{
    public static class CallCenterMethods
    {
        private static string? speechEndpoint;
        private static string? speechKey;
        private static string? speechModel;

        private static ConversationAnalysisClient? convClient;
        private static TextAnalyticsClient? textClient;


        public static async Task InitiateAsync(IConfiguration configuration)
        {
            try
            {
                // in case using Azure WebApp's App Settings
                if (Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME") != null && Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME").Contains("azurewebsites.net"))
                {
                    speechEndpoint = "https://" + configuration["AzureAISpeechRegion"] + ".api.cognitive.microsoft.com/speechtotext/v3.2/transcriptions";
                    speechKey = configuration["AzureAISpeechKey"];
                    speechModel = "https://" + configuration["AzureAISpeechRegion"] + ".api.cognitive.microsoft.com/speechtotext/v3.2/models/base/" + configuration["AzureAISpeechModel"];

                    convClient = new ConversationAnalysisClient(new Uri("https://" + configuration["AzureAILanguageRegion"] + ".api.cognitive.microsoft.com/"), new AzureKeyCredential(configuration["AzureAILanguageKey"]));
                    textClient = new TextAnalyticsClient(new Uri("https://" + configuration["AzureAILanguageRegion"] + ".api.cognitive.microsoft.com/"), new AzureKeyCredential(configuration["AzureAILanguageKey"]));
                }
                else
                {
                    var appSettings = new AppSettings();
                    configuration.GetSection("AppSettings").Bind(appSettings);

                    speechEndpoint = "https://" + appSettings.AzureAISpeechRegion + ".api.cognitive.microsoft.com/speechtotext/v3.2/transcriptions";
                    speechKey = appSettings.AzureAISpeechKey;
                    speechModel = "https://" + appSettings.AzureAISpeechRegion + ".api.cognitive.microsoft.com/speechtotext/v3.2/models/base/" + appSettings.AzureAISpeechModel;

                    convClient = new ConversationAnalysisClient(new Uri("https://" + appSettings.AzureAILanguageRegion + ".api.cognitive.microsoft.com/"), new AzureKeyCredential(appSettings.AzureAILanguageKey));
                    textClient = new TextAnalyticsClient(new Uri("https://" + appSettings.AzureAILanguageRegion + ".api.cognitive.microsoft.com/"), new AzureKeyCredential(appSettings.AzureAILanguageKey));
                }
            }
            catch (Exception ex)
            {
                // Return error message
                throw new Exception("Failed to initialize. Maybe issue to get app settings", ex);
            }

            await Task.CompletedTask;
        }

        public static async Task<string?> CreateTranscriptionAsync(string audioFileUrl)
        {

            var content = new
            {
                contentUrls = new string[] { audioFileUrl },
                properties = new
                {
                    diarizationEnabled = true,
                    punctuationMode = "DictatedAndAutomatic",
                    profanityFilterMode = "Masked"
                },
                locale = "ja-JP",
                displayName = $"call_center_{DateTime.Now.ToString()}",
                model = new { 
                    self = speechModel
                }
            };

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(speechEndpoint);
                request.Headers.Add("Ocp-Apim-Subscription-Key", speechKey);
                request.Content = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json");

                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<TranscriptionResponse>(await response.Content.ReadAsStringAsync());

                    // return transciption Id
                    return (result != null && !string.IsNullOrEmpty(result.self))? result.self.Split("/").Last() : null;
                }
                else
                {
                    return null;
                }

            }
        }
        
        public static async Task<string?> GetTranscriptionStatusAsync(string transcriptionId)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri(speechEndpoint + "/" + transcriptionId);
                request.Headers.Add("Ocp-Apim-Subscription-Key", speechKey);

                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<TranscriptionResponse>(await response.Content.ReadAsStringAsync());
                    return (result != null && !string.IsNullOrEmpty(result.status))? result.status : null;
                }
                else
                {
                    return null;
                }
            }

        }

        public static async Task<TranscriptionResult?> GetTranscriptionResultAsync(string transcriptionId)
        {
            TranscriptionFiles.Value? transcription = null;

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri(speechEndpoint + "/" + transcriptionId + "/files");
                request.Headers.Add("Ocp-Apim-Subscription-Key", speechKey);

                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<TranscriptionFiles>(await response.Content.ReadAsStringAsync());

                    if (result != null && result.values != null)
                    {
                        transcription = result.values.Where(x => x.kind == "Transcription").First();
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }

            }

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri(transcription.links.contentUrl);

                var response = await client.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    return (result != null ? JsonSerializer.Deserialize<TranscriptionResult>(result) : null);
                }
                else
                {
                    return null;
                }
            }
        }


        public static async Task<List<FormattedTranscription>> FormatTranscriptionResultAsync(TranscriptionResult transcription)
        {
            var transcriptionList = transcription.recognizedPhrases.Select(x => (
                x.offsetInTicks, x.durationInTicks, x.speaker,x.nBest.FirstOrDefault()?.display
                )).ToArray();

            var formattedTranscriptionList = new List<FormattedTranscription>();
            for (int i = 0; i < transcriptionList.Length; i++)
            {
                formattedTranscriptionList.Add(new FormattedTranscription
                {
                    ConversationId = i+1,
                    StartTime = TimeSpan.FromTicks((long)transcriptionList[i].offsetInTicks).ToString(@"hh\:mm\:ss"),
                    EndTime = TimeSpan.FromTicks((long)(transcriptionList[i].offsetInTicks + transcriptionList[i].durationInTicks)).ToString(@"hh\:mm\:ss"),
                    Duration = TimeSpan.FromTicks((long)transcriptionList[i].durationInTicks).ToString(@"hh\:mm\:ss"),
                    SpeakerId = transcriptionList[i].speaker,
                    Phrase = transcriptionList[i].display
                });
            }

            //return formattedTranscriptionList;
            return await Task.FromResult(formattedTranscriptionList);

        }

        public static async Task<List<SummarizationResult.Summary>> SummarizeTranscriptionAsync(List<FormattedTranscription> transcriptions)
        {
            var conversationItems =
                transcriptions.Select(x => new
                {
                    text = x.Phrase,
                    id = x.ConversationId.ToString(),
                    role = x.SpeakerId == 1 ? "Agent" : "Customer",
                    participantId = x.SpeakerId.ToString(),
                }
                ).ToArray();

            var data = new
            {
                analysisInput = new
                {
                    conversations = new[]
                    {
                        new
                        {
                            conversationItems = conversationItems,
                            id = "1",
                            language = "ja",
                            modality = "text",
                        },
                    }
                },
                tasks = new[]
                {
                    new
                    {
                        parameters = new
                        {
                            summaryAspects = new[]
                            {
                                "issue",
                                "resolution",
                            }
                        },
                        kind = "ConversationalSummarizationTask",
                        taskName = "1",
                    },
                },
            };

            var operation = await convClient.AnalyzeConversationsAsync(WaitUntil.Started, RequestContent.Create(data));
            operation.WaitForCompletion();

            var result = operation.Value.ToObjectFromJson<SummarizationResult>();
            var summaries = result.tasks.items[0].results.conversations[0].summaries.ToList();

            return summaries;
        }

        public static async Task<List<FormattedTranscription>> AnalyzeTranscriptionAsync(List<FormattedTranscription> transcriptions)
        {
            var sentimentList = new List<DocumentSentiment>();
            for (int i = 0; i * 10 < transcriptions.Count; i++)
            {
                var conversation = transcriptions.Where(x => ( x.ConversationId > i * 10 && x.ConversationId <= (i + 1) * 10)).Select(x => x.Phrase).ToList();
                var result = await textClient.AnalyzeSentimentBatchAsync(conversation, "ja");

                sentimentList.AddRange(result.Value.Select(x => x.DocumentSentiment).ToList());
            }

            for (int i = 0; i < transcriptions.Count; i++)
            {
                transcriptions[i].Sentiment = sentimentList[i].Sentiment.ToString();
                transcriptions[i].ConfidenceScores = new FormattedTranscription.Scores()
                {
                    Positive = sentimentList[i].ConfidenceScores.Positive,
                    Neutral = sentimentList[i].ConfidenceScores.Neutral,
                    Negative = sentimentList[i].ConfidenceScores.Negative
                };
            }

            return transcriptions;
        }

        public static async Task<List<FormattedTranscription>> DetectPIITranscriptionAsync(List<FormattedTranscription> transcriptions)
        {
            var piiList = new List<string>();
            for (int i = 0; i * 5 < transcriptions.Count; i++)
            {
                var conversation = transcriptions.Where(x => (x.ConversationId > i * 5 && x.ConversationId <= (i + 1) * 5)).Select(x => x.Phrase).ToList();
                var result = await textClient.RecognizePiiEntitiesBatchAsync(conversation, "ja");
                piiList.AddRange(result.Value.Select(x => x.Entities.RedactedText).ToList());
            }

            for (int i = 0; i < transcriptions.Count; i++)
            {
                transcriptions[i].RedactedPhrase = piiList[i];
            }

            return transcriptions;
        }
            
    }
}
