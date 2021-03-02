using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RAHUtility
{
    // returns true after the specified number of frames has passed
    public static bool UpdateLimiter(int _numberOfFrames)
    {
        if (Time.frameCount % _numberOfFrames == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // Get Mouse Position in World with z equal to zero
    public static Vector3 GetMouseWorldPosition(bool isZZero)
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (isZZero == false)
        {
            return worldPosition;
        }
        else
        {
            worldPosition.z = 0f;
            return worldPosition;
        }
    }


    // Create Text in the World - with parent object
    public static TextMesh CreateWorldText(string text, int fontSize, Color color, Transform parent = null, Vector3 localPos = default(Vector3))
    {
        GameObject myGO = new GameObject("World_Text", typeof(TextMesh));
        Transform transform = myGO.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localPos;
        TextMesh textMesh = myGO.GetComponent<TextMesh>();
        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.color = color;
        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.MiddleCenter;

        return textMesh;
    }
}
