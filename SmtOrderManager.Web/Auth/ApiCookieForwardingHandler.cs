using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace SmtOrderManager.Web.Auth;

public sealed class ApiCookieForwardingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _ctx;

    public ApiCookieForwardingHandler(IHttpContextAccessor ctx) => _ctx = ctx;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var cookie = _ctx.HttpContext?.Request.Headers.Cookie.ToString();
        if (!string.IsNullOrWhiteSpace(cookie) && !request.Headers.Contains(HeaderNames.Cookie))
        {
            request.Headers.Add(HeaderNames.Cookie, cookie);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
