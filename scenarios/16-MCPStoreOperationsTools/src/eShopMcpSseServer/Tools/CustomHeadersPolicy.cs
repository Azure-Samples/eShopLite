using Azure.Core;
using Azure.Core.Pipeline;

namespace eShopMcpSseServer.Tools;

internal class CustomHeadersPolicy : HttpPipelineSynchronousPolicy
{
    public override void OnSendingRequest(HttpMessage message)
    {
        message.Request.Headers.Add("x-ms-enable-preview", "true");
    }
}
