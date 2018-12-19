
namespace SentimentAnalysis.Models
{
    using Microsoft.AspNetCore.Http;

    public class TrainingDataFile
    {
        public IFormFile File { get; set; }
    }
}
