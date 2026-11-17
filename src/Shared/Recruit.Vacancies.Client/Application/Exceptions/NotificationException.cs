using System;
using System.Runtime.Serialization;

namespace Esfa.Recruit.Vacancies.Client.Application.Exceptions;

[Serializable]
public class NotificationException : Exception
{
    public NotificationException() { }
    public NotificationException(string message) : base(message) { }

    public NotificationException(string message, Exception inner) : base(message, inner) { }

    protected NotificationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}