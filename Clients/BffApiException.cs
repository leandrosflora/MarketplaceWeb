using System.Net;

namespace Marketplace.Web.Clients;

public sealed class BffApiException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public BffApiException(HttpStatusCode statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
