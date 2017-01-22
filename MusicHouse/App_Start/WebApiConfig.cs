using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using Neo4jClient;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis;

namespace MusicHouse.App_Start
{
    public static class WebApiConfig
    {
        public static IGraphClient GraphClient { get; private set; }

        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;

            var appXmlType = config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
            config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);

            //Use an IoC container and register as a Singleton
            var url = ConfigurationManager.AppSettings["http://localhost:7474/db/data"];
            var user = ConfigurationManager.AppSettings["neo4j"];
            var password = ConfigurationManager.AppSettings["napredneBaze"];
            var client = new GraphClient(new Uri("http://localhost:7474/db/data"), "neo4j", "napredneBaze");
            
            try
            {
                client.Connect();
            }
            catch(Exception ec)
            {
               HttpContext.Current.Application.Add("Exception", ec);
            }
            GraphClient = client;
            }

        public static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            string cacheConnection = ConfigurationManager.AppSettings["CacheConnection"].ToString();
            return ConnectionMultiplexer.Connect(cacheConnection);
            
        });

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }

    }
}