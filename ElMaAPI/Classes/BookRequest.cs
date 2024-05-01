namespace ElMaAPI;

public class BookRequest
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string SeriesName { get; set; }
    public string?[] AuthorBook { get; set; }
    public string?[] Editor { get; set; }
    public string Annotation { get; set; }
    public string Publisher { get; set; }
    public string PlaceOfPublication { get; set; }
    public int YearOfPublication { get; set; }
    public string BBK { get; set; }
    public List<int> Themes { get; set; }
    public byte[] Image { get; set; }
    public string ImageName { get; set; }
}