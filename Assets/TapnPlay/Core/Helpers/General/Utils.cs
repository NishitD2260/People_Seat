using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public static class Utils
{
    public static bool IsPointerOverUI()
    {
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        // Mobile: Check all touches
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    return true;
            }
        }
        return false;
#else
        // Desktop: Check mouse
        if (EventSystem.current.IsPointerOverGameObject())
            return true;
        PointerEventData pe = new(EventSystem.current)
        {
            position = Input.mousePosition
        };
        List<RaycastResult> hits = new();
        EventSystem.current.RaycastAll(pe, hits);
        return hits.Count > 0;
#endif
    }

    public static Vector2 GetWorldUIPosition(Vector3 worldPosition, Transform parent, Camera uiCamera, Camera worldCamera)
    {
        Vector3 screenPosition = worldCamera.WorldToScreenPoint(worldPosition);
        Vector3 uiCameraWorldPosition = uiCamera.ScreenToWorldPoint(screenPosition);
        Vector3 localPos = parent.InverseTransformPoint(uiCameraWorldPosition);
        return (Vector2)localPos;
    }

    #region Math Functions

    /// <summary>
    /// Remap a value from one range to another.
    /// </summary>
    /// <param name="x">The value to remap.</param>
    /// <param name="A">The minimum value of the input range.</param>
    /// <param name="B">The maximum value of the input range.</param>
    /// <param name="C">The minimum value of the output range.</param>
    /// <param name="D">The maximum value of the output range.</param>
    /// <returns>The remapped value.</returns>
    /// <example>
    /// <code>
    /// float remappedValue = Remap(5, 0, 10, 0, 100);
    /// </code>
    /// </example>
    public static float Remap(float x, float A, float B, float C, float D)
    {
        float remappedValue = C + (x - A) / (B - A) * (D - C);
        return remappedValue;
    }

    #endregion

    public static float GetTimeMilliseconds(float startTime)
    {
        return (Time.realtimeSinceStartup - startTime) * 1000f;
    }

    public static Vector2 GetCenter(List<Vector2> positions)
    {
        return new Vector2(positions.Average(position => position.x), positions.Average(position => position.y));
    }

    public static float GetHeightDifference(Vector3 pos1, Vector3 pos2)
    {
        return Mathf.Abs(pos1.y - pos2.y);
    }

    public static float GetDistance(Vector3 pos1, Vector3 pos2)
    {
        return Vector3.Distance(pos1, pos2);
    }

    public static float GetDistanceXZ(Vector3 pos1, Vector3 pos2)
    {
        return Vector3.Distance(new Vector3(pos1.x, 0, pos1.z), new Vector3(pos2.x, 0, pos2.z));
    }


    #region Array/List Functions
    public static void ShuffleArray<T>(T[] arr, int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            int rnd = UnityEngine.Random.Range(0, arr.Length);
            (arr[0], arr[rnd]) = (arr[rnd], arr[0]);
        }
    }
    public static void ShuffleArray<T>(T[] arr, int iterations, System.Random random)
    {
        for (int i = 0; i < iterations; i++)
        {
            int rnd = random.Next(0, arr.Length);
            (arr[0], arr[rnd]) = (arr[rnd], arr[0]);
        }
    }

    public static void ShuffleList<T>(List<T> list, int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            int rnd = UnityEngine.Random.Range(0, list.Count);
            (list[0], list[rnd]) = (list[rnd], list[0]);
        }
    }
    #endregion
}
