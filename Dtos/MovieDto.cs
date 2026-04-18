namespace MovieCatalogApiTests.Dtos;

public class MovieDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PosterUrl { get; set; } = string.Empty;
    public string TrailerLink { get; set; } = string.Empty;
    public bool IsWatched { get; set; }
}
