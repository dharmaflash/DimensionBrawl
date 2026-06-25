using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace IsekaiBrawl.Gameplay
{
    public class MetaMenuController : MonoBehaviour
    {
        private const string BattleSceneName = "Battle";

        [SerializeField] private Button storyButton;
        [SerializeField] private Button asyncPvpButton;
        [SerializeField] private Button sandboxButton;
        [SerializeField] private TMP_Text descriptionText;

        private void Awake()
        {
            ResolveReferences();

            if (storyButton != null)
            {
                storyButton.onClick.AddListener(LaunchStoryPve);
            }

            if (asyncPvpButton != null)
            {
                asyncPvpButton.onClick.AddListener(LaunchAsyncPvp);
            }

            if (sandboxButton != null)
            {
                sandboxButton.onClick.AddListener(LaunchSandbox);
            }
        }

        private void OnDestroy()
        {
            if (storyButton != null)
            {
                storyButton.onClick.RemoveListener(LaunchStoryPve);
            }

            if (asyncPvpButton != null)
            {
                asyncPvpButton.onClick.RemoveListener(LaunchAsyncPvp);
            }

            if (sandboxButton != null)
            {
                sandboxButton.onClick.RemoveListener(LaunchSandbox);
            }
        }

        private void Start()
        {
            SetDescription("Select a prototype flow.");
        }

        private void ResolveReferences()
        {
            if (storyButton == null)
            {
                storyButton = transform.Find("StoryButton")?.GetComponent<Button>();
            }

            if (asyncPvpButton == null)
            {
                asyncPvpButton = transform.Find("AsyncPvpButton")?.GetComponent<Button>();
            }

            if (sandboxButton == null)
            {
                sandboxButton = transform.Find("SandboxButton")?.GetComponent<Button>();
            }

            if (descriptionText == null)
            {
                descriptionText = GetComponentInChildren<TMP_Text>(true);
            }
        }

        private void LaunchStoryPve()
        {
            BattleModeContext.SetMode(BattleMode.StoryPve);
            SetDescription("Story PvE selected.");
            SceneManager.LoadScene(BattleSceneName);
        }

        private void LaunchAsyncPvp()
        {
            PveStageContext.Clear();
            BattleModeContext.SetMode(BattleMode.AsyncPvp);
            SetDescription("Async PvP selected.");
            SceneManager.LoadScene(BattleSceneName);
        }

        private void LaunchSandbox()
        {
            PveStageContext.Clear();
            BattleModeContext.SetMode(BattleMode.Sandbox);
            SetDescription("Battle test selected.");
            SceneManager.LoadScene(BattleSceneName);
        }

        private void SetDescription(string value)
        {
            if (descriptionText != null)
            {
                descriptionText.text = value;
            }
        }
    }
}
