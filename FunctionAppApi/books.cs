using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Data.OData.Atom;
using FunctionApp.Models;

namespace FunctionApp
{
    public static class books
    {
        private static readonly string DatabaseId = ConfigurationManager.AppSettings["database"];
        private static readonly string CollectionId = ConfigurationManager.AppSettings["collection"];
        private static DocumentClient client;

        [FunctionName("books")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route="books/{isbn?}")]HttpRequestMessage req, string isbn, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            client = new DocumentClient(new Uri(ConfigurationManager.AppSettings["endpoint"]), ConfigurationManager.AppSettings["authKey"]);
  
            switch (req.Method.ToString().ToLower())
            {
                case "post":
                    string data = await req.Content.ReadAsStringAsync();
                    var json = JsonConvert.DeserializeObject(data);
                    ResourceResponse<Document> response = await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId), json);
                    return req.CreateResponse(HttpStatusCode.Created, response);
                case "get":
                    // Check if a predicated get.
                    var queryString = req.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value);
                    if (queryString.ContainsKey("categories"))
                    {
                        KeyValuePair<string, string> kv = queryString.Single(qs => qs.Key == "categories");
                        string cats = kv.Value;

                        IDocumentQuery<Book> query = client.CreateDocumentQuery<Book>(
                                UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId),
                                new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true })
                            .Where(book => book.categories.Contains(cats))
                            .AsDocumentQuery();

                        List<Book> results = new List<Book>();

                        while (query.HasMoreResults)
                        {
                            results.AddRange(await query.ExecuteNextAsync<Book>());
                        }

                        if (results.Count > 0)
                        {
                            return req.CreateResponse(HttpStatusCode.OK, results);
                        }
                        else
                        {
                            return req.CreateResponse(HttpStatusCode.NotFound);
                        }
                    }
                    else
                    {
                        // Non predicated get.
                        try
                        {
                            Document document =
                                await client.ReadDocumentAsync(
                                    UriFactory.CreateDocumentUri(DatabaseId, CollectionId, isbn),
                                    new RequestOptions {PartitionKey = new PartitionKey(isbn)});
                            return req.CreateResponse(HttpStatusCode.OK, document);
                        }
                        catch (DocumentClientException e)
                        {
                            if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                            {
                                return req.CreateResponse(HttpStatusCode.NotFound);
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                default:
                    return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}
