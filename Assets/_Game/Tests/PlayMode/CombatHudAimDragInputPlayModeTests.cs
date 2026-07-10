using System.Collections;
using System.Reflection;
using DimensionBrawl.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DimensionBrawl.Tests
{
    public sealed class CombatHudAimDragInputPlayModeTests
    {
        [UnityTest]
        public IEnumerator DisableIgnoresDestroyedPlayerBindings()
        {
            GameObject movementObject = new("DestroyedMovementBinding", typeof(CharacterController));
            PlayerMovementController movementController = movementObject.AddComponent<PlayerMovementController>();
            GameObject inputObject = new("CombatHudAimDragInput", typeof(RectTransform));
            System.Type aimDragInputType = System.Type.GetType(
                "DimensionBrawl.UI.CombatHudAimDragInput, Assembly-CSharp",
                throwOnError: true);
            Component aimDragInput = inputObject.AddComponent(aimDragInputType);
            MethodInfo configure = aimDragInputType.GetMethod("Configure", BindingFlags.Instance | BindingFlags.Public);
            Assert.IsNotNull(configure);
            configure.Invoke(aimDragInput, new object[] { movementController, null, null, null });

            Object.Destroy(movementObject);
            yield return null;

            Assert.DoesNotThrow(() => inputObject.SetActive(false));
            LogAssert.NoUnexpectedReceived();

            Object.Destroy(inputObject);
            yield return null;
        }
    }
}
