
namespace SentimentAnalysis.Models
{
    using Microsoft.ML.Runtime.Api;

    public class SentimentData
    {
        [Column(ordinal: "0", name: "Label")]
        public float Sentiment;

        [Column(ordinal: "1")]
        public string SentimentText;
    }
}
