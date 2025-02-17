using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public Transform cameraPos { get { return _cameraPos; } private set { _cameraPos = value; } }
    [SerializeField] private Transform _cameraPos;

    public Transform pickaxePos { get { return _pickaxePos; } private set { _pickaxePos = value; } }
    [SerializeField] private Transform _pickaxePos;

    public Transform goldPos { get { return _goldPos; } private set { _goldPos = value; } }
    [SerializeField] private Transform _goldPos;

    public List<Transform> playersPos { get { return _playersPos; } private set { _playersPos = value; } }
    [SerializeField] private List<Transform> _playersPos;

    public Door door { get { return _door; } private set { _door = value; } }
    [SerializeField] private Door _door;

    public List<Mineral> mineral { get { return _mineral; } private set { _mineral = value; } }
    [SerializeField] private List<Mineral> _mineral;
}
