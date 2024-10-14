namespace Bible.Dto;

// For Sending Data to User Home Page
public class CardDto
{
    public int Id { get; set; }
    public string Verse { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}
public class HomeDto
{
    public string Owner { get; set; } = string.Empty;
    public string Search { get; set; } = string.Empty;
    public List<CardDto> Cards { get; set; } = new List<CardDto>() ?? throw new Exception();
    public string Origin { get; set; } = string.Empty;
}

// For Sending Data to User Collection Page
public class CardAndNoteDto
{
    public int Id { get; set; }
    public string Verse { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
}
public class CollectionDto
{
    public string Owner { get; set; } = string.Empty;
    public List<CardAndNoteDto> CardAndNote { get; set; } = new List<CardAndNoteDto>() ?? throw new Exception();
    public string Origin { get; set; } = string.Empty;
}

// For Sending Bible Data to User Bible Page
public class BookDto
{
    public string BookName { get; set; } = string.Empty;
    public int ChapterCount { get; set; }
}
public class BibleDto
{
    public string Owner { get; set; } = string.Empty;
    public List<BookDto> NewTestament { get; set; } = new List<BookDto>() ?? throw new Exception();
    public List<BookDto> OldTestament { get; set; } = new List<BookDto>() ?? throw new Exception();
    public string Origin { get; set; } = string.Empty;
}







