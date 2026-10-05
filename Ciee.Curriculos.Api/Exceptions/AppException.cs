using System;

namespace Ciee.Curriculos.Api.Exceptions;

public abstract class AppException(string message, string title, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string Title { get; } = title;
}
