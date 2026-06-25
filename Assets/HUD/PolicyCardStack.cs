using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Tumpukan kartu Kebijakan di pojok kiri-bawah. Default: kartu menumpuk
/// (saling tindih, sedikit miring). Saat hover: naik, sedikit membesar, dan
/// menyebar ke kanan seperti mengipas kartu. Animasi pakai lerp halus.
///
/// Kartu = anak-anak GameObject ini (urutan sibling = urutan tumpukan,
/// anak terakhir tampil paling depan/kanan).
/// </summary>
public class PolicyCardStack : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Saat menumpuk (default: sejajar)")]
    [SerializeField] private Vector2 stackStep = new Vector2(4f, 0f); // nyaris sejajar (peek tepi tipis)
    [SerializeField] private float stackRot = 0f;                     // tanpa miring saat menumpuk

    [Header("Saat hover (menyebar sedikit ke kanan)")]
    [SerializeField] private float fanStepX = 26f;   // jarak sebar ke kanan
    [SerializeField] private float riseY = 14f;      // naik sedikit
    [SerializeField] private float fanRot = 2f;      // miring antar kartu saat menyebar
    [SerializeField] private float hoverScale = 1.05f;

    [Header("Animasi")]
    [SerializeField] private float speed = 12f;

    private bool hovered;
    private RectTransform[] cards;

    void Awake()
    {
        var list = new List<RectTransform>();
        foreach (Transform c in transform) list.Add((RectTransform)c);
        cards = list.ToArray();
    }

    public void OnPointerEnter(PointerEventData e) { hovered = true; }
    public void OnPointerExit(PointerEventData e)  { hovered = false; }

    void Update()
    {
        if (cards == null || cards.Length == 0) return;

        float t = 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime); // lerp bebas frame-rate
        for (int i = 0; i < cards.Length; i++)
        {
            GetTarget(i, out Vector2 pos, out float rot, out float scale);
            var rt = cards[i];
            rt.anchoredPosition = Vector2.Lerp(rt.anchoredPosition, pos, t);
            rt.localRotation = Quaternion.Euler(0, 0, Mathf.LerpAngle(rt.localEulerAngles.z, rot, t));
            rt.localScale = Vector3.Lerp(rt.localScale, Vector3.one * scale, t);
        }
    }

    // Target posisi/rotasi/skala kartu ke-i, tergantung sedang hover atau tidak.
    private void GetTarget(int i, out Vector2 pos, out float rot, out float scale)
    {
        if (hovered)
        {
            pos = new Vector2(i * fanStepX, riseY);
            rot = -i * fanRot;       // makin ke kanan makin miring (efek kipas)
            scale = hoverScale;
        }
        else
        {
            pos = new Vector2(i * stackStep.x, i * stackStep.y);
            rot = -i * stackRot;
            scale = 1f;
        }
    }
}
