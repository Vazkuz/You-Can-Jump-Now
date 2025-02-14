using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MovablePlatform : TriggerTarget
{
    [SerializeField] private Transform inactivePos;
    [SerializeField] private Transform activePos;
    [SerializeField] private float timeToDestiny = 1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Collision Variables")]
    [SerializeField] private Vector2 rightBoxSize;
    [SerializeField] private Vector2 upBoxSize;
    [SerializeField] private float castDistRight;
    [SerializeField] private float castDistUp;

    private NetworkVariable<bool> canMove = new NetworkVariable<bool>(true);
    private float speed;
    private Rigidbody2D rb;
    private float errorThreshold = 0.01f;

    private Vector2 direction;

    protected void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        speed = (inactivePos.position - activePos.position).magnitude / timeToDestiny;
    }
    public override void Activate()
    {
        base.Activate();

        direction = (activePos.position - transform.position).normalized;
        CheckIfCanMove();
    }

    public override void Deactivate()
    {
        base.Deactivate();

        direction = (inactivePos.position - transform.position).normalized;
        CheckIfCanMove();
    }

    //protected void Update()
    //{
    //    if (isActive.Value && transform.position == activePos.position) return;
    //    if (!isActive.Value && transform.position == inactivePos.position) return;

    //    if (isActive.Value)
    //    {
    //        transform.position = Vector3.MoveTowards(transform.position, activePos.position, speed * Time.deltaTime);
    //    }
    //    else
    //    {
    //        transform.position = Vector3.MoveTowards(transform.position, inactivePos.position, speed * Time.deltaTime);
    //    }
    //}

    protected void FixedUpdate()
    {
        if (!canMove.Value) return;

        if (isActive.Value && Vector2.Distance(transform.position, activePos.position) <= errorThreshold) return;
        if (!isActive.Value && Vector2.Distance(transform.position, inactivePos.position) <= errorThreshold) return;


        rb.MovePosition(rb.position + speed * direction * Time.fixedDeltaTime);
    }

    protected void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer != Mathf.Log(groundLayer, 2)) return;

        print($"Entra colision contra {collision.gameObject.name}: ");
        CheckIfCanMove();
    }

    protected void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer != Mathf.Log(groundLayer, 2)) return;

        print($"Sale colision contra {collision.gameObject.name}: ");
        CheckIfCanMove();
    }

    private void CheckIfCanMove()
    {
        if (!IsServer) return;

        //if (other == null)
        //{
        //    canMove.Value = true;
        //    return;
        //}

        //Vector2 collisionVector = collision.transform.position - transform.position;

        //if (Vector2.Dot(collisionVector, direction) <= 0)
        //{
        //    canMove.Value = false;
        //}
        foreach(Vector2 collDir in CollidersList())
        {
            if(Vector2.Dot(collDir, direction) > 0)
            {
                canMove.Value = false;
                return;
            }
        }
        canMove.Value = true;
    }

    /// <summary>
    /// Drawing the cube that is used to check if player is grounded.
    /// </summary>
    protected void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position - transform.up * castDistUp, upBoxSize); //down
        Gizmos.DrawWireCube(transform.position + transform.up * castDistUp, upBoxSize); //up
        Gizmos.DrawWireCube(transform.position - transform.right * castDistRight, rightBoxSize); //left
        Gizmos.DrawWireCube(transform.position + transform.right * castDistRight, rightBoxSize); //right
    }


    public List<Vector2> CollidersList()
    {
        Vector2 leftCol = Physics2D.BoxCast(transform.position, rightBoxSize, 0, -transform.right, castDistRight, groundLayer) ? new Vector2(-1, 0) : Vector2.zero;
        Vector2 rightCol = Physics2D.BoxCast(transform.position, rightBoxSize, 0, transform.right, castDistRight, groundLayer) ? new Vector2(1, 0) : Vector2.zero;
        Vector2 downCol = Physics2D.BoxCast(transform.position, upBoxSize, 0, -transform.up, castDistUp, groundLayer) ? new Vector2(0, -1) : Vector2.zero;
        Vector2 upCol = Physics2D.BoxCast(transform.position, upBoxSize, 0, transform.up, castDistUp, groundLayer) ? new Vector2(0, 1) : Vector2.zero;
        List<Vector2> listReturn = new()
            {
                leftCol,
                rightCol,
                downCol,
                upCol
            };
        return listReturn;
    }
}
