namespace HttpProgramming.Model;

public class Subject
{
    public Subject(int id, string name, int ect)
    {
        Id = id;
        Name = name;
        Ect = ect;
    }

    public int Id { get; }

    public string Name { get; }

    public int Ect { get; }
}