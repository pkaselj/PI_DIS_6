namespace HttpProgramming.Model;

public class StudentsRepository
{

    private readonly IEnumerable<Subject> _subjects = new[]
    {
        new Subject(1, "PIZ", 10),
        new Subject(2, "DIS", 12),
    };

    private readonly ICollection<Student> _students;

    public StudentsRepository(int numberOfStudents)
    {
        _students = Enumerable
            .Range(0, numberOfStudents)
            .Select(index => new Student(
                id: index,
                name: $"Student{index}",
                dob: new DateOnly(1999, 1, 1),
                grades: new[] { 2d, 3d, 4d, 4d },
                subjects: _subjects))
            .ToList();
    }

    public IEnumerable<Student> GetAllStudents()
    {
        return _students;
    }

    public Student? GetStudent(int id)
    {
        return _students.FirstOrDefault(student => student.Id == id);
    }

    public void CreateStudent(string name)
    {
        var id = _students.Max(s => s.Id) + 1;

        var student = new Student(
            id: id,
            name: name,
            dob: new DateOnly(1999, 1, 1),
            grades: new[] { 2d, 5d, 3d, 4d },
            subjects: _subjects);

        _students.Add(student);
    }

    public void UpdateStudent(int id, string newName)
    {
        var studentToUpdate = _students.FirstOrDefault(student => student.Id == id);

        if (studentToUpdate is null)
        {
            throw new Exception("Student was not found");
        }

        var updatedStudent = new Student(
            id: studentToUpdate.Id,
            name: newName,
            dob: studentToUpdate.Dob,
            grades: studentToUpdate.Grades,
            subjects: studentToUpdate.Subjects);

        _ = _students.Remove(studentToUpdate);

        _students.Add(updatedStudent);
    }

    public void DeleteStudent(int id)
    {
        var studentToDelete = _students.FirstOrDefault(student => student.Id == id);

        if (studentToDelete is null)
        {
            throw new Exception("Student was not found");
        }

        _ = _students.Remove(studentToDelete);
    }
}
