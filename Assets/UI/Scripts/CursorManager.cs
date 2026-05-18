using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [Header("Cursor")]
    public Texture2D aimReticle;
    public Vector2 hotspot = Vector2.zero;

    void Awake()
    {
        if (aimReticle != null)
        {
            hotspot = new Vector2(aimReticle.width / 2f, aimReticle.height / 2f);
            Cursor.SetCursor(aimReticle, hotspot, CursorMode.Auto);
        }

        Cursor.visible = true;
    }
}