using UnityEngine;

public class Level : MonoBehaviour
{
    public Transform cameraPos { get { return _cameraPos; } private set { _cameraPos = value; } }
    [SerializeField] private Transform _cameraPos;

    public Transform pickaxePos { get { return _pickaxePos; } private set { _pickaxePos = value; } }
    [SerializeField] private Transform _pickaxePos;

    public Transform goldPos { get { return _goldPos; } private set { _goldPos = value; } }
    [SerializeField] private Transform _goldPos;

    public Transform player1Pos { get { return _player1Pos; } private set { _player1Pos = value; } }
    [SerializeField] private Transform _player1Pos;

    public Transform player2Pos { get { return _player2Pos; } private set { _player2Pos = value; } }
    [SerializeField] private Transform _player2Pos;

}
