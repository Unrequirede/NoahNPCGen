using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net;
using System.IO;

namespace NoahNPCGen.Pages
{
    public class CharacterModel : PageModel
    {
        public string displayName = "", displayRace = "", displayClass = "", displaySubClass = "", displayBackG = "", displayAlignment = "";
        public int displayLevel = 0;
        public int displayExp;
        public void OnGetSingleOrder(string charName, string charRace, string charClass, string charSubClass, int charLevel, string charBackG, string CharAlignment)
        {
            displayName = charName;
            displayRace = charRace;
            displayClass = charClass;
            displaySubClass = charSubClass;
            displayLevel = charLevel;
            displayBackG = charBackG;
            displayAlignment = CharAlignment;
            displayExp = displayLevel * 5;
        }
        public Dictionary<string, dynamic> LoadAPI(string url)
        {
            WebRequest request = WebRequest.Create("https://www.dnd5eapi.co/api/" + url);
            request.Method = "GET";
            using var webStream = request.GetResponse().GetResponseStream();

            using var reader = new StreamReader(webStream);
            var data = reader.ReadToEnd();

            return JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(data);
        }

        public void OnGet()
        {
        }
    }
}
