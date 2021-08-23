﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;

namespace TheLion.Stardew.Professions.Framework.TreasureHunt
{
	/// <summary>Base class for treasure hunts.</summary>
	public abstract class TreasureHunt
	{
		public Vector2? TreasureTile { get; protected set; } = null;

		protected string HuntStartedMessage { get; set; }
		protected string HuntFailedMessage { get; set; }
		protected Texture2D Icon { get; set; }

		protected uint TimeLimit;
		protected uint Elapsed;
		protected readonly Random Random = new(Guid.NewGuid().GetHashCode());

		private readonly double _baseTriggerChance;
		private double _accumulatedBonus = 1.0;

		/// <summary>Construct an instance.</summary>
		protected TreasureHunt()
		{
			_baseTriggerChance = ModEntry.Config.ChanceToStartTreasureHunt;
		}

		/// <summary>Check for completion or failure on every update tick.</summary>
		/// <param name="ticks">The number of ticks Elapsed since the game started.</param>
		internal void Update(uint ticks)
		{
			if (!Game1.shouldTimePass(ignore_multiplayer: true)) return;

			if (ticks % 60 == 0 && ++Elapsed > TimeLimit) Fail();
			else CheckForCompletion();
		}

		/// <summary>Reset the accumulated bonus chance to trigger a new hunt.</summary>
		internal void ResetAccumulatedBonus()
		{
			_accumulatedBonus = 1.0;
		}

		/// <summary>Start a new treasure hunt or adjust the odds for the next attempt.</summary>
		protected bool TryStartNewHunt()
		{
			if (Random.NextDouble() > _baseTriggerChance * _accumulatedBonus)
			{
				_accumulatedBonus *= 1.0 + Game1.player.DailyLuck;
				return false;
			}

			_accumulatedBonus = 1.0;
			return true;
		}

		/// <summary>Try to start a new hunt at this location.</summary>
		/// <param name="location">The game location.</param>
		internal abstract void TryStartNewHunt(GameLocation location);

		/// <summary>Check if the player has found the treasure tile.</summary>
		protected abstract void CheckForCompletion();

		/// <summary>End the hunt unsuccessfully.</summary>
		protected abstract void Fail();

		/// <summary>Reset treasure tile and unsubscribe treasure hunt update event.</summary>
		internal abstract void End();
	}
}