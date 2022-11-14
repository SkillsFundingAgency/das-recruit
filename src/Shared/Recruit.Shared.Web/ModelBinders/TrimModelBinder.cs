using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Esfa.Recruit.Shared.Web.ModelBinders
{
    public class TrimModelBinder : IModelBinder
    {
        private readonly IModelBinder _fallbackBinder;

        public TrimModelBinder(IModelBinder fallbackBinder)
        {
            _fallbackBinder = fallbackBinder ?? throw new ArgumentNullException(nameof(fallbackBinder));
        }
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (valueProviderResult.FirstValue is string str &&
                !string.IsNullOrWhiteSpace(str))
            {
                bindingContext.Result = ModelBindingResult.Success(str.Trim());
                return Task.CompletedTask;
            }
            return _fallbackBinder.BindModelAsync(bindingContext);
        }
    }
}
