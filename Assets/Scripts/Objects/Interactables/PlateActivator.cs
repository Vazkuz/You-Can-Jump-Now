using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateActivator : MonoBehaviour
{
    public enum ActivatorType
    {
        dwarf,
        gold,
        pickaxe
    }

    public ActivatorType type { get { return typeValue; } private set { typeValue = value; } }
    [SerializeField] private ActivatorType typeValue;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
