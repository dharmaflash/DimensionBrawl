using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IsekaiBrawl.Gameplay
{
    public enum MobileMoveInputMode
    {
        Joystick = 0,
        TapAssist = 1
    }

    public class MobileBattleControls : MonoBehaviour
    {
        public enum SummonPlacementFailureReason
        {
            None = 0,
            LaneNotSelected = 1
        }

        [SerializeField] private Color panelTint = new(0.05f, 0.08f, 0.14f, 0.42f);
        [SerializeField] private Color stickReadyTint = new(0.34f, 0.86f, 1f, 0.28f);
        [SerializeField] private Color stickActiveTint = new(0.42f, 0.95f, 1f, 0.46f);
        [SerializeField] private Color skillReadyTint = new(0.24f, 0.86f, 0.54f, 0.42f);
        [SerializeField] private Color skillBlockedTint = new(0.86f, 0.4f, 0.4f, 0.68f);
        [SerializeField] private Color skillCooldownTint = new(0.95f, 0.72f, 0.34f, 0.56f);
        [SerializeField] private Color utilityButtonTint = new(0.28f, 0.42f, 0.94f, 0.26f);
        [SerializeField] private Color tapAssistButtonTint = new(0.14f, 0.7f, 0.96f, 0.96f);
        [SerializeField] private Color directDodgeLockTint = new(1f, 0.62f, 0.3f, 0.1f);
        [SerializeField] private Color directDodgeActiveTint = new(1f, 0.42f, 0.28f, 0.14f);
        [SerializeField] private MobileMoveInputMode defaultInputMode = MobileMoveInputMode.Joystick;
        [SerializeField] private float tapArrivalDistance = 0.32f;
        [SerializeField] private float tapCancelDistance = 0.42f;

        private RectTransform safeAreaRoot;
        private RectTransform moveTrayRoot;
        private RectTransform actionTrayRoot = null;
        private RectTransform stickRoot;
        private RectTransform stickVisual;
        private RectTransform stickKnob;
        private RectTransform posturePushButtonRoot = null;
        private RectTransform postureHoldButtonRoot = null;
        private RectTransform postureFallbackButtonRoot = null;
        private RectTransform skillButtonRoot;
        private RectTransform mapButtonRoot;
        private RectTransform moveModeButtonRoot = null;
        private RectTransform laneRailRoot;
        private RectTransform laneMarkerContainer;
        private RectTransform summonPlacementPreviewRoot;
        private RectTransform summonPlacementLaneRibbon;
        private RectTransform summonPlacementGuideLine;
        private RectTransform summonPlacementGuideMarker;
        private RectTransform summonPlacementLandingMarker;
        private RectTransform summonPlacementPocketMarker;
        private RectTransform summonPlacementBlockerMarker;
        private RectTransform summonPlacementRewardMarker;
        private Transform summonPlacementWorldPreviewRoot;
        private Transform summonPlacementWorldLandingMarker;
        private Transform summonPlacementWorldPocketMarker;
        private Transform summonPlacementWorldBlockerMarker;
        private Transform summonPlacementWorldRewardMarker;
        private LineRenderer summonPlacementWorldGuideLine;
        private RectTransform tapMovePadRoot;
        private RectTransform targetLockMarkerRoot;
        private RectTransform directDodgePadRoot;
        private RectTransform overviewPadRoot;
        private RectTransform zoomInButtonRoot;
        private RectTransform zoomOutButtonRoot;
        private RectTransform centerButtonRoot;
        private Image stickVisualImage;
        private Image stickKnobImage;
        private Image skillButtonImage;
        private Image mapButtonImage;
        private Image moveModeButtonImage = null;
        private Image laneRailImage;
        private Image laneLeftButtonImage;
        private Image laneRightButtonImage;
        private Image summonPlacementLaneRibbonImage;
        private Image summonPlacementGuideLineImage;
        private Image summonPlacementGuideMarkerImage;
        private Image summonPlacementLandingMarkerImage;
        private Image summonPlacementPocketMarkerImage;
        private Image summonPlacementBlockerMarkerImage;
        private Image summonPlacementRewardMarkerImage;
        private Image targetLockMarkerImage;
        private Image zoomInButtonImage;
        private Image zoomOutButtonImage;
        private Image centerButtonImage;
        private TMP_Text moveHintText;
        private TMP_Text posturePushButtonText = null;
        private TMP_Text postureHoldButtonText = null;
        private TMP_Text postureFallbackButtonText = null;
        private TMP_Text skillButtonText;
        private TMP_Text mapButtonText;
        private TMP_Text moveModeButtonText = null;
        private TMP_Text laneLeftButtonText;
        private TMP_Text laneRightButtonText;
        private TMP_Text laneStatusText;
        private TMP_Text summonPlacementPreviewText;
        private TMP_Text summonPlacementLandingText;
        private TMP_Text summonPlacementPocketText;
        private TMP_Text summonPlacementBlockerText;
        private TMP_Text summonPlacementRewardText;
        private TMP_Text tapMovePadHintText;
        private TMP_Text targetLockMarkerText;
        private TMP_Text directDodgeTitleText;
        private TMP_Text directDodgeLeftText;
        private TMP_Text directDodgeRightText;
        private TMP_Text overviewPadHintText;
        private TMP_Text zoomInButtonText;
        private TMP_Text zoomOutButtonText;
        private TMP_Text centerButtonText;
        private Button skillButton;
        private Button mapButton;
        private Button laneLeftButton = null;
        private Button laneRightButton = null;
        private Button zoomInButton;
        private Button zoomOutButton;
        private Button centerButton;
        private TMP_FontAsset runtimeFont;
        private PlayerController playerController;
        private PlayerSkillController playerSkillController;
        private BattleEnergySystem battleEnergySystem;
        private BattleCamera battleCamera;
        private Camera battleViewCamera;
        private EnemyAI enemyAI;
        private SummonSpawner summonSpawner;
        private bool pendingSkillPress;
        private bool pendingMapTogglePress;
        private bool pendingOverviewCenterPress;
        private float pendingOverviewZoomStep;
        private Vector2 pendingOverviewDragDelta;
        private bool isTouchLayoutActive;
        private Vector2 moveVector;
        private float pendingDirectDodgeDirection;
        private int pendingFocusLaneIndex = -1;
        private MobileMoveInputMode currentInputMode;
        private Vector3 tapMoveDestination;
        private bool hasTapMoveDestination;
        private bool isDirectDodgeModeActive;
        private bool isSummonPlacementActive;
        private int previewLaneIndex = BattleLaneUtility.DefaultLaneCount / 2;
        private bool hasPreviewDropWorldPosition;
        private Vector3 previewDropWorldPosition;
        private SummonPlacementFailureReason lastPlacementFailureReason = SummonPlacementFailureReason.None;
        private Rect lastSafeArea;
        private Vector2 lastRootSize;
        private int defaultDragThreshold = -1;
        private readonly List<Image> laneMarkerImages = new();
        private readonly List<Button> laneMarkerButtons = new();
        private readonly List<TMP_Text> laneMarkerTexts = new();

        public static MobileBattleControls Instance { get; private set; }

        public bool IsTouchLayoutActive => isTouchLayoutActive;
        public MobileMoveInputMode CurrentInputMode => currentInputMode;
        public static bool IsAutoCombatMovementEnabled => Instance != null && Instance.isTouchLayoutActive;
        public static bool IsDirectDodgeModeActive => Instance != null && Instance.isTouchLayoutActive && Instance.isDirectDodgeModeActive;
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            currentInputMode = defaultInputMode;
            EnsureUi();
            ApplySafeAreaAndLayout(forceRefresh: true);
        }

        private void OnEnable()
        {
            EnsureUi();
            ApplySafeAreaAndLayout(forceRefresh: true);
        }

        private void OnDisable()
        {
            moveVector = Vector2.zero;
            ClearTapMoveDestination();
            pendingSkillPress = false;
            pendingMapTogglePress = false;
            pendingOverviewCenterPress = false;
            pendingOverviewZoomStep = 0f;
            pendingOverviewDragDelta = Vector2.zero;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            EnsureUi();
            ResolveReferences();
            bool shouldUseTouchLayout = ShouldUseMobileLayout(transform as RectTransform);
            if (shouldUseTouchLayout != isTouchLayoutActive)
            {
                isTouchLayoutActive = shouldUseTouchLayout;
                if (!isTouchLayoutActive)
                {
                    ClearTapMoveDestination();
                    pendingSkillPress = false;
                    pendingMapTogglePress = false;
                    pendingOverviewCenterPress = false;
                    pendingOverviewZoomStep = 0f;
                }

                SetRootActiveState(isTouchLayoutActive);
                ApplySafeAreaAndLayout(forceRefresh: true);
            }
            else
            {
                ApplySafeAreaAndLayout(forceRefresh: false);
            }

            UpdateDirectDodgeModeState();
            UpdateButtonVisualState();
            UpdateDragThreshold();
        }

        public static bool ShouldUseMobileLayout(RectTransform root)
        {
            float screenWidth = Screen.width > 0 ? Screen.width : (root != null && root.rect.width > 1f ? root.rect.width : 1080f);
            float screenHeight = Screen.height > 0 ? Screen.height : (root != null && root.rect.height > 1f ? root.rect.height : 1920f);
            float aspect = screenWidth <= 0.001f ? 1f : screenHeight / screenWidth;
            bool hasTouchDevice =
#if ENABLE_INPUT_SYSTEM
                Touchscreen.current != null;
#else
                Input.touchSupported;
#endif

            return Application.isMobilePlatform || hasTouchDevice || screenWidth <= 900f || aspect >= 1.35f;
        }

        public static bool TryGetMoveInput(out Vector2 moveInput)
        {
            moveInput = Vector2.zero;
            return false;
        }

        public static bool TryConsumeDirectDodge(out float directionSign)
        {
            if (Instance != null && Instance.isTouchLayoutActive && Mathf.Abs(Instance.pendingDirectDodgeDirection) > 0.001f)
            {
                directionSign = Instance.pendingDirectDodgeDirection;
                Instance.pendingDirectDodgeDirection = 0f;
                return true;
            }

            directionSign = 0f;
            return false;
        }

        public static bool TryConsumeFocusLaneSelection(out int focusLaneIndex)
        {
            if (Instance != null && Instance.isTouchLayoutActive && Instance.pendingFocusLaneIndex >= 0)
            {
                focusLaneIndex = Instance.pendingFocusLaneIndex;
                Instance.pendingFocusLaneIndex = -1;
                return true;
            }

            focusLaneIndex = BattleLaneUtility.DefaultLaneCount / 2;
            return false;
        }

        public static bool BeginSummonPlacement(Vector2 screenPosition)
        {
            if (Instance == null)
            {
                return false;
            }

            return Instance.BeginSummonPlacementInternal(screenPosition);
        }

        public static void UpdateSummonPlacement(Vector2 screenPosition)
        {
            Instance?.UpdateSummonPlacementInternal(screenPosition);
        }

        public static bool TryCompleteSummonPlacement(Vector2 screenPosition, out int laneIndex)
        {
            return TryCompleteSummonPlacement(screenPosition, out laneIndex, out _);
        }

        public static bool TryCompleteSummonPlacement(
            Vector2 screenPosition,
            out int laneIndex,
            out SummonPlacementFailureReason failureReason)
        {
            if (Instance != null)
            {
                return Instance.TryCompleteSummonPlacementInternal(screenPosition, out laneIndex, out failureReason);
            }

            laneIndex = BattleLaneUtility.DefaultLaneCount / 2;
            failureReason = SummonPlacementFailureReason.LaneNotSelected;
            return false;
        }

        public static void CancelSummonPlacement()
        {
            Instance?.CancelSummonPlacementInternal();
        }

        public static bool ConsumeSkillPressed()
        {
            if (Instance == null || !Instance.isTouchLayoutActive || !Instance.pendingSkillPress)
            {
                return false;
            }

            Instance.pendingSkillPress = false;
            return true;
        }

        public static bool ConsumeMapTogglePressed()
        {
            if (Instance == null || !Instance.isTouchLayoutActive || !Instance.pendingMapTogglePress)
            {
                return false;
            }

            Instance.pendingMapTogglePress = false;
            return true;
        }

        public static bool TryConsumeOverviewDrag(out Vector2 dragDelta)
        {
            if (Instance != null && Instance.isTouchLayoutActive && Instance.pendingOverviewDragDelta.sqrMagnitude > 0.001f)
            {
                dragDelta = Instance.pendingOverviewDragDelta;
                Instance.pendingOverviewDragDelta = Vector2.zero;
                return true;
            }

            dragDelta = Vector2.zero;
            return false;
        }

        public static bool TryConsumeOverviewZoomStep(out float zoomStep)
        {
            if (Instance != null && Instance.isTouchLayoutActive && Mathf.Abs(Instance.pendingOverviewZoomStep) > 0.001f)
            {
                zoomStep = Instance.pendingOverviewZoomStep;
                Instance.pendingOverviewZoomStep = 0f;
                return true;
            }

            zoomStep = 0f;
            return false;
        }

        public static bool ConsumeOverviewCenterPressed()
        {
            if (Instance == null || !Instance.isTouchLayoutActive || !Instance.pendingOverviewCenterPress)
            {
                return false;
            }

            Instance.pendingOverviewCenterPress = false;
            return true;
        }

        internal void SetMoveVector(Vector2 normalizedMove)
        {
            moveVector = Vector2.ClampMagnitude(normalizedMove, 1f);
            RefreshStickVisual();
        }

        internal void ClearMoveVector()
        {
            moveVector = Vector2.zero;
            RefreshStickVisual();
        }

        internal void HandleTapMovePointer(PointerEventData eventData)
        {
            if (!isTouchLayoutActive || currentInputMode != MobileMoveInputMode.TapAssist)
            {
                return;
            }

            if (battleCamera != null && battleCamera.IsOverviewMode)
            {
                return;
            }

            if (!TryResolveTapMoveDestination(eventData.position, eventData.pressEventCamera, out Vector3 targetPosition))
            {
                return;
            }

            if (playerController == null)
            {
                return;
            }

            Vector3 currentPosition = playerController.transform.position;
            Vector3 planarDelta = targetPosition - currentPosition;
            planarDelta.y = 0f;
            if (planarDelta.sqrMagnitude <= tapCancelDistance * tapCancelDistance)
            {
                ClearTapMoveDestination();
                return;
            }

            tapMoveDestination = targetPosition;
            hasTapMoveDestination = true;
        }

        internal void HandleBattlefieldTapRelease(PointerEventData eventData, Vector2 pointerDownScreenPosition)
        {
            if (!isTouchLayoutActive || isSummonPlacementActive || isDirectDodgeModeActive)
            {
                return;
            }

            if (battleCamera != null && battleCamera.IsOverviewMode)
            {
                return;
            }

            if ((eventData.position - pointerDownScreenPosition).sqrMagnitude > 28f * 28f)
            {
                return;
            }

            ResolveReferences();
            if (playerController == null)
            {
                return;
            }

            if (!TryResolveLockableTargetAtScreenPosition(eventData.position, out SummonUnit summonTarget, out EnemyAI bossTarget, out BattleStructure structureTarget))
            {
                return;
            }

            bool changed = summonTarget != null
                ? playerController.ToggleManualTarget(summonTarget)
                : structureTarget != null
                    ? playerController.ToggleManualTarget(structureTarget)
                    : bossTarget != null && playerController.ToggleManualTarget(bossTarget);
            if (changed)
            {
                CardHandTouchCoordinator.SuppressClicks(0.08f);
            }
        }

        private Vector2 ResolveMoveInput()
        {
            if (isDirectDodgeModeActive)
            {
                return Vector2.zero;
            }

            return currentInputMode == MobileMoveInputMode.TapAssist
                ? ResolveTapAssistMoveInput()
                : moveVector;
        }

        private Vector2 ResolveTapAssistMoveInput()
        {
            if (!hasTapMoveDestination || playerController == null)
            {
                return Vector2.zero;
            }

            Vector3 planarDelta = tapMoveDestination - playerController.transform.position;
            planarDelta.y = 0f;
            if (planarDelta.sqrMagnitude <= tapArrivalDistance * tapArrivalDistance)
            {
                ClearTapMoveDestination();
                return Vector2.zero;
            }

            return Vector2.ClampMagnitude(new Vector2(planarDelta.x, planarDelta.z), 1f);
        }

        private void UpdateTapAssistMovement()
        {
            if (currentInputMode != MobileMoveInputMode.TapAssist)
            {
                return;
            }

            if (battleCamera != null && battleCamera.IsOverviewMode)
            {
                ClearTapMoveDestination();
            }
        }

        private void UpdateDirectDodgeModeState()
        {
            bool shouldEnable =
                isTouchLayoutActive &&
                HasAnyDirectProjectileWarning() &&
                (battleCamera == null || !battleCamera.IsOverviewMode);

            if (shouldEnable == isDirectDodgeModeActive)
            {
                return;
            }

            isDirectDodgeModeActive = shouldEnable;
            pendingDirectDodgeDirection = 0f;
            isSummonPlacementActive = false;
            moveVector = Vector2.zero;
            ClearTapMoveDestination();
            RefreshStickVisual();
        }

        private bool HasAnyDirectProjectileWarning()
        {
            return HasAnyDirectProjectileDanger(out _, out _);
        }

        private bool HasAnyDirectProjectileDanger(out bool hasActiveProjectileDanger, out bool hasLockingDanger)
        {
            hasActiveProjectileDanger = enemyAI != null &&
                enemyAI.IsDirectProjectileDangerActive &&
                enemyAI.ActiveDirectProjectileCount > 0;
            hasLockingDanger = enemyAI != null && enemyAI.IsDirectProjectileLocking;

            if (PveProjectileEmitter.HasAnyDirectProjectileDanger)
            {
                hasActiveProjectileDanger = true;
            }

            if (PveProjectileEmitter.HasAnyDirectProjectileLocking)
            {
                hasLockingDanger = true;
            }

            return hasActiveProjectileDanger || hasLockingDanger;
        }

        internal void HandleDirectDodgeRelease(PointerEventData eventData, Vector2 pointerDownScreenPosition)
        {
            if (!isDirectDodgeModeActive || directDodgePadRoot == null)
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    directDodgePadRoot,
                    pointerDownScreenPosition,
                    eventData.pressEventCamera,
                    out Vector2 startLocalPoint))
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    directDodgePadRoot,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 endLocalPoint))
            {
                return;
            }

            float minimumFlickDistance = Mathf.Max(36f, directDodgePadRoot.rect.width * 0.08f);
            float deltaX = endLocalPoint.x - startLocalPoint.x;
            if (Mathf.Abs(deltaX) < minimumFlickDistance)
            {
                return;
            }

            pendingDirectDodgeDirection = Mathf.Sign(deltaX);
            CardHandTouchCoordinator.SuppressClicks(0.08f);
        }

        internal void HandleLaneSwipeRelease(PointerEventData eventData, Vector2 pointerDownScreenPosition)
        {
            // Manual player lane selection is retired in the escort-lock prototype.
        }

        private void SetInputMode(MobileMoveInputMode nextMode)
        {
            if (currentInputMode == nextMode)
            {
                return;
            }

            currentInputMode = nextMode;
            moveVector = Vector2.zero;
            ClearTapMoveDestination();
            RefreshStickVisual();
        }

        private void ToggleInputMode()
        {
            SetInputMode(currentInputMode == MobileMoveInputMode.Joystick
                ? MobileMoveInputMode.TapAssist
                : MobileMoveInputMode.Joystick);
        }

        private bool TryResolveTapMoveDestination(Vector2 screenPosition, Camera eventCamera, out Vector3 targetPosition)
        {
            ResolveReferences();
            if (playerController == null)
            {
                targetPosition = Vector3.zero;
                return false;
            }

            Camera worldCamera = battleViewCamera != null
                ? battleViewCamera
                : battleCamera != null
                    ? battleCamera.GetComponent<Camera>()
                    : Camera.main;
            if (worldCamera == null)
            {
                targetPosition = Vector3.zero;
                return false;
            }

            float groundY = playerController.transform.position.y;
            Plane groundPlane = new(Vector3.up, new Vector3(0f, groundY, 0f));
            Ray inputRay = worldCamera.ScreenPointToRay(screenPosition);
            if (!groundPlane.Raycast(inputRay, out float hitDistance))
            {
                targetPosition = Vector3.zero;
                return false;
            }

            targetPosition = inputRay.GetPoint(hitDistance);
            targetPosition = playerController.ClampToMovementBounds(targetPosition);
            targetPosition.y = groundY;
            return true;
        }

        private bool TryResolveLockableTargetAtScreenPosition(
            Vector2 screenPosition,
            out SummonUnit summonTarget,
            out EnemyAI bossTarget,
            out BattleStructure structureTarget)
        {
            summonTarget = null;
            bossTarget = null;
            structureTarget = null;

            Camera worldCamera = battleViewCamera != null
                ? battleViewCamera
                : battleCamera != null
                    ? battleCamera.GetComponent<Camera>()
                    : Camera.main;
            if (worldCamera == null)
            {
                return false;
            }

            Ray ray = worldCamera.ScreenPointToRay(screenPosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, 256f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);
            if (hits == null || hits.Length == 0)
            {
                return false;
            }

            float bestDistance = float.MaxValue;
            for (int index = 0; index < hits.Length; index++)
            {
                Collider hitCollider = hits[index].collider;
                if (hitCollider == null)
                {
                    continue;
                }

                SummonUnit hitSummon = hitCollider.GetComponent<SummonUnit>();
                if (hitSummon == null)
                {
                    hitSummon = hitCollider.GetComponentInParent<SummonUnit>();
                }

                if (hitSummon != null && hitSummon.IsAlive && !hitSummon.IsPlayerTeam && hits[index].distance < bestDistance)
                {
                    summonTarget = hitSummon;
                    bossTarget = null;
                    structureTarget = null;
                    bestDistance = hits[index].distance;
                    continue;
                }

                BattleStructure hitStructure = hitCollider.GetComponent<BattleStructure>();
                if (hitStructure == null)
                {
                    hitStructure = hitCollider.GetComponentInParent<BattleStructure>();
                }

                if (hitStructure != null && !hitStructure.IsDestroyed && hits[index].distance < bestDistance)
                {
                    summonTarget = null;
                    bossTarget = null;
                    structureTarget = hitStructure;
                    bestDistance = hits[index].distance;
                    continue;
                }

                EnemyAI hitBoss = hitCollider.GetComponent<EnemyAI>();
                if (hitBoss == null)
                {
                    hitBoss = hitCollider.GetComponentInParent<EnemyAI>();
                }

                if (hitBoss != null && hitBoss.isActiveAndEnabled && hitBoss.gameObject.activeInHierarchy && hits[index].distance < bestDistance)
                {
                    summonTarget = null;
                    bossTarget = hitBoss;
                    structureTarget = null;
                    bestDistance = hits[index].distance;
                }
            }

            return summonTarget != null || bossTarget != null || structureTarget != null;
        }

        private void ClearTapMoveDestination()
        {
            hasTapMoveDestination = false;
            tapMoveDestination = Vector3.zero;
        }

        internal void AddOverviewDrag(Vector2 dragDelta)
        {
            pendingOverviewDragDelta += dragDelta;
        }

        internal void QueueOverviewZoomStep(float zoomStep)
        {
            pendingOverviewZoomStep += zoomStep;
        }

        private void HandleSkillButtonPressed()
        {
            if (!isTouchLayoutActive)
            {
                return;
            }

            pendingSkillPress = true;
        }

        private void HandleMapButtonPressed()
        {
            if (!isTouchLayoutActive)
            {
                return;
            }

            pendingMapTogglePress = true;
        }

        private void HandleMoveModeButtonPressed()
        {
            if (!isTouchLayoutActive)
            {
                return;
            }

            // Legacy move mode toggle intentionally disabled in the focus-lane prototype.
        }

        private void HandleFocusLanePressed(int laneIndex)
        {
            // Manual player lane selection is retired in the escort-lock prototype.
        }

        private int ResolveCurrentFocusLaneForSwipe()
        {
            if (playerController != null)
            {
                return BattleLaneUtility.ClampLaneIndex(playerController.PreferredLaneIndex);
            }

            if (summonSpawner != null)
            {
                return BattleLaneUtility.ClampLaneIndex(summonSpawner.SelectedLaneIndex);
            }

            return BattleLaneUtility.DefaultLaneCount / 2;
        }

        private bool BeginSummonPlacementInternal(Vector2 screenPosition)
        {
            if (!isTouchLayoutActive || isDirectDodgeModeActive || laneRailRoot == null)
            {
                return false;
            }

            isSummonPlacementActive = true;
            hasPreviewDropWorldPosition = false;
            previewDropWorldPosition = Vector3.zero;
            lastPlacementFailureReason = SummonPlacementFailureReason.None;
            int initialLane = summonSpawner != null
                ? summonSpawner.SelectedLaneIndex
                : BattleLaneUtility.DefaultLaneCount / 2;
            previewLaneIndex = BattleLaneUtility.ClampLaneIndex(initialLane);
            if (TryResolveDefaultSummonDropWorldPosition(previewLaneIndex, out Vector3 defaultDropWorldPosition))
            {
                previewDropWorldPosition = defaultDropWorldPosition;
                hasPreviewDropWorldPosition = true;
            }
            UpdateSummonPlacementInternal(screenPosition);
            return true;
        }

        private void UpdateSummonPlacementInternal(Vector2 screenPosition)
        {
            if (!isSummonPlacementActive)
            {
                return;
            }

            if (TryResolveSummonDropFromScreenPosition(screenPosition, out int laneIndex, out Vector3 dropWorldPosition))
            {
                previewLaneIndex = laneIndex;
                previewDropWorldPosition = dropWorldPosition;
                hasPreviewDropWorldPosition = true;
                summonSpawner?.SetSelectedLane(previewLaneIndex);
            }
        }

        private bool TryCompleteSummonPlacementInternal(
            Vector2 screenPosition,
            out int laneIndex,
            out SummonPlacementFailureReason failureReason)
        {
            laneIndex = previewLaneIndex;
            failureReason = SummonPlacementFailureReason.None;
            if (!isSummonPlacementActive)
            {
                failureReason = SummonPlacementFailureReason.LaneNotSelected;
                return false;
            }

            bool resolved = TryResolveSummonDropFromScreenPosition(screenPosition, out laneIndex, out Vector3 dropWorldPosition);
            if (resolved)
            {
                previewLaneIndex = laneIndex;
                previewDropWorldPosition = dropWorldPosition;
                hasPreviewDropWorldPosition = true;
                summonSpawner?.SetSelectedLane(previewLaneIndex);
                if (hasPreviewDropWorldPosition)
                {
                    summonSpawner?.SetPendingPlacementWorldPosition(previewDropWorldPosition);
                }
                lastPlacementFailureReason = SummonPlacementFailureReason.None;
            }
            else
            {
                failureReason = SummonPlacementFailureReason.LaneNotSelected;
                lastPlacementFailureReason = failureReason;
                summonSpawner?.ClearPendingPlacementWorldPosition();
                ShowPlacementFailureFeedback(failureReason);
            }

            isSummonPlacementActive = false;
            return resolved;
        }

        private void CancelSummonPlacementInternal()
        {
            isSummonPlacementActive = false;
            hasPreviewDropWorldPosition = false;
            previewDropWorldPosition = Vector3.zero;
            summonSpawner?.ClearPendingPlacementWorldPosition();
            lastPlacementFailureReason = SummonPlacementFailureReason.None;
        }

        private bool TryResolveSummonDropFromScreenPosition(Vector2 screenPosition, out int laneIndex, out Vector3 dropWorldPosition)
        {
            laneIndex = BattleLaneUtility.DefaultLaneCount / 2;
            dropWorldPosition = Vector3.zero;
            ResolveReferences();
            if (safeAreaRoot == null || playerController == null)
            {
                return false;
            }

            BattleManager battleManager = BattleManager.Instance;
            Camera worldCamera = battleViewCamera != null
                ? battleViewCamera
                : battleCamera != null
                    ? battleCamera.GetComponent<Camera>()
                    : Camera.main;
            if (battleManager == null || worldCamera == null)
            {
                return false;
            }

            float groundY = playerController.transform.position.y;
            Plane groundPlane = new(Vector3.up, new Vector3(0f, groundY, 0f));
            Ray inputRay = worldCamera.ScreenPointToRay(screenPosition);
            if (!groundPlane.Raycast(inputRay, out float hitDistance))
            {
                return false;
            }

            Vector3 rawWorldPosition = inputRay.GetPoint(hitDistance);
            dropWorldPosition = ClampSummonDropWorldPosition(rawWorldPosition, battleManager, out laneIndex);
            return true;
        }

        private bool TryResolveDefaultSummonDropWorldPosition(int laneIndex, out Vector3 dropWorldPosition)
        {
            ResolveReferences();
            if (playerController == null)
            {
                dropWorldPosition = Vector3.zero;
                return false;
            }

            BattleManager battleManager = BattleManager.Instance;
            Vector3 basePosition = playerController.transform.position + new Vector3(0f, 0f, 1.25f);
            dropWorldPosition = ClampSummonDropWorldPosition(basePosition, battleManager, out int resolvedLaneIndex);
            previewLaneIndex = resolvedLaneIndex;
            return true;
        }

        private Vector3 ClampSummonDropWorldPosition(Vector3 worldPosition, BattleManager battleManager, out int laneIndex)
        {
            Vector3 playerPosition = playerController.transform.position;
            float halfWidth = battleManager != null
                ? Mathf.Max(4.8f, battleManager.LaneHalfWidth * 0.86f)
                : 5.4f;
            float minZ = playerPosition.z - 0.7f;
            float maxZ = playerPosition.z + 2.8f;

            Vector3 clamped = worldPosition;
            clamped.x = Mathf.Clamp(clamped.x, playerPosition.x - halfWidth, playerPosition.x + halfWidth);
            clamped.z = Mathf.Clamp(clamped.z, minZ, maxZ);
            clamped.y = playerPosition.y;
            clamped = playerController.ClampToMovementBounds(clamped);

            laneIndex = battleManager != null
                ? battleManager.GetNearestLaneIndex(clamped.x)
                : BattleLaneUtility.GetNearestLaneIndex(clamped.x, BattleLaneUtility.BuildLaneAnchors(5.75f));
            float laneCenterX = battleManager != null
                ? battleManager.GetLaneCenterX(laneIndex)
                : BattleLaneUtility.GetLaneCenterX(laneIndex, 5.75f);
            clamped.x = Mathf.Lerp(clamped.x, laneCenterX, 0.12f);
            return clamped;
        }

        private bool TryGetLaneScreenSamplePoint(int laneIndex, out Vector3 laneWorld)
        {
            laneWorld = Vector3.zero;
            BattleManager battleManager = BattleManager.Instance;
            if (battleManager == null)
            {
                return false;
            }

            int clampedLane = BattleLaneUtility.ClampLaneIndex(laneIndex, battleManager.LaneCount);
            if (battleManager.TryGetSummonLanePreview(clampedLane, out BattleManager.SummonLanePreview preview))
            {
                laneWorld = preview.LandingPosition + new Vector3(0f, 0.1f, 0f);
                return true;
            }

            float sampleY = playerController != null ? playerController.transform.position.y : 0f;
            float sampleZ = battleManager.SummonSpawnPoint != null
                ? battleManager.SummonSpawnPoint.position.z + 0.8f
                : Mathf.Clamp(battleManager.LaneLength * 0.14f, 4.5f, battleManager.LaneLength - 10f);
            laneWorld = new Vector3(battleManager.GetLaneCenterX(clampedLane), sampleY, sampleZ);
            return true;
        }

        private void ShowPlacementFailureFeedback(SummonPlacementFailureReason failureReason)
        {
            if (failureReason == SummonPlacementFailureReason.None)
            {
                return;
            }

            Vector3 feedbackPosition = playerController != null
                ? playerController.transform.position + new Vector3(0f, 2.2f, 0f)
                : Vector3.zero;
            BattlePresentationController.Instance?.ShowWorldText(
                feedbackPosition,
                "레인 선택 안 됨",
                new Color(1f, 0.56f, 0.44f, 1f),
                3.8f,
                0.7f);
        }

        private void HandlePosturePushPressed()
        { }

        private void HandlePostureHoldPressed()
        { }

        private void HandlePostureFallbackPressed()
        { }

        private void HandleZoomInButtonPressed()
        {
            if (!isTouchLayoutActive)
            {
                return;
            }

            pendingOverviewZoomStep += 1f;
        }

        private void HandleZoomOutButtonPressed()
        {
            if (!isTouchLayoutActive)
            {
                return;
            }

            pendingOverviewZoomStep -= 1f;
        }

        private void HandleCenterButtonPressed()
        {
            if (!isTouchLayoutActive)
            {
                return;
            }

            pendingOverviewCenterPress = true;
        }

        private void ResolveReferences()
        {
            if (battleEnergySystem == null)
            {
                battleEnergySystem = BattleEnergySystem.Instance;
            }

            if (battleCamera == null)
            {
                battleCamera = FindFirstObjectByType<BattleCamera>();
            }

            if (battleViewCamera == null && battleCamera != null)
            {
                battleViewCamera = battleCamera.GetComponent<Camera>();
            }

            if (enemyAI == null)
            {
                enemyAI = FindFirstObjectByType<EnemyAI>();
            }

            if (summonSpawner == null)
            {
                summonSpawner = FindFirstObjectByType<SummonSpawner>();
            }

            if (playerController == null)
            {
                playerController = BattleManager.Instance != null ? BattleManager.Instance.PlayerController : null;
                if (playerController == null)
                {
                    playerController = FindFirstObjectByType<PlayerController>();
                }
            }

            if (playerSkillController == null)
            {
                PlayerController resolvedPlayer = BattleManager.Instance != null ? BattleManager.Instance.PlayerController : null;
                if (resolvedPlayer == null)
                {
                    resolvedPlayer = FindFirstObjectByType<PlayerController>();
                }

                if (resolvedPlayer != null)
                {
                    playerController = resolvedPlayer;
                    playerSkillController = resolvedPlayer.GetComponent<PlayerSkillController>();
                }
            }
        }

        private void EnsureUi()
        {
            RectTransform root = transform as RectTransform;
            if (root == null)
            {
                return;
            }

            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;

            runtimeFont ??= RuntimeUIFontUtility.EnsureKoreanFallback();
            if (runtimeFont == null)
            {
                return;
            }

            if (safeAreaRoot == null)
            {
                safeAreaRoot = EnsureRect("SafeAreaRoot", root);
                safeAreaRoot.SetAsLastSibling();
            }

            if (stickRoot == null)
            {
                stickRoot = EnsurePanel("MoveStickRoot", safeAreaRoot, panelTint);
                stickVisual = EnsurePanel("StickVisual", stickRoot, stickReadyTint);
                stickKnob = EnsurePanel("StickKnob", stickRoot, new Color(1f, 1f, 1f, 0.92f));
                stickVisualImage = stickVisual.GetComponent<Image>();
                stickKnobImage = stickKnob.GetComponent<Image>();
                moveHintText = EnsureText("MoveHintText", stickRoot, runtimeFont, 14f, TextAlignmentOptions.Center);
                moveHintText.text = "\uC774\uB3D9";

                MobileStickPad stickPad = stickRoot.gameObject.GetComponent<MobileStickPad>();
                if (stickPad == null)
                {
                    stickPad = stickRoot.gameObject.AddComponent<MobileStickPad>();
                }

                stickPad.Initialize(this, stickRoot, stickKnob);
            }

            if (moveTrayRoot == null)
            {
                moveTrayRoot = EnsurePanel("MoveTray", safeAreaRoot, new Color(0.03f, 0.06f, 0.11f, 0.3f));
                Image moveTrayImage = moveTrayRoot.GetComponent<Image>();
                if (moveTrayImage != null)
                {
                    moveTrayImage.raycastTarget = false;
                }
                moveTrayRoot.SetAsFirstSibling();
            }

            if (tapMovePadRoot == null)
            {
                tapMovePadRoot = EnsurePanel("TapMovePad", safeAreaRoot, new Color(1f, 1f, 1f, 0.01f));
                tapMovePadHintText = EnsureText("TapMoveHintText", tapMovePadRoot, runtimeFont, 15f, TextAlignmentOptions.Center);
                tapMovePadHintText.text = "적 또는 목표를 눌러 고정";
                tapMovePadHintText.color = new Color(0.82f, 0.92f, 1f, 0.84f);
                tapMovePadHintText.text = "\uC801 \uB610\uB294 \uBAA9\uD45C\uB97C \uB20C\uB7EC \uACE0\uC815";

                MobileTapMovePad tapMovePad = tapMovePadRoot.gameObject.GetComponent<MobileTapMovePad>();
                if (tapMovePad == null)
                {
                    tapMovePad = tapMovePadRoot.gameObject.AddComponent<MobileTapMovePad>();
                }

                tapMovePad.Initialize(this);
                tapMovePadRoot.SetAsFirstSibling();
            }

            if (targetLockMarkerRoot == null)
            {
                targetLockMarkerRoot = EnsurePanel("TargetLockMarker", safeAreaRoot, new Color(0.08f, 0.2f, 0.34f, 0.9f));
                targetLockMarkerImage = targetLockMarkerRoot.GetComponent<Image>();
                if (targetLockMarkerImage != null)
                {
                    targetLockMarkerImage.raycastTarget = false;
                }

                targetLockMarkerText = EnsureText("Label", targetLockMarkerRoot, runtimeFont, 12f, TextAlignmentOptions.Center);
                targetLockMarkerText.text = "LOCK";
                targetLockMarkerText.color = new Color(0.94f, 0.98f, 1f, 1f);
                targetLockMarkerRoot.gameObject.SetActive(false);
                targetLockMarkerRoot.SetAsLastSibling();
            }

            if (directDodgePadRoot == null)
            {
                directDodgePadRoot = EnsurePanel("DirectDodgePad", safeAreaRoot, directDodgeLockTint);
                directDodgeTitleText = EnsureText("DirectDodgeTitle", directDodgePadRoot, runtimeFont, 17f, TextAlignmentOptions.Center);
                directDodgeTitleText.text = "직격 위험";
                directDodgeLeftText = EnsureText("DirectDodgeLeft", directDodgePadRoot, runtimeFont, 15f, TextAlignmentOptions.Center);
                directDodgeLeftText.text = "왼쪽 회피";
                directDodgeRightText = EnsureText("DirectDodgeRight", directDodgePadRoot, runtimeFont, 15f, TextAlignmentOptions.Center);
                directDodgeRightText.text = "오른쪽 회피";

                MobileDirectDodgePad directDodgePad = directDodgePadRoot.gameObject.GetComponent<MobileDirectDodgePad>();
                if (directDodgePad == null)
                {
                    directDodgePad = directDodgePadRoot.gameObject.AddComponent<MobileDirectDodgePad>();
                }

                directDodgeTitleText.text = "\uC9C1\uACA9 \uC704\uD5D8";
                directDodgeLeftText.text = "\uC67C\uCABD \uD68C\uD53C";
                directDodgeRightText.text = "\uC624\uB978\uCABD \uD68C\uD53C";
                directDodgePad.Initialize(this);
                directDodgePadRoot.SetAsLastSibling();
            }

            if (skillButtonRoot == null)
            {
                skillButtonRoot = EnsurePanel("SkillButton", safeAreaRoot, skillReadyTint);
                skillButtonImage = skillButtonRoot.GetComponent<Image>();
                skillButton = skillButtonRoot.GetComponent<Button>();
                if (skillButton == null)
                {
                    skillButton = skillButtonRoot.gameObject.AddComponent<Button>();
                }

                skillButton.onClick.RemoveListener(HandleSkillButtonPressed);
                skillButton.onClick.AddListener(HandleSkillButtonPressed);
                skillButtonText = EnsureText("Label", skillButtonRoot, runtimeFont, 18f, TextAlignmentOptions.Center);
            }

            if (mapButtonRoot == null)
            {
                mapButtonRoot = EnsurePanel("MapButton", safeAreaRoot, utilityButtonTint);
                mapButtonImage = mapButtonRoot.GetComponent<Image>();
                mapButton = mapButtonRoot.GetComponent<Button>();
                if (mapButton == null)
                {
                    mapButton = mapButtonRoot.gameObject.AddComponent<Button>();
                }

                mapButton.onClick.RemoveListener(HandleMapButtonPressed);
                mapButton.onClick.AddListener(HandleMapButtonPressed);
                mapButtonText = EnsureText("Label", mapButtonRoot, runtimeFont, 16f, TextAlignmentOptions.Center);
            }

            if (laneRailRoot == null)
            {
                laneRailRoot = EnsurePanel("LaneRail", safeAreaRoot, new Color(0.05f, 0.09f, 0.16f, 0.08f));
                laneRailImage = laneRailRoot.GetComponent<Image>();
                MobileLaneSwipePad laneSwipePad = laneRailRoot.gameObject.GetComponent<MobileLaneSwipePad>();
                if (laneSwipePad == null)
                {
                    laneSwipePad = laneRailRoot.gameObject.AddComponent<MobileLaneSwipePad>();
                }

                laneSwipePad.Initialize(this);

                laneStatusText = EnsureText("LaneStatus", laneRailRoot, runtimeFont, 12f, TextAlignmentOptions.Center);
                laneStatusText.text = "추천: 2번 레인 소환";
                laneStatusText.raycastTarget = false;
                laneStatusText.text = "\uCD94\uCC9C: 2\uBC88 \uB808\uC778 \uC18C\uD658";

                laneMarkerContainer = EnsureRect("LaneMarkers", laneRailRoot);
                laneMarkerImages.Clear();
                laneMarkerButtons.Clear();
                laneMarkerTexts.Clear();
                for (int index = 0; index < BattleLaneUtility.DefaultLaneCount; index++)
                {
                    RectTransform markerRect = EnsurePanel($"LaneMarker{index}", laneMarkerContainer, new Color(0.34f, 0.82f, 1f, 0.16f));
                    Image markerImage = markerRect.GetComponent<Image>();
                    if (markerImage != null)
                    {
                        markerImage.raycastTarget = false;
                    }

                    Button markerButton = markerRect.GetComponent<Button>() ?? markerRect.gameObject.AddComponent<Button>();
                    markerButton.onClick.RemoveAllListeners();
                    markerButton.interactable = false;
                    TMP_Text markerText = EnsureText("LaneLabel", markerRect, runtimeFont, 11f, TextAlignmentOptions.Center);
                    markerText.text = (index + 1).ToString();
                    markerText.raycastTarget = false;
                    laneMarkerImages.Add(markerImage);
                    laneMarkerButtons.Add(markerButton);
                    laneMarkerTexts.Add(markerText);
                }
            }

            if (summonPlacementPreviewRoot == null)
            {
                summonPlacementPreviewRoot = EnsureRect("SummonPlacementPreview", safeAreaRoot);
                summonPlacementPreviewRoot.SetAsLastSibling();

                summonPlacementLaneRibbon = EnsurePanel("LaneRibbon", summonPlacementPreviewRoot, new Color(0.18f, 0.78f, 1f, 0.12f));
                summonPlacementLaneRibbonImage = summonPlacementLaneRibbon.GetComponent<Image>();
                if (summonPlacementLaneRibbonImage != null)
                {
                    summonPlacementLaneRibbonImage.raycastTarget = false;
                }

                summonPlacementGuideLine = EnsurePanel("LaneGuideLine", summonPlacementPreviewRoot, new Color(0.24f, 0.88f, 1f, 0.24f));
                summonPlacementGuideLineImage = summonPlacementGuideLine.GetComponent<Image>();
                if (summonPlacementGuideLineImage != null)
                {
                    summonPlacementGuideLineImage.raycastTarget = false;
                }

                summonPlacementGuideMarker = EnsurePanel("LaneGuideMarker", summonPlacementPreviewRoot, new Color(0.09f, 0.2f, 0.32f, 0.94f));
                summonPlacementGuideMarkerImage = summonPlacementGuideMarker.GetComponent<Image>();
                if (summonPlacementGuideMarkerImage != null)
                {
                    summonPlacementGuideMarkerImage.raycastTarget = false;
                }

                summonPlacementPreviewText = EnsureText("LaneGuideText", summonPlacementGuideMarker, runtimeFont, 14f, TextAlignmentOptions.Center);
                summonPlacementPreviewText.text = "DROP LANE 3";
                summonPlacementPreviewText.color = new Color(0.88f, 0.97f, 1f, 1f);

                summonPlacementLandingMarker = EnsurePanel("LandingMarker", summonPlacementPreviewRoot, new Color(0.98f, 0.58f, 0.96f, 0.95f));
                summonPlacementLandingMarkerImage = summonPlacementLandingMarker.GetComponent<Image>();
                summonPlacementLandingText = EnsureText("Label", summonPlacementLandingMarker, runtimeFont, 11f, TextAlignmentOptions.Center);
                summonPlacementLandingText.text = "소환";
                summonPlacementLandingText.color = new Color(0.12f, 0.05f, 0.13f, 1f);

                summonPlacementPocketMarker = EnsurePanel("PocketMarker", summonPlacementPreviewRoot, new Color(0.45f, 0.95f, 1f, 0.95f));
                summonPlacementPocketMarkerImage = summonPlacementPocketMarker.GetComponent<Image>();
                summonPlacementPocketText = EnsureText("Label", summonPlacementPocketMarker, runtimeFont, 11f, TextAlignmentOptions.Center);
                summonPlacementPocketText.text = "POCKET";
                summonPlacementPocketText.color = new Color(0.05f, 0.12f, 0.16f, 1f);

                summonPlacementBlockerMarker = EnsurePanel("BlockerMarker", summonPlacementPreviewRoot, new Color(1f, 0.78f, 0.34f, 0.95f));
                summonPlacementBlockerMarkerImage = summonPlacementBlockerMarker.GetComponent<Image>();
                summonPlacementBlockerText = EnsureText("Label", summonPlacementBlockerMarker, runtimeFont, 11f, TextAlignmentOptions.Center);
                summonPlacementBlockerText.text = "BREAK";
                summonPlacementBlockerText.color = new Color(0.18f, 0.12f, 0.05f, 1f);

                summonPlacementRewardMarker = EnsurePanel("RewardMarker", summonPlacementPreviewRoot, new Color(0.42f, 1f, 0.68f, 0.95f));
                summonPlacementRewardMarkerImage = summonPlacementRewardMarker.GetComponent<Image>();
                summonPlacementRewardText = EnsureText("Label", summonPlacementRewardMarker, runtimeFont, 11f, TextAlignmentOptions.Center);
                summonPlacementRewardText.text = "REWARD";
                summonPlacementRewardText.color = new Color(0.06f, 0.14f, 0.08f, 1f);
            }

            if (overviewPadRoot == null)
            {
                overviewPadRoot = EnsurePanel("OverviewDragPad", safeAreaRoot, new Color(1f, 1f, 1f, 0.01f));
                overviewPadHintText = EnsureText("OverviewHintText", overviewPadRoot, runtimeFont, 15f, TextAlignmentOptions.Center);
                overviewPadHintText.text = "DRAG TO PAN";
                overviewPadHintText.color = new Color(0.82f, 0.92f, 1f, 0.84f);

                MobileOverviewPad overviewPad = overviewPadRoot.gameObject.GetComponent<MobileOverviewPad>();
                if (overviewPad == null)
                {
                    overviewPad = overviewPadRoot.gameObject.AddComponent<MobileOverviewPad>();
                }

                overviewPad.Initialize(this);
                overviewPadRoot.SetAsFirstSibling();
            }

            if (zoomInButtonRoot == null)
            {
                zoomInButtonRoot = EnsurePanel("ZoomInButton", safeAreaRoot, utilityButtonTint);
                zoomInButtonImage = zoomInButtonRoot.GetComponent<Image>();
                zoomInButton = zoomInButtonRoot.GetComponent<Button>() ?? zoomInButtonRoot.gameObject.AddComponent<Button>();
                zoomInButton.onClick.RemoveListener(HandleZoomInButtonPressed);
                zoomInButton.onClick.AddListener(HandleZoomInButtonPressed);
                zoomInButtonText = EnsureText("Label", zoomInButtonRoot, runtimeFont, 20f, TextAlignmentOptions.Center);
                zoomInButtonText.text = "+";
            }

            if (zoomOutButtonRoot == null)
            {
                zoomOutButtonRoot = EnsurePanel("ZoomOutButton", safeAreaRoot, utilityButtonTint);
                zoomOutButtonImage = zoomOutButtonRoot.GetComponent<Image>();
                zoomOutButton = zoomOutButtonRoot.GetComponent<Button>() ?? zoomOutButtonRoot.gameObject.AddComponent<Button>();
                zoomOutButton.onClick.RemoveListener(HandleZoomOutButtonPressed);
                zoomOutButton.onClick.AddListener(HandleZoomOutButtonPressed);
                zoomOutButtonText = EnsureText("Label", zoomOutButtonRoot, runtimeFont, 20f, TextAlignmentOptions.Center);
                zoomOutButtonText.text = "-";
            }

            if (centerButtonRoot == null)
            {
                centerButtonRoot = EnsurePanel("CenterButton", safeAreaRoot, utilityButtonTint);
                centerButtonImage = centerButtonRoot.GetComponent<Image>();
                centerButton = centerButtonRoot.GetComponent<Button>() ?? centerButtonRoot.gameObject.AddComponent<Button>();
                centerButton.onClick.RemoveListener(HandleCenterButtonPressed);
                centerButton.onClick.AddListener(HandleCenterButtonPressed);
                centerButtonText = EnsureText("Label", centerButtonRoot, runtimeFont, 12f, TextAlignmentOptions.Center);
                centerButtonText.text = "CENTER";
            }

            RuntimeUIFontUtility.ApplyRecursively(safeAreaRoot);
        }

        private void ApplySafeAreaAndLayout(bool forceRefresh)
        {
            if (safeAreaRoot == null)
            {
                return;
            }

            RectTransform root = transform as RectTransform;
            if (root == null)
            {
                return;
            }

            Rect safeArea = Screen.safeArea;
            Vector2 rootSize = root.rect.size;
            if (!forceRefresh && safeArea == lastSafeArea && rootSize == lastRootSize)
            {
                return;
            }

            lastSafeArea = safeArea;
            lastRootSize = rootSize;

            Vector2 anchorMin = new(
                Screen.width <= 0 ? 0f : safeArea.xMin / Screen.width,
                Screen.height <= 0 ? 0f : safeArea.yMin / Screen.height);
            Vector2 anchorMax = new(
                Screen.width <= 0 ? 1f : safeArea.xMax / Screen.width,
                Screen.height <= 0 ? 1f : safeArea.yMax / Screen.height);

            safeAreaRoot.anchorMin = anchorMin;
            safeAreaRoot.anchorMax = anchorMax;
            safeAreaRoot.offsetMin = Vector2.zero;
            safeAreaRoot.offsetMax = Vector2.zero;

            if (summonPlacementPreviewRoot != null)
            {
                summonPlacementPreviewRoot.anchorMin = Vector2.zero;
                summonPlacementPreviewRoot.anchorMax = Vector2.one;
                summonPlacementPreviewRoot.offsetMin = Vector2.zero;
                summonPlacementPreviewRoot.offsetMax = Vector2.zero;
            }

            float safeWidth = safeAreaRoot.rect.width > 1f ? safeAreaRoot.rect.width : root.rect.width;
            float safeHeight = safeAreaRoot.rect.height > 1f ? safeAreaRoot.rect.height : root.rect.height;
            float scale = Mathf.Clamp(safeWidth / 720f, 0.84f, 1.12f);
            float aspect = safeWidth <= 0.001f ? 1f : safeHeight / safeWidth;
            bool compact = safeWidth <= 560f || aspect >= 1.65f;
            float widthBlend = Mathf.Clamp01((safeWidth - 360f) / 300f);
            float handLayoutScale = RuntimeCanvasLayoutUtility.ResolveSoftScale(root, 0.38f, 1.48f);
            float handBottomMargin = Mathf.Lerp(10f, 14f, widthBlend) * handLayoutScale;
            float handHeight = Mathf.Lerp(140f, 148f, widthBlend) * handLayoutScale;
            float handTop = handBottomMargin + handHeight;
            float moveTrayCenterY = handTop + Mathf.Lerp(64f, 78f, widthBlend);
            float tapPadBottomOffset = handTop + Mathf.Lerp(2f, 6f, widthBlend);
            float actionTrayCenterY = handTop + Mathf.Lerp(184f, 210f, widthBlend);

            if (stickRoot != null && stickRoot.TryGetComponent(out Image stickPanelImage))
            {
                stickPanelImage.color = new Color(0.04f, 0.08f, 0.13f, compact ? 0.18f : 0.22f);
            }

            if (stickRoot != null)
            {
                float stickSize = Mathf.Lerp(126f, 146f, widthBlend);
                stickRoot.anchorMin = new Vector2(0f, 0f);
                stickRoot.anchorMax = new Vector2(0f, 0f);
                stickRoot.pivot = new Vector2(0.5f, 0.5f);
                stickRoot.sizeDelta = Vector2.one * stickSize;
                stickRoot.anchoredPosition = new Vector2((stickSize * 0.5f) + (12f * scale), moveTrayCenterY);

                if (stickVisual != null)
                {
                    stickVisual.anchorMin = new Vector2(0.5f, 0.5f);
                    stickVisual.anchorMax = new Vector2(0.5f, 0.5f);
                    stickVisual.pivot = new Vector2(0.5f, 0.5f);
                    stickVisual.sizeDelta = Vector2.one * (stickSize * 0.76f);
                    stickVisual.anchoredPosition = Vector2.zero;
                }

                if (stickKnob != null)
                {
                    stickKnob.anchorMin = new Vector2(0.5f, 0.5f);
                    stickKnob.anchorMax = new Vector2(0.5f, 0.5f);
                    stickKnob.pivot = new Vector2(0.5f, 0.5f);
                    stickKnob.sizeDelta = Vector2.one * (stickSize * 0.34f);
                }

                if (moveHintText != null)
                {
                    RectTransform hintRect = moveHintText.rectTransform;
                    hintRect.anchorMin = new Vector2(0.5f, 1f);
                    hintRect.anchorMax = new Vector2(0.5f, 1f);
                    hintRect.pivot = new Vector2(0.5f, 0f);
                    hintRect.sizeDelta = new Vector2(96f, 20f);
                    hintRect.anchoredPosition = new Vector2(0f, 6f);
                    moveHintText.fontSize = Mathf.Lerp(11f, 14f, widthBlend);
                }
            }

            if (moveTrayRoot != null)
            {
                float trayWidth = Mathf.Lerp(144f, 164f, widthBlend);
                float trayHeight = Mathf.Lerp(156f, 178f, widthBlend);
                moveTrayRoot.anchorMin = new Vector2(0f, 0f);
                moveTrayRoot.anchorMax = new Vector2(0f, 0f);
                moveTrayRoot.pivot = new Vector2(0.5f, 0.5f);
                moveTrayRoot.sizeDelta = new Vector2(trayWidth, trayHeight);
                moveTrayRoot.anchoredPosition = new Vector2((trayWidth * 0.5f) + (8f * scale), moveTrayCenterY);

                if (moveTrayRoot.TryGetComponent(out Image moveTrayImage))
                {
                    moveTrayImage.color = new Color(0.06f, 0.1f, 0.16f, compact ? 0.22f : 0.26f);
                }
            }

            if (tapMovePadRoot != null)
            {
                tapMovePadRoot.anchorMin = new Vector2(0f, 0f);
                tapMovePadRoot.anchorMax = new Vector2(1f, 1f);
                tapMovePadRoot.offsetMin = new Vector2(8f, tapPadBottomOffset);
                tapMovePadRoot.offsetMax = new Vector2(-8f, -8f);

                if (tapMovePadHintText != null)
                {
                    RectTransform hintRect = tapMovePadHintText.rectTransform;
                    hintRect.anchorMin = new Vector2(0.5f, 1f);
                    hintRect.anchorMax = new Vector2(0.5f, 1f);
                    hintRect.pivot = new Vector2(0.5f, 1f);
                    hintRect.sizeDelta = new Vector2(220f, 22f);
                    hintRect.anchoredPosition = new Vector2(0f, -8f);
                    tapMovePadHintText.fontSize = Mathf.Lerp(12f, 15f, widthBlend);
                }
            }

            if (targetLockMarkerRoot != null)
            {
                targetLockMarkerRoot.anchorMin = new Vector2(0.5f, 0.5f);
                targetLockMarkerRoot.anchorMax = new Vector2(0.5f, 0.5f);
                targetLockMarkerRoot.pivot = new Vector2(0.5f, 0.5f);
                targetLockMarkerRoot.sizeDelta = new Vector2(Mathf.Lerp(74f, 92f, widthBlend), Mathf.Lerp(26f, 32f, widthBlend));

                if (targetLockMarkerText != null)
                {
                    RectTransform markerTextRect = targetLockMarkerText.rectTransform;
                    markerTextRect.anchorMin = Vector2.zero;
                    markerTextRect.anchorMax = Vector2.one;
                    markerTextRect.offsetMin = Vector2.zero;
                    markerTextRect.offsetMax = Vector2.zero;
                    targetLockMarkerText.fontSize = Mathf.Lerp(11f, 13f, widthBlend);
                }
            }

            if (directDodgePadRoot != null)
            {
                float dodgePadBottom = handTop + Mathf.Lerp(6f, 10f, widthBlend);
                float dodgePadTop = dodgePadBottom + Mathf.Lerp(46f, 56f, widthBlend);
                directDodgePadRoot.anchorMin = new Vector2(0f, 0f);
                directDodgePadRoot.anchorMax = new Vector2(1f, 0f);
                directDodgePadRoot.offsetMin = new Vector2(8f, dodgePadBottom);
                directDodgePadRoot.offsetMax = new Vector2(-8f, dodgePadTop);
                directDodgePadRoot.SetAsLastSibling();

                if (directDodgeTitleText != null)
                {
                    RectTransform titleRect = directDodgeTitleText.rectTransform;
                    titleRect.anchorMin = new Vector2(0.5f, 1f);
                    titleRect.anchorMax = new Vector2(0.5f, 1f);
                    titleRect.pivot = new Vector2(0.5f, 1f);
                    titleRect.sizeDelta = new Vector2(200f, 20f);
                    titleRect.anchoredPosition = new Vector2(0f, -7f);
                    directDodgeTitleText.fontSize = Mathf.Lerp(12.5f, 14.5f, widthBlend);
                }

                if (directDodgeLeftText != null)
                {
                    RectTransform leftRect = directDodgeLeftText.rectTransform;
                    leftRect.anchorMin = new Vector2(0.25f, 0.5f);
                    leftRect.anchorMax = new Vector2(0.25f, 0.5f);
                    leftRect.pivot = new Vector2(0.5f, 0.5f);
                    leftRect.sizeDelta = new Vector2(108f, 18f);
                    leftRect.anchoredPosition = new Vector2(0f, -5f);
                    directDodgeLeftText.fontSize = Mathf.Lerp(10.5f, 12.5f, widthBlend);
                }

                if (directDodgeRightText != null)
                {
                    RectTransform rightRect = directDodgeRightText.rectTransform;
                    rightRect.anchorMin = new Vector2(0.75f, 0.5f);
                    rightRect.anchorMax = new Vector2(0.75f, 0.5f);
                    rightRect.pivot = new Vector2(0.5f, 0.5f);
                    rightRect.sizeDelta = new Vector2(108f, 18f);
                    rightRect.anchoredPosition = new Vector2(0f, -5f);
                    directDodgeRightText.fontSize = Mathf.Lerp(10.5f, 12.5f, widthBlend);
                }
            }

            if (laneRailRoot != null)
            {
                float laneRailBottom = handTop + Mathf.Lerp(66f, 80f, widthBlend);
                float laneRailHeight = Mathf.Lerp(30f, 34f, widthBlend);
                laneRailRoot.anchorMin = new Vector2(0f, 0f);
                laneRailRoot.anchorMax = new Vector2(1f, 0f);
                float railInset = Mathf.Lerp(18f, 24f, widthBlend);
                laneRailRoot.offsetMin = new Vector2(railInset, laneRailBottom);
                laneRailRoot.offsetMax = new Vector2(-railInset, laneRailBottom + laneRailHeight);

                RectTransform laneLeftRoot = laneLeftButton != null ? laneLeftButton.transform as RectTransform : null;
                if (laneLeftRoot != null)
                {
                    laneLeftRoot.anchorMin = new Vector2(0f, 0.5f);
                    laneLeftRoot.anchorMax = new Vector2(0f, 0.5f);
                    laneLeftRoot.pivot = new Vector2(0f, 0.5f);
                    laneLeftRoot.sizeDelta = new Vector2(Mathf.Lerp(44f, 52f, widthBlend), Mathf.Lerp(30f, 36f, widthBlend));
                    laneLeftRoot.anchoredPosition = new Vector2(8f, 0f);
                }

                RectTransform laneRightRoot = laneRightButton != null ? laneRightButton.transform as RectTransform : null;
                if (laneRightRoot != null)
                {
                    laneRightRoot.anchorMin = new Vector2(1f, 0.5f);
                    laneRightRoot.anchorMax = new Vector2(1f, 0.5f);
                    laneRightRoot.pivot = new Vector2(1f, 0.5f);
                    laneRightRoot.sizeDelta = new Vector2(Mathf.Lerp(44f, 52f, widthBlend), Mathf.Lerp(30f, 36f, widthBlend));
                    laneRightRoot.anchoredPosition = new Vector2(-8f, 0f);
                }

                if (laneStatusText != null)
                {
                    RectTransform statusRect = laneStatusText.rectTransform;
                    statusRect.anchorMin = new Vector2(0.5f, 0f);
                    statusRect.anchorMax = new Vector2(0.5f, 0f);
                    statusRect.pivot = new Vector2(0.5f, 0f);
                    statusRect.sizeDelta = new Vector2(292f, 16f);
                    statusRect.anchoredPosition = new Vector2(0f, 2.5f);
                    laneStatusText.fontSize = Mathf.Lerp(11f, 12.2f, widthBlend);
                }

                if (laneMarkerContainer != null)
                {
                    laneMarkerContainer.anchorMin = new Vector2(0.5f, 1f);
                    laneMarkerContainer.anchorMax = new Vector2(0.5f, 1f);
                    laneMarkerContainer.pivot = new Vector2(0.5f, 1f);
                    laneMarkerContainer.sizeDelta = new Vector2(Mathf.Lerp(196f, 236f, widthBlend), 14f);
                    laneMarkerContainer.anchoredPosition = new Vector2(0f, -2f);

                    float markerSpacing = laneMarkerImages.Count > 1
                        ? laneMarkerContainer.sizeDelta.x / (laneMarkerImages.Count - 1f)
                        : 0f;
                    for (int index = 0; index < laneMarkerImages.Count; index++)
                    {
                        Image markerImage = laneMarkerImages[index];
                        if (markerImage == null)
                        {
                            continue;
                        }

                        RectTransform markerRect = markerImage.rectTransform;
                        markerRect.anchorMin = new Vector2(0f, 0.5f);
                        markerRect.anchorMax = new Vector2(0f, 0.5f);
                        markerRect.pivot = new Vector2(0.5f, 0.5f);
                        markerRect.sizeDelta = new Vector2(Mathf.Lerp(20f, 26f, widthBlend), Mathf.Lerp(11f, 13f, widthBlend));
                        markerRect.anchoredPosition = new Vector2((markerSpacing * index) - (laneMarkerContainer.sizeDelta.x * 0.5f), 0f);

                        if (index < laneMarkerTexts.Count && laneMarkerTexts[index] != null)
                        {
                            RectTransform labelRect = laneMarkerTexts[index].rectTransform;
                            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
                            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
                            labelRect.pivot = new Vector2(0.5f, 0.5f);
                            labelRect.sizeDelta = markerRect.sizeDelta;
                            labelRect.anchoredPosition = Vector2.zero;
                            laneMarkerTexts[index].fontSize = Mathf.Lerp(8.8f, 9.8f, widthBlend);
                        }
                    }
                }
            }

            if (skillButtonRoot != null)
            {
                float skillWidth = Mathf.Lerp(48f, 54f, widthBlend);
                float skillHeight = Mathf.Lerp(29f, 34f, widthBlend);
                float actionBaseY = handTop + Mathf.Lerp(16f, 20f, widthBlend);
                skillButtonRoot.anchorMin = new Vector2(1f, 0f);
                skillButtonRoot.anchorMax = new Vector2(1f, 0f);
                skillButtonRoot.pivot = new Vector2(0.5f, 0.5f);
                skillButtonRoot.sizeDelta = new Vector2(skillWidth, skillHeight);
                skillButtonRoot.anchoredPosition = new Vector2(-(skillWidth * 0.5f) - (14f * scale), actionBaseY);
                if (skillButtonText != null)
                {
                    skillButtonText.fontSize = Mathf.Lerp(9.5f, 10.5f, widthBlend);
                }
            }

            if (mapButtonRoot != null)
            {
                float mapWidth = Mathf.Lerp(36f, 44f, widthBlend);
                float mapHeight = Mathf.Lerp(14f, 18f, widthBlend);
                float mapBaseY = handTop + Mathf.Lerp(42f, 48f, widthBlend);
                mapButtonRoot.anchorMin = new Vector2(1f, 0f);
                mapButtonRoot.anchorMax = new Vector2(1f, 0f);
                mapButtonRoot.pivot = new Vector2(0.5f, 0.5f);
                mapButtonRoot.sizeDelta = new Vector2(mapWidth, mapHeight);
                mapButtonRoot.anchoredPosition = new Vector2(-(mapWidth * 0.5f) - (14f * scale), mapBaseY);
                if (mapButtonText != null)
                {
                    mapButtonText.fontSize = Mathf.Lerp(8.1f, 9.2f, widthBlend);
                }
            }

            if (moveModeButtonRoot != null)
            {
                float modeWidth = Mathf.Lerp(74f, 90f, widthBlend);
                float modeHeight = Mathf.Lerp(30f, 38f, widthBlend);
                float modeCenterY = handTop + Mathf.Lerp(158f, 182f, widthBlend);
                moveModeButtonRoot.anchorMin = new Vector2(0f, 0f);
                moveModeButtonRoot.anchorMax = new Vector2(0f, 0f);
                moveModeButtonRoot.pivot = new Vector2(0.5f, 0.5f);
                moveModeButtonRoot.sizeDelta = new Vector2(modeWidth, modeHeight);
                moveModeButtonRoot.anchoredPosition = new Vector2((modeWidth * 0.5f) + (12f * scale), modeCenterY);
            }

            if (actionTrayRoot != null)
            {
                float trayWidth = Mathf.Lerp(112f, 128f, widthBlend);
                float trayHeight = Mathf.Lerp(172f, 196f, widthBlend);
                actionTrayRoot.anchorMin = new Vector2(1f, 0f);
                actionTrayRoot.anchorMax = new Vector2(1f, 0f);
                actionTrayRoot.pivot = new Vector2(0.5f, 0.5f);
                actionTrayRoot.sizeDelta = new Vector2(trayWidth, trayHeight);
                actionTrayRoot.anchoredPosition = new Vector2(-(trayWidth * 0.5f) - (12f * scale), actionTrayCenterY);

                if (actionTrayRoot.TryGetComponent(out Image actionTrayImage))
                {
                    actionTrayImage.color = new Color(0.06f, 0.1f, 0.16f, compact ? 0.26f : 0.3f);
                }
            }

            if (posturePushButtonRoot != null)
            {
                ConfigurePostureButton(posturePushButtonRoot, posturePushButtonText, 0.5f, 0.82f, widthBlend);
            }

            if (postureHoldButtonRoot != null)
            {
                ConfigurePostureButton(postureHoldButtonRoot, postureHoldButtonText, 0.5f, 0.5f, widthBlend);
            }

            if (postureFallbackButtonRoot != null)
            {
                ConfigurePostureButton(postureFallbackButtonRoot, postureFallbackButtonText, 0.5f, 0.18f, widthBlend);
            }

            if (overviewPadRoot != null)
            {
                overviewPadRoot.anchorMin = new Vector2(0f, 0f);
                overviewPadRoot.anchorMax = new Vector2(1f, 1f);
                overviewPadRoot.offsetMin = new Vector2(8f, tapPadBottomOffset);
                overviewPadRoot.offsetMax = new Vector2(-8f, -8f);

                if (overviewPadHintText != null)
                {
                    RectTransform hintRect = overviewPadHintText.rectTransform;
                    hintRect.anchorMin = new Vector2(0.5f, 1f);
                    hintRect.anchorMax = new Vector2(0.5f, 1f);
                    hintRect.pivot = new Vector2(0.5f, 1f);
                    hintRect.sizeDelta = new Vector2(220f, 22f);
                    hintRect.anchoredPosition = new Vector2(0f, -8f);
                    overviewPadHintText.fontSize = Mathf.Lerp(12f, 15f, widthBlend);
                }
            }

            float utilityColumnX = -(compact ? 74f : 82f) - (18f * scale);
            if (zoomInButtonRoot != null)
            {
                zoomInButtonRoot.anchorMin = new Vector2(1f, 0.5f);
                zoomInButtonRoot.anchorMax = new Vector2(1f, 0.5f);
                zoomInButtonRoot.pivot = new Vector2(0.5f, 0.5f);
                zoomInButtonRoot.sizeDelta = new Vector2(compact ? 52f : 58f, compact ? 42f : 46f);
                zoomInButtonRoot.anchoredPosition = new Vector2(utilityColumnX, compact ? 28f : 36f);
            }

            if (zoomOutButtonRoot != null)
            {
                zoomOutButtonRoot.anchorMin = new Vector2(1f, 0.5f);
                zoomOutButtonRoot.anchorMax = new Vector2(1f, 0.5f);
                zoomOutButtonRoot.pivot = new Vector2(0.5f, 0.5f);
                zoomOutButtonRoot.sizeDelta = new Vector2(compact ? 52f : 58f, compact ? 42f : 46f);
                zoomOutButtonRoot.anchoredPosition = new Vector2(utilityColumnX, compact ? -22f : -18f);
            }

            if (centerButtonRoot != null)
            {
                centerButtonRoot.anchorMin = new Vector2(1f, 0.5f);
                centerButtonRoot.anchorMax = new Vector2(1f, 0.5f);
                centerButtonRoot.pivot = new Vector2(0.5f, 0.5f);
                centerButtonRoot.sizeDelta = new Vector2(compact ? 68f : 78f, compact ? 30f : 34f);
                centerButtonRoot.anchoredPosition = new Vector2(utilityColumnX, compact ? -70f : -66f);
            }

            RefreshStickVisual();
        }

        private void UpdateButtonVisualState()
        {
            bool isOverviewMode = battleCamera != null && battleCamera.IsOverviewMode;
            bool showDirectDodgePad = isTouchLayoutActive && isDirectDodgeModeActive && !isOverviewMode;
            bool showJoystickControls = false;
            bool showTapMovePad = isTouchLayoutActive && !isOverviewMode && !showDirectDodgePad && !isSummonPlacementActive;

            if (stickRoot != null)
            {
                stickRoot.gameObject.SetActive(showJoystickControls);
            }

            if (moveTrayRoot != null)
            {
                moveTrayRoot.gameObject.SetActive(showJoystickControls);
            }

            if (tapMovePadRoot != null)
            {
                tapMovePadRoot.gameObject.SetActive(showTapMovePad);
            }

            if (directDodgePadRoot != null)
            {
                directDodgePadRoot.gameObject.SetActive(showDirectDodgePad);
                if (showDirectDodgePad && directDodgePadRoot.TryGetComponent(out Image directDodgeImage))
                {
                    directDodgeImage.color = HasAnyDirectProjectileDanger(out bool hasActiveProjectileDanger, out _) && hasActiveProjectileDanger
                        ? directDodgeActiveTint
                        : directDodgeLockTint;
                }
            }

            if (moveModeButtonRoot != null)
            {
                moveModeButtonRoot.gameObject.SetActive(false);
            }

            if (laneRailRoot != null)
            {
                laneRailRoot.gameObject.SetActive(isTouchLayoutActive && !isOverviewMode && !showDirectDodgePad);
            }

            UpdateSummonPlacementPreviewVisuals(isOverviewMode, showDirectDodgePad);

            if (moveModeButtonImage != null)
            {
                moveModeButtonImage.color = currentInputMode == MobileMoveInputMode.TapAssist
                    ? tapAssistButtonTint
                    : utilityButtonTint;
            }

            if (moveModeButtonText != null)
            {
                moveModeButtonText.text = currentInputMode == MobileMoveInputMode.TapAssist ? "TAP" : "STICK";
            }

            if (tapMovePadHintText != null)
            {
                tapMovePadHintText.text = playerController != null && playerController.HasManualTargetLock
                    ? "목표 고정"
                    : "적 또는 목표를 눌러 고정";
                tapMovePadHintText.gameObject.SetActive(false);
            }

            if (directDodgeTitleText != null)
            {
                directDodgeTitleText.text = HasAnyDirectProjectileDanger(out bool hasActiveProjectileDanger, out _) && hasActiveProjectileDanger
                    ? "직격 위험"
                    : "회피 준비";
            }

            if (actionTrayRoot != null)
            {
                actionTrayRoot.gameObject.SetActive(false);
            }

            if (skillButtonRoot != null)
            {
                skillButtonRoot.gameObject.SetActive(isTouchLayoutActive && !showDirectDodgePad);
            }

            if (mapButtonRoot != null)
            {
                mapButtonRoot.gameObject.SetActive(isTouchLayoutActive && !showDirectDodgePad);
            }

            if (skillButtonImage != null)
            {
                if (playerSkillController == null)
                {
                    skillButtonImage.color = skillBlockedTint;
                    if (skillButtonText != null)
                    {
                        skillButtonText.text = "버스트";
                    }
                }
                else
                {
                    float currentEnergy = battleEnergySystem != null ? battleEnergySystem.CurrentEnergy : 0f;
                    float cost = playerSkillController.ActiveSkillEnergyCost;
                    if (playerSkillController.CooldownRemaining > 0.01f)
                    {
                        skillButtonImage.color = skillCooldownTint;
                        if (skillButtonText != null)
                        {
                            skillButtonText.text = $"버스트\n{playerSkillController.CooldownRemaining:0.0}s";
                        }
                    }
                    else if (currentEnergy < cost)
                    {
                        skillButtonImage.color = skillBlockedTint;
                        if (skillButtonText != null)
                        {
                            float shortage = Mathf.Max(0f, cost - currentEnergy);
                            skillButtonText.text = $"버스트\n+{Mathf.CeilToInt(shortage)}E";
                        }
                    }
                    else
                    {
                        skillButtonImage.color = skillReadyTint;
                        if (skillButtonText != null)
                        {
                            skillButtonText.text = "버스트\n준비";
                        }
                    }
                }
            }

            if (mapButtonImage != null)
            {
                mapButtonImage.color = Color.Lerp(utilityButtonTint, new Color(0.1f, 0.16f, 0.24f, 0.9f), 0.48f);
            }

            if (mapButtonText != null)
            {
                mapButtonText.text = isOverviewMode ? "복귀" : "전황";
            }

            if (overviewPadRoot != null)
            {
                overviewPadRoot.gameObject.SetActive(isTouchLayoutActive && isOverviewMode);
                if (!isOverviewMode)
                {
                    pendingOverviewDragDelta = Vector2.zero;
                    pendingOverviewZoomStep = 0f;
                    pendingOverviewCenterPress = false;
                }
            }

            bool showOverviewUtilities = battleCamera != null && battleCamera.IsOverviewMode && isTouchLayoutActive;
            if (zoomInButtonRoot != null)
            {
                zoomInButtonRoot.gameObject.SetActive(showOverviewUtilities);
            }

            if (zoomOutButtonRoot != null)
            {
                zoomOutButtonRoot.gameObject.SetActive(showOverviewUtilities);
            }

            if (centerButtonRoot != null)
            {
                centerButtonRoot.gameObject.SetActive(showOverviewUtilities);
            }

            if (zoomInButtonImage != null)
            {
                zoomInButtonImage.color = utilityButtonTint;
            }

            if (zoomOutButtonImage != null)
            {
                zoomOutButtonImage.color = utilityButtonTint;
            }

            if (centerButtonImage != null)
            {
                centerButtonImage.color = utilityButtonTint;
            }

            UpdateLaneRailVisuals();
            UpdateTargetLockMarker();
        }

        private void UpdateSummonPlacementPreviewVisuals(bool isOverviewMode, bool showDirectDodgePad)
        {
            if (summonPlacementPreviewRoot == null)
            {
                return;
            }

            bool shouldShow =
                isTouchLayoutActive &&
                isSummonPlacementActive &&
                !isOverviewMode &&
                !showDirectDodgePad;

            UpdateSummonPlacementWorldPreview(shouldShow);
            summonPlacementPreviewRoot.gameObject.SetActive(false);
            if (!shouldShow || safeAreaRoot == null)
            {
                return;
            }

            int laneIndex = BattleLaneUtility.ClampLaneIndex(previewLaneIndex);
            if (!TryResolveLanePreviewLocalPoint(laneIndex, out Vector2 laneLocalPoint, out float laneScreenWidth))
            {
                summonPlacementPreviewRoot.gameObject.SetActive(false);
                return;
            }

            BattleManager battleManager = BattleManager.Instance;
            BattleManager.SummonLanePreview preview = default;
            bool hasPreview = battleManager != null && battleManager.TryGetSummonLanePreview(laneIndex, out preview);
            float railTop = laneRailRoot != null
                ? laneRailRoot.offsetMax.y + 6f
                : safeAreaRoot.rect.yMin + (safeAreaRoot.rect.height * 0.16f);
            Vector2 guidePoint = laneLocalPoint;
            Vector2 landingPoint = laneLocalPoint;
            Vector2 pocketPoint = laneLocalPoint;
            Vector2 blockerPoint = laneLocalPoint;
            Vector2 rewardPoint = laneLocalPoint;
            if (hasPreviewDropWorldPosition)
            {
                if (TryResolveWorldPreviewLocalPoint(previewDropWorldPosition + (Vector3.up * 0.12f), out Vector2 resolvedLandingPoint))
                {
                    landingPoint = resolvedLandingPoint;
                    guidePoint = resolvedLandingPoint;
                }
            }

            if (hasPreview)
            {
                TryResolveWorldPreviewLocalPoint(preview.FirstPocketPosition + (Vector3.up * 0.25f), out pocketPoint);
                if (preview.HasBlocker)
                {
                    TryResolveWorldPreviewLocalPoint(preview.BlockerPosition + (Vector3.up * 1.1f), out blockerPoint);
                }

                if (preview.HasRewardObjective)
                {
                    TryResolveWorldPreviewLocalPoint(preview.RewardObjectivePosition + (Vector3.up * 1f), out rewardPoint);
                }
            }

            float markerY = Mathf.Clamp(
                Mathf.Max(guidePoint.y, pocketPoint.y),
                railTop + 56f,
                safeAreaRoot.rect.yMax - 76f);
            float topY = markerY;
            if (hasPreview && preview.HasBlocker)
            {
                topY = Mathf.Max(topY, blockerPoint.y);
            }

            if (hasPreview && preview.HasRewardObjective)
            {
                topY = Mathf.Max(topY, rewardPoint.y);
            }

            float guideHeight = Mathf.Max(40f, topY - railTop - 20f);
            float lineWidth = Mathf.Clamp(laneScreenWidth * 0.18f, 8f, 16f);
            float markerWidth = Mathf.Clamp(laneScreenWidth * 0.72f, 72f, 124f);
            float ribbonWidth = Mathf.Clamp(laneScreenWidth * 0.82f, 64f, 140f);
            float pulse = 0.82f + (Mathf.Sin(Time.unscaledTime * 8.4f) * 0.12f);

            if (summonPlacementLaneRibbon != null)
            {
                summonPlacementLaneRibbon.anchorMin = new Vector2(0.5f, 0f);
                summonPlacementLaneRibbon.anchorMax = new Vector2(0.5f, 0f);
                summonPlacementLaneRibbon.pivot = new Vector2(0.5f, 0f);
                summonPlacementLaneRibbon.anchoredPosition = new Vector2(guidePoint.x, railTop);
                summonPlacementLaneRibbon.sizeDelta = new Vector2(ribbonWidth, guideHeight + 20f);
            }

            if (summonPlacementLaneRibbonImage != null)
            {
                summonPlacementLaneRibbonImage.color = new Color(0.18f, 0.78f, 1f, Mathf.Clamp01(pulse * 0.14f));
            }

            if (summonPlacementGuideLine != null)
            {
                summonPlacementGuideLine.anchorMin = new Vector2(0.5f, 0f);
                summonPlacementGuideLine.anchorMax = new Vector2(0.5f, 0f);
                summonPlacementGuideLine.pivot = new Vector2(0.5f, 0f);
                summonPlacementGuideLine.anchoredPosition = new Vector2(guidePoint.x, railTop);
                summonPlacementGuideLine.sizeDelta = new Vector2(lineWidth, guideHeight);
            }

            if (summonPlacementGuideLineImage != null)
            {
                summonPlacementGuideLineImage.color = new Color(0.28f, 0.9f, 1f, Mathf.Clamp01(pulse * 0.34f));
            }

            if (summonPlacementGuideMarker != null)
            {
                summonPlacementGuideMarker.anchorMin = new Vector2(0.5f, 0f);
                summonPlacementGuideMarker.anchorMax = new Vector2(0.5f, 0f);
                summonPlacementGuideMarker.pivot = new Vector2(0.5f, 0.5f);
                summonPlacementGuideMarker.anchoredPosition = new Vector2(guidePoint.x, markerY);
                summonPlacementGuideMarker.sizeDelta = new Vector2(markerWidth, 34f);
            }

            if (summonPlacementGuideMarkerImage != null)
            {
                summonPlacementGuideMarkerImage.color = new Color(0.08f, 0.18f, 0.3f, Mathf.Clamp01(0.88f + (pulse * 0.08f)));
            }

            string stateLabel = hasPreview
                ? preview.PreviewState switch
                {
                    BattleManager.SummonLanePreviewState.Break => "돌파",
                    BattleManager.SummonLanePreviewState.Reward => "보상",
                    _ => "배치"
                }
                : "배치";
            if (summonPlacementPreviewText != null)
            {
                summonPlacementPreviewText.text = $"소환: {laneIndex + 1}번 {stateLabel}";
                summonPlacementPreviewText.fontSize = markerWidth >= 104f ? 14f : 12.5f;
            }

            PositionSummonPreviewMarker(
                summonPlacementLandingMarker,
                summonPlacementLandingMarkerImage,
                summonPlacementLandingText,
                hasPreviewDropWorldPosition,
                landingPoint,
                laneScreenWidth,
                new Color(0.98f, 0.58f, 0.96f, 0.95f),
                "소환");

            PositionSummonPreviewMarker(
                summonPlacementPocketMarker,
                summonPlacementPocketMarkerImage,
                summonPlacementPocketText,
                true,
                pocketPoint,
                laneScreenWidth,
                new Color(0.45f, 0.95f, 1f, 0.95f),
                "합류");

            PositionSummonPreviewMarker(
                summonPlacementBlockerMarker,
                summonPlacementBlockerMarkerImage,
                summonPlacementBlockerText,
                hasPreview && preview.HasBlocker,
                blockerPoint,
                laneScreenWidth,
                new Color(1f, 0.78f, 0.34f, 0.95f),
                "차단");

            PositionSummonPreviewMarker(
                summonPlacementRewardMarker,
                summonPlacementRewardMarkerImage,
                summonPlacementRewardText,
                hasPreview && preview.HasRewardObjective,
                rewardPoint,
                laneScreenWidth,
                new Color(0.42f, 1f, 0.68f, 0.95f),
                "보상");
        }

        private void UpdateSummonPlacementWorldPreview(bool shouldShow)
        {
            EnsureSummonPlacementWorldPreview();
            if (summonPlacementWorldPreviewRoot == null)
            {
                return;
            }

            summonPlacementWorldPreviewRoot.gameObject.SetActive(shouldShow);
            if (!shouldShow)
            {
                return;
            }

            ResolveReferences();
            BattleManager battleManager = BattleManager.Instance;
            if (battleManager == null)
            {
                summonPlacementWorldPreviewRoot.gameObject.SetActive(false);
                return;
            }

            int laneIndex = BattleLaneUtility.ClampLaneIndex(previewLaneIndex);
            BattleManager.SummonLanePreview preview = default;
            bool hasPreview = battleManager.TryGetSummonLanePreview(laneIndex, out preview);
            float worldY = playerController != null ? playerController.transform.position.y : 0f;
            Vector3 landingPosition = hasPreviewDropWorldPosition
                ? previewDropWorldPosition
                : hasPreview
                    ? preview.LandingPosition
                    : new Vector3(
                        battleManager.GetLaneCenterX(laneIndex),
                        worldY,
                        battleManager.SummonSpawnPoint != null ? battleManager.SummonSpawnPoint.position.z : 0f);
            Vector3 pocketPosition = hasPreview
                ? preview.FirstPocketPosition
                : landingPosition + new Vector3(0f, 0f, 1.15f);

            SetSummonPlacementWorldMarker(
                summonPlacementWorldLandingMarker,
                true,
                landingPosition + new Vector3(0f, 0.04f, 0f),
                new Vector3(0.95f, 0.02f, 0.95f));
            SetSummonPlacementWorldMarker(
                summonPlacementWorldPocketMarker,
                true,
                pocketPosition + new Vector3(0f, 0.24f, 0f),
                new Vector3(0.24f, 0.44f, 0.24f));
            SetSummonPlacementWorldMarker(
                summonPlacementWorldBlockerMarker,
                hasPreview && preview.HasBlocker,
                preview.BlockerPosition + new Vector3(0f, 0.92f, 0f),
                new Vector3(0.34f, 0.8f, 0.34f));
            SetSummonPlacementWorldMarker(
                summonPlacementWorldRewardMarker,
                hasPreview && preview.HasRewardObjective,
                preview.RewardObjectivePosition + new Vector3(0f, 0.88f, 0f),
                new Vector3(0.34f, 0.74f, 0.34f));

            if (summonPlacementWorldGuideLine != null)
            {
                summonPlacementWorldGuideLine.gameObject.SetActive(true);
                summonPlacementWorldGuideLine.SetPosition(0, landingPosition + new Vector3(0f, 0.05f, 0f));
                summonPlacementWorldGuideLine.SetPosition(1, pocketPosition + new Vector3(0f, 0.2f, 0f));
            }
        }

        private void EnsureSummonPlacementWorldPreview()
        {
            if (summonPlacementWorldPreviewRoot != null)
            {
                return;
            }

            GameObject rootObject = new("SummonPlacementWorldPreview");
            summonPlacementWorldPreviewRoot = rootObject.transform;
            summonPlacementWorldPreviewRoot.SetParent(transform.parent, false);

            summonPlacementWorldLandingMarker = CreateSummonPlacementWorldMarker(
                "Landing",
                PrimitiveType.Cylinder,
                new Color(0.98f, 0.58f, 0.96f, 0.9f));
            summonPlacementWorldPocketMarker = CreateSummonPlacementWorldMarker(
                "Pocket",
                PrimitiveType.Cube,
                new Color(0.45f, 0.95f, 1f, 0.92f));
            summonPlacementWorldBlockerMarker = CreateSummonPlacementWorldMarker(
                "Blocker",
                PrimitiveType.Cube,
                new Color(1f, 0.78f, 0.34f, 0.92f));
            summonPlacementWorldRewardMarker = CreateSummonPlacementWorldMarker(
                "Reward",
                PrimitiveType.Cube,
                new Color(0.42f, 1f, 0.68f, 0.92f));

            GameObject lineObject = new("GuideLine");
            lineObject.transform.SetParent(summonPlacementWorldPreviewRoot, false);
            summonPlacementWorldGuideLine = lineObject.AddComponent<LineRenderer>();
            summonPlacementWorldGuideLine.useWorldSpace = true;
            summonPlacementWorldGuideLine.loop = false;
            summonPlacementWorldGuideLine.positionCount = 2;
            summonPlacementWorldGuideLine.numCapVertices = 3;
            summonPlacementWorldGuideLine.startWidth = 0.08f;
            summonPlacementWorldGuideLine.endWidth = 0.08f;
            summonPlacementWorldGuideLine.alignment = LineAlignment.View;
            Material lineMaterial = CreateSummonPlacementWorldMaterial(new Color(0.45f, 0.95f, 1f, 0.8f));
            summonPlacementWorldGuideLine.material = lineMaterial;
            summonPlacementWorldGuideLine.startColor = lineMaterial.color;
            summonPlacementWorldGuideLine.endColor = lineMaterial.color;
        }

        private Transform CreateSummonPlacementWorldMarker(string name, PrimitiveType primitiveType, Color color)
        {
            GameObject markerObject = GameObject.CreatePrimitive(primitiveType);
            markerObject.name = $"SummonWorld{name}";
            markerObject.transform.SetParent(summonPlacementWorldPreviewRoot, false);
            Collider markerCollider = markerObject.GetComponent<Collider>();
            if (markerCollider != null)
            {
                Destroy(markerCollider);
            }

            Renderer markerRenderer = markerObject.GetComponent<Renderer>();
            if (markerRenderer != null)
            {
                markerRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                markerRenderer.receiveShadows = false;
                markerRenderer.material = CreateSummonPlacementWorldMaterial(color);
            }

            return markerObject.transform;
        }

        private static Material CreateSummonPlacementWorldMaterial(Color color)
        {
            Shader shader = Shader.Find("Sprites/Default");
            shader ??= Shader.Find("Universal Render Pipeline/Unlit");
            shader ??= Shader.Find("Standard");
            Material material = new(shader);
            material.color = color;
            return material;
        }

        private static void SetSummonPlacementWorldMarker(Transform marker, bool isActive, Vector3 position, Vector3 scale)
        {
            if (marker == null)
            {
                return;
            }

            marker.gameObject.SetActive(isActive);
            if (!isActive)
            {
                return;
            }

            marker.position = position;
            marker.localScale = scale;
        }

        private bool TryResolveLanePreviewLocalPoint(int laneIndex, out Vector2 localPoint, out float laneScreenWidth)
        {
            localPoint = Vector2.zero;
            laneScreenWidth = 104f;
            ResolveReferences();
            if (safeAreaRoot == null)
            {
                return false;
            }

            BattleManager battleManager = BattleManager.Instance;
            if (battleManager != null && battleViewCamera != null)
            {
                int clampedLane = BattleLaneUtility.ClampLaneIndex(laneIndex);
                if (!TryGetLaneScreenSamplePoint(clampedLane, out Vector3 laneWorld))
                {
                    return false;
                }

                Vector3 laneScreen = battleViewCamera.WorldToScreenPoint(laneWorld);
                if (laneScreen.z > 0f &&
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(safeAreaRoot, laneScreen, null, out localPoint))
                {
                    float referenceDistance = 0f;
                    if (clampedLane > 0)
                    {
                        if (TryGetLaneScreenSamplePoint(clampedLane - 1, out Vector3 leftLaneWorld))
                        {
                            referenceDistance = Mathf.Max(referenceDistance, Mathf.Abs(laneScreen.x - battleViewCamera.WorldToScreenPoint(leftLaneWorld).x));
                        }
                    }

                    if (clampedLane < battleManager.LaneCount - 1)
                    {
                        if (TryGetLaneScreenSamplePoint(clampedLane + 1, out Vector3 rightLaneWorld))
                        {
                            referenceDistance = Mathf.Max(referenceDistance, Mathf.Abs(laneScreen.x - battleViewCamera.WorldToScreenPoint(rightLaneWorld).x));
                        }
                    }

                    laneScreenWidth = referenceDistance > 1f ? referenceDistance : 104f;
                    return true;
                }
            }

            Rect rect = safeAreaRoot.rect;
            float normalized = BattleLaneUtility.DefaultLaneCount <= 1
                ? 0.5f
                : laneIndex / (float)(BattleLaneUtility.DefaultLaneCount - 1);
            localPoint = new Vector2(
                Mathf.Lerp(rect.xMin + 28f, rect.xMax - 28f, normalized),
                Mathf.Lerp(rect.yMin + (rect.height * 0.36f), rect.yMin + (rect.height * 0.52f), 0.5f));
            laneScreenWidth = rect.width / Mathf.Max(1f, BattleLaneUtility.DefaultLaneCount + 0.6f);
            return true;
        }

        private bool TryResolveWorldPreviewLocalPoint(Vector3 worldPosition, out Vector2 localPoint)
        {
            localPoint = Vector2.zero;
            ResolveReferences();
            if (safeAreaRoot == null || battleViewCamera == null)
            {
                return false;
            }

            Vector3 screenPoint = battleViewCamera.WorldToScreenPoint(worldPosition);
            if (screenPoint.z <= 0f)
            {
                return false;
            }

            return RectTransformUtility.ScreenPointToLocalPointInRectangle(safeAreaRoot, screenPoint, null, out localPoint);
        }

        private static void PositionSummonPreviewMarker(
            RectTransform markerRoot,
            Image markerImage,
            TMP_Text markerText,
            bool shouldShow,
            Vector2 localPoint,
            float laneScreenWidth,
            Color color,
            string label)
        {
            if (markerRoot == null)
            {
                return;
            }

            markerRoot.gameObject.SetActive(shouldShow);
            if (!shouldShow)
            {
                return;
            }

            markerRoot.anchorMin = new Vector2(0.5f, 0f);
            markerRoot.anchorMax = new Vector2(0.5f, 0f);
            markerRoot.pivot = new Vector2(0.5f, 0.5f);
            markerRoot.anchoredPosition = localPoint + new Vector2(0f, 10f);
            markerRoot.sizeDelta = new Vector2(Mathf.Clamp(laneScreenWidth * 0.62f, 72f, 112f), 24f);
            if (markerImage != null)
            {
                markerImage.color = color;
                markerImage.raycastTarget = false;
            }

            if (markerText != null)
            {
                markerText.text = label;
                markerText.raycastTarget = false;
            }
        }

        private void UpdateTargetLockMarker()
        {
            if (targetLockMarkerRoot == null)
            {
                return;
            }

            Transform lockedTargetTransform = null;
            ManualTargetLockKind lockedTargetKind = ManualTargetLockKind.None;
            bool shouldShowMarker =
                isTouchLayoutActive &&
                !isDirectDodgeModeActive &&
                (battleCamera == null || !battleCamera.IsOverviewMode) &&
                playerController != null &&
                playerController.TryGetManualTargetLock(out lockedTargetTransform, out _, out lockedTargetKind) &&
                lockedTargetTransform != null &&
                battleViewCamera != null &&
                safeAreaRoot != null;
            targetLockMarkerRoot.gameObject.SetActive(shouldShowMarker);
            if (!shouldShowMarker)
            {
                return;
            }

            Vector3 worldMarkerPosition = lockedTargetTransform.position + new Vector3(
                0f,
                lockedTargetKind == ManualTargetLockKind.Boss ? 1.55f : 1.15f,
                0f);
            Vector3 screenPoint = battleViewCamera.WorldToScreenPoint(worldMarkerPosition);
            if (screenPoint.z <= 0f ||
                !RectTransformUtility.ScreenPointToLocalPointInRectangle(safeAreaRoot, screenPoint, null, out Vector2 localPoint))
            {
                targetLockMarkerRoot.gameObject.SetActive(false);
                return;
            }

            targetLockMarkerRoot.anchoredPosition = localPoint + new Vector2(0f, 24f);
            targetLockMarkerRoot.SetAsLastSibling();

            if (targetLockMarkerText != null)
            {
                targetLockMarkerText.text = lockedTargetKind switch
                {
                    ManualTargetLockKind.Boss => "보스",
                    ManualTargetLockKind.Structure => "목표",
                    _ => "집중"
                };
            }

            if (targetLockMarkerImage != null)
            {
                targetLockMarkerImage.color = lockedTargetKind switch
                {
                    ManualTargetLockKind.Boss => new Color(0.38f, 0.16f, 0.16f, 0.92f),
                    ManualTargetLockKind.Structure => new Color(0.24f, 0.2f, 0.08f, 0.92f),
                    _ => new Color(0.08f, 0.2f, 0.34f, 0.92f)
                };
            }
        }

        private void UpdateLaneRailVisuals()
        {
            if (laneRailRoot == null)
            {
                return;
            }

            int activeLane = playerController != null
                ? playerController.CurrentLaneIndex
                : BattleLaneUtility.DefaultLaneCount / 2;
            int escortLane = playerController != null
                ? playerController.EscortLaneIndex
                : activeLane;
            int summonLane = summonSpawner != null
                ? summonSpawner.SelectedLaneIndex
                : BattleLaneUtility.DefaultLaneCount / 2;
            int previewLane = isSummonPlacementActive ? previewLaneIndex : summonLane;
            int lockedLaneIndex = -1;
            ManualTargetLockKind lockedTargetKind = ManualTargetLockKind.None;
            bool hasLockedTarget = playerController != null &&
                playerController.TryGetManualTargetLock(out _, out lockedLaneIndex, out lockedTargetKind);

            activeLane = BattleLaneUtility.ClampLaneIndex(activeLane);
            escortLane = BattleLaneUtility.ClampLaneIndex(escortLane);
            previewLane = BattleLaneUtility.ClampLaneIndex(previewLane);
            lockedLaneIndex = hasLockedTarget ? BattleLaneUtility.ClampLaneIndex(lockedLaneIndex) : -1;

            int recommendedLane = escortLane;
            string ribbonText;
            if (isSummonPlacementActive)
            {
                recommendedLane = previewLane;
                string previewStateLabel = "배치";
                BattleManager currentBattleManager = BattleManager.Instance;
                if (currentBattleManager != null &&
                    currentBattleManager.TryGetSummonLanePreview(previewLane, out BattleManager.SummonLanePreview preview))
                {
                    previewStateLabel = preview.PreviewState switch
                    {
                        BattleManager.SummonLanePreviewState.Break => "돌파",
                        BattleManager.SummonLanePreviewState.Reward => "보상",
                        _ => "배치"
                    };
                }

                ribbonText = $"배치: {previewLane + 1}번 {previewStateLabel}";
            }
            else if (playerController == null)
            {
                recommendedLane = ResolveDefaultRecommendLane(summonLane);
                ribbonText = $"추천: {recommendedLane + 1}번 소환";
            }
            else if (playerController.CurrentRetreatReason != PlayerRetreatReason.None)
            {
                recommendedLane = escortLane;
                ribbonText = $"후퇴: {escortLane + 1}번 이탈";
            }
            else if (hasLockedTarget)
            {
                recommendedLane = lockedLaneIndex;
                ribbonText = $"집중: {lockedLaneIndex + 1}번 {ResolveLockedTargetLabel(lockedTargetKind)}";
            }
            else if (playerController.CurrentEscortPhase == BattleManager.EscortPhase.Ready)
            {
                recommendedLane = ResolveDefaultRecommendLane(summonLane);
                ribbonText = $"추천: {recommendedLane + 1}번 소환";
            }
            else if (BattleManager.Instance != null &&
                BattleManager.Instance.TryGetLaneCombatState(escortLane, out BattleManager.LaneCombatState escortLaneState) &&
                escortLaneState.EscortPhase == BattleManager.EscortPhase.BlockerHold)
            {
                recommendedLane = escortLane;
                ribbonText = $"추천: {escortLane + 1}번 차단 파괴";
            }
            else if (TryFindRewardDirectiveLane(out int rewardLaneIndex))
            {
                recommendedLane = rewardLaneIndex;
                ribbonText = $"추천: {rewardLaneIndex + 1}번 보상";
            }
            else
            {
                recommendedLane = escortLane;
                ribbonText = playerController.CurrentEscortPhase == BattleManager.EscortPhase.Join
                    ? $"호위: {escortLane + 1}번 합류"
                    : $"호위: {escortLane + 1}번 유지";
            }

            if (laneStatusText != null)
            {
                laneStatusText.text = ribbonText;
                laneStatusText.color = new Color(0.93f, 0.97f, 1f, 0.98f);
            }

            if (laneRailImage != null)
            {
                laneRailImage.color = isSummonPlacementActive
                    ? new Color(0.08f, 0.15f, 0.22f, 0.055f)
                    : new Color(0.05f, 0.09f, 0.16f, 0.022f);
            }

            for (int index = 0; index < laneMarkerImages.Count; index++)
            {
                Image markerImage = laneMarkerImages[index];
                if (markerImage == null)
                {
                    continue;
                }

                bool isActiveLane = index == activeLane;
                bool isEscortLane = index == escortLane;
                bool isLockedLane = hasLockedTarget && index == lockedLaneIndex;
                bool isRecommendedLane = index == recommendedLane;
                Color markerColor = new Color(0.26f, 0.36f, 0.5f, 0.16f);
                if (isLockedLane)
                {
                    markerColor = new Color(1f, 0.58f, 0.22f, 0.88f);
                }
                else if (isRecommendedLane)
                {
                    markerColor = new Color(0.42f, 0.97f, 1f, 0.78f);
                }
                else if (isEscortLane)
                {
                    markerColor = new Color(0.94f, 0.97f, 1f, 0.64f);
                }
                else if (isActiveLane)
                {
                    markerColor = new Color(0.58f, 0.78f, 0.96f, 0.56f);
                }

                markerImage.color = markerColor;
                markerImage.rectTransform.sizeDelta = isLockedLane || isRecommendedLane
                    ? new Vector2(markerImage.rectTransform.sizeDelta.x, 26f)
                    : isEscortLane
                        ? new Vector2(markerImage.rectTransform.sizeDelta.x, 24f)
                        : new Vector2(markerImage.rectTransform.sizeDelta.x, 21f);

                if (index < laneMarkerTexts.Count && laneMarkerTexts[index] != null)
                {
                    TMP_Text markerText = laneMarkerTexts[index];
                    markerText.text = (index + 1).ToString();
                    markerText.color = isLockedLane || isRecommendedLane
                        ? new Color(0.05f, 0.12f, 0.16f, 1f)
                        : isEscortLane
                            ? new Color(0.18f, 0.28f, 0.36f, 1f)
                            : new Color(0.84f, 0.93f, 1f, 0.96f);
                    markerText.fontStyle = isLockedLane || isRecommendedLane ? FontStyles.Bold : FontStyles.Normal;
                }
            }
        }

        private string ResolveRetreatReason()
        {
            if (playerController == null)
            {
                return "재정비";
            }

            if (!playerController.CurrentLaneHasLiveAllies)
            {
                return "같은 레인 아군이 없습니다";
            }

            BattleManager currentBattleManager = BattleManager.Instance;
            if (currentBattleManager != null &&
                currentBattleManager.TryGetPlayerTerritoryState(out BattleManager.PlayerTerritoryState territoryState))
            {
                if (territoryState.IsInEnemyBaseZone)
                {
                    return "너무 깊게 진입했습니다";
                }

                if (territoryState.IsInCoverBreakGrace)
                {
                    return "전선이 무너졌습니다";
                }

                if (territoryState.OverextendDistance > 0.01f)
                {
                    return $"안전선보다 {territoryState.OverextendDistance:0.0}m 앞서 있습니다";
                }
            }

            if (playerController.CurrentLanePressureState == BattleManager.LanePressureState.Collapse)
            {
                return "적 압박이 우세합니다";
            }

            return "후열로 복귀합니다";
        }

        private static int ResolveDefaultRecommendLane(int referenceLane)
        {
            return BattleLaneUtility.ClampLaneIndex(referenceLane <= 2 ? 1 : 3);
        }

        private bool TryFindRewardDirectiveLane(out int laneIndex)
        {
            laneIndex = -1;
            if (playerController == null)
            {
                return false;
            }

            bool hasDirectiveContext = playerController.CurrentLaneHasLiveAllies ||
                playerController.CurrentEscortPhase == BattleManager.EscortPhase.Join;
            if (!hasDirectiveContext)
            {
                return false;
            }

            int[] priorityLanes =
            {
                BattleLaneUtility.ClampLaneIndex(playerController.EscortLaneIndex),
                1,
                3,
                2
            };

            for (int index = 0; index < priorityLanes.Length; index++)
            {
                int candidateLane = BattleLaneUtility.ClampLaneIndex(priorityLanes[index]);
                if (BattleStructure.FindNearestRoleInLaneAlongAdvance(candidateLane, isPlayerTeam: true, BattleStructureRole.RewardObjective) != null)
                {
                    laneIndex = candidateLane;
                    return true;
                }
            }

            return false;
        }

        private static string ResolveLockedTargetLabel(ManualTargetLockKind lockedTargetKind)
        {
            return lockedTargetKind switch
            {
                ManualTargetLockKind.Boss => "보스",
                ManualTargetLockKind.Structure => "목표",
                _ => "적"
            };
        }

        private void RefreshStickVisual()
        {
            if (stickKnob == null || stickRoot == null)
            {
                return;
            }

            float radius = Mathf.Min(stickRoot.rect.width, stickRoot.rect.height) * 0.24f;
            stickKnob.anchoredPosition = moveVector * radius;
            if (stickVisualImage != null)
            {
                stickVisualImage.color = moveVector.sqrMagnitude > 0.01f ? stickActiveTint : stickReadyTint;
            }

            if (stickKnobImage != null)
            {
                stickKnobImage.color = new Color(1f, 1f, 1f, moveVector.sqrMagnitude > 0.01f ? 1f : 0.92f);
            }
        }

        private static void ConfigurePostureButton(RectTransform buttonRoot, TMP_Text label, float anchorX, float anchorY, float widthBlend)
        {
            if (buttonRoot == null)
            {
                return;
            }

            buttonRoot.anchorMin = new Vector2(anchorX, anchorY);
            buttonRoot.anchorMax = new Vector2(anchorX, anchorY);
            buttonRoot.pivot = new Vector2(0.5f, 0.5f);
            buttonRoot.sizeDelta = new Vector2(Mathf.Lerp(88f, 102f, widthBlend), Mathf.Lerp(40f, 48f, widthBlend));
            buttonRoot.anchoredPosition = Vector2.zero;

            if (label != null)
            {
                RectTransform labelRect = label.rectTransform;
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;
                label.fontSize = Mathf.Lerp(13f, 15.5f, widthBlend);
            }
        }

        private void SetRootActiveState(bool isActive)
        {
            if (safeAreaRoot != null && safeAreaRoot.gameObject.activeSelf != isActive)
            {
                safeAreaRoot.gameObject.SetActive(isActive);
            }
        }

        private void UpdateDragThreshold()
        {
            if (EventSystem.current == null)
            {
                return;
            }

            if (defaultDragThreshold < 0)
            {
                defaultDragThreshold = EventSystem.current.pixelDragThreshold;
            }

            EventSystem.current.pixelDragThreshold = isTouchLayoutActive
                ? Mathf.Max(defaultDragThreshold, 24)
                : defaultDragThreshold;
        }

        private static RectTransform EnsureRect(string name, RectTransform parent)
        {
            Transform existing = parent.Find(name);
            GameObject gameObject = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            return rect;
        }

        private static RectTransform EnsurePanel(string name, RectTransform parent, Color tint)
        {
            Transform existing = parent.Find(name);
            GameObject gameObject = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(Image));
            gameObject.transform.SetParent(parent, false);
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;

            Image image = gameObject.GetComponent<Image>();
            image.sprite = RuntimeUISpriteUtility.GetPanelSprite();
            image.type = Image.Type.Sliced;
            image.color = tint;
            image.raycastTarget = true;
            return rect;
        }

        private static TMP_Text EnsureText(string name, RectTransform parent, TMP_FontAsset font, float fontSize, TextAlignmentOptions alignment)
        {
            Transform existing = parent.Find(name);
            GameObject gameObject = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            gameObject.transform.SetParent(parent, false);
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;

            TextMeshProUGUI text = gameObject.GetComponent<TextMeshProUGUI>();
            text.font = font;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.text = string.Empty;
            text.raycastTarget = false;
            RuntimeUIFontUtility.ApplyToText(text);
            return text;
        }
    }

    public class MobileStickPad : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private MobileBattleControls owner;
        private RectTransform boundsRect;
        private RectTransform knobRect;
        private int activePointerId = int.MinValue;

        public void Initialize(MobileBattleControls controls, RectTransform bounds, RectTransform knob)
        {
            owner = controls;
            boundsRect = bounds;
            knobRect = knob;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            activePointerId = eventData.pointerId;
            UpdateStick(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId)
            {
                return;
            }

            UpdateStick(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId)
            {
                return;
            }

            activePointerId = int.MinValue;
            owner?.ClearMoveVector();
            if (knobRect != null)
            {
                knobRect.anchoredPosition = Vector2.zero;
            }
        }

        private void UpdateStick(PointerEventData eventData)
        {
            if (owner == null || boundsRect == null)
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(boundsRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            {
                owner.ClearMoveVector();
                return;
            }

            float radius = Mathf.Min(boundsRect.rect.width, boundsRect.rect.height) * 0.5f;
            if (radius <= 0.001f)
            {
                owner.ClearMoveVector();
                return;
            }

            Vector2 normalized = Vector2.ClampMagnitude(localPoint / radius, 1f);
            owner.SetMoveVector(normalized);
        }
    }

    public class MobileTapMovePad : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private MobileBattleControls owner;
        private int activePointerId = int.MinValue;
        private Vector2 pointerDownScreenPosition;

        public void Initialize(MobileBattleControls controls)
        {
            owner = controls;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            activePointerId = eventData.pointerId;
            pointerDownScreenPosition = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId)
            {
                return;
            }

            activePointerId = int.MinValue;
            owner?.HandleBattlefieldTapRelease(eventData, pointerDownScreenPosition);
        }
    }

    public class MobileDirectDodgePad : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private MobileBattleControls owner;
        private int activePointerId = int.MinValue;
        private Vector2 pointerDownScreenPosition;

        public void Initialize(MobileBattleControls controls)
        {
            owner = controls;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            activePointerId = eventData.pointerId;
            pointerDownScreenPosition = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId)
            {
                return;
            }

            activePointerId = int.MinValue;
            owner?.HandleDirectDodgeRelease(eventData, pointerDownScreenPosition);
        }
    }

    public class MobileLaneSwipePad : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private MobileBattleControls owner;
        private int activePointerId = int.MinValue;
        private Vector2 pointerDownScreenPosition;

        public void Initialize(MobileBattleControls controls)
        {
            owner = controls;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            activePointerId = eventData.pointerId;
            pointerDownScreenPosition = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId)
            {
                return;
            }

            activePointerId = int.MinValue;
            owner?.HandleLaneSwipeRelease(eventData, pointerDownScreenPosition);
        }
    }

    public class MobileOverviewPad : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private MobileBattleControls owner;
        private int activePointerId = int.MinValue;

        public void Initialize(MobileBattleControls controls)
        {
            owner = controls;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            activePointerId = eventData.pointerId;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId || owner == null)
            {
                return;
            }

            owner.AddOverviewDrag(eventData.delta);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId == activePointerId)
            {
                activePointerId = int.MinValue;
            }
        }
    }
}
