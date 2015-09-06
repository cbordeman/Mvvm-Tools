using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmTools.Web.Attributes
{
    public class EnforceHttpsHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // if request is remote, enforce https
            if (request.RequestUri.Scheme != Uri.UriSchemeHttps)
            {
                return Task<HttpResponseMessage>.Factory.StartNew(
                    () =>
                    {
                        var response = new HttpResponseMessage(HttpStatusCode.Forbidden)
                        {
                            Content = new StringContent("HTTPS Required")
                        };

                        return response;
                    }, cancellationToken);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
