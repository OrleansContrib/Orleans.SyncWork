using System;
using BCrypt.Net;

namespace Orleans.SyncWork.Tests.SerializationSurrogates;

[GenerateSerializer]
public struct SaltParseExceptionSerializationSurrogate
{
    [Id(0)]
    public string Message { get; set; }
    [Id(1)]
    public Exception? InnerException { get; set; }
}

[RegisterConverter]
public sealed class MyForeignLibraryValueTypeSurrogateConverter :
    IConverter<SaltParseException, SaltParseExceptionSerializationSurrogate>
{
    public SaltParseException ConvertFromSurrogate(
        in SaltParseExceptionSerializationSurrogate surrogate) =>
        new(surrogate.Message, surrogate.InnerException);

    public SaltParseExceptionSerializationSurrogate ConvertToSurrogate(
        in SaltParseException value) =>
        new() { Message = value.Message, InnerException = value.InnerException };
}
