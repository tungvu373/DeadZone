using UnityEngine;

public class Waypoints : MonoBehaviour
{
    public static Transform[] points;

    void Awake()
    {
        points = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            points[i] = transform.GetChild(i);
        }
    }

    // Vẽ đường màu vàng trong Scene view để dễ chỉnh đường đi
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        for (int i = 0; i < transform.childCount; i++)
        {
            Gizmos.DrawWireSphere(transform.GetChild(i).position, 0.2f);
            if (i < transform.childCount - 1)
            {
                Gizmos.DrawLine(
                    transform.GetChild(i).position,
                    transform.GetChild(i + 1).position
                );
            }
        }
    }
}