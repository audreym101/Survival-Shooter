using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;

public class ARPlacementManager : MonoBehaviour
{
    [SerializeField] GameObject placementIndicatorPrefab;
    [SerializeField] GameObject gameWorld;

    ARRaycastManager _raycastManager;
    ARPlaneManager _planeManager;
    GameObject _indicator;
    bool _gamePlaced;

    static readonly List<ARRaycastHit> Hits = new List<ARRaycastHit>();

    void Awake()
    {
        _raycastManager = GetComponent<ARRaycastManager>();
        _planeManager = GetComponent<ARPlaneManager>();
        if (placementIndicatorPrefab != null)
            _indicator = Instantiate(placementIndicatorPrefab);
        if (gameWorld != null)
            gameWorld.SetActive(false);
    }

    void Update()
    {
        if (_gamePlaced) return;

        UpdateIndicator();

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            TryPlaceGame();

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            TryPlaceGame();
    }

    void UpdateIndicator()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        if (_raycastManager.Raycast(screenCenter, Hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = Hits[0].pose;
            if (_indicator != null)
            {
                _indicator.SetActive(true);
                _indicator.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
            }
        }
        else
        {
            if (_indicator != null) _indicator.SetActive(false);
        }
    }

    void TryPlaceGame()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        if (!_raycastManager.Raycast(screenCenter, Hits, TrackableType.PlaneWithinPolygon)) return;

        Pose hitPose = Hits[0].pose;
        _gamePlaced = true;

        if (_indicator != null) _indicator.SetActive(false);

        if (gameWorld != null)
        {
            gameWorld.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
            gameWorld.SetActive(true);
        }

        // Disable plane visuals after placement
        foreach (var plane in _planeManager.trackables)
            plane.gameObject.SetActive(false);
        _planeManager.enabled = false;

        GameManager.Instance?.StartGame();
    }
}
