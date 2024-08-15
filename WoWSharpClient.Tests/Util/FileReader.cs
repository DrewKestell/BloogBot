namespace WoWSharpClient.Tests.Util
{
    internal class FileReader
    {
        public static byte[] ReadBinaryFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The file {filePath} does not exist.");
            }

            try
            {
                byte[] fileData = File.ReadAllBytes(filePath);
                return fileData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while reading the file: {ex.Message}");
                throw;
            }
        }
    }
}
