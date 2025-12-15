using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using PocMsGateway.DTOs;

public class DeleteTaskHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.Method == HttpMethod.Delete)
        {
            var query = QueryHelpers.ParseQuery(request.RequestUri!.Query);

            if (!query.TryGetValue("id", out var idValue) ||
                !int.TryParse(idValue, out var taskId))
            {
                throw new InvalidOperationException("O id é obrigatório.");
            }

            var body = new DeleteTaskRequest
            {
                TaskId = taskId
            };

            var json = JsonSerializer.Serialize(body);

            request.Method = HttpMethod.Post;
            request.Content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
