//
// FreshTrack Cloud API Examples.
//

using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FreshTrack
{
    public class CloudApiException : Exception
    {
        private static string FormatMessage(string message, string? context)
        {
            return string.IsNullOrEmpty(context)
                ? message
                : $"{message} :: {context}";
        }

        public CloudApiException(string message, string? context = null)
            : base(CloudApiException.FormatMessage(message, context))
        {
            //
        }
    }

    public class CloudApiClient
    {
        private readonly Uri graphQlUrl;
        private readonly string email;
        private readonly string password;
        private string? authToken;

        public CloudApiClient(Uri endpointUrl, string email, string password)
        {
            this.graphQlUrl = new Uri(endpointUrl, "api/graphql");
            this.email = email;
            this.password = password;
            this.authToken = null;
        }

        private JToken? ExecuteGraphQlQuery(string query, dynamic variables)
        {
            using var client = new HttpClient();

            client.Timeout = TimeSpan.FromSeconds(10);

            if(!string.IsNullOrEmpty(this.authToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.authToken);
            }

            var payload = new {
                query,
                variables,
            };

            var requestContent = new StringContent(
                JsonConvert.SerializeObject(payload, Formatting.None),
                Encoding.UTF8,
                "application/json"
            );

            var responseContent = client
                .PostAsync(this.graphQlUrl, requestContent)
                .GetAwaiter()
                .GetResult()
                .Content
                .ReadAsStringAsync()
                .GetAwaiter()
                .GetResult();

            var responseData = JObject.Parse(responseContent);
            if(responseData.TryGetValue("errors", out var errors))
            {
                throw new CloudApiException("GraphQL request contains errors", errors.ToString());
            }

            return responseData["data"];
        }

        private void AuthenticateIfRequired()
        {
            if(!string.IsNullOrEmpty(this.authToken))
            {
                return;
            }

            var data = this.ExecuteGraphQlQuery(@"
                mutation($email: String!, $credentials: String!, $expiresOn: DateTime!) {
                    authenticateWithCredentials(authData: {
                        email: $email,
                        credentials: $credentials,
                        expiresOn: $expiresOn,
                    }) {
                        authToken {
                            token
                        }
                    }
                }",
                new {
                    email = this.email,
                    credentials = this.password,
                    expiresOn = "9999-01-01T00:00:00Z",
                }
            );

            var authToken = data?["authenticateWithCredentials"]?["authToken"]?["token"]?.ToString();

            if(authToken == null)
            {
                throw new CloudApiException("Authentication failed; request was successful but no authentication token was returned");
            }

            this.authToken = authToken;
        }

        public JToken? ExecuteAction(string name, object arguments)
        {
            this.AuthenticateIfRequired();

            var data = this.ExecuteGraphQlQuery(@"
                mutation($name: String!, $arguments: JSONString!) {
                    executeAction(name: $name, arguments: $arguments) {
                        result {
                            data
                            errors {
                                code
                                message
                            }
                        }
                    }
                }",
                new {
                    name,
                    arguments = JsonConvert.SerializeObject(arguments),
                }
            );

            var actionResult = data?["executeAction"]?["result"];

            if(actionResult == null)
            {
                throw new CloudApiException("Action execution failed; request was successful but no result data was returned");
            }

            return actionResult;
        }
    }
}
