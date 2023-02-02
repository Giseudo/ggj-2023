using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{
    [SerializeField]
    private float _rootMaxDistance = 20f;

    public float RootMaxDistance => _rootMaxDistance;
}
