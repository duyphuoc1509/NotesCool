namespace NotesCool.Shared.Errors;

public class ApiException : Exception
{
    public int StatusCode { get; }
    public string Code { get; }
    public ApiException(string code, string message, int statusCode = 400) : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }
}
