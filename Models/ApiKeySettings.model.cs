public class ApiKeySettings
{
    public bool Enable { get; set; }
    public List<ApiClientKey> Keys { get; set; }
}

public class ApiClientKey
{
    public string Id { get; set; }
    public string Client { get; set; }
    public string Key { get; set; }
    public List<string> Scopes { get; set; }
}
