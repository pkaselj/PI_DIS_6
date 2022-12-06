using HttpProgramming.Model;

namespace HttpProgramming.ManualRestApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var server = new ManualRestServer(32000, new StudentsRepository(10));
        await server.Listen();
    }
}