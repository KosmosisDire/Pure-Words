using UnityEngine;



public static class TransformExtentions
{
    public static bool HasChild(this Transform t, Transform transform)
    {
        for(int i = 0; i < t.childCount; i++)
        {
            if(t.GetChild(i) == transform)
            {
                return true;
            }
        }

        return false;
    }
}