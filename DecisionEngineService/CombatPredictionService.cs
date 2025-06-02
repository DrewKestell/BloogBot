using Communication;
using Microsoft.Data.Sqlite;
using Microsoft.ML;

namespace DecisionEngineService
{
    public class CombatPredictionService
    {
        private readonly MLContext _mlContext;
        private PredictionEngine<ActivitySnapshot, ActivitySnapshot> _predictionEngine;
        private ITransformer _trainedModel;
        private readonly string _connectionString;
        private readonly string _dataDirectory;
        private readonly string _processedDirectory;

        public CombatPredictionService(string connectionString, string dataDirectory, string processedDirectory)
        {
            _mlContext = new MLContext();
            _connectionString = connectionString;
            _dataDirectory = dataDirectory;
            _processedDirectory = processedDirectory;

            // Load the initial trained model from the SQLite database
            _trainedModel = LoadModelFromDatabase();

            // Create a prediction engine based on the loaded model
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<ActivitySnapshot, ActivitySnapshot>(_trainedModel);

            // Start monitoring for new `.bin` files
            MonitorForNewData();
        }

        // Method to load the model from the SQLite database
        private ITransformer LoadModelFromDatabase()
        {
            ITransformer model = null;

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT ModelData FROM TrainedModel ORDER BY Id DESC LIMIT 1";
                var modelData = command.ExecuteScalar() as byte[];

                if (modelData != null)
                {
                    using var memoryStream = new MemoryStream(modelData);
                    DataViewSchema modelSchema;
                    model = _mlContext.Model.Load(memoryStream, out modelSchema);
                }
            }

            return model;
        }

        // Method to predict the action based on input combat data
        public ActivitySnapshot PredictAction(ActivitySnapshot inputData)
        {
            if (_predictionEngine == null)
            {
                throw new InvalidOperationException("Prediction engine is not initialized.");
            }

            // Predict the action based on the input data
            ActivitySnapshot prediction = _predictionEngine.Predict(inputData);
            return prediction;
        }

        // Method to update the model if necessary
        public void UpdateModel()
        {
            _trainedModel = LoadModelFromDatabase();
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<ActivitySnapshot, ActivitySnapshot>(_trainedModel);
        }

        // Method to monitor the data directory for new `.bin` files
        private void MonitorForNewData()
        {
            FileSystemWatcher watcher = new()
            {
                Path = _dataDirectory,
                Filter = "*.bin",
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
            };

            watcher.Created += OnNewDataFile;
            watcher.EnableRaisingEvents = true;
        }

        // Event handler for when a new `.bin` file is detected
        private void OnNewDataFile(object source, FileSystemEventArgs e)
        {
            string filePath = e.FullPath;

            // Load and process the new data file
            IDataView newData = LoadData(filePath);

            // Update the model with the new data
            RetrainModel(newData);

            // Move the processed file to the processed directory
            MoveProcessedFile(filePath);
        }

        // Method to load data from a `.bin` file
        private IDataView LoadData(string filePath)
        {
            // Assuming `.bin` files are in a format that ML.NET can load directly.
            // If not, you'll need to parse them and convert to IDataView.
            return _mlContext.Data.LoadFromBinary(filePath);
        }

        // Method to retrain the model with new data
        private void RetrainModel(IDataView newData)
        {
            // Assuming you're using a similar pipeline to the one in the initial model training
            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("ActionTaken")
                .Append(_mlContext.Transforms.Concatenate("Features",
                    "self.health",
                    "self.max_health",
                    "self.position.x",
                    "self.position.y",
                    "self.position.z",
                    "self.facing",
                    "target_id",
                    "nearby_units"))
                .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("ActionTaken", "Features"))
                .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedAction", "PredictedLabel"));

            // Combine the new data with any existing data if necessary
            var combinedData = CombineWithExistingData(newData);

            // Train the model
            _trainedModel = pipeline.Fit(combinedData);

            // Update the prediction engine with the new model
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<ActivitySnapshot, ActivitySnapshot>(_trainedModel);

            // Save the updated model to the SQLite database
            SaveModelToDatabase(_trainedModel);
        }

        // Method to combine new data with existing data
        private static IDataView CombineWithExistingData(IDataView newData)
        {
            // Load existing data if needed and combine with newData
            // Assuming the existing data is stored in a format you can load
            // For simplicity, this example assumes only new data is used
            return newData;
        }

        // Method to save the trained model to the SQLite database
        private void SaveModelToDatabase(ITransformer model)
        {
            using var memoryStream = new MemoryStream();
            _mlContext.Model.Save(model, null, memoryStream);
            var modelData = memoryStream.ToArray();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO TrainedModel (ModelData) VALUES (@ModelData)";
            command.Parameters.AddWithValue("@ModelData", modelData);
            command.ExecuteNonQuery();
        }

        // Method to move processed `.bin` file to another directory
        private void MoveProcessedFile(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            string destPath = Path.Combine(_processedDirectory, fileName);

            if (File.Exists(destPath))
            {
                File.Delete(destPath);
            }

            File.Move(filePath, destPath);
        }
    }
}
