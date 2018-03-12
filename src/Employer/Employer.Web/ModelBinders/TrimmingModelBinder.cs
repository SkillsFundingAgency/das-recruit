using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.ModelBinders
{
    /// <summary>
    /// Taken from https://vikutech.blogspot.co.uk/2018/02/trim-text-in-mvc-core-through-model-binder.html
    /// </summary>
    public class TrimmingModelBinder : IModelBinder
    {
        private readonly IModelBinder FallbackBinder;
        public TrimmingModelBinder(IModelBinder fallbackBinder)
        {
            FallbackBinder = fallbackBinder ?? throw new ArgumentNullException(nameof(fallbackBinder));
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (valueProviderResult != null && valueProviderResult.FirstValue is string str && !string.IsNullOrEmpty(str))
            {
                bindingContext.Result = ModelBindingResult.Success(str.Trim());
                return Task.CompletedTask;
            }

            return FallbackBinder.BindModelAsync(bindingContext);
        }
    }
}
