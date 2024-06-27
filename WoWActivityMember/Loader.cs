namespace WoWActivityMember
{
    class Loader
    {
        static Thread thread;

        static int Load(string args)
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
