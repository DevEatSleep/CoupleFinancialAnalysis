namespace BlazorFrontend.Models;

public class PersonData
{
    public string Name { get; set; } = "-";
    public string Age { get; set; } = "-";
    public string Salary { get; set; } = "-";
    public int? NameId { get; set; }
    public int? AgeId { get; set; }
    public int? SalaryId { get; set; }
}
