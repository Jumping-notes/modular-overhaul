﻿using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;

namespace TheLion.Stardew.Professions.Framework.Extensions
{
	public static class CrabPotExtensions
	{
		/// <summary>Whether the crab pot instance is using magnet as bait.</summary>
		public static bool HasMagnet(this CrabPot crabpot)
		{
			return crabpot.bait.Value != null && Util.Objects.BaitById.TryGetValue(crabpot.bait.Value.ParentSheetIndex, out var baitName) && baitName.Equals("Magnet");
		}

		/// <summary>Whether the crab pot instance is using wild bait.</summary>
		public static bool HasWildBait(this CrabPot crabpot)
		{
			return crabpot.bait.Value != null && Util.Objects.BaitById.TryGetValue(crabpot.bait.Value.ParentSheetIndex, out var baitName) && baitName.Equals("Wild Bait");
		}

		/// <summary>Whether the crab pot instance is using magic bait.</summary>
		public static bool HasMagicBait(this CrabPot crabpot)
		{
			return crabpot.bait.Value != null && Util.Objects.BaitById.TryGetValue(crabpot.bait.Value.ParentSheetIndex, out var baitName) && baitName.Equals("Magic Bait");
		}

		/// <summary>Whether the crab pot instance should catch ocean-specific shellfish.</summary>
		/// <param name="location">The location of the crab pot.</param>
		public static bool ShouldCatchOceanFish(this CrabPot crabpot, GameLocation location)
		{
			return location is Beach || location.catchOceanCrabPotFishFromThisSpot((int)crabpot.TileLocation.X, (int)crabpot.TileLocation.Y);
		}
	}
}