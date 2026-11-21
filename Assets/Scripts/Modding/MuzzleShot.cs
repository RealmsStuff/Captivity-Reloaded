using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaptivityReloaded.Modding
{
	// The rules for a shot that starts at the muzzle instead of the shoulder:
	//  - it flies toward the cursor, or along the barrel when the cursor is no further out than the muzzle;
	//  - something solid between the shoulder and the muzzle stops it there, so a barrel poked through a wall still
	//    cannot shoot what is behind the wall;
	//  - an enemy whose body covers the muzzle is not hit at all: the shot starts past them and goes on to whoever
	//    is in front of the muzzle.
	public static class MuzzleShot
	{
		// How far around the muzzle point a body counts as covering it.
		public const float CoverRadius = 0.06f;
		// A cursor this far ahead of the muzzle is aimed at; one level with the muzzle or behind it is not, and the
		// shot follows the barrel. In between the two directions are mixed, so the shot never snaps from one to the other.
		public const float AimedAtFrom = 0.6f, BarrelUntil = 0.1f;

		public static Vector2 Direction(Vector2 i_muzzle, Vector2 i_barrel, Vector2 i_aim)
		{
			Vector2 barrel = i_barrel.sqrMagnitude > 1e-8f ? i_barrel.normalized : Vector2.right;
			Vector2 toAim = i_aim - i_muzzle;
			float ahead = Vector2.Dot(toAim, barrel);
			if (ahead <= BarrelUntil) return barrel;
			float aimed = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(BarrelUntil, AimedAtFrom, ahead));
			Vector2 mixed = Vector2.Lerp(barrel, toAim.normalized, aimed);
			return mixed.sqrMagnitude > 1e-8f ? mixed.normalized : barrel;
		}

		// The hits of a ray from the muzzle with every collider of a body that covers the muzzle taken out.
		// i_ownerOf names the body a collider belongs to (null: not part of a body, never taken out).
		public static RaycastHit2D[] HitsPastTheMuzzle(RaycastHit2D[] i_hits, IEnumerable<Collider2D> i_onMuzzle, Func<Collider2D, object> i_ownerOf)
		{
			if (i_hits == null || i_hits.Length == 0 || i_onMuzzle == null) return i_hits ?? new RaycastHit2D[0];
			HashSet<object> covering = null;
			foreach (Collider2D collider in i_onMuzzle)
			{
				object owner = collider == null ? null : i_ownerOf(collider);
				if (owner == null) continue;
				if (covering == null) covering = new HashSet<object>();
				covering.Add(owner);
			}
			if (covering == null) return i_hits;
			List<RaycastHit2D> kept = new List<RaycastHit2D>(i_hits.Length);
			foreach (RaycastHit2D hit in i_hits)
			{
				object owner = hit.collider == null ? null : i_ownerOf(hit.collider);
				if (owner == null || !covering.Contains(owner)) kept.Add(hit);
			}
			return kept.ToArray();
		}

		// The hits of a ray from the shoulder to the muzzle up to and including the first solid one, with bodies
		// left out (they are behind the muzzle). False when nothing solid is in the way: the shot leaves the muzzle.
		public static bool TryFindBlock(RaycastHit2D[] i_hits, Func<RaycastHit2D, bool> i_isSolid, Func<Collider2D, object> i_ownerOf, out RaycastHit2D[] o_hits)
		{
			o_hits = null;
			if (i_hits == null) return false;
			List<RaycastHit2D> upToBlock = new List<RaycastHit2D>();
			foreach (RaycastHit2D hit in i_hits)
			{
				if (hit.collider == null || i_ownerOf(hit.collider) != null) continue;
				upToBlock.Add(hit);
				if (!i_isSolid(hit)) continue;
				o_hits = upToBlock.ToArray();
				return true;
			}
			return false;
		}
	}
}
