namespace BloogBot
{
    public class GatherRoute
    {
        public GatherRoute(int id, string name, string nodeNames, TravelPath travelPath)
        {
            Id = id;
            Name = name;
            TravelPath = travelPath;
            NodeNames = nodeNames;
        }

        public int Id { get; }

        public string Name { get; }

        public TravelPath TravelPath { get; }

        public string NodeNames { get; }
    }
}
