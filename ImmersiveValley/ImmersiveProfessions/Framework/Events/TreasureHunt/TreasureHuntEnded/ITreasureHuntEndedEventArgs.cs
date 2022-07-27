﻿namespace DaLion.Stardew.Professions.Framework.Events.TreasureHunt;

#region using directives

using TreasureHunts;

#endregion using directives

/// <summary>Interface for the arguments of a <see cref="TreasureHuntEndedEvent"/>.</summary>
public interface ITreasureHuntEndedEventArgs
{
    /// <summary>The player who triggered the event.</summary>
    Farmer Player { get; }

    /// <summary>Whether this event relates to a Scavenger or Prospector hunt.</summary>
    TreasureHuntType Type { get; }

    /// <summary>Whether the player successfully discovered the treasure.</summary>
    bool TreasureFound { get; }
}