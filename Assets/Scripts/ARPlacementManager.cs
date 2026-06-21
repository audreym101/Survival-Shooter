using System.Collections;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class ARPlacementManager : MonoBehaviour
{
    [Header("AR Components")]
    [SerializeField] ARRaycastManager raycastManager;
    [SerializeField] ARPlaneManager planeManager;
    [SerializeField] ARSession arSession;
    [SerializeField] ARCameraManager arCameraManager;
    [SerializeField] ARCameraBackground arCameraBackground;

    [Header("Placement")]
    [SerializeField] GameObject placementIndicator;
    [SerializeField] GameObject gameWorldPrefab;

    GameObject spawnedGameWorld;
    Pose placementPose;
    bool placementPoseIsValid = false;
    static bool hasPlacedWorld;

    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetPlacementState()
    {
        hasPlacedWorld = false;
        hits.Clear();
    }

    void Start()
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
        // Spawn GameWorld automatically in Editor
        if (gameWorldPrefab != null)
        {
            spawnedGameWorld = Instantiate(
                gameWorldPrefab,
                Vector3.zero,
                Quaternion.identity
            );

            Debug.Log("Editor Mode: GameWorld spawned");
            GameWorldGround.Register(spawnedGameWorld.transform);
            hasPlacedWorld = true;

            GameManager.Instance?.StartGame();
        }

        if (placementIndicator != null)
            placementIndicator.SetActive(false);
#endif
    }

    void Update()
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

    void UpdatePlacementPose()
    {
        Vector2 screenCenter =
            new Vector2(Screen.width / 2, Screen.height / 2);

        raycastManager.Raycast(
            screenCenter,
            hits,
            TrackableType.Planes
        );

        placementPoseIsValid = hits.Count > 0;

        if (placementPoseIsValid)
        {
            placementPose = hits[0].pose;

            Vector3 cameraForward =
                Camera.main.transform.forward;

            Vector3 cameraBearing =
                new Vector3(
                    cameraForward.x,
                    0,
                    cameraForward.z
                ).normalized;

            placementPose.rotation =
                Quaternion.LookRotation(cameraBearing);
        }
    }

    void UpdatePlacementIndicator()
    {
        if (placementIndicator == null)
            return;

        if (placementPoseIsValid)
        {
            placementIndicator.SetActive(true);

            placementIndicator.transform.SetPositionAndRotation(
                placementPose.position,
                placementPose.rotation
            );
        }
        else
        {
            placementIndicator.SetActive(false);
        }
    }

    void PlaceGameWorld()
    {
        if (spawnedGameWorld != null || hasPlacedWorld)
            return;

        spawnedGameWorld = Instantiate(
            gameWorldPrefab,
            placementPose.position,
            placementPose.rotation
        );

        GameWorldGround.Register(spawnedGameWorld.transform);
        hasPlacedWorld = true;

        placementIndicator.SetActive(false);

        Debug.Log("Game World Placed!");

        GameManager.Instance?.StartGame();
    }

    void EnableHorizontalPlaneTracking()
    {
#if UNITY_EDITOR
        return;
#else
        if (arSession != null)
            arSession.enabled = true;

        if (arCameraManager != null)
            arCameraManager.enabled = true;

        if (arCameraBackground != null)
            arCameraBackground.enabled = true;

        if (planeManager == null)
            return;

        planeManager.enabled = true;
        planeManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
#endif
    }

    IEnumerator PrepareARSession()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);

            float permissionWaitDeadline = Time.realtimeSinceStartup + 10f;
            while (!Permission.HasUserAuthorizedPermission(Permission.Camera) &&
                   Time.realtimeSinceStartup < permissionWaitDeadline)
            {
                yield return null;
            }

            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Debug.LogError("Camera permission was not granted, so AR plane tracking cannot start.");
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
            Debug.LogError("AR is unsupported on this phone or ARCore is unavailable.");
            yield break;
        }
#endif

        EnableHorizontalPlaneTracking();
    }
}
