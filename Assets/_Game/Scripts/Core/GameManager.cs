using System;
using System.Collections;
using UnityEngine;

namespace IsekaiBrawl.Gameplay
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public event Action<GameState> OnStateChanged;

        [SerializeField] private GameState currentState = GameState.Idle;
        [SerializeField] private float battleDuration = 165f;
        [SerializeField] private float battleStartDelay = 2.4f;

        public GameState CurrentState => currentState;
        public float RemainingTime { get; private set; }
        public float BattleDuration => battleDuration;
        public float ElapsedBattleTime => Mathf.Max(0f, battleDuration - RemainingTime);

        private Coroutine battleStartRoutine;

        private bool IsTerminalState =>
            currentState == GameState.Victory ||
            currentState == GameState.Defeat ||
            currentState == GameState.TimeUp;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            ApplyStoryStageOverrides();
            RemainingTime = battleDuration;
        }

        private void Start()
        {
            StartBattle();
        }

        private void Update()
        {
            if (currentState != GameState.Battle)
            {
                return;
            }

            RemainingTime = Mathf.Max(0f, RemainingTime - Time.deltaTime);
            if (RemainingTime <= 0f)
            {
                HandleTimeUp();
            }
        }

        public void ChangeState(GameState newState)
        {
            if (currentState == newState)
            {
                return;
            }

            currentState = newState;
            OnStateChanged?.Invoke(currentState);
        }

        public void StartBattle()
        {
            if (IsTerminalState || battleStartRoutine != null)
            {
                return;
            }

            RemainingTime = battleDuration;
            ChangeState(GameState.BattleStart);
            battleStartRoutine = StartCoroutine(BeginBattleAfterIntro());
        }

        public void EndBattle(bool playerWin)
        {
            if (IsTerminalState)
            {
                return;
            }

            ChangeState(playerWin ? GameState.Victory : GameState.Defeat);
        }

        private void HandleTimeUp()
        {
            if (IsTerminalState)
            {
                return;
            }

            BattleManager battleManager = BattleManager.Instance;
            if (battleManager == null)
            {
                ChangeState(GameState.TimeUp);
                return;
            }

            if (Mathf.Approximately(battleManager.CurrentPlayerBaseHP, battleManager.CurrentEnemyBaseHP))
            {
                ChangeState(GameState.TimeUp);
                return;
            }

            EndBattle(battleManager.CurrentPlayerBaseHP > battleManager.CurrentEnemyBaseHP);
        }

        private void ApplyStoryStageOverrides()
        {
            if (BattleModeContext.CurrentMode != BattleMode.StoryPve || PveStageContext.SelectedStage == null)
            {
                return;
            }

            if (PveStageContext.SelectedStage.TimeLimit > 1f)
            {
                battleDuration = PveStageContext.SelectedStage.TimeLimit;
            }
        }

        private IEnumerator BeginBattleAfterIntro()
        {
            yield return new WaitForSeconds(Mathf.Max(0f, battleStartDelay));
            battleStartRoutine = null;
            ChangeState(GameState.Battle);
        }
    }
}
