﻿namespace DaLion.Stardew.Tools.Framework.Effects;

#region using directives

using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;

#endregion using directives

/// <summary>Interface for applying a tool's effects.</summary>
internal interface IEffect
{
    /// <summary>Apply the tool effect to the given tile.</summary>
    /// <param name="tile">The tile to modify.</param>
    /// <param name="tileObj">The object on the tile.</param>
    /// <param name="tileFeature">The feature on the tile.</param>
    /// <param name="tool">The tool selected by the player (if any).</param>
    /// <param name="location">The current location.</param>
    /// <param name="who">The current player.</param>
    public bool Apply(Vector2 tile, SObject tileObj, TerrainFeature tileFeature, Tool tool, GameLocation location,
        Farmer who);
}