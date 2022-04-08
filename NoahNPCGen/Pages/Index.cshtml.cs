using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.IO;

namespace NoahNPCGen.Pages
{
    public class IndexModel : PageModel
    {
        public Dictionary<string, dynamic> LoadAPI(string url)
        {
            WebRequest request = WebRequest.Create("https://www.dnd5eapi.co/api/" + url);
            request.Method = "GET";
            using var webStream = request.GetResponse().GetResponseStream();

            using var reader = new StreamReader(webStream);
            var data = reader.ReadToEnd();

            return JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(data);
        }

        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }
        public string pizza = "0";
        public IActionResult OnPost(string selectName, string selectRace, string selectClass, string selectSubclass, int selectLevel, string selectBackG, string selectAlignment)
        {
            return RedirectToPage("Character", "SingleOrder", new { charName = selectName, charRace = selectRace, charClass = selectClass, charSubClass = selectSubclass, charLevel = selectLevel + 1, charBackG = selectBackG, charAlignment = selectAlignment });
        }
        public void OnGet()
        {

        }

    }
}
