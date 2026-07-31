using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("Setup")]
    public Image fillImage;
    public GameObject visual;            // ✅ MỚI: phần hình bị ẩn/hiện
    public bool hideWhenFull = true;

    [Header("Color (tùy chọn)")]
    public Gradient colorGradient;
    public bool useGradient = false;

    public void SetHealth(float current, float max)
    {
        float percent = Mathf.Clamp01(current / max);

        fillImage.fillAmount = percent;

        if (useGradient)
            fillImage.color = colorGradient.Evaluate(percent);

        // ✅ Chỉ ẩn phần Visual — script này (trên Canvas) luôn sống
        if (visual != null)
            visual.SetActive(!hideWhenFull || percent < 1f);
    }

    void LateUpdate()
    {
        transform.rotation = Quaternion.identity;   // giờ luôn chạy vì Canvas không bao giờ bị tắt
    }
}