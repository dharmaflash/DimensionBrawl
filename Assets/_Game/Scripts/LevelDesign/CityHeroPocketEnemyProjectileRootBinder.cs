using System;
using DimensionBrawl.Enemies;
using UnityEngine;

namespace DimensionBrawl.LevelDesign
{
    /// <summary>
    /// Restores the scene-owned projectile parent after serialization. The reviewed
    /// enemy driver deliberately keeps its runtime override transient, so the city
    /// direct-load proof owns this small scene-local binding adapter.
    /// </summary>
    [DefaultExecutionOrder(-10000)]
    [DisallowMultipleComponent]
    public sealed class CityHeroPocketEnemyProjectileRootBinder : MonoBehaviour
    {
        [SerializeField] private BasicSoldierProjectileAttackDriver driver;
        [SerializeField] private Transform projectileRoot;

        public BasicSoldierProjectileAttackDriver Driver => driver;
        public Transform ProjectileRoot => projectileRoot;
        public bool IsConfigured => TryValidateConfiguration(out _);

        public void Configure(
            BasicSoldierProjectileAttackDriver newDriver,
            Transform newProjectileRoot)
        {
            driver = newDriver;
            projectileRoot = newProjectileRoot;
            ApplyBinding();
        }

        public void ApplyBinding()
        {
            if (!TryValidateConfiguration(out string error))
            {
                throw new InvalidOperationException(error);
            }

            driver.ConfigureRuntimeProjectileRoot(projectileRoot);
        }

        public bool TryValidateConfiguration(out string error)
        {
            if (driver == null)
            {
                error = "City enemy projectile binder has no driver.";
                return false;
            }
            if (projectileRoot == null)
            {
                error = "City enemy projectile binder has no scene-owned projectile root.";
                return false;
            }
            if (driver.gameObject.scene != gameObject.scene
                || projectileRoot.gameObject.scene != gameObject.scene)
            {
                error = "City enemy projectile binder references an object outside its scene.";
                return false;
            }
            if (projectileRoot == driver.transform
                || projectileRoot.IsChildOf(driver.transform))
            {
                error = "City enemy projectiles may not inherit the moving enemy hierarchy.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private void Awake()
        {
            ApplyBinding();
        }

        private void OnEnable()
        {
            ApplyBinding();
        }
    }
}
