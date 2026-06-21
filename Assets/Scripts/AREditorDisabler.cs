using UnityEngine;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// Disables AR components in the editor since AR is not available in play mode.
/// This prevents errors about missing XR providers.
/// </summary>
public class AREditorDisabler
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void DisableARInEditor()
    {
#if UNITY_EDITOR
        // Disable all AR components to prevent XR subsystem errors in editor
        ARSession[] sessions = Object.FindObjectsOfType<ARSession>();
        foreach (ARSession session in sessions)
        {
            session.enabled = false;
        }

        ARCameraManager[] cameras = Object.FindObjectsOfType<ARCameraManager>();
        foreach (ARCameraManager cam in cameras)
        {
            cam.enabled = false;
        }

        ARPlaneManager[] planeManagers = Object.FindObjectsOfType<ARPlaneManager>();
        foreach (ARPlaneManager manager in planeManagers)
        {
            manager.enabled = false;
        }

        ARRaycastManager[] raycastManagers = Object.FindObjectsOfType<ARRaycastManager>();
        foreach (ARRaycastManager manager in raycastManagers)
        {
            manager.enabled = false;
        }

        Debug.Log("✓ AR components disabled for editor mode");
#endif
    }
}
