using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

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

    private static bool hasPlacedWorld;
    private static readonly List<ARRaycastHit> hits = new();

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
        if (ARSession.state < ARSessionState.SessionTracking)
            return;

        UpdatePlacementPose();
        UpdatePlacementIndicator();

        if (placementPoseIsValid &&
            Input.touchCount > 0 &&
            Input.GetTouch(0).phase == TouchPhase.Began)
        {
            PlaceGameWorld();
        }
#endif
    }

    private void UpdatePlacementPose()
    {
        Vector2 screenCenter =
            new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (raycastManager.Raycast(
            screenCenter,
            hits,
            TrackableType.Planes))
        {
            placementPoseIsValid = true;
            placementPose = hits[0].pose;

            Vector3 cameraForward = Camera.main.transform.forward;
            Vector3 cameraBearing =
                new Vector3(cameraForward.x, 0, cameraForward.z).normalized;

            placementPose.rotation =
                Quaternion.LookRotation(cameraBearing);
        }
        else
        {
            placementPoseIsValid = false;
        }
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

    private void PlaceGameWorld()
    {
        if (spawnedGameWorld != null || hasPlacedWorld)
            return;

        spawnedGameWorld = Instantiate(
            gameWorldPrefab,
            placementPose.position,
            placementPose.rotation);

        GameWorldGround.Register(spawnedGameWorld.transform);

        hasPlacedWorld = true;

        if (placementIndicator != null)
            placementIndicator.SetActive(false);

        Debug.Log("Game World Placed!");

        GameManager.Instance?.StartGame();
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

        yield return null;
    }
}