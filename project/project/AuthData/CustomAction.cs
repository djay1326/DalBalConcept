using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace project.AuthData
{
    public class CustomAction : Attribute, IActionFilter // here "Attribute" plays an important part majorly
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            Debug.WriteLine("This is onActionExecuted");
            var controller = context.Controller as Controller;
            if (controller == null) return;
            controller.ViewBag.msgdata = "This message comes from onActionExecuted function"; // Line 16 to 18 sets Viewbag for this custom Action Filter
            //context.Controller.ViewBag.msg = "This is onActionExecuted";
            //throw new NotImplementedException();
        }

        public void OnActionExecuting(ActionExecutingContext context)
         {
            Debug.WriteLine("This is onActionExecuting");
            //throw new NotImplementedException();
        }
    }
}
