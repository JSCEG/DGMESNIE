using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace NSIE.Filters
{
    public class InvariantDecimalModelBinder : IModelBinder
    {
        private readonly DecimalModelBinder _baseBinder;

        public InvariantDecimalModelBinder(Type modelType, ILoggerFactory loggerFactory)
        {
            _baseBinder = new DecimalModelBinder(NumberStyles.Any, loggerFactory);
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null) throw new ArgumentNullException(nameof(bindingContext));

            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == ValueProviderResult.None)
            {
                return _baseBinder.BindModelAsync(bindingContext);
            }

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

            var value = valueProviderResult.FirstValue;
            if (string.IsNullOrWhiteSpace(value))
            {
                return _baseBinder.BindModelAsync(bindingContext);
            }

            // Normalizar separadores decimales: convertir comas a puntos para procesar con InvariantCulture
            var normalizedValue = value.Replace(',', '.').Trim();

            if (decimal.TryParse(normalizedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                bindingContext.Result = ModelBindingResult.Success(result);
                return Task.CompletedTask;
            }

            // Si falla la conversión personalizada, dejar que el binder base intente parsear (para reportar el error estándar)
            return _baseBinder.BindModelAsync(bindingContext);
        }
    }

    public class InvariantDecimalModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var modelType = context.Metadata.ModelType;

            if (modelType == typeof(decimal) || modelType == typeof(decimal?))
            {
                var loggerFactory = (ILoggerFactory)context.Services.GetService(typeof(ILoggerFactory));
                return new InvariantDecimalModelBinder(modelType, loggerFactory);
            }

            return null;
        }
    }
}
