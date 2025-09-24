//
// FreshTrack Cloud API Examples.
//

using DotNetEnv;

namespace FreshTrack
{
    public class Program
    {
        private static string GetEnvVariable(string name, string? defaultValue = null)
        {
            var value = Environment.GetEnvironmentVariable(name) ?? defaultValue;

            if(string.IsNullOrEmpty(value))
            {
                throw new Exception($"Improper configuration; settings variable {name} is required");
            }

            return value;
        }

        public static void Main(string[] args)
        {
            Env.Load();

            var endpointUrl = Program.GetEnvVariable("FTS_CLOUD_API_ENDPOINT_URL");
            var userEmail = Program.GetEnvVariable("FTS_CLOUD_API_USER_EMAIL");
            var userPassword = Program.GetEnvVariable("FTS_CLOUD_API_USER_PASSWORD");

            var client = new CloudApiClient(new Uri(endpointUrl), userEmail, userPassword);

            var result = client.ExecuteAction("fts_action_insert_logger", new {
                ref_logger_id = "Test Logger ID 1234",
                freshtrack_link_type = "pallet",
                freshtrack_link = "12345",
                ref_track_id = "Test Track ID 1234",
                company = "Test Company",
            });

            Console.WriteLine(result);
        }
    }
}
