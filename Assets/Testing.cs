using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    public Vector3 a;
    public Vector3 b;
    public Vector3 c;

    public float w1 = 0.3f;
    public float w2 = 0.2f;

    // Start is called before the first frame update
    void Update()
    {
        Debug.DrawLine(a, b);
        Debug.DrawLine(b, c);

        Vector2 inter = HelperFunctions.GetIntersectionPoint(a, b, c, w1, w2);
        Debug.DrawLine(new Vector3(inter.x, inter.y, 0), new Vector3(inter.x, inter.y, 3));
    }

    public static float acot(float x)
    {
        return Mathf.PI / 2 - Mathf.Atan(x);
    }

    //// Update is called once per frame
    //public static void GetIntersection(Vector2 a, Vector2 midPoint, Vector2 b, float w1, float w2)
    //{
    //    Vector3 line1 = midPoint - a;
    //    Vector3 line2 = b - midPoint;

    //    Debug.DrawLine(a, midPoint);
    //    Debug.DrawLine(midPoint, b);

    //    float angle = 180 - Vector3.Angle(line1, line2);

    //    float angleRad = Mathf.Deg2Rad * angle;
    //    float theta1 = acot((w2 / w1 + Mathf.Cos(angleRad)) / Mathf.Sin(angleRad));

    //    float d1 = w1 / Mathf.Tan(theta1);

    //    Vector2 oppositeLine1 = -line1.normalized;
    //    Vector2 leftOfOL1 = new Vector3(-oppositeLine1.y, 0, oppositeLine1.x);
    //    Vector2 offset = oppositeLine1 * d1 + leftOfOL1 * w1;
    //    Vector2 p = midPoint + offset;
    //}

    //float getTheta1 (float thetaT, float w1, float w2)
    //{
    //    float theta1 = acot((w2 / w1 + Mathf.Cos(thetaT)) / Mathf.Sin(thetaT));
    //    return  theta1;

    //}

	
}
