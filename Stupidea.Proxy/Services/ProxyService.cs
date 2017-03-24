using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;

namespace Stupidea.Proxy.Services
{
    public interface IProxyService
    {
        void Start();

        void Stop();
    }

    public class ProxyService : IProxyService
    {
        private const string randomArticleUrl = @"https://en.wikipedia.org/w/api.php?action=query&format=json&generator=random&grnnamespace=0&prop=info&inprop=url";

        private readonly ProxyServer server;

        private bool shouldRedirect = false;

        public ProxyService()
        {
            server = new ProxyServer("Stupidea Proxy", "Stupidea Proxy")
            {
                TrustRootCertificate = true
            };
        }

        public void Start()
        {
            var endpoint = new ExplicitProxyEndPoint(IPAddress.Any, 8000, true);

            server.AddEndPoint(endpoint);

            server.BeforeRequest += OnRequest;
            server.Start();

            server.SetAsSystemHttpProxy(endpoint);
            server.SetAsSystemHttpsProxy(endpoint);
        }

        public void Stop()
        {
            server.BeforeRequest -= OnRequest;
            server.Stop();
        }

        private async Task OnRequest(object sender, SessionEventArgs e)
        {
            if (e.WebSession.Request
                            .RequestUri.AbsoluteUri.StartsWith("https://en.wikipedia.org/wiki/"))
            {
                try
                {
                    var request = WebRequest.Create(randomArticleUrl);
                    var response = await request.GetResponseAsync();

                    var body = default(string);

                    using (var reader = new StreamReader(response.GetResponseStream(),
                                                         Encoding.UTF8))
                    {
                        body = await reader.ReadToEndAsync();
                    }

                    var fullurl = JObject.Parse(body)["query"]["pages"]
                                         .First.Children().First()["fullurl"].ToString();

                    if (shouldRedirect)
                    {
                        await e.Redirect(fullurl);
                    }
                }
                catch
                {
                    // Failed? Whatever.
                }
                finally
                {
                    shouldRedirect = !shouldRedirect;
                }
            }
        }
    }
}