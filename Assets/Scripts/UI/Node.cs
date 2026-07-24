using UnityEngine;

public class Node : MonoBehaviour
{
    [Header("Colors")]
    public Color hoverColor = new Color(0f, 1f, 0f, 0.5f);      // xanh: xây được
    public Color occupiedColor = new Color(1f, 0f, 0f, 0.5f);   // đỏ: đã có tower

    [HideInInspector] public GameObject tower;   // tower đang đứng trên ô (null = trống)

    private SpriteRenderer rend;
    private Color startColor;

    void Awake()
    {
        rend = GetComponent<SpriteRenderer>();
        startColor = rend.color;
    }

    public bool IsEmpty()
    {
        return tower == null;
    }

    public void Highlight()
    {
        rend.color = IsEmpty() ? hoverColor : occupiedColor;
    }

    public void ResetColor()
    {
        rend.color = startColor;
    }

    public Vector3 GetBuildPosition()
    {
        return transform.position;   // tower snap vào giữa ô
    }
}