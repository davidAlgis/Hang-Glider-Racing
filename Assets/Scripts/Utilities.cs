using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;

public class Utilities : MonoBehaviour
{
    //Help for debug
    public static void instantiateSphereAtPosition(Vector3 position, string name = "Sphere")
    {
        GameObject Sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Sphere.name = name;
        Sphere.transform.position = position;
        Sphere.GetComponent<SphereCollider>().enabled = false;
        //Sphere.transform.localScale /= 2;
    }

    public static bool isCloseEpsilonVec3(Vector3 x, Vector3 y, float epsilon = 0.5f)
    {
        if (Vector3.Distance(x, y) < epsilon)
            return true;
        else
            return false;
    }



    public static Vector2 rotate(Vector2 vec,  float theta)
    {
        //convert degree to radian
        theta *= (Mathf.PI / 180);
        Vector2 afterRotation = new Vector2(Mathf.Cos(theta) * vec.x - Mathf.Sin(theta) * vec.y, Mathf.Sin(theta) * vec.x + Mathf.Cos(theta) * vec.y);

        return afterRotation;
    }

}


[System.Serializable]
public class Pair<T, U>
{
    public Pair(T first, U second)
    {
        this.first = first;
        this.second = second;
    }

    public T first { get; set; }
    public U second { get; set; }
};