using System;

namespace DimensionBrawl.UI
{
    public readonly struct UISceneTransitionTicket : IEquatable<UISceneTransitionTicket>
    {
        internal UISceneTransitionTicket(
            int ownerInstanceId,
            uint generation,
            UIRouteId routeId,
            int sourceSceneHandle,
            string destinationSceneName,
            string destinationScenePath)
        {
            OwnerInstanceId = ownerInstanceId;
            Generation = generation;
            RouteId = routeId;
            SourceSceneHandle = sourceSceneHandle;
            DestinationSceneName = destinationSceneName ?? string.Empty;
            DestinationScenePath = destinationScenePath ?? string.Empty;
        }

        public int OwnerInstanceId { get; }
        public uint Generation { get; }
        public UIRouteId RouteId { get; }
        public int SourceSceneHandle { get; }
        public string DestinationSceneName { get; }
        public string DestinationScenePath { get; }
        public bool IsValid => OwnerInstanceId != 0
            && Generation != 0
            && !string.IsNullOrWhiteSpace(DestinationSceneName);

        public bool Equals(UISceneTransitionTicket other)
        {
            return OwnerInstanceId == other.OwnerInstanceId
                && Generation == other.Generation
                && RouteId == other.RouteId
                && SourceSceneHandle == other.SourceSceneHandle
                && string.Equals(
                    DestinationSceneName,
                    other.DestinationSceneName,
                    StringComparison.Ordinal)
                && string.Equals(
                    DestinationScenePath,
                    other.DestinationScenePath,
                    StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is UISceneTransitionTicket other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = OwnerInstanceId;
                hash = (hash * 397) ^ Generation.GetHashCode();
                hash = (hash * 397) ^ (int)RouteId;
                hash = (hash * 397) ^ SourceSceneHandle;
                hash = (hash * 397) ^ StringComparer.Ordinal.GetHashCode(
                    DestinationSceneName ?? string.Empty);
                hash = (hash * 397) ^ StringComparer.Ordinal.GetHashCode(
                    DestinationScenePath ?? string.Empty);
                return hash;
            }
        }

        public static bool operator ==(UISceneTransitionTicket left, UISceneTransitionTicket right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(UISceneTransitionTicket left, UISceneTransitionTicket right)
        {
            return !left.Equals(right);
        }
    }
}
