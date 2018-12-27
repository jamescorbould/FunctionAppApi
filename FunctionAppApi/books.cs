using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace FunctionApp
{
    public static class books
    {
        private static readonly string DatabaseId = ConfigurationManager.AppSettings["database"];
        private static readonly string CollectionId = ConfigurationManager.AppSettings["collection"];
        private static DocumentClient client;

        [FunctionName("books")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route="books/{id?}")]HttpRequestMessage req, string id, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            client = new DocumentClient(new Uri(ConfigurationManager.AppSettings["endpoint"]), ConfigurationManager.AppSettings["authKey"]);
            
            switch (req.Method.ToString().ToLower())
            {
                case "post":
                    string data = await req.Content.ReadAsStringAsync();
                    var json = JsonConvert.DeserializeObject(data);
                    return req.CreateResponse(HttpStatusCode.Created, json);
                case "get":
                    try
                    {
                        Document document =
                            await client.ReadDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id));
                        return req.CreateResponse(HttpStatusCode.OK, document);
                    }
                    catch (DocumentClientException e)
                    {
                        if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            return null;
                        }
                        else
                        {
                            throw;
                        }
                    }
                default:
                    return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}
