namespace Esfa.Recruit.Employer.Web.ModelBinders
{
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
    using System;

    public class TrimModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (!context.Metadata.IsComplexType && context.Metadata.ModelType == typeof(string))
            {
                return new TrimModelBinder(new SimpleTypeModelBinder(context.Metadata.ModelType));
            }
            return null;
        }
    }
}
