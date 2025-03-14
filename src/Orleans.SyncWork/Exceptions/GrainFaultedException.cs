using System;

namespace Orleans.SyncWork.Exceptions;

/// <summary>
/// Wrapping exception thrown when an exception is encountered during grain execution worker code.
/// </summary>
/// <param name="innerException"></param>
[GenerateSerializer]
public class GrainFaultedException(Exception innerException) : Exception("The grain reported a fault, see inner exception for details", innerException);
