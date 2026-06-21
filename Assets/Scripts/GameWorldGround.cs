using UnityEngine;

public static class GameWorldGround
{
    public static float GroundY { get; private set; }
    public static Transform WorldRoot { get; private set; }
    public static Vector3 PlayCenter { get; private set; }
    public static float PlayRadius { get; private set; } = 3f;
    public static bool HasWorld => WorldRoot != null;

    public static void Register(Transform worldRoot)
    {
        if (worldRoot == null)
            return;

        Register(worldRoot, worldRoot.position.y, null);
    }

    public static void Register(Transform worldRoot, float groundY)
    {
        Register(worldRoot, groundY, null);
    }

    public static void Register(Transform worldRoot, float groundY, Bounds? playBounds)
    {
        if (worldRoot == null)
            return;

        WorldRoot = worldRoot;
        GroundY = groundY;
        PlayCenter = ProjectToGround(worldRoot.position);

        if (playBounds.HasValue)
        {
            Bounds bounds = playBounds.Value;
            PlayCenter = ProjectToGround(bounds.center);
            PlayRadius = Mathf.Max(1.5f, Mathf.Min(bounds.extents.x, bounds.extents.z));
        }
    }

    public static Vector3 ProjectToGround(Vector3 position)
    {
        position.y = GroundY;
        return position;
    }

    public static Vector3 GroundedDirection(Vector3 from, Vector3 to)
    {
        from.y = GroundY;
        to.y = GroundY;

        Vector3 direction = to - from;
        return direction.sqrMagnitude > Mathf.Epsilon ? direction.normalized : Vector3.zero;
    }

    public static Vector3 ClampToPlayArea(Vector3 position, float edgePadding = 0.3f)
    {
        Vector3 groundedPosition = ProjectToGround(position);
        Vector3 offset = groundedPosition - PlayCenter;
        offset.y = 0f;

        float maxDistance = Mathf.Max(0.5f, PlayRadius - edgePadding);
        if (offset.sqrMagnitude > maxDistance * maxDistance)
            groundedPosition = PlayCenter + offset.normalized * maxDistance;

        groundedPosition.y = GroundY;
        return groundedPosition;
    }
}
