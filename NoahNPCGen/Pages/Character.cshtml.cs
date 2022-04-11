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
        public static Random rnd = new Random();
        public string displayName = "", displayRace = "", displayClass = "", displaySubClass = "", displayBackG = "", displayAlignment = "";
        public int displayLevel = -1, displayExp = -1, displayStr = -1, displayDex = -1, displayCon = -1, displayInt = -1, displayWis = -1, displayCha = -1, displayStrMod = -1, displayDexMod = -1, displayConMod = -1, displayIntMod = -1, displayWisMod = -1, displayChaMod = -1, displayProf = -1, displayHitDice = -1, displayHP = -1;
        public string strSav, dexSav, conSav, intSav, wisSav, chaSav, acroPro, animPro, arcaPro, athlPro, decePro, histPro, insiPro, intiPro, invePro, mediPro, natuPro, percPro, perfPro, persPro, reliPro, sleiPro, steaPro, survPro;
        public List<string> otherProf = new List<string>();
        public List<string> itemSelect = new List<string>();

        public void OnGetSingleOrder(string charName, string charRace, string charClass, string charSubClass, int charLevel, string charBackG, string CharAlignment)
        {
            displayName = charName;
            displayRace = charRace;
            displayClass = charClass;
            displaySubClass = charSubClass;
            displayLevel = charLevel;
            displayBackG = charBackG;
            displayAlignment = CharAlignment;
            displayHitDice = (int)LoadAPI("classes/" + displayClass.ToLower())["hit_die"];
            GetStats();
            GetProf();
            GetLevelInfo(charLevel);
            GetEquiptment();
            GetAttacks();
            
        }

        private void GetAttacks()
        {
            foreach ()
        }

        private void GetEquiptment()
        {
            foreach (dynamic item in LoadAPI("classes/" + displayClass.ToLower())["starting_equipment"])
            {
                itemSelect.Add(item["equipment"]["name"].ToString() + " x" + item["quantity"].ToString());
            }
            foreach (dynamic choice in LoadAPI("classes/" + displayClass.ToLower())["starting_equipment_options"])
            {
                List<dynamic> itemOptions = new List<dynamic>();
                foreach (dynamic item in choice["from"])
                    itemOptions.Add(item);
                for (int i = 0; i < (int)choice["choose"]; i++)
                {
                    int ranOpt = rnd.Next(0, itemOptions.Count);
                    dynamic middleMan = itemOptions.ElementAt(ranOpt); //middle man is random object from classes/*class*/["starting_equipment_options"]*object*[from]
                    try
                    {
                        try
                        {
                            itemSelect.Add(middleMan["equipment"]["name"].ToString() + " x" + middleMan["quantity"]);
                        }
                        catch
                        {
                            List<string> itemType = new List<string>();
                            try
                            {
                                foreach (dynamic weapon in LoadAPI("equipment-categories/" + middleMan["equipment_option"]["from"]["equipment_category"]["index"].ToString())["equipment"])
                                    itemType.Add(weapon["name"].ToString());
                                for (int j = 0; j < (int)middleMan["equipment_option"]["choose"]; j++)
                                {
                                    int ranItem = rnd.Next(0, itemType.Count);
                                    itemSelect.Add(itemType.ElementAt(ranItem) + " x1");
                                }
                            }
                            catch
                            {
                                Console.WriteLine("TRY: " + middleMan.ToString());
                                foreach (dynamic item in LoadAPI("equipment-categories/" + middleMan["equipment_category"]["index"].ToString())["equipment"])
                                    itemType.Add(item["name"].ToString());
                                int ranItem = rnd.Next(0, itemType.Count);
                                itemSelect.Add(itemType.ElementAt(ranItem) + " x1");
                                Console.WriteLine("GOT: " + itemType.ElementAt(ranItem));
                            }
                        }
                    }
                    catch
                    {
                        string ranSet = rnd.Next(0, itemOptions.Count).ToString();
                        try
                        {
                            itemSelect.Add(middleMan[ranSet]["equipment"]["name"].ToString() + " x" + middleMan[ranSet]["quantity"]);
                        }
                        catch
                        {
                            List<string> itemType = new List<string>();
                            try
                            {
                                foreach (dynamic weapon in LoadAPI("equipment-categories/" + middleMan[ranSet]["equipment_option"]["from"]["equipment_category"]["index"].ToString())["equipment"])
                                    itemType.Add(weapon["name"].ToString());
                                for (int j = 0; j < (int)middleMan["equipment_option"]["choose"]; j++)
                                {
                                    int ranItem = rnd.Next(0, itemType.Count);
                                    itemSelect.Add(itemType.ElementAt(ranItem) + " x1");
                                }
                            }
                            catch
                            {
                                foreach (dynamic item in LoadAPI("equipment-categories/" + middleMan[ranSet]["equipment_category"]["index"].ToString())["equipment"])
                                    itemType.Add(item["name"].ToString());
                                for (int j = 0; j < (int)middleMan["equipment_option"]["choose"]; j++)
                                {
                                    int ranItem = rnd.Next(0, itemType.Count);
                                    itemSelect.Add(itemType.ElementAt(ranItem) + " x1");
                                }
                            }
                        }
                        itemOptions.RemoveAt(ranOpt);
                    }
                }
            }
        }

        public string AllItems()
        {
            string result = "";
            foreach (string item in itemSelect)
                result += item + "\n";
            return result;
        }

        private void GetLevelInfo(int charLevel)
        {
            if (charLevel >= 1)
            {
                displayExp = 0;
                displayProf = 2;
                displayHP = displayHitDice + displayConMod;
            }
            if (charLevel >= 2)
            {
                displayExp = 300;
                displayHP += LevelUpHP(rnd.Next(displayHitDice), displayConMod);
            }
            if (charLevel >= 3)
            {
                displayExp = 900;
                displayHP += LevelUpHP(rnd.Next(displayHitDice), displayConMod);
            }
            if (charLevel >= 4)
            {
                displayExp = 2700;
                displayHP += LevelUpHP(rnd.Next(displayHitDice), displayConMod);
            }
            if (charLevel >= 5)
            {
                displayExp = 6500;
                displayProf = 3;
                displayHP += LevelUpHP(rnd.Next(displayHitDice), displayConMod);
            }
            if (charLevel >= 6)
            {
                displayExp = 14000;
                displayHP += LevelUpHP(rnd.Next(displayHitDice), displayConMod);
            }
            if (charLevel >= 7)
            {
                displayExp = 23000;
                displayHP += LevelUpHP(rnd.Next(displayHitDice), displayConMod);
            }
            if (charLevel >= 8)
            {
                displayExp = 34000;
                displayHP += LevelUpHP(rnd.Next(displayHitDice), displayConMod);
            }
            if (charLevel >= 9)
            {
                displayExp = 48000;
                displayHP += LevelUpHP(rnd.Next(displayHitDice), displayConMod);
                displayProf = 4;
            }
            if (charLevel >= 10)
            {
                displayExp = 64000;
                displayHP += LevelUpHP(rnd.Next(displayHitDice), displayConMod);
            }
            if (charLevel >= 11)
            {
                displayExp = 85000;
                displayHP += LevelUpHP(rnd.Next(displayHitDice), displayConMod);
            }
            if (charLevel >= 12)
            {
                displayExp = 100000;
                displayHP += LevelUpHP(rnd.Next(displayHitDice), displayConMod);
            }
            if (charLevel >= 13)
            {
                displayExp = 120000;
                displayHP += LevelUpHP(rnd.Next(displayHitDice), displayConMod);
            }
            if (charLevel >= 14)
            {
                displayExp = 140000;
                displayHP += LevelUpHP(rnd.Next(displayHitDice), displayConMod);
            }
            if (charLevel >= 15)
            {
                displayExp = 165000;
                displayHP += LevelUpHP(rnd.Next(displayHitDice), displayConMod);
                displayProf = 5;
            }
            if (charLevel >= 16)
            {
                displayExp = 195000;
                displayHP += LevelUpHP(rnd.Next(displayHitDice), displayConMod);
            }
            if (charLevel >= 17)
            {
                displayExp = 225000;
                displayProf = 6;
                displayHP += LevelUpHP(rnd.Next(displayHitDice), displayConMod);
            }
            if (charLevel >= 18)
            {
                displayExp = 265000;
                displayHP += LevelUpHP(rnd.Next(displayHitDice), displayConMod);
            }
            if (charLevel >= 19)
            {
                displayExp = 305000;
                displayHP += LevelUpHP(rnd.Next(displayHitDice), displayConMod);
            }
            if (charLevel >= 20)
            {
                displayExp = 355000;
                displayHP += LevelUpHP(rnd.Next(displayHitDice), displayConMod);
            }
        }

        public int LevelUpHP(int die, int mod)
        {
            if (die + mod > 1)
                return die + mod;
            else
                return 1;
        }

        //checks API for a class's saving throws and proficiencies, making the ones that are, "checked" for input box
        private void GetProf()
        {
            if ("str" == LoadAPI("classes/" + displayClass.ToLower())["saving_throws"][0]["index"].ToString() || "str" == LoadAPI("classes/" + displayClass.ToLower())["saving_throws"][1]["index"].ToString())
                strSav = "checked";
            if ("dex" == LoadAPI("classes/" + displayClass.ToLower())["saving_throws"][0]["index"].ToString() || "dex" == LoadAPI("classes/" + displayClass.ToLower())["saving_throws"][1]["index"].ToString())
                dexSav = "checked";
            if ("con" == LoadAPI("classes/" + displayClass.ToLower())["saving_throws"][0]["index"].ToString() || "con" == LoadAPI("classes/" + displayClass.ToLower())["saving_throws"][1]["index"].ToString())
                conSav = "checked";
            if ("int" == LoadAPI("classes/" + displayClass.ToLower())["saving_throws"][0]["index"].ToString() || "int" == LoadAPI("classes/" + displayClass.ToLower())["saving_throws"][1]["index"].ToString())
                intSav = "checked";
            if ("wis" == LoadAPI("classes/" + displayClass.ToLower())["saving_throws"][0]["index"].ToString() || "wis" == LoadAPI("classes/" + displayClass.ToLower())["saving_throws"][1]["index"].ToString())
                wisSav = "checked";
            if ("cha" == LoadAPI("classes/" + displayClass.ToLower())["saving_throws"][0]["index"].ToString() || "cha" == LoadAPI("classes/" + displayClass.ToLower())["saving_throws"][1]["index"].ToString())
                chaSav = "checked";

            var profSele = new List<string>();
            //check if object begins with "skill-" string as the API sorts proficiencies inconsistently
            dynamic correctProf = null;
            foreach (dynamic profPoss in LoadAPI("classes/" + displayClass.ToLower())["proficiency_choices"])
            {
                if (profPoss["from"][0]["index"].ToString().Substring(0, 6) == "skill-")
                    correctProf = profPoss;
            }

            //adds each proficiency choice to list
            foreach (var profChoice in correctProf["from"])
                profSele.Add(profChoice["index"].ToString());

            //randomly assigns skill proficiencies as they are usually user prefereence
            for (int i = 0; i < (int)correctProf["choose"]; i++)
            {
                string chosenProf = profSele.ElementAt(rnd.Next(0, profSele.Count));
                switch (chosenProf)
                {
                    case "skill-acrobatics":
                        acroPro = "checked";
                        break;
                    case "skill-animal-handling":
                        animPro = "checked";
                        break;
                    case "skill-arcana":
                        arcaPro = "checked";
                        break;
                    case "skill-athletics":
                        athlPro = "checked";
                        break;
                    case "skill-deception":
                        decePro = "checked";
                        break;
                    case "skill-history":
                        histPro = "checked";
                        break;
                    case "skill-insight":
                        insiPro = "checked";
                        break;
                    case "skill-intimidation":
                        intiPro = "checked";
                        break;
                    case "skill-investigation":
                        invePro = "checked";
                        break;
                    case "skill-medicine":
                        mediPro = "checked";
                        break;
                    case "skill-nature":
                        natuPro = "checked";
                        break;
                    case "skill-perception":
                        percPro = "checked";
                        break;
                    case "skill-performance":
                        perfPro = "checked";
                        break;
                    case "skill-persuasion":
                        persPro = "checked";
                        break;
                    case "skill-religion":
                        reliPro = "checked";
                        break;
                    case "skill-sleight-of-hand":
                        sleiPro = "checked";
                        break;
                    case "skill-stealth":
                        steaPro = "checked";
                        break;
                    case "skill-survival":
                        survPro = "checked";
                        break;
                }
            }


            profSele = new List<string>();
            //check if object begins with "skill-" string as the API sorts proficiencies inconsistently
            correctProf = null;
            foreach (dynamic profPoss in LoadAPI("classes/" + displayClass.ToLower())["proficiency_choices"])
            {
                if (profPoss["from"][0]["index"].ToString().Substring(0, 6) != "skill-")
                {
                    correctProf = profPoss;
                    foreach (var profChoice in correctProf["from"])
                        profSele.Add(profChoice["name"].ToString());
                    //randomly assigns tool and instrument proficiencies as they are usually user prefereence
                    for (int i = 0; i < (int)correctProf["choose"]; i++)
                    {
                        otherProf.Add(profSele.ElementAt(rnd.Next(0, profSele.Count)));
                    }
                }
            }
            foreach (dynamic weapProf in LoadAPI("classes/" + displayClass.ToLower())["proficiencies"])
            {
                otherProf.Add(weapProf["name"].ToString());
            }
        }

        public string DisplayOtherProf()
        {
            string result = "";
            foreach (string prof in otherProf)
                result += prof + ", ";
            return result.Substring(0, result.Length-2);
        }

        //gets the bonus of a skill the character has proficiency in
        public int ProfBonus(int abl, string pro)
        {
            if (pro == "checked")
                return abl + displayProf;
            return abl;
        }
        public int ProfBonus(int abl, string pro, int init)
        {
            if (pro == "checked")
                return abl + displayProf + init;
            return abl + init;
        }

        //rolls six stats, one for each ability score, and assigns them based off class characteristics
        public void GetStats()
        {
            int[] rolledStats = new int[6];
            for (int i = 0; i < 6; i++)
            {
                int[] statRoll = { rnd.Next(1, 7), rnd.Next(1, 7), rnd.Next(1, 7), rnd.Next(1, 7) };
                rolledStats[i] = statRoll.Sum() - statRoll.Min();
            }
            
            //highest stats assigned to multiclass prerequisites as best indicator of important abilities
            try
            {
                FillStat(LoadAPI("classes/" + displayClass.ToLower() + "/multi-classing/")["prerequisites"][0]["ability_score"]["index"].ToString(), rolledStats);
                FillStat(LoadAPI("classes/" + displayClass.ToLower() + "/multi-classing/")["prerequisites"][1]["ability_score"]["index"].ToString(), rolledStats);
            }
            catch { }

            //next highest stat assigned to ability modifier (if any)
            try
            {
                FillStat(LoadAPI("classes/" + displayClass.ToLower() + "/spellcasting/")["spellcasting_ability"]["index"].ToString(), rolledStats);
            } catch { }

            //next assigned to saving throws
            try
            {
                FillStat(LoadAPI("classes/" + displayClass.ToLower())["saving_throws"][0]["index"].ToString(), rolledStats);
                FillStat(LoadAPI("classes/" + displayClass.ToLower())["saving_throws"][1]["index"].ToString(), rolledStats);
            } catch { }

            //rest assigned randomly, as they are usually subject to player preference
            FillStat("str", rolledStats);
            FillStat("dex", rolledStats);
            FillStat("con", rolledStats);
            FillStat("int", rolledStats);
            FillStat("wis", rolledStats);
            FillStat("cha", rolledStats);

            //get stat modifiers based on stats
            displayStrMod = GetAblMod(displayStr);
            displayDexMod = GetAblMod(displayDex);
            displayConMod = GetAblMod(displayCon);
            displayIntMod = GetAblMod(displayInt);
            displayWisMod = GetAblMod(displayWis);
            displayChaMod = GetAblMod(displayCha);
        }

        //assigns highest stat in an array to the stat named in the string
        public void FillStat (string stat, int[] statArr)
        {
            if ("str" == stat && displayStr == -1)
            {
                displayStr = statArr.Max();
                statArr[statArr.ToList().IndexOf(displayStr)] = 0;
            }
            if ("dex" == stat && displayDex == -1)
            {
                displayDex = statArr.Max();
                statArr[statArr.ToList().IndexOf(displayDex)] = 0;
            }
            if ("con" == stat && displayCon == -1)
            {
                displayCon = statArr.Max();
                statArr[statArr.ToList().IndexOf(displayCon)] = 0;
            }
            if ("int" == stat && displayInt == -1)
            {
                displayInt = statArr.Max();
                statArr[statArr.ToList().IndexOf(displayInt)] = 0;
            }
            if ("wis" == stat && displayWis == -1)
            {
                displayWis = statArr.Max();
                statArr[statArr.ToList().IndexOf(displayWis)] = 0;
            }
            if ("cha" == stat && displayCha == -1)
            {
                displayCha = statArr.Max();
                statArr[statArr.ToList().IndexOf(displayCha)] = 0;
            }
        }

        //calculates the ability modifier based on D&D's equation
        public int GetAblMod(int abl)
        {
            if (abl > 10)
                return (abl - 10) / 2;
            else
                return (abl - 11) / 2;
        }

        //gets information from API with specific webaddress
        public static Dictionary<string, dynamic> LoadAPI(string url)
        {
            WebRequest request = WebRequest.Create("https://www.dnd5eapi.co/api/" + url);
            request.Method = "GET";
            using var webStream = request.GetResponse().GetResponseStream();

            using var reader = new StreamReader(webStream);
            var data = reader.ReadToEnd();

            return JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(data);
        }
    }
}
