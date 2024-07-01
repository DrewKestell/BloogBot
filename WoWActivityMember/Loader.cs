namespace WoWActivityMember
{
    internal class Loader
    {
        private static Thread thread;

        private static int Load(string args)
        {
            thread = new Thread(() =>
            {
                Program.Main(args.Split(" "));
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            return 1;
        }
    }
}
