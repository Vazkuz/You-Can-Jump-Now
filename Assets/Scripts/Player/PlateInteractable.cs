using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateInteractable : MonoBehaviour
{
    public float weight { get { return _weight; } private set { _weight = value; } }
    [SerializeField] private float _weight;
}
