namespace HachijouBot
{
    class Program
    {
        public static Task Main(string[] args) => new Program().MainAsync();

        public Program()
        {
        }

        public async Task MainAsync()
        {
            //This is where we get the Token value from the configuration file

            Hachijou hachiChan = Hachijou.GetInstance();
            await hachiChan.Initialize();

            // Block the program until it is closed.
            await Task.Delay(-1);
        }
    }
}