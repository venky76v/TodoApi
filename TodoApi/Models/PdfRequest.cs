namespace TodoApi.Models;

public class PdfRequest
{
    public string Name { get; set; }
    public string Std { get; set; }
    public List<FeesStructure> Fees { get; set; }
    public DateTime Date { get; set; }
}
