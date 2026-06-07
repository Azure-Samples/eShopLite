internal sealed class FoundryLocalModelCatalogOptions
{
    public string SelectedModel { get; set; } = string.Empty;

    public Dictionary<string, FoundryLocalModelCatalogEntry> Models { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

internal sealed class FoundryLocalModelCatalogEntry
{
    public string ModelAlias { get; set; } = string.Empty;

    public string? DisplayName { get; set; }

    public string? Description { get; set; }

    public bool? DownloadIfMissing { get; set; }

    public bool? UnloadOnExit { get; set; }
}
