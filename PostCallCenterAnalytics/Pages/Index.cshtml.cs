using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace PostCallCenterAnalytics.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly AppSettings _appSettings;
        private IConfiguration _configuration;

        public string? ProcessMessage { get; set; }
        public IFormFile? AudioFile { get; set; }
        public string? TranscriptionId { get; set; }
        public string? TranscriptionStatus { get; set; }
        public string? TranscriptionResult { get; set; }
        public string? AnalyzedResult { get; set; }
        public string? SummarizedResult { get; set; }


        public IndexModel(ILogger<IndexModel> logger, IOptions<AppSettings> appSettings, IConfiguration configuration)
        {
            _logger = logger;
            _appSettings = appSettings.Value;
            _configuration = configuration;
        }


        public async Task OnGetAsync()
        {
            //ProcessMessage = "Upload wav file to start the process.";
            await CallCenterMethods.InitiateAsync(_configuration);
        }
        public async Task SetValuesFromMemoryAsync()
        {
            TranscriptionId = _configuration["TranscriptionId"] ?? string.Empty;
            TranscriptionStatus = _configuration["TranscriptionStatus"] ?? string.Empty;
            TranscriptionResult = _configuration["TranscriptionResult"] ?? string.Empty;
            AnalyzedResult = _configuration["AnalyzedResult"] ?? string.Empty;
            SummarizedResult = _configuration["SummarizedResult"] ?? string.Empty;

            await Task.CompletedTask;
        }

        public async Task ClearValuesFromMemoryAsync()
        {
            _configuration["TranscriptionId"] = null;
            _configuration["TranscriptionStatus"] = null;
            _configuration["TranscriptionResult"] = null;
            _configuration["FormattedTranscriptionJson"] = null;
            _configuration["AnalyzedResult"] = null;
            _configuration["SummarizedResult"] = null;

            await Task.CompletedTask;
        }

        public async Task ClearValuesFromPageAsync()
        {
            TranscriptionId = null;
            TranscriptionStatus = null;
            TranscriptionResult = null;
            SummarizedResult = null;
            AnalyzedResult = null;

            await Task.CompletedTask;
        }


        public async Task OnPostRecognizeAsync()
        {
            await ClearValuesFromPageAsync();
            await ClearValuesFromMemoryAsync();

            if (AudioFile == null)
            {
                // Handle the case where AudioFile is null
                _logger.LogError("AudioFile is null.");
                return;
            }

            var audioFile = @"wwwroot\data\" + AudioFile.FileName;
            using (var stream = new FileStream(audioFile, FileMode.OpenOrCreate))
            {
                await AudioFile.CopyToAsync(stream);
            }

            var audioFileUrl = "https://" + HttpContext.Request.Host.ToString() + "/data/" + AudioFile.FileName;
            _configuration["TranscriptionId"] = await CallCenterMethods.CreateTranscriptionAsync(audioFileUrl);

            if (!string.IsNullOrEmpty(_configuration["TranscriptionId"]))
            {
                _configuration["TranscriptionStatus"] = await CallCenterMethods.GetTranscriptionStatusAsync(_configuration["TranscriptionId"]);
            }
            else
            {
                //ProcessMessage = "Issue on posting file to Azure AI Speech API.";
            }

            await SetValuesFromMemoryAsync();
        }

        public async Task OnPostCheckStatusAsync()
        {
            var transcriptionId = _configuration["TranscriptionId"];

            if (!string.IsNullOrEmpty(transcriptionId))
            {
                var transcriptionStatus = await CallCenterMethods.GetTranscriptionStatusAsync(transcriptionId);

                if (transcriptionStatus != null)
                {
                    _configuration["TranscriptionId"] = transcriptionId;
                    _configuration["TranscriptionStatus"] = transcriptionStatus;
                }
                else
                {
                    //ProcessMessage = "Issue on checking status to Azure AI Speech API. Try again later.";
                }
            }
            else
            {
                //ProcessMessage = "Please upload wav file first.";
            }

            await SetValuesFromMemoryAsync();
        }

        public async Task OnPostShowTranscriptionAsync()
        {
            if (string.IsNullOrEmpty(_configuration["TranscriptionResult"]))
            {
                var transcriptionId = _configuration["TranscriptionId"];
                var transcriptionStatus = _configuration["TranscriptionStatus"];

                var transcriptionContent = await CallCenterMethods.GetTranscriptionResultAsync(transcriptionId);

                ////read json from file
                //var json = System.IO.File.ReadAllText(@"wwwroot\data\transcription.json");
                //var transcriptionContent = JsonSerializer.Deserialize<TranscriptionResult>(json);

                if (transcriptionContent != null)
                {
                    var formattedContent = await CallCenterMethods.FormatTranscriptionResultAsync(transcriptionContent);

                    _configuration["TranscriptionResult"] = string.Join("\n", formattedContent.Select(x => $"{x.StartTime}\t{(x.SpeakerId == 1 ? "Operator " : "Customer")}\t{x.Phrase}").ToList());

                    _configuration["FormattedTranscriptionJson"] = JsonSerializer.Serialize(
                        formattedContent,
                        new JsonSerializerOptions { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) }
                    );
                }
                else
                {
                    //ProcessMessage = "Issue on getting transcription from Azure AI Speech. Try again later.";
                }
            }
            else
            {
                //ProcessMessage = "Process failed. Try again later.";
            }

            await SetValuesFromMemoryAsync();
        }

        public async Task OnPostSummarizeAsync()
        {
            var formattedTranscriptionJson = _configuration["FormattedTranscriptionJson"];

            if(!string.IsNullOrEmpty(formattedTranscriptionJson))
            {
                var formattedTranscriptionList = JsonSerializer.Deserialize<List<FormattedTranscription>>(formattedTranscriptionJson);

                ////read json from file
                //var json = System.IO.File.ReadAllText(@"wwwroot\data\formatted_transcription.json");
                //var formattedContent = JsonSerializer.Deserialize<List<FormattedTranscription>>(json);

                var summaries = await CallCenterMethods.SummarizeTranscriptionAsync(formattedTranscriptionList);

                _configuration["SummarizedResult"] = string.Join("\n", summaries.Select(x => $"{x.aspect}\n{x.text}"));
            }
            else
            {
                //ProcessMessage = "Process failed. Try again later.";
            }

            await SetValuesFromMemoryAsync();
        }

        public async Task OnPostAnalyzeAsync()
        {
            var formattedTranscriptionJson = _configuration["FormattedTranscriptionJson"];

            if (!string.IsNullOrEmpty(formattedTranscriptionJson))
            {
                var formattedTranscriptionList = JsonSerializer.Deserialize<List<FormattedTranscription>>(formattedTranscriptionJson);

                ////read json from file
                //var json = System.IO.File.ReadAllText(@"wwwroot\data\formatted_transcription.json");
                //var formattedTranscriptionList = JsonSerializer.Deserialize<List<FormattedTranscription>>(json);

                var formattedContentWithSentiment = await CallCenterMethods.AnalyzeTranscriptionAsync(formattedTranscriptionList);
                var analyzedFormattedContent = await CallCenterMethods.DetectPIITranscriptionAsync(formattedContentWithSentiment);

                _configuration["AnalyzedResult"] = string.Join("\n", analyzedFormattedContent.Select(x =>
                                                                $"{x.StartTime}\t" +
                                                                $"{(x.SpeakerId == 1 ? "Operator " : "Customer")}\t" +
                                                                $"{x.Sentiment}" + "(" +
                                                                $"{(x.Sentiment == "Neutral" ? x.ConfidenceScores.Neutral : (x.Sentiment == "Positive" ? x.ConfidenceScores.Positive : x.ConfidenceScores.Negative))}" +
                                                                ")\t" +
                                                                $"{x.RedactedPhrase}"
                                                        ).ToList()
                                                    );
            }
            else
            {
                //ProcessMessage = "Process failed. Try again later.";
            }

            await SetValuesFromMemoryAsync();
        }
    }
}
