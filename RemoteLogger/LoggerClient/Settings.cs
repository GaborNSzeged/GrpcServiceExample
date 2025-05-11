using Microsoft.Extensions.Configuration;

namespace LoggerClient
{
    internal class Settings
    {
        public Settings()
        {
            LoadCofing();
        }

        public string AgentName { get; private set; } = string.Empty;
        public string ServerAddress { get; private set; } = string.Empty;

        private void LoadCofing()
        {
            // AppDomain.CurrentDomain.BaseDirectory
            //var builder = new ConfigurationBuilder()
            //   .SetBasePath(Directory.GetCurrentDirectory())
            //   .AddJsonFile("config.json", optional: false);

            var builder = new ConfigurationBuilder()
              .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("LoggerClientConfig.json", optional: false);

            // App dir: C:\gRPC\gRPC\Projects\RemoteLogger\LoggerClient\bin\Debug\net8.0\
            //++C:\gRPC\gRPC\Projects\RemoteLogger\ConsoleAppFramework\bin\Debug\net472

            // Console.WriteLine($"++ {Directory.GetCurrentDirectory()}");
            //Console.ReadLine(); 

            IConfiguration config = builder.Build();

            ServerAddress = LoadServereAddress(config);
            AgentName = LoadAgentName(config);
        }

        private string LoadServereAddress(IConfiguration config)
        {
            IConfigurationSection? section = config.GetSection("Server");
            section = section?.GetSection("Address");
            string? address = section != null ? section.Value : string.Empty;

            if (string.IsNullOrEmpty(address))
            {
                throw new ArgumentNullException("Server address is missing from config.json");
            }

            return address;
        }

        private string LoadAgentName(IConfiguration config)
        {
            IConfigurationSection section = config.GetSection("LoggerConfig");
            section = section.GetSection("ClientName");
            string? agentName = section.Value;

            if (string.IsNullOrEmpty(agentName))
            {
                agentName = "Agent_0";
            }

            return agentName;
        }
    }
}

//1.Directory.GetCurrentDirectory()
//What it does: This method retrieves the current working directory of the process.
//The current working directory is the directory from which the application was launched or
//explicitly set by the parent process.

//Why it returns the caller's directory: When an application is started using Process.Start,
//it inherits the working directory of the parent process (the process that called Process.Start).
//Unless explicitly specified via ProcessStartInfo.WorkingDirectory, this working directory remains
//the same as the caller's. Therefore, when the launched application calls Directory.GetCurrentDirectory(),
//it returns the parent's working directory, not the application's own directory.

//2. AppDomain.CurrentDomain.BaseDirectory
//What it does: This property retrieves the directory path where the application's executable resides.

//Why it returns the correct directory: This is specific to the application itself—it
//reflects the physical location of the application's .exe file. Since it is tied to the
//entry point of the application, it accurately points to the directory of the started
//process regardless of the working directory inherited from the parent process.

//Example to Illustrate:
//If you have:

//A parent application (ParentApp.exe) in C:\ParentApp\

//A child application (ChildApp.exe) in C:\ChildApp\

//And you use Process.Start in ParentApp.exe to start ChildApp.exe without setting ProcessStartInfo.WorkingDirectory, you'll observe:

//Directory.GetCurrentDirectory() in ChildApp.exe → Returns C:\ParentApp\ (inherited from the parent process).

//AppDomain.CurrentDomain.BaseDirectory in ChildApp.exe → Returns C:\ChildApp\ (the directory of ChildApp.exe).

//If you want the launched application's Directory.GetCurrentDirectory() to reflect its own directory,
//you can explicitly set the WorkingDirectory in ProcessStartInfo when starting the process:

//csharp
//ProcessStartInfo startInfo = new ProcessStartInfo
//                             {
//                                 FileName = @"C:\ChildApp\ChildApp.exe",
//                                 WorkingDirectory = @"C:\ChildApp\"
//                             };

//Process.Start(startInfo);
//This way, Directory.GetCurrentDirectory() in the child process will point to C:\ChildApp\.