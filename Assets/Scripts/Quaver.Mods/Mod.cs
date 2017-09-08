using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Mods
{
	public class Mod
	{
		// Name of the mod
		public string Name;

		// Type of Mod
		public ModType Type;

		// The identifier of the mod in the ModsEnum
		public ModIdentifier ModIdentifier;

		// Description of the mod
		public string Description;

		// The score multiplier of the mod
		public float ScoreMultiplier;

		// Is the mod ranked?
		public bool Ranked;

		// A list of all other mods that aren't compatiable with this one.
		public ModIdentifier[] IncompatibleMods;

		// Are you allowed to fail the song with this mod on?
		public bool FailureAllowed;
	}
}
