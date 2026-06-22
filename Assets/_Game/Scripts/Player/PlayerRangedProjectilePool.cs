using System.Collections.Generic;
using DimensionBrawl.Combat;
using UnityEngine;

namespace DimensionBrawl.Player
{
    internal sealed class PlayerRangedProjectilePool
    {
        private readonly List<LaneActionProjectile> projectiles = new List<LaneActionProjectile>(12);

        public int ActiveCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < projectiles.Count; i++)
                {
                    if (projectiles[i] != null && projectiles[i].IsActive)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public LaneActionProjectile Get(LaneActionProjectile prefab, Transform root)
        {
            for (int i = 0; i < projectiles.Count; i++)
            {
                LaneActionProjectile projectile = projectiles[i];
                if (projectile != null && !projectile.IsActive)
                {
                    return projectile;
                }
            }

            if (prefab == null)
            {
                return null;
            }

            LaneActionProjectile instance = CreateInstance(prefab, root);
            projectiles.Add(instance);
            return instance;
        }

        public void Prewarm(LaneActionProjectile prefab, Transform root, int count)
        {
            if (prefab == null)
            {
                return;
            }

            int targetCount = Mathf.Max(0, count);
            while (projectiles.Count < targetCount)
            {
                projectiles.Add(CreateInstance(prefab, root));
            }
        }

        private static LaneActionProjectile CreateInstance(LaneActionProjectile prefab, Transform root)
        {
            LaneActionProjectile instance = Object.Instantiate(prefab, root);
            instance.gameObject.SetActive(false);
            return instance;
        }
    }
}
