using Elastic.Clients.Elasticsearch;

public class ApplicationSearchService
{
    private const string IndexName = "applications";
    private readonly ElasticsearchClient _client;
    private readonly ILogger<ApplicationSearchService> _logger;

    public ApplicationSearchService(ElasticsearchClient client, ILogger<ApplicationSearchService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task IndexAsync(ApplicationDoc doc)
    {
        try
        {
            var response = await _client.IndexAsync(doc, i => i.Index(IndexName).Id(doc.Id));
            if (!response.IsValidResponse)
            {
                _logger.LogWarning("ElasticSearch index failed.  {Debug} ", response.DebugInformation);
            }
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Elasticsearch index failed");}
    }

    public async Task DeleteAsync(int id)
    {
        try
        {
            var response = await _client.DeleteAsync(IndexName, id);
            if(!response.IsValidResponse)
            {
                _logger.LogWarning("ElasticSearch delete failed: {Debug}", response.DebugInformation);
            }
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Elasticsearch delete failed");}
    }

    public async Task<List<ApplicationDoc>> SearchAsync(string userId, string text, string? status, int size = 25)
    {
        var response = await _client.SearchAsync<ApplicationDoc>( s => s
            .Indices(IndexName)
            .Size(size)
            .Query(q => q
                .Bool(b => b
                    .Must(m => m
                        .MultiMatch(mm => mm
                            .Query(text)
                            .Fields(new[] { "company", "title", "notes"})))
                    .Filter(f => f
                        .Term(t => t.Field("userId.keyword").Value(userId))))));
        
        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Elasticsearch search failed: {response.DebugInformation}");
        }
        return response.Documents.ToList();
    }

    public async Task ReindexAllAsync(IEnumerable<ApplicationDoc> docs)
    {
        var response = await _client.BulkAsync(b => b.Index(IndexName).IndexMany(docs));
        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Bulk index failed: {response.DebugInformation}");
        }
    }

}