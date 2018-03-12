using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;

namespace Esfa.Recruit.Employer.Web.ModelBinders
{
    public class TrimmingModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (!context.Metadata.IsComplexType && context.Metadata.ModelType == typeof(string))
            {
                return new TrimmingModelBinder(new SimpleTypeModelBinder(context.Metadata.ModelType));
            }
            return null;
        }
    }
}
