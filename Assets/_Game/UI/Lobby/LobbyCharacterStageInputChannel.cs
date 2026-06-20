using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    [CreateAssetMenu(menuName = "DimensionBrawl/UI/Lobby Character Stage Input Channel")]
    public sealed class LobbyCharacterStageInputChannel : ScriptableObject
    {
        public event Action BeginInteractionRequested;
        public event Action<float> HorizontalDragRequested;
        public event Action EndInteractionRequested;
        public event Action TapRequested;

        public void RaiseBeginInteraction()
        {
            BeginInteractionRequested?.Invoke();
        }

        public void RaiseHorizontalDrag(float deltaPixels)
        {
            HorizontalDragRequested?.Invoke(deltaPixels);
        }

        public void RaiseEndInteraction()
        {
            EndInteractionRequested?.Invoke();
        }

        public void RaiseTap()
        {
            TapRequested?.Invoke();
        }
    }
}
