using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class ARPlacementManager : MonoBehaviour
{
    [Header("AR Components")]
    [SerializeField] ARRaycastManager raycastManager;

    [Header("Placement")]
    [SerializeField] GameObject placementIndicator;
    [SerializeField] GameObject gameWorldPrefab;

    GameObject spawnedGameWorld;
    Pose placementPose;
    bool placementPoseIsValid = false;

    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Start()
    {
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

            GameManager.Instance?.StartGame();
        }

        if (placementIndicator != null)
            placementIndicator.SetActive(false);
#endif
    }

    void Update()
    {
#if !UNITY_EDITOR
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
        if (spawnedGameWorld != null)
            return;

        spawnedGameWorld = Instantiate(
            gameWorldPrefab,
            placementPose.position,
            placementPose.rotation
        );

        placementIndicator.SetActive(false);

        Debug.Log("Game World Placed!");

        GameManager.Instance?.StartGame();
    }
}