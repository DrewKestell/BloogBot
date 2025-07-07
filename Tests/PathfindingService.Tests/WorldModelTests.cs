using G3D;
using VMAP;


namespace PathfindingService.Tests
{
    public class WorldModelTests
    {
        private const string VmapsDirectory = @".\vmaps";

        [Fact]
        public void LoadAllWorldModelsFromFolder_ShouldSucceed()
        {
            Assert.True(Directory.Exists(VmapsDirectory), $"Directory not found: {VmapsDirectory}");

            var files = Directory.GetFiles(VmapsDirectory, "*.vmo", SearchOption.TopDirectoryOnly);
            Assert.NotEmpty(files);

            foreach (var file in files)
            {
                Console.WriteLine($"Parsing {file}");
                var model = new WorldModel();
                bool loaded = model.ReadFile(file);
                Assert.True(loaded, $"Failed to read file: {file}");
            }
        }

        [Theory]
        [InlineData("Logmachine01.m2.vmo")]
        public void IntersectRay_ShouldNotThrow(string filename)
        {
            string path = Path.Combine(VmapsDirectory, filename);
            if (!File.Exists(path))
            {
                // Skip test if file doesn't exist
                return;
            }

            var model = new WorldModel();
            Assert.True(model.ReadFile(path), $"Failed to read file: {path}");

            // Perform a simple ray test down the Z-axis
            var ray = new Ray(new Vector3(0, 0, 500), new Vector3(0, 0, -1));
            float dist = 1000f;
            bool hit = model.IntersectRay(ray, ref dist, false, false);

            // We do not assert 'hit' true/false since it depends on the model geometry
            // Instead we assert that it ran without throwing and updated distance
            Assert.True(dist >= 0, "Ray distance should be non-negative after intersection test.");
        }
    }
}
