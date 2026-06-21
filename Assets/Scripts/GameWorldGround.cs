using UnityEngine;

public static class GameWorldGround
{
    public static float GroundY { get; private set; }
    public static Transform WorldRoot { get; private set; }
    public static bool HasWorld => WorldRoot != null;

    public static void Register(Transform worldRoot)
    {
        if (worldRoot == null)
            return;

        WorldRoot = worldRoot;
        GroundY = worldRoot.position.y;
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
}
