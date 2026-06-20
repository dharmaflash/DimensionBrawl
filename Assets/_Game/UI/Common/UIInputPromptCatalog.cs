using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    public enum UIInputPromptDevice
    {
        KeyboardMouse = 0,
        Gamepad = 10,
        Mobile = 20
    }

    [CreateAssetMenu(menuName = "DimensionBrawl/UI/Input Prompt Catalog")]
    public sealed class UIInputPromptCatalog : ScriptableObject
    {
        [Serializable]
        public struct PromptEntry
        {
            [SerializeField] private string actionName;
            [SerializeField] private UIInputPromptDevice device;
            [SerializeField] private string promptLabel;
            [SerializeField] private string iconKey;

            public string ActionName => actionName;
            public UIInputPromptDevice Device => device;
            public string PromptLabel => promptLabel;
            public string IconKey => iconKey;
        }

        [SerializeField] private PromptEntry[] prompts = Array.Empty<PromptEntry>();

        public bool TryGetPrompt(string actionName, UIInputPromptDevice device, out PromptEntry prompt)
        {
            for (int i = 0; i < prompts.Length; i++)
            {
                if (string.Equals(prompts[i].ActionName, actionName, StringComparison.Ordinal)
                    && prompts[i].Device == device)
                {
                    prompt = prompts[i];
                    return true;
                }
            }

            prompt = default;
            return false;
        }
    }
}
