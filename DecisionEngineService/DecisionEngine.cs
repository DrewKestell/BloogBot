using Communication;
using System.Data.SQLite;

namespace DecisionEngineService
{
    public class DecisionEngine
    {
        private MLModel _model;
        private SQLiteDatabase _db;
        private string _binFileDirectory;
        private FileSystemWatcher _fileWatcher;

        public DecisionEngine(string binFileDirectory, SQLiteDatabase db)
        {
            _binFileDirectory = binFileDirectory;
            _db = db;
            _model = LoadModelFromDatabase();
            InitializeFileWatcher();
        }

        private void InitializeFileWatcher()
        {
            _fileWatcher = new FileSystemWatcher(_binFileDirectory, "*.bin")
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
            };
            _fileWatcher.Created += OnBinFileCreated;
            _fileWatcher.EnableRaisingEvents = true;
        }

        private void OnBinFileCreated(object sender, FileSystemEventArgs e)
        {
            ProcessBinFile(e.FullPath);
        }

        private void ProcessBinFile(string filePath)
        {
            var snapshots = ReadBinFile(filePath);
            foreach (var snapshot in snapshots)
            {
                _model.LearnFromSnapshot(snapshot);
            }
            SaveModelToDatabase();
            File.Delete(filePath); // Clean up after processing
        }

        public List<ActionMap> GetNextActions(ActivitySnapshot snapshot)
        {
            return _model.Predict(snapshot);
        }

        private List<ActivitySnapshot> ReadBinFile(string filePath)
        {
            List<ActivitySnapshot> snapshots = [];
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                while (stream.Position < stream.Length)
                {
                    ActivitySnapshot snapshot = ActivitySnapshot.Parser.ParseDelimitedFrom(stream);
                    snapshots.Add(snapshot);
                }
            }
            return snapshots;
        }

        private void SaveModelToDatabase()
        {
            _db.SaveModelWeights(_model.GetWeights());
        }

        private MLModel LoadModelFromDatabase()
        {
            var weights = _db.LoadModelWeights();
            return new MLModel(weights);
        }
    }

    public class MLModel
    {
        private List<float> _weights;

        public MLModel(List<float> initialWeights)
        {
            _weights = initialWeights;
        }

        public void LearnFromSnapshot(ActivitySnapshot snapshot)
        {
            AdjustWeights(snapshot);
        }

        public List<ActionMap> Predict(ActivitySnapshot snapshot)
        {
            return GenerateActionMap(snapshot);
        }

        public List<float> GetWeights()
        {
            return _weights;
        }

        private void AdjustWeights(ActivitySnapshot snapshot)
        {
            // Example learning: if currentAction succeeds, increase weights for similar actions
            if (snapshot.CurrentAction.ActionResult == ActionResult.Success)
            {
                // Logic to increase weight for the current action type
                _weights[(int)snapshot.CurrentAction.ActionType]++;
            }
            else if (snapshot.CurrentAction.ActionResult == ActionResult.Failure)
            {
                // Decrease weight if the action failed
                _weights[(int)snapshot.CurrentAction.ActionType]--;
            }
        }

        private List<ActionMap> GenerateActionMap(ActivitySnapshot snapshot)
        {
            List<ActionMap> actionMaps = [];

            // Example decision-making logic: 
            // If player's health is below 50%, add a heal action
            if (snapshot.Player.Unit.Health < snapshot.Player.Unit.MaxHealth * 0.5)
            {
                actionMaps.Add(new ActionMap
                {
                    Actions = {
                        new ActionMessage
                        {
                            ActionType = ActionType.CastSpell,
                            Parameters = {
                                new ActionParameter { IntParam = 12345 } // Healing Spell ID
                            }
                        }
                    }
                });
            }

            // If there are more than 2 nearby hostile units, suggest AoE attack
            if (snapshot.NearbyUnits.Count(unit => unit.UnitFlags == 16 /* Hostile flag */) > 2)
            {
                actionMaps.Add(new ActionMap
                {
                    Actions = {
                        new ActionMessage
                        {
                            ActionType = ActionType.CastSpell,
                            Parameters = {
                                new ActionParameter { IntParam = 6789 } // AoE Spell ID
                            }
                        }
                    }
                });
            }

            // Example logic for moving to a different location
            actionMaps.Add(new ActionMap
            {
                Actions = {
                    new ActionMessage
                    {
                        ActionType = ActionType.Goto,
                        Parameters = {
                            new ActionParameter { FloatParam = snapshot.Player.Unit.GameObject.Base.Position.X },
                            new ActionParameter { FloatParam = snapshot.Player.Unit.GameObject.Base.Position.Y },
                            new ActionParameter { FloatParam = snapshot.Player.Unit.GameObject.Base.Position.Z }
                        }
                    }
                }
            });

            return actionMaps;
        }
    }

    public class SQLiteDatabase(string connectionString)
    {
        private readonly string _connectionString = connectionString;

        public void SaveModelWeights(List<float> weights)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var cmd = new SQLiteCommand("INSERT INTO ModelWeights (weights) VALUES (@weights)", connection);
            cmd.Parameters.AddWithValue("@weights", string.Join(",", weights));
            cmd.ExecuteNonQuery();
        }

        public List<float> LoadModelWeights()
        {
            List<float> weights = [];
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using var cmd = new SQLiteCommand("SELECT weights FROM ModelWeights ORDER BY id DESC LIMIT 1", connection);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string weightsStr = reader.GetString(0);
                    weights = weightsStr.Split(',').Select(float.Parse).ToList();
                }
            }

            return weights;
        }
    }
}
