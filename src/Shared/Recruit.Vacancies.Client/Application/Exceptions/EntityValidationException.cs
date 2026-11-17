using System;
using System.Runtime.Serialization;
using Esfa.Recruit.Vacancies.Client.Application.Validation;

namespace Esfa.Recruit.Vacancies.Client.Application.Exceptions;

[Serializable]
public class EntityValidationException : Exception
{
    public EntityValidationResult ValidationResult { get; private set; }

    public EntityValidationException() { }
    public EntityValidationException(string message) : base(message) { }
    public EntityValidationException(string message, EntityValidationResult validationResult) : base(message) 
    {
        ValidationResult = validationResult;
    }

    public EntityValidationException(string message, EntityValidationResult validationResult, Exception inner) : base(message, inner)
    {
        ValidationResult = validationResult;
    }

    public EntityValidationException(string message, Exception inner) : base(message, inner) { }

    protected EntityValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        ValidationResult = (EntityValidationResult)info.GetValue(nameof(ValidationResult), typeof(EntityValidationResult));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (info == null) 
        {
            throw new ArgumentNullException(nameof(info));
        }

        info.AddValue(nameof(ValidationResult), ValidationResult, typeof(EntityValidationResult));

        base.GetObjectData(info, context);
    }
}