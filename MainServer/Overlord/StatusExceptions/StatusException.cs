namespace Overlord.StatusExceptions;

public class StatusException(int statusNumber, string message) : Exception(message)
{
    public int StatusNumber { get; private set; } = statusNumber;
}

public class NotFound404Exception(string message) : StatusException(404, message);
public class BadRequest400Exception(string message) : StatusException(400, message);
public class SomethingWentWrong500Exception(string message) : StatusException(500, message);
public class ImATeapot418Exception(string message) : StatusException(418, message);
