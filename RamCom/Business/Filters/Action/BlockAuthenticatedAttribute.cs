using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Filters.Action
{
    public class BlockAuthenticatedAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var user = context.HttpContext.User;
            if (user != null && user.Identity.IsAuthenticated)
            {
                context.Result = new RedirectToActionResult("Index", "Home", new { area = "" });
            }
               
        }
    }
}
