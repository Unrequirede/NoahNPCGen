using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NoahNPCGen.Pages
{
    public class TestModel : PageModel
    {
        public string totalMessage = "";
        public void OnPost()
        {
            var name = Request.Form["Name"];
            var email = Request.Form["Email"];
            ViewData["confirmation"] = $"{name}, information will be sent to {email}";
        }

        public void OnGet()
        {
        }
    }
}
