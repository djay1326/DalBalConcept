using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project.AuthData
{
    public class CustomExceptionFilter : Attribute, IExceptionFilter // here "Attribute" plays an important part majorly
    {
        private readonly IModelMetadataProvider _modelMetadataProvider;

        public CustomExceptionFilter(
            IModelMetadataProvider modelMetadataProvider)
        {
            _modelMetadataProvider = modelMetadataProvider;
        }

        public void OnException(ExceptionContext context)
        {
            var result = new ViewResult { ViewName = "CustomError" };
            result.ViewData = new ViewDataDictionary(_modelMetadataProvider, context.ModelState);
            result.ViewData.Add("Exception", context.Exception);

            // Here we can pass additional detailed data via ViewData
            context.ExceptionHandled = true; // mark exception as handled
            context.Result = result;
        }
        // Note: I have added services.AddControllersWithViews(config =>
        //                    config.Filters.Add(typeof(CustomExceptionFilter)));
        // in startup.cs file just for this exception filter.
    }
}
