﻿#nullable enable
namespace DaLion.Stardew.Professions;

#region using directives
using static System.String;
using static System.FormattableString;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

using Common.Extensions;
using Common.Integrations;
using Extensions;
using Framework;
using Framework.Ultimate;
using Framework.Utility;

#endregion using directives

internal static class ConsoleCommands
{
    /// <summary>Register all internally defined console commands.</summary>
    /// <param name="helper">The console command API.</param>
    internal static void Register(this ICommandHelper helper)
    {
        helper.Add("player_levels", "List the player's current skill levels and experience.",
            PrintLocalPlayerSkillLevels);
        helper.Add("player_setlevels", "Set the player's levels for the specified skills.",
            SetLocalPlayerSkillLevels);
        helper.Add("player_resetlevels", "Reset all the player's skill levels.",
            ResetLocalPlayerSkills);
        helper.Add("player_clearnewlevels", "Clear the player's cache of new levels for te specified skills.",
            ClearLocalPlayerNewLevels);
        helper.Add("player_professions", "List the player's current professions.",
            PrintLocalPlayerProfessions);
        helper.Add("player_addprofessions",
            "Add the specified professions to the local player, without affecting skill levels." +
            GetUsageForAddProfessions(),
            AddProfessionsToLocalPlayer);
        helper.Add("player_resetprofessions",
            "Remove all professions from the local player for the specified skills, or for all skills if none are specified. Does not affect skill level",
            ResetLocalPlayerProfessions);
        helper.Add("player_whichult",
            "Check the currently registered Ultimate.",
            PrintUltimateIndex);
        helper.Add("player_registerult",
            "Change the currently registered Ultimate.",
            SetUltimateIndex);
        helper.Add("player_readyult", "Max-out the Ultimate meter, or set it to the specified percentage.",
            SetUltimateChargeValue);
        helper.Add("player_maxanimalfriendship",
            "Max-out the friendship of all owned animals.",
            SetMaxAnimalFriendship);
        helper.Add("player_maxanimalmood", "Max-out the mood of all owned animals.",
            SetMaxAnimalMood);
        helper.Add("player_fishingprogress",
            "Check your fishing progress for Anglers.",
            PrintFishingAudit);
        helper.Add("player_maxfish",
            "Max out your fishing progress for Anglers.",
            SetMaxFishingProgress);
        helper.Add("wol_data",
            "Check the current value of all mod data fields." + GetUsageForSetModData(),
            PrintModData);
        helper.Add("wol_setdata", "Set a new value for a mod data field.",
            SetModData);
        helper.Add("wol_events", "List currently subscribed mod events.",
            PrintSubscribedEvents);
        helper.Add("wol_resetthehunt",
            "Forcefully reset the current Treasure Hunt with a new target treasure tile.",
            RerollTreasureTile);
    }

    #region command handlers

    /// <summary>List the current skill levels of the local player.</summary>
    internal static void PrintLocalPlayerSkillLevels(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            Log.W("You must load a save first.");
            return;
        }

        Log.I($"Farming level: {Game1.player.GetUnmodifiedSkillLevel(Skill.Farming)} ({Game1.player.experiencePoints[Skill.Farming]} exp)");
        Log.I($"Fishing level: {Game1.player.GetUnmodifiedSkillLevel(Skill.Fishing)} ({Game1.player.experiencePoints[Skill.Fishing]} exp)");
        Log.I($"Foraging level: {Game1.player.GetUnmodifiedSkillLevel(Skill.Foraging)} ({Game1.player.experiencePoints[Skill.Foraging]} exp)");
        Log.I($"Mining level: {Game1.player.GetUnmodifiedSkillLevel(Skill.Mining)} ({Game1.player.experiencePoints[Skill.Mining]} exp)");
        Log.I($"Combat level: {Game1.player.GetUnmodifiedSkillLevel(Skill.Combat)} ({Game1.player.experiencePoints[Skill.Combat]} exp)");
        
        if (ModEntry.LuckSkillApi is not null)
            Log.I($"Luck level: {Game1.player.GetUnmodifiedSkillLevel(Skill.Luck)} ({Game1.player.experiencePoints[Skill.Luck]} exp)");

        foreach (var skill in ModEntry.CustomSkills.Values.OfType<CustomSkill>())
            Log.I($"{skill.DisplayName} level: {skill.CurrentLevel} ({skill.CurrentExp} exp)");
    }

    /// <summary>Reset all skills for the local player.</summary>
    internal static void ResetLocalPlayerSkills(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            Log.W("You must load a save first.");
            return;
        }

        if (!args.Any())
        {
            Game1.player.farmingLevel.Value = 0;
            Game1.player.fishingLevel.Value = 0;
            Game1.player.foragingLevel.Value = 0;
            Game1.player.miningLevel.Value = 0;
            Game1.player.combatLevel.Value = 0;
            Game1.player.luckLevel.Value = 0;
            Game1.player.newLevels.Clear();
            for (var i = 0; i <= 5; ++i)
            {
                Game1.player.experiencePoints[i] = 0;
                if (ModEntry.Config.ForgetRecipesOnSkillReset && i < 5)
                    Game1.player.ForgetRecipesForSkill(Skill.FromValue(i));
            }

            LevelUpMenu.RevalidateHealth(Game1.player);

            foreach (var (_, skill) in ModEntry.CustomSkills)
            {
                ModEntry.SpaceCoreApi!.AddExperienceForCustomSkill(Game1.player, skill.StringId, -skill.CurrentExp);
                if (ModEntry.Config.ForgetRecipesOnSkillReset &&
                    skill.StringId == "blueberry.LoveOfCooking.CookingSkill")
                    Game1.player.ForgetRecipesForLoveOfCookingSkill();
            }
        }
        else
        {
            foreach (var arg in args)
            {
                if (Skill.TryFromName(arg, true, out var skill))
                {
                    // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                    switch (skill)
                    {
                        case Farmer.farmingSkill:
                            Game1.player.farmingLevel.Value = 0;
                            break;
                        case Farmer.fishingSkill:
                            Game1.player.fishingLevel.Value = 0;
                            break;
                        case Farmer.foragingSkill:
                            Game1.player.foragingLevel.Value = 0;
                            break;
                        case Farmer.miningSkill:
                            Game1.player.miningLevel.Value = 0;
                            break;
                        case Farmer.combatSkill:
                            Game1.player.combatLevel.Value = 0;
                            break;
                        case Farmer.luckSkill:
                            Game1.player.luckLevel.Value = 0;
                            break;
                    }

                    Game1.player.experiencePoints[skill] = 0;
                    Game1.player.newLevels.Set(Game1.player.newLevels.Where(p => p.X != skill).ToList());
                    if (ModEntry.Config.ForgetRecipesOnSkillReset && skill < Skill.Luck)
                        Game1.player.ForgetRecipesForSkill(skill);
                }
                else if (ModEntry.CustomSkills.Values.Any(s => string.Equals(s.DisplayName, arg, StringComparison.CurrentCultureIgnoreCase)))
                {
                    var customSkill = ModEntry.CustomSkills.Values.Single(s =>
                        string.Equals(s.DisplayName, arg, StringComparison.CurrentCultureIgnoreCase));
                    ModEntry.SpaceCoreApi!.AddExperienceForCustomSkill(Game1.player, customSkill.StringId, -customSkill.CurrentExp);

                    var newLevels =
                        (List<KeyValuePair<string, int>>) ExtendedSpaceCoreAPI.GetCustomSkillNewLevels.GetValue(null)!;
                    ExtendedSpaceCoreAPI.GetCustomSkillNewLevels.SetValue(null,
                        newLevels.Where(pair => pair.Key != customSkill.StringId).ToList());

                    if (ModEntry.Config.ForgetRecipesOnSkillReset &&
                        customSkill.StringId == "blueberry.LoveOfCooking.CookingSkill")
                        Game1.player.ForgetRecipesForLoveOfCookingSkill();
                }
                else
                {
                   Log.W($"Ignoring unknown skill {arg}.");
                }
            }
        }
    }

    /// <summary>Set the specified skill level.</summary>
    internal static void SetLocalPlayerSkillLevels(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            Log.W("You must load a save first.");
            return;
        }

        if (args.Length < 2 || args.Length % 2 != 0)
        {
            Log.W("You must provide both a skill name and new level.");
            GetUsageForSetSkillLevels();
            return;
        }

        if (string.Equals(args[0], "all", StringComparison.InvariantCultureIgnoreCase))
        {
            if (!int.TryParse(args[1], out var newLevel))
            {
                Log.W("New level must be a valid integer.");
                GetUsageForSetSkillLevels();
                return;
            }

            foreach (var skill in Skill.List)
                Game1.player.SetSkillLevel(skill, newLevel, true);

            foreach (var customSkill in ModEntry.CustomSkills.Values)
                if (customSkill is CustomSkill customSkill2)
                    Game1.player.SetCustomSkillLevel(customSkill2, newLevel);
        }

        var argsList = args.ToList();
        while (argsList.Any())
        {
            if (!int.TryParse(args[1], out var newLevel))
            {
                Log.W("New level must be a valid integer.");
                GetUsageForSetSkillLevels();
                return;
            }

            var skillName = args[0];
            if (!Skill.TryFromName(skillName, true, out var skill))
            {
                var found = ModEntry.CustomSkills.Values.SingleOrDefault(s =>
                    string.Equals(s.StringId, skillName, StringComparison.CurrentCultureIgnoreCase) ||
                    string.Equals(s.DisplayName, skillName, StringComparison.CurrentCultureIgnoreCase));
                if (found is not CustomSkill customSkill)
                {
                    Log.W("You must provide a valid skill name.");
                    GetUsageForSetSkillLevels();
                    return;
                }

                Game1.player.SetCustomSkillLevel(customSkill, newLevel);
            }
            else
            {
                Game1.player.SetSkillLevel(skill, newLevel, true);
            }

            argsList.RemoveAt(0);
            argsList.RemoveAt(0);
        }
    }

    internal static void ClearLocalPlayerNewLevels(string commands, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            Log.W("You must load a save first.");
            return;
        }

        if (!args.Any())
            Game1.player.newLevels.Clear();
        else
            foreach (var arg in args)
            {
                if (Skill.TryFromName(arg, true, out var skill))
                {
                    Game1.player.newLevels.Set(Game1.player.newLevels.Where(p => p.X != skill).ToList());
                }
                else if (ModEntry.CustomSkills.Values.Any(s =>
                             string.Equals(s.DisplayName, arg, StringComparison.CurrentCultureIgnoreCase)))
                {
                    var customSkill = ModEntry.CustomSkills.Values.Single(s =>
                        string.Equals(s.DisplayName, arg, StringComparison.CurrentCultureIgnoreCase));
                    var newLevels =
                        (List<KeyValuePair<string, int>>) ExtendedSpaceCoreAPI.GetCustomSkillNewLevels.GetValue(null)!;
                    ExtendedSpaceCoreAPI.GetCustomSkillNewLevels.SetValue(null,
                        newLevels.Where(pair => pair.Key != customSkill.StringId).ToList());
                }
                else
                {
                    Log.W($"Ignoring unknown skill {arg}.");
                }
            }
    }

    /// <summary>List the current professions of the local player.</summary>
    internal static void PrintLocalPlayerProfessions(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            Log.W("You must load a save first.");
            return;
        }

        if (!Game1.player.professions.Any())
        {
            Log.I($"Farmer {Game1.player.Name} doesn't have any professions.");
            return;
        }

        var message = $"Farmer {Game1.player.Name}'s professions:";
        foreach (var pid in Game1.player.professions)
        {
            string name;
            if (Profession.TryFromValue(pid > 100 ? pid - 100 : pid, out var profession))
                name = profession.StringId + (pid > 100 ? " (P)" : Empty);
            else if (ModEntry.CustomProfessions.ContainsKey(pid))
                name = ModEntry.CustomProfessions[pid].StringId;
            else
                name = $"Unknown profession {pid}";

            message += "\n\t- " + name;
        }

        Log.I(message);
    }

    /// <summary>Add specified professions to the local player.</summary>
    internal static void AddProfessionsToLocalPlayer(string command, string[] args)
    {
        if (!args.Any())
        {
            Log.W("You must specify at least one profession." + GetUsageForAddProfessions());
            return;
        }

        if (!Context.IsWorldReady)
        {
            Log.W("You must load a save first.");
            return;
        }

        var prestige = args.Any(a => a is "-p" or "--prestiged");
        if (prestige) args = args.Except(new[] {"-p", "--prestiged"}).ToArray();

        List<int> professionsToAdd = new();
        foreach (var arg in args)
        {
            if (string.Equals(arg, "all", StringComparison.InvariantCultureIgnoreCase))
            {
                var range = Profession.GetRange().ToArray();
                if (prestige) range = range.Concat(Enumerable.Range(100, 30)).ToArray();
                range = range.Concat(ModEntry.CustomProfessions.Values.Select(p => p.Id)).ToArray();

                professionsToAdd.AddRange(range);
                Log.I($"Added all {(prestige ? "prestiged " : "")}professions to farmer {Game1.player.Name}.");
                break;
            }

            if (Profession.TryFromName(arg, true, out var profession) ||
                Profession.TryFromLocalizedName(arg, true, out profession))
            {
                if (!prestige && Game1.player.HasProfession(profession) ||
                    prestige && Game1.player.HasProfession(profession, true))
                {
                    Log.W($"Farmer {Game1.player.Name} already has the {profession.StringId} profession.");
                    continue;
                }

                professionsToAdd.Add(profession.Id);
                if (prestige) professionsToAdd.Add(profession + 100);
                Log.I(
                    $"Added {profession.StringId}{(prestige ? " (P)" : "")} profession to farmer {Game1.player.Name}.");
            }
            else
            {
                var customProfession = ModEntry.CustomProfessions.Values.SingleOrDefault(p =>
                    string.Equals(arg, p.StringId.TrimAll(), StringComparison.InvariantCultureIgnoreCase) ||
                    string.Equals(arg, p.GetDisplayName().TrimAll(), StringComparison.InvariantCultureIgnoreCase));
                if (customProfession is null)
                {
                    Log.W($"Ignoring unknown profession {arg}.");
                    continue;
                }

                if (prestige)
                {
                    Log.W($"Cannot prestige custom skill profession {customProfession.StringId}.");
                    continue;
                }

                if (Game1.player.HasProfession(customProfession))
                {
                    Log.W($"Farmer {Game1.player.Name} already has the {customProfession.StringId} profession.");
                    continue;
                }

                professionsToAdd.Add(customProfession.Id);
                if (prestige) professionsToAdd.Add(profession + 100);
                Log.I(
                    $"Added the {profession.StringId} profession to farmer {Game1.player.Name}.");
            }
        }

        LevelUpMenu levelUpMenu = new();
        foreach (var pid in professionsToAdd.Distinct().Except(Game1.player.professions))
        {
            Game1.player.professions.Add(pid);
            levelUpMenu.getImmediateProfessionPerk(pid);
        }

        LevelUpMenu.RevalidateHealth(Game1.player);
    }

    /// <summary>Remove professions from the local player.</summary>
    internal static void ResetLocalPlayerProfessions(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            Log.W("You must load a save first.");
            return;
        }

        var didResetCombat = false;
        if (!args.Any())
        {
            for (var i = Game1.player.professions.Count - 1; i >= 0; --i)
            {
                var pid = Game1.player.professions[i];
                Game1.player.professions.RemoveAt(i);
                LevelUpMenu.removeImmediateProfessionPerk(pid);
            }

            didResetCombat = true;
        }
        else
        {
            foreach (var arg in args)
            {
                if (!Skill.TryFromName(arg, true, out var skill))
                {
                    Log.W($"Ignoring unknown skill {arg}.");
                    continue;
                }

                var toRemove = Game1.player.GetProfessionsForSkill(skill);
                foreach (var p in toRemove) Game1.player.professions.Remove(p.Id);

                if (skill == Skill.Combat) didResetCombat = true;
            }
        }

        if (!didResetCombat) return;
        
        ModEntry.PlayerState.RegisteredUltimate = null;
        Game1.player.WriteData(ModData.UltimateIndex, null);
        LevelUpMenu.RevalidateHealth(Game1.player);
    }

    /// <summary>Set <see cref="UltimateHUD.Value" /> to the desired percent value, or max it out if no value is specified.</summary>
    internal static void SetUltimateChargeValue(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            Log.W("You must load a save first.");
            return;
        }

        if (ModEntry.PlayerState.RegisteredUltimate is null)
        {
            Log.W("Not registered to an Ultimate.");
            return;
        }

        if (!args.Any())
        {
            ModEntry.PlayerState.RegisteredUltimate.ChargeValue = ModEntry.PlayerState.RegisteredUltimate.MaxValue;
            return;
        }

        if (args.Length > 1)
        {
            Log.W("Too many arguments. Specify a single value between 0 and 100.");
            return;
        }

        if (!int.TryParse(args[0], out var value) || value is < 0 or > 100)
        {
            Log.W("Bad arguments. Specify an integer value between 0 and 100.");
            return;
        }

        ModEntry.PlayerState.RegisteredUltimate.ChargeValue = (double) value * ModEntry.PlayerState.RegisteredUltimate.MaxValue / 100.0;
    }

    /// <summary>
    ///     Reset the Ultimate instance to a different combat profession's, in case you have more
    ///     than one.
    /// </summary>
    internal static void SetUltimateIndex(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            Log.W("You must load a save first.");
            return;
        }

        if (!args.Any() || args.Length > 1)
        {
            Log.W("You must specify a single value.");
            return;
        }

        if (!Game1.player.professions.Any(p => p is >= 26 and < 30))
        {
            Log.W("You don't have any 2nd-tier combat professions.");
            return;
        }

        args[0] = args[0].ToLowerInvariant().FirstCharToUpper();
        if (!Enum.TryParse<UltimateIndex>(args[0], true, out var index))
        {
            Log.W("You must enter a valid 2nd-tier combat profession.");
            return;
        }

        var profession = Profession.FromValue((int) index);
        if (!Game1.player.HasProfession(profession))
        {
            Log.W("You don't have this profession.");
            return;
        }

#pragma warning disable CS8509
        ModEntry.PlayerState.RegisteredUltimate = index switch
#pragma warning restore CS8509
        {
            UltimateIndex.BruteFrenzy => new UndyingFrenzy(),
            UltimateIndex.PoacherAmbush => new Ambush(),
            UltimateIndex.PiperPandemic => new Enthrall(),
            UltimateIndex.DesperadoBlossom => new DeathBlossom()
        };
        Game1.player.WriteData(ModData.UltimateIndex, index.ToString());
    }

    /// <summary>Print the currently registered Ultimate.</summary>
    internal static void PrintUltimateIndex(string command, string[] args)
    {
        if (ModEntry.PlayerState.RegisteredUltimate is null)
        {
            Log.I("Not registered to an Ultimate.");
            return;
        }

        var key = ModEntry.PlayerState.RegisteredUltimate.Index.ToString().SplitCamelCase()[0].ToLowerInvariant();
        var professionDisplayName = ModEntry.i18n.Get(key + ".name.male");
        var ultiName = ModEntry.i18n.Get(key + ".ulti");
        Log.I($"Registered to {professionDisplayName}'s {ultiName}.");
    }

    /// <summary>Set all farm animals owned by the local player to the max friendship value.</summary>
    internal static void SetMaxAnimalFriendship(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            Log.W("You must load a save first.");
            return;
        }

        var animals = Game1.getFarm().getAllFarmAnimals().Where(a =>
            a.ownerID.Value == Game1.player.UniqueMultiplayerID || !Context.IsMultiplayer).ToList();
        var count = animals.Count;
        if (count <= 0)
        {
            Log.W("You don't own any animals.");
            return;
        }

        foreach (var animal in animals) animal.friendshipTowardFarmer.Value = 1000;
        Log.I($"Maxed the friendship of {count} animals");
    }

    /// <summary>Set all farm animals owned by the local player to the max mood value.</summary>
    internal static void SetMaxAnimalMood(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            Log.W("You must load a save first.");
            return;
        }

        var animals = Game1.getFarm().getAllFarmAnimals().Where(a =>
            a.ownerID.Value == Game1.player.UniqueMultiplayerID || !Context.IsMultiplayer).ToList();
        var count = animals.Count;

        if (count <= 0)
        {
            Log.W("You don't own any animals.");
            return;
        }

        foreach (var animal in animals) animal.happiness.Value = 255;
        Log.I($"Maxed the mood of {count} animals");
    }

    /// <summary>Check current fishing progress.</summary>
    internal static void PrintFishingAudit(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            Log.W("You must load a save first.");
            return;
        }

        if (!Game1.player.fishCaught.Pairs.Any())
        {
            Log.W("You haven't caught any fish.");
            return;
        }

        var fishData = Game1.content
            .Load<Dictionary<int, string>>(PathUtilities.NormalizeAssetName("Data/Fish"))
            .Where(p => !p.Key.IsIn(152, 153, 157) && !p.Value.Contains("trap"))
            .ToDictionary(p => p.Key, p => p.Value);
        int numLegendariesCaught = 0, numMaxSizedCaught = 0;
        var caughtFishNames = new List<string>();
        var nonMaxSizedCaught = new Dictionary<string, Tuple<int, int>>();
        var result = Empty;
        foreach (var (key, value) in Game1.player.fishCaught.Pairs)
        {
            if (!fishData.TryGetValue(key, out var specificFishData)) continue;

            var dataFields = specificFishData.Split('/');
            if (ObjectLookups.LegendaryFishNames.Contains(dataFields[0]))
            {
                ++numLegendariesCaught;
            }
            else
            {
                if (value[1] > Convert.ToInt32(dataFields[4]))
                    ++numMaxSizedCaught;
                else
                    nonMaxSizedCaught.Add(dataFields[0],
                        new(value[1], Convert.ToInt32(dataFields[4])));
            }

            caughtFishNames.Add(dataFields[0]);
        }

        var priceMultiplier = Game1.player.HasProfession(Profession.Angler)
            ? CurrentCulture($"{numMaxSizedCaught * 0.01f + numLegendariesCaught * 0.05f:p0}")
            : "Zero. You're not an Angler.";
        result +=
            $"Species caught: {Game1.player.fishCaught.Count()}/{fishData.Count}\nMax-sized: {numMaxSizedCaught}/{Game1.player.fishCaught.Count()}\nLegendaries: {numLegendariesCaught}/10\nTotal Angler price bonus: {priceMultiplier}\n\nThe following caught fish are not max-sized:";
        result = nonMaxSizedCaught.Keys.Aggregate(result,
            (current, fish) =>
                current +
                $"\n\t- {fish} (current: {nonMaxSizedCaught[fish].Item1}, max: {nonMaxSizedCaught[fish].Item2})");

        var seasonFish = from specificFishData in fishData.Values
            where specificFishData.Split('/')[6].Contains(Game1.currentSeason)
            select specificFishData.Split('/')[0];

        result += "\n\nThe following fish can be caught this season:";
        result = seasonFish.Except(caughtFishNames).Aggregate(result, (current, fish) => current + $"\n\t- {fish}");

        Log.I(result);
    }

    /// <summary>Set the local player's fishing records to include one of every fish at max size.</summary>
    internal static void SetMaxFishingProgress(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            Log.W("You must load a save first.");
            return;
        }

        var fishData = Game1.content
            .Load<Dictionary<int, string>>(PathUtilities.NormalizeAssetName("Data/Fish"))
            .Where(p => !p.Key.IsIn(152, 153, 157) && !p.Value.Contains("trap"))
            .ToDictionary(p => p.Key, p => p.Value);
        foreach (var (key, value) in fishData)
        {
            var dataFields = value.Split('/');
            if (Game1.player.fishCaught.ContainsKey(key))
            {
                var caught = Game1.player.fishCaught[key];
                caught[1] = Convert.ToInt32(dataFields[4]) + 1;
                Game1.player.fishCaught[key] = caught;
                Game1.stats.checkForFishingAchievements();
            }
            else
            {
                Game1.player.fishCaught.Add(key, new[] {1, Convert.ToInt32(dataFields[4]) + 1});
            }
        }
    }

    /// <summary>Print the current value of every mod data field to the console.</summary>
    internal static void PrintModData(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            Log.W("You must load a save first.");
            return;
        }

        var message = $"Farmer {Game1.player.Name}'s mod data:";
        var value = Game1.player.ReadData(ModData.EcologistItemsForaged);
        message += "\n\t- " +
            (!IsNullOrEmpty(value)
                ? $"{ModData.EcologistItemsForaged}: {value} ({ModEntry.Config.ForagesNeededForBestQuality - int.Parse(value)} needed for best quality)"
                : $"Mod data does not contain an entry for {ModData.EcologistItemsForaged}.");

        value = Game1.player.ReadData(ModData.GemologistMineralsCollected);
        message += "\n\t- " +
            (!IsNullOrEmpty(value)
                ? $"{ModData.GemologistMineralsCollected}: {value} ({ModEntry.Config.MineralsNeededForBestQuality - int.Parse(value)} needed for best quality)"
                : $"Mod data does not contain an entry for {ModData.GemologistMineralsCollected}.");

        value = Game1.player.ReadData(ModData.ProspectorHuntStreak);
        message += "\n\t- " +
            (!IsNullOrEmpty(value)
                ? $"{ModData.ProspectorHuntStreak}: {value} (affects treasure quality)"
                : $"Mod data does not contain an entry for {ModData.ProspectorHuntStreak}.");

        value = Game1.player.ReadData(ModData.ScavengerHuntStreak);
        message += "\n\t- " +
            (!IsNullOrEmpty(value)
                ? $"{ModData.ScavengerHuntStreak}: {value} (affects treasure quality)"
                : $"Mod data does not contain an entry for {ModData.ScavengerHuntStreak}.");

        value = Game1.player.ReadData(ModData.ConservationistTrashCollectedThisSeason);
        message += "\n\t- " +
            (!IsNullOrEmpty(value)
                // ReSharper disable once PossibleLossOfFraction
                ? CurrentCulture($"{ModData.ConservationistTrashCollectedThisSeason}: {value} (expect a {Math.Min(int.Parse(value) / ModEntry.Config.TrashNeededPerTaxBonusPct / 100f, ModEntry.Config.ConservationistTaxBonusCeiling):p0} tax deduction next season)")
                : $"Mod data does not contain an entry for {ModData.ConservationistTrashCollectedThisSeason}.");

        value = Game1.player.ReadData(ModData.ConservationistActiveTaxBonusPct);
        message += "\n\t- " + 
            (!IsNullOrEmpty(value)
                ? CurrentCulture($"{ModData.ConservationistActiveTaxBonusPct}: {float.Parse(value):p0}")
                : $"Mod data does not contain an entry for {ModData.ConservationistActiveTaxBonusPct}.");

        Log.I(message);
    }

    /// <summary>Set a new value to the specified mod data field.</summary>
    internal static void SetModData(string command, string[] args)
    {
        if (!args.Any() || args.Length != 2)
        {
            Log.W("You must specify a data field and value." + GetUsageForSetModData());
            return;
        }

        if (!int.TryParse(args[1], out var value) || value < 0)
        {
            Log.W("You must specify a positive integer value.");
            return;
        }

        if (!Context.IsWorldReady)
        {
            Log.W("You must load a save first.");
            return;
        }

        switch (args[0].ToLowerInvariant())
        {
            case "forage":
            case "itemsforaged":
            case "ecologist":
            case "ecologistitemsforaged":
                SetEcologistItemsForaged(value);
                break;

            case "minerals":
            case "mineralscollected":
            case "gemologist":
            case "gemologistmineralscollected":
                SetGemologistMineralsCollected(value);
                break;

            case "shunt":
            case "scavengerhunt":
            case "scavenger":
            case "scavengerhuntstreak":
                SetScavengerHuntStreak(value);
                break;

            case "phunt":
            case "prospectorhunt":
            case "prospector":
            case "prospectorhuntstreak":
                SetProspectorHuntStreak(value);
                break;

            case "trash":
            case "trashcollected":
            case "conservationist":
            case "conservationisttrashcollectedthisseason":
                SetConservationistTrashCollectedThisSeason(value);
                break;

            default:
                var message = $"'{args[0]}' is not a settable data field.\n" + GetAvailableDataFields();
                Log.W(message);
                break;
        }
    }

    /// <summary>Print the currently subscribed mod events to the console.</summary>
    internal static void PrintSubscribedEvents(string command, string[] args)
    {
        var message = "Enabled events:";
        message = EventManager.GetAllEnabled()
            .Aggregate(message, (current, next) => current + "\n\t- " + next.GetType().Name);
        Log.I(message);
    }

    /// <summary>Force a new treasure tile to be selected for the currently active Treasure Hunt.</summary>
    internal static void RerollTreasureTile(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            Log.W("You must load a save first.");
            return;
        }

        if (!ModEntry.PlayerState.ScavengerHunt.IsActive && !ModEntry.PlayerState.ProspectorHunt.IsActive)
        {
            Log.W("There is no Treasure Hunt currently active.");
            return;
        }

        if (ModEntry.PlayerState.ScavengerHunt.IsActive)
        {
            var v = ModEntry.ModHelper.Reflection.GetMethod(ModEntry.PlayerState.ScavengerHunt, "ChooseTreasureTile").Invoke<Vector2?>(Game1.currentLocation);
            if (v is null)
            {
                Log.W("Couldn't find a valid treasure tile after 10 tries.");
                return;
            }

            Game1.currentLocation.MakeTileDiggable(v.Value);
            ModEntry.ModHelper.Reflection.GetProperty<Vector2?>(ModEntry.PlayerState.ScavengerHunt, "TreasureTile")
                .SetValue(v);
            ModEntry.ModHelper.Reflection.GetField<uint>(ModEntry.PlayerState.ScavengerHunt, "elapsed").SetValue(0);

            Log.I("The Scavenger Hunt was reset.");
        }
        else if (ModEntry.PlayerState.ProspectorHunt.IsActive)
        {
            var v = ModEntry.ModHelper.Reflection.GetMethod(ModEntry.PlayerState.ProspectorHunt, "ChooseTreasureTile").Invoke<Vector2?>(Game1.currentLocation);
            if (v is null)
            {
                Log.W("Couldn't find a valid treasure tile after 10 tries.");
                return;
            }

            ModEntry.ModHelper.Reflection.GetProperty<Vector2?>(ModEntry.PlayerState.ProspectorHunt, "TreasureTile")
                .SetValue(v);
            ModEntry.ModHelper.Reflection.GetField<int>(ModEntry.PlayerState.ProspectorHunt, "Elapsed").SetValue(0);

            Log.I("The Prospector Hunt was reset.");
        }
    }

    #endregion command handlers

    #region private methods

    /// <summary>Tell the dummies how to use the console command.</summary>
    private static string GetUsageForSetSkillLevels()
    {
        var result = "\n\nUsage: player_setlevels <skill1> <newLevel> <skill2> <newLevel> ...";
        result += "\n\nParameters:";
        result += "\n\t<skill>\t- a valid skill name, or 'all'";
        result += "\n\t<newLevel>\t- a valid integer level";
        result += "\n\nExamples:";
        result += "\n\tplayer_setlevels farming 5 cooking 10";
        return result;
    }

    /// <summary>Tell the dummies how to use the console command.</summary>
    private static string GetUsageForAddProfessions()
    {
        var result = "\n\nUsage: player_addprofessions [--prestige] <profession1> <profession2> ... <professionN>";
        result += "\n\nParameters:";
        result += "\n\t<profession>\t- a valid profession name, or 'all'";
        result += "\n\nOptional flags:";
        result +=
            "\n\t--prestige, -p\t- add the prestiged versions of the specified professions (will automatically add the base versions as well)";
        result += "\n\nExamples:";
        result += "\n\tplayer_addprofessions artisan brute";
        result += "\n\tplayer_addprofessions -p all";
        return result;
    }

    /// <summary>Tell the dummies how to use the console command.</summary>
    private static string GetUsageForSetModData()
    {
        var result = "\n\nUsage: wol_setdata <field> <value>";
        result += "\n\nParameters:";
        result += "\n\t<field>\t- the name of the field";
        result += "\\n\t<value>\t- the desired new value";
        result += "\n\nExamples:";
        result += "\n\twol_setdata EcologistItemsForaged 100";
        result += "\n\twol_setdata trash 500";
        result += "\n\n" + GetAvailableDataFields();
        return result;
    }

    /// <summary>Tell the dummies which fields they can set.</summary>
    private static string GetAvailableDataFields()
    {
        var result = "Available data fields:";
        result += $"\n\t- {ModData.EcologistItemsForaged} (shortcut 'forages')";
        result += $"\n\t- {ModData.GemologistMineralsCollected} (shortcut 'minerals')";
        result += $"\n\t- {ModData.ProspectorHuntStreak} (shortcut 'phunt')";
        result += $"\n\t- {ModData.ScavengerHuntStreak} (shortcut 'shunt')";
        result += $"\n\t- {ModData.ConservationistTrashCollectedThisSeason} (shortcut 'trash')";
        return result;
    }

    /// <summary>Set a new value to the EcologistItemsForaged data field.</summary>
    private static void SetEcologistItemsForaged(int value)
    {
        if (!Game1.player.HasProfession(Profession.Ecologist))
        {
            Log.W("You must have the Ecologist profession.");
            return;
        }

        Game1.player.WriteData(ModData.EcologistItemsForaged, value.ToString());
        Log.I($"Items foraged as Ecologist was set to {value}.");
    }

    /// <summary>Set a new value to the GemologistMineralsCollected data field.</summary>
    private static void SetGemologistMineralsCollected(int value)
    {
        if (!Game1.player.HasProfession(Profession.Gemologist))
        {
            Log.W("You must have the Gemologist profession.");
            return;
        }

        Game1.player.WriteData(ModData.GemologistMineralsCollected, value.ToString());
        Log.I($"Minerals collected as Gemologist was set to {value}.");
    }

    /// <summary>Set a new value to the ProspectorHuntStreak data field.</summary>
    private static void SetProspectorHuntStreak(int value)
    {
        if (!Game1.player.HasProfession(Profession.Prospector))
        {
            Log.W("You must have the Prospector profession.");
            return;
        }

        Game1.player.WriteData(ModData.ProspectorHuntStreak, value.ToString());
        Log.I($"Prospector Hunt was streak set to {value}.");
    }

    /// <summary>Set a new value to the ScavengerHuntStreak data field.</summary>
    private static void SetScavengerHuntStreak(int value)
    {
        if (!Game1.player.HasProfession(Profession.Scavenger))
        {
            Log.W("You must have the Scavenger profession.");
            return;
        }

        Game1.player.WriteData(ModData.ScavengerHuntStreak, value.ToString());
        Log.I($"Scavenger Hunt streak was set to {value}.");
    }

    /// <summary>Set a new value to the ConservationistTrashCollectedThisSeason data field.</summary>
    private static void SetConservationistTrashCollectedThisSeason(int value)
    {
        if (!Game1.player.HasProfession(Profession.Conservationist))
        {
            Log.W("You must have the Conservationist profession.");
            return;
        }

        Game1.player.WriteData(ModData.ConservationistTrashCollectedThisSeason, value.ToString());
        Log.I($"Conservationist trash collected in the current season was set to {value}.");
    }

    #endregion private methods
}