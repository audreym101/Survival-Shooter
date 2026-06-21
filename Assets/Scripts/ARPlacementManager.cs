using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.EnhancedTouch;
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

public class ARPlacementManager : MonoBehaviour
{
    [Header("AR Components")]
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlaneManager planeManager;
    [SerializeField] private ARSession arSession;
    [SerializeField] private ARCameraManager arCameraManager;
    [SerializeField] private ARCameraBackground arCameraBackground;

    [Header("Placement")]
    [SerializeField] private GameObject placementIndicator;
    [SerializeField] private GameObject gameWorldPrefab;

    private GameObject spawnedGameWorld;
    private Pose placementPose;
    private bool placementPoseIsValid;
    private bool arIsReady;

    private static bool hasPlacedWorld;
    private static readonly List<ARRaycastHit> hits = new();
    private const TrackableType PlacementTrackables =
        TrackableType.PlaneWithinPolygon | TrackableType.PlaneEstimated;

    private void OnEnable()
    {
#if ENABLE_INPUT_SYSTEM
        EnhancedTouchSupport.Enable();
#endif
    }

    private void OnDisable()
    {
#if ENABLE_INPUT_SYSTEM
        EnhancedTouchSupport.Disable();
#endif
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetPlacementState()
    {
        hasPlacedWorld = false;
        hits.Clear();
    }

    private void Start()
    {
        if (hasPlacedWorld)
        {
            enabled = false;
            return;
        }

        if (planeManager == null)
            planeManager = FindFirstObjectByType<ARPlaneManager>();

        if (arSession == null)
            arSession = FindFirstObjectByType<ARSession>();

        if (arCameraManager == null)
            arCameraManager = FindFirstObjectByType<ARCameraManager>();

        if (arCameraBackground == null)
            arCameraBackground = FindFirstObjectByType<ARCameraBackground>();

        StartCoroutine(PrepareARSession());

#if UNITY_EDITOR
        if (gameWorldPrefab != null)
        {
            spawnedGameWorld = Instantiate(
                gameWorldPrefab,
                Vector3.zero,
                Quaternion.identity);

            Debug.Log("Editor Mode: GameWorld spawned");

            GameWorldGround.Register(spawnedGameWorld.transform);
            hasPlacedWorld = true;

            GameManager.Instance?.StartGame();
        }

        if (placementIndicator != null)
            placementIndicator.SetActive(false);
#endif
    }

    private void Update()
    {
#if !UNITY_EDITOR
        if (!arIsReady || ARSession.state < ARSessionState.SessionTracking)
            return;

        UpdatePlacementPose();
        UpdatePlacementIndicator();

        if (TryGetTapPosition(out Vector2 tapPosition))
        {
            TryPlaceGameWorld(tapPosition);
        }
#endif
    }

    private bool TryGetTapPosition(out Vector2 screenPosition)
    {
        screenPosition = default;

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.touchCount > 0 &&
            Input.GetTouch(0).phase == TouchPhase.Began)
        {
            screenPosition = Input.GetTouch(0).position;
            return true;
        }
#endif

#if ENABLE_INPUT_SYSTEM
        foreach (UnityEngine.InputSystem.EnhancedTouch.Touch touch in Touch.activeTouches)
        {
            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                screenPosition = touch.screenPosition;
                return true;
            }
        }
#endif

        return false;
    }

    private void UpdatePlacementPose()
    {
        Vector2 screenCenter =
            new Vector2(Screen.width / 2f, Screen.height / 2f);

        placementPoseIsValid = TryGetPlacementPose(screenCenter, out placementPose);
    }

    private bool TryGetPlacementPose(Vector2 screenPosition, out Pose pose)
    {
        pose = default;

        if (raycastManager == null ||
            !raycastManager.Raycast(screenPosition, hits, PlacementTrackables))
        {
            return false;
        }

        pose = hits[0].pose;

        if (Camera.main != null)
        {
            Vector3 cameraForward = Camera.main.transform.forward;
            Vector3 cameraBearing =
                new Vector3(cameraForward.x, 0f, cameraForward.z);

            if (cameraBearing.sqrMagnitude > 0.001f)
                pose.rotation = Quaternion.LookRotation(cameraBearing.normalized);
        }

        return true;
    }

    private void UpdatePlacementIndicator()
    {
        if (placementIndicator == null)
            return;

        placementIndicator.SetActive(placementPoseIsValid);

        if (placementPoseIsValid)
        {
            placementIndicator.transform.SetPositionAndRotation(
                placementPose.position,
                placementPose.rotation);
        }
    }

    private void TryPlaceGameWorld(Vector2 screenPosition)
    {
        if (TryGetPlacementPose(screenPosition, out Pose tapPose))
        {
            PlaceGameWorld(tapPose);
        }
        else if (placementPoseIsValid)
        {
            PlaceGameWorld(placementPose);
        }
        else
        {
            Debug.Log("No AR plane found yet. Move the phone over a flat surface, then tap again.");
        }
    }

    private void PlaceGameWorld(Pose pose)
    {
        if (spawnedGameWorld != null || hasPlacedWorld)
            return;

        if (gameWorldPrefab == null)
        {
            Debug.LogError("Game world prefab is not assigned.");
            return;
        }

        spawnedGameWorld = Instantiate(
            gameWorldPrefab,
            pose.position,
            pose.rotation);

        GameWorldGround.Register(spawnedGameWorld.transform);

        hasPlacedWorld = true;
        placementPoseIsValid = false;

        if (placementIndicator != null)
            placementIndicator.SetActive(false);

        if (planeManager != null)
            planeManager.enabled = false;

        Debug.Log("Game World Placed!");

        GameManager.Instance?.StartGame();
        enabled = false;
    }

    private void EnableHorizontalPlaneTracking()
    {
#if !UNITY_EDITOR
        if (arSession != null)
            arSession.enabled = true;

        if (arCameraManager != null)
            arCameraManager.enabled = true;

        if (arCameraBackground != null)
            arCameraBackground.enabled = true;

        if (planeManager != null)
        {
            planeManager.enabled = true;
            planeManager.requestedDetectionMode =
                PlaneDetectionMode.Horizontal;
        }
#endif
    }

    private IEnumerator PrepareARSession()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);

            float deadline =
                Time.realtimeSinceStartup + 10f;

            while (!Permission.HasUserAuthorizedPermission(Permission.Camera) &&
                   Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Debug.LogError("Camera permission denied.");
                yield break;
            }
        }
#endif

#if !UNITY_EDITOR
        if (ARSession.state == ARSessionState.None ||
            ARSession.state == ARSessionState.CheckingAvailability)
        {
            yield return ARSession.CheckAvailability();
        }

        if (ARSession.state == ARSessionState.NeedsInstall)
        {
            yield return ARSession.Install();
        }

        if (ARSession.state == ARSessionState.Unsupported)
        {
            Debug.LogError("AR is unsupported on this device.");
            yield break;
        }
#endif

        EnableHorizontalPlaneTracking();
        arIsReady = true;

        yield return null;
    }
}
