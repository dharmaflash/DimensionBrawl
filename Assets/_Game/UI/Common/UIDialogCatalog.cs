using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    [CreateAssetMenu(menuName = "DimensionBrawl/UI/Dialog Catalog")]
    public sealed class UIDialogCatalog : ScriptableObject
    {
        [Serializable]
        public struct DialogEntry
        {
            [SerializeField] private string id;
            [SerializeField] private string titleTextKey;
            [SerializeField] private string bodyTextKey;
            [SerializeField] private string confirmTextKey;
            [SerializeField] private string cancelTextKey;
            [SerializeField] private string iconKey;
            [SerializeField] private Color accentColor;
            [SerializeField] private bool cancelVisible;

            public string Id => id;
            public string TitleTextKey => titleTextKey;
            public string BodyTextKey => bodyTextKey;
            public string ConfirmTextKey => confirmTextKey;
            public string CancelTextKey => cancelTextKey;
            public string IconKey => iconKey;
            public Color AccentColor => accentColor;
            public bool CancelVisible => cancelVisible;
        }

        [SerializeField] private DialogEntry[] dialogs = Array.Empty<DialogEntry>();

        public bool TryGetDialog(string id, out DialogEntry dialog)
        {
            for (int i = 0; i < dialogs.Length; i++)
            {
                if (string.Equals(dialogs[i].Id, id, StringComparison.Ordinal))
                {
                    dialog = dialogs[i];
                    return true;
                }
            }

            dialog = default;
            return false;
        }
    }
}
