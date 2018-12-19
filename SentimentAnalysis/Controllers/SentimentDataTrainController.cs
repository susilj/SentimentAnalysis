
namespace SentimentAnalysis.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.ML;
    using Microsoft.ML.Core.Data;
    using Microsoft.ML.Runtime.Api;
    using Microsoft.ML.Runtime.Data;
    using Microsoft.ML.Transforms.Text;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using SentimentAnalysis.Models;
    using static Microsoft.ML.Runtime.Data.BinaryClassifierEvaluator;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Hosting;

    [Route("api/[controller]")]
    [ApiController]
    public class SentimentDataTrainController : ControllerBase
    {
        static readonly string _trainDataPath = Path.Combine(Environment.CurrentDirectory, "Data", "TrainingData"); //, "wikipedia-detox-250-line-data.tsv");

        static readonly string _testDataPath = Path.Combine(Environment.CurrentDirectory, "Data", "TestData", "wikipedia-detox-250-line-test.tsv");

        static readonly string _modelPath = Path.Combine(Environment.CurrentDirectory, "Data", "TrainedModel", "Model.zip");

        static TextLoader _textLoader;

        private readonly IHostingEnvironment hostingEnvironment;

        public SentimentDataTrainController(IHostingEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
        }

        [Route("[action]")]
        public IActionResult Train()
        {
            MLContext mlContext = new MLContext(seed: 0);

            _textLoader = mlContext.Data.TextReader(new TextLoader.Arguments()
            {
                Separator = "tab",
                HasHeader = true,
                Column = new[]
                {
                    new TextLoader.Column("Label", DataKind.Bool, 0),
                    new TextLoader.Column("SentimentText", DataKind.Text, 1)
                }
            });

            var model = Train(mlContext, _trainDataPath);

            //Evaluate(mlContext, model);

            //Predict(mlContext, model);

            //PredictWithModelLoadedFromFile(mlContext);

            return Ok();
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult Predict([FromBody]PredictionContextData context)
        {
            MLContext mlContext = new MLContext(seed: 0);

            var results = PredictWithModelLoadedFromFile(mlContext, context.PredictionText);

            return Ok(results);
        }

        [Route("[action]")]
        public IActionResult Test()
        {
            MLContext mlContext = new MLContext(seed: 0);

            var results = Evaluate(mlContext);

            return Ok(results);
        }

        [Route("[action]")]
        public IActionResult ListTrainDataFiles()
        {
            IEnumerable<string> files = Directory.EnumerateFiles(Path.Combine(Environment.CurrentDirectory, "Data", "TrainingData"), "*.tsv", SearchOption.AllDirectories)
            .Select(x => Path.GetFileName(x));
            return Ok(files);
        }

        [HttpGet]
        [Route("[action]/{fileName}")]
        public async Task<IActionResult> DownloadFile([FromRoute]string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                string fullPath = Path.Combine(Environment.CurrentDirectory, "Data", "TrainingData", fileName);

                if (System.IO.File.Exists(fullPath))
                {
                    var memory = new MemoryStream();

                    using (var stream = new FileStream(fullPath, FileMode.Open))
                    {
                        await stream.CopyToAsync(memory);
                    }

                    memory.Position = 0;

                    return File(memory, "text/plain");

                    //HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                    //var fileStream = new FileStream(fullPath, FileMode.Open);
                    //response.Content = new StreamContent(fileStream);
                    //response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    //response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                    //response.Content.Headers.ContentDisposition.FileName = fileName;
                    //return response;
                }

                return NotFound();
            }

            return BadRequest();
        }

        [HttpPost]
        [Route("[action]")]
        //public async Task<IActionResult> UploadFile([FromBody]TrainingDataFile context)
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            var filePath = Path.GetTempFileName();

            if (file.Length > 0)
            {
                filePath = Path.Combine(hostingEnvironment.ContentRootPath, "Data", "TrainingData", file.FileName);
                //filePath = Path.Combine(
                //        Directory.GetCurrentDirectory(), "wwwroot",
                //        file.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                //using (var stream = new FileStream(filePath, FileMode.Create))
                //{
                //    await file.CopyToAsync(stream);
                //}
            }

            return Ok(new { filePath });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Upload(List<IFormFile> files)
        {
            return Ok();
        }

        private ITransformer Train(MLContext mlContext, string dataPath)
        {
            string[] files = Directory.GetFiles(dataPath, "*.tsv");

            IDataView dataView = _textLoader.Read(files);

            var pipeline = mlContext.Transforms.Text.FeaturizeText("SentimentText", "Features")
                .Append(mlContext.BinaryClassification.Trainers.FastTree(numLeaves: 50, numTrees: 50, minDatapointsInLeaves: 20));

            Console.WriteLine("=============== Create and Train the Model ===============");
            var model = pipeline.Fit(dataView);
            Console.WriteLine("=============== End of training ===============");
            Console.WriteLine();

            SaveModelAsFile(mlContext, model);

            return model;
        }

        private CalibratedResult Evaluate(MLContext mlContext)
        {
            ITransformer loadedModel;

            using (var stream = new FileStream(_modelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                loadedModel = mlContext.Model.Load(stream);
            }

            return Evaluate(mlContext, loadedModel);
        }

        public static CalibratedResult Evaluate(MLContext mlContext, ITransformer model)
        {
            IDataView dataView = _textLoader.Read(_testDataPath);

            Console.WriteLine("=============== Evaluating Model accuracy with Test data===============");
            var predictions = model.Transform(dataView);

            var metrics = mlContext.BinaryClassification.Evaluate(predictions, "Label");

            Console.WriteLine();
            Console.WriteLine("Model quality metrics evaluation");
            Console.WriteLine("--------------------------------");
            Console.WriteLine($"Accuracy: {metrics.Accuracy:P2}");
            Console.WriteLine($"Auc: {metrics.Auc:P2}");
            Console.WriteLine($"F1Score: {metrics.F1Score:P2}");
            Console.WriteLine("=============== End of model evaluation ===============");

            return metrics;
        }

        private void SaveModelAsFile(MLContext mlContext, ITransformer model)
        {
            using (var fs = new FileStream(_modelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                mlContext.Model.Save(model, fs);

            Console.WriteLine("The model is saved to {0}", _modelPath);
        }

        private static void Predict(MLContext mlContext, ITransformer model)
        {
            var predictionFunction = model.MakePredictionFunction<SentimentData, SentimentPrediction>(mlContext);

            SentimentData sampleStatement = new SentimentData
            {
                SentimentText = "This is a very rude movie"
            };

            var resultprediction = predictionFunction.Predict(sampleStatement);

            Console.WriteLine();
            Console.WriteLine("=============== Prediction Test of model with a single sample and test dataset ===============");

            Console.WriteLine();
            Console.WriteLine($"Sentiment: {sampleStatement.SentimentText} | Prediction: {(Convert.ToBoolean(resultprediction.Prediction) ? "Toxic" : "Not Toxic")} | Probability: {resultprediction.Probability} ");
            Console.WriteLine("=============== End of Predictions ===============");
            Console.WriteLine();
        }

        private IEnumerable<(SentimentData, SentimentPrediction)> PredictWithModelLoadedFromFile(MLContext mlContext, string sentimentText)
        {
            IEnumerable<SentimentData> sentiments = new[]
            {
                new SentimentData
                {
                    SentimentText = sentimentText
                }
            };

            ITransformer loadedModel;
            using (var stream = new FileStream(_modelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                loadedModel = mlContext.Model.Load(stream);
            }

            // Create prediction engine
            var sentimentStreamingDataView = mlContext.CreateStreamingDataView(sentiments);
            var predictions = loadedModel.Transform(sentimentStreamingDataView);

            // Use the model to predict whether comment data is toxic (1) or nice (0).
            var predictedResults = predictions.AsEnumerable<SentimentPrediction>(mlContext, reuseRowObject: false);

            Console.WriteLine();

            Console.WriteLine("=============== Prediction Test of loaded model with a multiple samples ===============");

            var sentimentsAndPredictions = sentiments.Zip(predictedResults, (sentiment, prediction) => (sentiment, prediction));

            foreach (var item in sentimentsAndPredictions)
            {
                Console.WriteLine($"Sentiment: {item.sentiment.SentimentText} | Prediction: {(Convert.ToBoolean(item.prediction.Prediction) ? "Toxic" : "Not Toxic")} | Probability: {item.prediction.Probability} ");
            }

            Console.WriteLine("=============== End of predictions ===============");
            return sentimentsAndPredictions;
        }
    }
}