using System;
using DimensionBrawl.LevelDesign;
using UnityEngine;

namespace DimensionBrawl.UI.StageClear
{
    [DisallowMultipleComponent]
    public sealed class StageRunUiRouteResolver : MonoBehaviour, IStageRunUiRouteResolver
    {
        [SerializeField] private UIScreenRouteTable routeTable;

        public bool TryResolve(
            StageUiRouteId routeId,
            out StageRunUiRouteTarget target,
            out string error)
        {
            target = null;
            error = string.Empty;
            int routeValue = (int)routeId;
            if (routeId == StageUiRouteId.None
                || !Enum.IsDefined(typeof(UIRouteId), routeValue))
            {
                error = $"Stage UI route {routeId} has no canonical UI route mapping.";
                return false;
            }

            UIRouteId uiRouteId = (UIRouteId)routeValue;
            if (routeTable == null || !routeTable.TryGetRoute(uiRouteId, out UIScreenRouteTable.Route route))
            {
                error = $"UI route table does not contain {uiRouteId}.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(route.SceneName)
                || string.IsNullOrWhiteSpace(route.ScenePath))
            {
                error = $"UI route {uiRouteId} has no stable scene identity.";
                return false;
            }

            target = new StageRunUiRouteTarget(routeId, route.SceneName, route.ScenePath);
            return true;
        }
    }
}
