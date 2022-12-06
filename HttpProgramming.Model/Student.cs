namespace HttpProgramming.Model;

public class Student
{
    public Student(
        int id,
        string name,
        DateOnly dob,
        IEnumerable<double> grades,
        IEnumerable<Subject> subjects)
    {
        Id = id;
        Name = name;
        Dob = dob;
        Grades = grades;
        Subjects = subjects;
    }

    public int Id { get; }

    public string Name { get; }

    public DateOnly Dob { get; }

    public IEnumerable<double> Grades { get; }

    public IEnumerable<Subject> Subjects { get; }
}