using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using HttpProgramming.Model;

namespace HttpProgramming.ManualRestApi;

public partial class ManualRestServer
{
    private readonly int _portNumber;
    private readonly StudentsRepository _studentsRepository;

    public ManualRestServer(int portNumber, StudentsRepository studentsRepository)
    {
        _portNumber = portNumber;
        _studentsRepository = studentsRepository;
    }

    public async Task Listen()
    {
        var listener = new HttpListener();
        listener.Prefixes.Add($"http://localhost:{_portNumber}/");

        listener.Start();

        Console.WriteLine("Server is listening...");
        try
        {
            while (true)
            {
                var context = await listener.GetContextAsync();

                Console.WriteLine("Server got request...");

                await HandleRequest(context);
            }
        }
        catch
        {
            listener.Stop();
        }
    }

    private async Task HandleRequest(HttpListenerContext httpListenerContext)
    {
        var request = httpListenerContext.Request;
        var response = httpListenerContext.Response;

        var requestMethod = request.HttpMethod;
        var requestUrl = request.Url?.AbsolutePath ?? string.Empty;
        var requestBody = await GetRequestBody(request);

        var (responseString, responseStatusCode) = GetAppropriateResponseForRequest(
            requestUrl: requestUrl,
            requestMethod: requestMethod,
            requestBody: requestBody);

        var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

        response.ContentLength64 = buffer.Length;
        response.StatusCode = (int)responseStatusCode;

        var output = response.OutputStream;
        await output.WriteAsync(buffer);

        output.Close();
    }

    private static T? Deserialize<T>(string serializedStudent)
    {
        var serializerOptions = new JsonSerializerOptions();
        serializerOptions.Converters.Add(new DateOnlyJSONSerializer());
        return JsonSerializer.Deserialize<T>(serializedStudent, serializerOptions);
    }

    private static string Serialize<T>(T model)
    {
        var serializerOptions = new JsonSerializerOptions();
        serializerOptions.Converters.Add(new DateOnlyJSONSerializer());
        return JsonSerializer.Serialize(model, serializerOptions);
    }

    private delegate (string responseData, HttpStatusCode) CallbackForRequest(
        string requestUrl,
        string requestMethod,
        string requestBody);

    private (string responseData, HttpStatusCode) ReturnNotFoundResponse(
        string requestUrl,
        string requestMethod,
        string requestBody)
    {
        return ("File not found", HttpStatusCode.NotFound);
    }
    private (string responseData, HttpStatusCode) GetAppropriateResponseForRequest(
        string requestUrl,
        string requestMethod,
        string requestBody)
    {
        CallbackForRequest requestHandler;
        switch (requestMethod)
        {
            case "GET":
                if (requestUrl == "/students")
                {
                    requestHandler = GetAllStudents;
                }
                else
                {
                    requestHandler = GetStudentWithId;
                }
                break;
            case "POST":
                requestHandler = CreateStudent;
                break;
            case "PUT":
                requestHandler = UpdateStudentWithId;
                break;
            case "DELETE":
                requestHandler = DeleteStudentWithId;
                break;
            default:
                requestHandler = ReturnNotFoundResponse;
                break;
        }

        try
        {
            return requestHandler(requestUrl, requestMethod, requestBody);
        }
        catch (Exception e)
        {
            return (e.Message, HttpStatusCode.InternalServerError);
        }
    }

    private (string responseData, HttpStatusCode) GetAllStudents(
        string requestUrl,
        string requestMethod,
        string requestBody)
    {
        var students = _studentsRepository.GetAllStudents();

        var responseData = Serialize(students);

        return (responseData, HttpStatusCode.OK);
    }
    private (string responseData, HttpStatusCode) GetStudentWithId(
        string requestUrl,
        string requestMethod,
        string requestBody)
    {
        var getStudentId = GetStudentIdFromInputUrl(requestUrl);
        if (!getStudentId.HasValue)
        {
            return ("Not Implemented", HttpStatusCode.NotImplemented);
        }

        var student = _studentsRepository.GetStudent(getStudentId.Value);

        if (student is null)
        {
            return ("Not Found", HttpStatusCode.NotFound);
        }

        var studentString = Serialize(student);

        return (studentString, HttpStatusCode.OK);
    }
    private (string responseData, HttpStatusCode) CreateStudent(
        string requestUrl,
        string requestMethod,
        string requestBody)
    {
        if (String.IsNullOrEmpty(requestBody))
        {
            return ("Bad Request", HttpStatusCode.BadRequest);
        }

        var name = requestBody;

        _studentsRepository.CreateStudent(name);

        return ("Created", HttpStatusCode.Created);

    }
    private (string responseData, HttpStatusCode) UpdateStudentWithId(
        string requestUrl,
        string requestMethod,
        string requestBody)
    {
        var getStudentId = GetStudentIdFromInputUrl(requestUrl);
        if (!getStudentId.HasValue)
        {
            return ("Not Implemented", HttpStatusCode.NotImplemented);
        }

        var student = _studentsRepository.GetStudent(getStudentId.Value);

        if (student is null)
        {
            return ("Not Found", HttpStatusCode.NotFound);
        }

        if (String.IsNullOrEmpty(requestBody))
        {
            return ("Bad Request", HttpStatusCode.BadRequest);
        }

        var name = requestBody;

        _studentsRepository.UpdateStudent((int)getStudentId, name);

        return ("OK", HttpStatusCode.OK);
    }
    private (string responseData, HttpStatusCode) DeleteStudentWithId(
        string requestUrl,
        string requestMethod,
        string requestBody)
    {
        var getStudentId = GetStudentIdFromInputUrl(requestUrl);
        if (!getStudentId.HasValue)
        {
            return ("Not Implemented", HttpStatusCode.NotImplemented);
        }

        var student = _studentsRepository.GetStudent(getStudentId.Value);

        if (student is null)
        {
            return ("Not Found", HttpStatusCode.NotFound);
        }

        _studentsRepository.DeleteStudent((int)getStudentId);

        return ("OK", HttpStatusCode.OK);
    }

    private static async Task<string> GetRequestBody(HttpListenerRequest request)
    {
        if (!request.HasEntityBody)
            return string.Empty;

        var body = request.InputStream;
        var encoding = request.ContentEncoding;
        var reader = new StreamReader(body, encoding);

        var requestBodyString = await reader.ReadToEndAsync();

        return requestBodyString;
    }

    private static int? GetStudentIdFromInputUrl(string requestUrl)
    {
        var splittedString = requestUrl.Split("/");
        return splittedString.Length != 3 ? null : int.TryParse(splittedString[2], out var studentId) ? studentId : null;
    }
}