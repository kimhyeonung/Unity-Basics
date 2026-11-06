using UnityEngine;

/// <summary>
/// 현재 카메라 화면의 월드 좌표 경계를 저장하고 제공하는 정적 유틸리티 클래스.
/// 
/// 사용 예:
/// ScreenWorldBounds.Initialize(Camera.main);
/// 
/// if (ScreenWorldBounds.IsOutside(transform.position))
/// {
///     gameObject.SetActive(false);
/// }
/// </summary>
public static class ScreenWorldBounds
{
    private static Camera targetCamera;

    private static bool initialized;

    private static float minX;
    private static float maxX;
    private static float minY;
    private static float maxY;

    private static float width;
    private static float height;

    /// <summary>
    /// 화면 경계 계산에 사용할 Z 평면.
    /// 일반적인 2D 게임에서는 오브젝트들이 z = 0에 있으므로 기본값 0.
    /// </summary>
    private static float worldZ = 0f;

    public static bool Initialized => initialized;

    public static float MinX => minX;
    public static float MaxX => maxX;
    public static float MinY => minY;
    public static float MaxY => maxY;

    public static float Width => width;
    public static float Height => height;

    public static Vector2 Center => new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
    public static Vector2 Min => new Vector2(minX, minY);
    public static Vector2 Max => new Vector2(maxX, maxY);

    /// <summary>
    /// 카메라 기준으로 화면의 월드 좌표 경계를 계산한다.
    /// 보통 GameManager 또는 첫 Scene 초기화 시점에서 한 번 호출한다.
    /// </summary>
    public static void Initialize(Camera camera, float targetWorldZ = 0f)
    {
        if (camera == null)
        {
            Debug.LogError("ScreenWorldBounds Initialize Failed: camera is null.");
            return;
        }

        targetCamera = camera;
        worldZ = targetWorldZ;

        Refresh();

        initialized = true;

        Debug.Log(
            $"ScreenWorldBounds Initialized | " +
            $"Min({minX:F2}, {minY:F2}) Max({maxX:F2}, {maxY:F2}) " +
            $"Size({width:F2}, {height:F2})"
        );
    }

    /// <summary>
    /// 현재 카메라 위치와 화면 크기를 기준으로 경계를 다시 계산한다.
    /// 카메라가 움직이거나 해상도가 바뀌는 게임에서는 필요할 때 호출한다.
    /// </summary>
    public static void Refresh()
    {
        if (targetCamera == null)
        {
            Debug.LogError("ScreenWorldBounds Refresh Failed: targetCamera is null.");
            return;
        }

        float distanceFromCameraToWorldZ = worldZ - targetCamera.transform.position.z;

        Vector3 bottomLeft = targetCamera.ViewportToWorldPoint(
            new Vector3(0f, 0f, distanceFromCameraToWorldZ)
        );

        Vector3 topRight = targetCamera.ViewportToWorldPoint(
            new Vector3(1f, 1f, distanceFromCameraToWorldZ)
        );

        minX = bottomLeft.x;
        minY = bottomLeft.y;
        maxX = topRight.x;
        maxY = topRight.y;

        width = maxX - minX;
        height = maxY - minY;
    }

    /// <summary>
    /// 지정한 위치가 화면 안에 있는지 확인한다.
    /// padding을 주면 화면보다 안쪽/바깥쪽으로 판정 범위를 조절할 수 있다.
    /// padding > 0 : 화면보다 조금 바깥까지 허용
    /// padding < 0 : 화면 안쪽으로 더 엄격하게 판정
    /// </summary>
    public static bool IsInside(Vector3 worldPosition, float padding = 0f)
    {
        EnsureInitialized();

        return worldPosition.x >= minX - padding &&
               worldPosition.x <= maxX + padding &&
               worldPosition.y >= minY - padding &&
               worldPosition.y <= maxY + padding;
    }

    /// <summary>
    /// 지정한 위치가 화면 밖에 있는지 확인한다.
    /// </summary>
    public static bool IsOutside(Vector3 worldPosition, float padding = 0f)
    {
        return !IsInside(worldPosition, padding);
    }

    public static bool IsOutLeft(Vector3 worldPosition, float padding = 0f)
    {
        EnsureInitialized();
        return worldPosition.x < minX - padding;
    }

    public static bool IsOutRight(Vector3 worldPosition, float padding = 0f)
    {
        EnsureInitialized();
        return worldPosition.x > maxX + padding;
    }

    public static bool IsOutBottom(Vector3 worldPosition, float padding = 0f)
    {
        EnsureInitialized();
        return worldPosition.y < minY - padding;
    }

    public static bool IsOutTop(Vector3 worldPosition, float padding = 0f)
    {
        EnsureInitialized();
        return worldPosition.y > maxY + padding;
    }

    /// <summary>
    /// 위치를 화면 안쪽으로 제한한다.
    /// Player가 화면 밖으로 나가지 않게 할 때 사용하기 좋다.
    /// </summary>
    public static Vector3 ClampPosition(Vector3 worldPosition, float padding = 0f)
    {
        EnsureInitialized();

        worldPosition.x = Mathf.Clamp(worldPosition.x, minX + padding, maxX - padding);
        worldPosition.y = Mathf.Clamp(worldPosition.y, minY + padding, maxY - padding);

        return worldPosition;
    }

    /// <summary>
    /// 화면 밖 위치에서 랜덤 Spawn 위치를 만들 때 사용할 수 있는 보조 함수.
    /// 화면 경계보다 padding만큼 바깥쪽의 위치를 반환한다.
    /// </summary>
    public static Vector3 GetRandomPositionOutside(float padding = 1f)
    {
        EnsureInitialized();

        int side = Random.Range(0, 4);

        float x = 0f;
        float y = 0f;

        switch (side)
        {
            case 0: // Left
                x = minX - padding;
                y = Random.Range(minY, maxY);
                break;

            case 1: // Right
                x = maxX + padding;
                y = Random.Range(minY, maxY);
                break;

            case 2: // Bottom
                x = Random.Range(minX, maxX);
                y = minY - padding;
                break;

            case 3: // Top
                x = Random.Range(minX, maxX);
                y = maxY + padding;
                break;
        }

        return new Vector3(x, y, worldZ);
    }

    /// <summary>
    /// 현재 계산된 경계를 Scene View에서 확인하고 싶을 때 사용.
    /// MonoBehaviour의 OnDrawGizmos에서 호출하면 된다.
    /// </summary>
    public static void DrawGizmos(Color color)
    {
        if (!initialized)
            return;

        Gizmos.color = color;

        Vector3 center = new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, worldZ);
        Vector3 size = new Vector3(width, height, 0f);

        Gizmos.DrawWireCube(center, size);
    }

    private static void EnsureInitialized()
    {
        if (initialized)
            return;

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("ScreenWorldBounds is not initialized and Camera.main is null.");
            return;
        }

        Initialize(mainCamera, worldZ);
    }
}