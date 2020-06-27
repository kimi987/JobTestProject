
using UnityEngine;

public class Utils 
{
    public static Vector3 GetMouseWorldPosition()
    {
        return Camera.main == null ? Vector3.zero : Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0,0, 13.5f));
    }
}
