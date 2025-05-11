namespace LoggerServer.Helpers
{
    public class AppSettingsHelper
    {
        public static string GetLogDirectory(ConfigurationManager configurationManager)
        {
            if (configurationManager != null)
            {
                IConfigurationSection? vv = configurationManager.GetSection("LogFilesDirectory");
                if (vv != null)
                {
                    string path = vv.Value;

                    if (string.IsNullOrEmpty(path))
                    {
                        // use the default one
                    }
                    return path;
                }
            }

            return string.Empty;
        }
    }
}
