using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace NoahNPCGen.Pages
{
    public class CharacterModel : PageModel
    {
        public static Random rnd = new Random();
        public string displayName = "", displayRace = "", displayClass = "", displaySubClass = "", displayBackG = "", displayAlignment = "";
        public int displayLevel = -1, displayExp = -1, displayStr = -1, displayDex = -1, displayCon = -1, displayInt = -1, displayWis = -1, displayCha = -1, displayStrMod = -1, displayDexMod = -1, displayConMod = -1, displayIntMod = -1, displayWisMod = -1, displayChaMod = -1, displayProf = -1, displayHitDice = -1, displayHP = -1, displayAC = -1, displaySpeed = -1;
        public string strSav, dexSav, conSav, intSav, wisSav, chaSav, acroPro, animPro, arcaPro, athlPro, decePro, histPro, insiPro, intiPro, invePro, mediPro, natuPro, percPro, perfPro, persPro, reliPro, sleiPro, steaPro, survPro;
        public List<string> otherProf = new List<string>();
        public List<string[]> attackList = new List<string[]>();
        public List<DNDItem> itemSelect = new List<DNDItem>();
        public class DNDItem
        {
            public string Index { get; set; }
            public string Name { get; set; }
            public int Quantity { get; set; }
            public DNDItem(string index, string name, int quantity)
            {
                Index = index;
                Name = name;
                Quantity = quantity;
            }
        }

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
            GetEquipment();
            GetCombat();

        }

        //calculates weapon names, attack bonuses, and damage/type, as well as AC
        private void GetCombat()
        {
            foreach (DNDItem item in itemSelect)
            {
                dynamic middleMan = LoadAPI("equipment/" + item.Index);
                if ("weapon" == middleMan["equipment_category"]["index"].ToString())
                {
                    int atkBonus = 0;
                    string atkDam = "", damType = middleMan["damage"]["damage_type"]["name"].ToString();
                    bool isFinesse = false, isMonk = false;
                    foreach (string proficiency in otherProf)
                    {
                        if (proficiency == middleMan["weapon_category"].ToString() + " Weapons" || proficiency.ToLower() == middleMan["index"].ToString() + "s")
                        {
                            atkBonus += displayProf;
                        }
                    }
                    foreach (dynamic property in middleMan["properties"])
                    {
                        if (property["index"] == "finesse")
                            isFinesse = true;
                        if (property["index"] == "monk" && displayClass == "Monk")
                            isMonk = true;
                    }
                    if (isMonk)
                    {
                        int martArtDie = int.Parse(LoadAPI("classes/" + displayClass.ToLower() + "/levels/" + displayLevel)["class_specific"]["martial_arts"]["dice_value"].ToString());
                        if (int.Parse(middleMan["damage"]["damage_dice"].ToString().Split('d')[1]) < martArtDie)
                            atkDam = "1d" + martArtDie;
                        else
                            atkDam = middleMan["damage"]["damage_dice"];
                    }
                    else
                        atkDam = middleMan["damage"]["damage_dice"];
                    if ((isFinesse || displayClass == "Monk") && (displayDexMod > displayStrMod))
                    {
                        atkBonus += displayDexMod;
                        atkDam += " + " + displayDexMod;
                    }
                    else
                    {
                        atkBonus += displayStrMod;
                        atkDam += " + " + displayStrMod;
                    }
                    string[] atk = { item.Name, atkBonus.ToString(), atkDam + " " + damType};
                    attackList.Add(atk);
                }

                if ("armor" == middleMan["equipment_category"]["index"].ToString() && "shield" != middleMan["index"])
                {
                    displayAC = middleMan["armor_class"]["base"];
                    if ((bool)middleMan["armor_class"]["dex_bonus"])
                    {
                        int dexMax = 99;
                        try
                        {
                            dexMax = (int)middleMan["armor_class"]["max_bonus"];
                        } catch { }
                        if (dexMax < displayDexMod)
                            displayAC += (int)middleMan["armor_class"]["max_bonus"];
                        else
                            displayAC += displayDexMod;
                    }
                    if (displayStr < (int)middleMan["str_minimum"])
                    {
                        displaySpeed -= 10;
                    }
                }
                if (displayAC == -1)
                {
                    displayAC = 10 + displayDexMod;
                }
            }
        }

        //generates character equipment from starting-equipment from class
        private void GetEquipment()
        {
            dynamic charaEqu = LoadAPI("classes/" + displayClass.ToLower());
            foreach (dynamic item in charaEqu["starting_equipment"])
                itemSelect.Add(new DNDItem(item["equipment"]["index"].ToString(), item["equipment"]["name"].ToString(), (int)item["quantity"]));
            foreach (dynamic option in charaEqu["starting_equipment_options"]) //cycles through each choice the player makes
            {
                for (int i = 0; i < (int)option["choose"]; i++)
                {
                    JArray options = (JArray)option["from"];
                    int rndChoice = rnd.Next(0, options.Count);
                    bool setOfItems = false, itemCategory = false, optionCategory = false;
                    try //checks to see if the option is actually several items
                    {
                        Console.WriteLine(option["from"][rndChoice]["0"]);
                        setOfItems = true;
                    } catch { }
                    if (!setOfItems)
                    {
                        try //checks to see if item in option is an option of a category such as "simple weapon" or "martial weapon"
                        {
                            Console.WriteLine(option["from"][rndChoice]["equipment_option"]);
                            itemCategory = true;
                        } catch { }
                        if (!itemCategory)
                        {
                            try //checks to see if option itself is of a category
                            {
                                Console.WriteLine(option["from"][rndChoice]["equipment_category"]);
                                optionCategory = true;
                            } catch { }
                            if (!optionCategory)
                            {
                                string index = option["from"][rndChoice]["equipment"]["index"].ToString();
                                string name = option["from"][rndChoice]["equipment"]["name"].ToString();
                                int quantity = (int)option["from"][rndChoice]["quantity"];
                                itemSelect.Add(new DNDItem(index, name, quantity));
                            }
                            else
                            {
                                List<dynamic> itemType = new List<dynamic>();
                                foreach (dynamic item in LoadAPI("equipment-categories/" + option["from"][rndChoice]["equipment_category"]["index"].ToString())["equipment"])
                                    itemType.Add(item);
                                for (int j = 0; j < (int)option["choose"]; j++)
                                {
                                    int ranItem = rnd.Next(0, itemType.Count);
                                    itemSelect.Add(new DNDItem(itemType.ElementAt(ranItem)["index"].ToString(), itemType.ElementAt(ranItem)["name"].ToString(), 1));
                                }
                            }
                        }
                        else
                        {
                            List<dynamic> itemType = new List<dynamic>();
                            foreach (dynamic item in LoadAPI("equipment-categories/" + option["from"][rndChoice]["equipment_option"]["from"]["equipment_category"]["index"].ToString())["equipment"])
                                itemType.Add(item);
                            for (int j = 0; j < (int)option["from"][rndChoice]["equipment_option"]["choose"]; j++)
                            {
                                int ranItem = rnd.Next(0, itemType.Count);
                                itemSelect.Add(new DNDItem(itemType.ElementAt(ranItem)["index"].ToString(), itemType.ElementAt(ranItem)["name"].ToString(), 1));
                            }
                        }

                    }
                    else
                    {
                        try
                        {
                            Console.WriteLine(option["from"][rndChoice]["equipment_option"]);
                            itemCategory = true;
                        } catch { }
                        if (!itemCategory)
                        {
                            int j = 0;
                            while (option["from"][rndChoice][j.ToString()] != null)
                            {
                                //json can't cycle through strings, so do a regular for loop, sonverting the ints to strings
                                string index = option["from"][rndChoice][j.ToString()]["equipment"]["index"].ToString();
                                string name = option["from"][rndChoice][j.ToString()]["equipment"]["name"].ToString();
                                int quantity = (int)option["from"][rndChoice][j.ToString()]["quantity"];
                                itemSelect.Add(new DNDItem(index, name, quantity));
                                j++;
                            }
                        }
                        else
                        {
                            List<dynamic> itemType = new List<dynamic>();
                            foreach (dynamic item in LoadAPI("equipment-categories/" + option["from"]["equipment_option"]["equipment_category"]["index"].ToString())["equipment"])
                                itemType.Add(item);
                            for (int j = 0; j < (int)option["from"]["equipment_option"]["choose"]; j++)
                            {
                                int ranItem = rnd.Next(0, itemType.Count);
                                itemSelect.Add(new DNDItem(itemType.ElementAt(ranItem)["index"].ToString(), itemType.ElementAt(ranItem)["name"].ToString(), 1));
                            }
                        }
                    }
                }
            }
        }

        //prints items into equipment box
        public string AllItems()
        {
            string result = "";
            foreach (DNDItem item in itemSelect)
            {
                result += item.Name;
                result += " x" + item.Quantity + "\n";
            }
            return result;
        }

        //levels up the character
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

            //adds each proficiency choice from class to list
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
