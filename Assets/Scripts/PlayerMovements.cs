using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMovements : MonoBehaviour
{
    public Animator _animator;
    public CharacterController _cController;

    public Transform _model;
    private bool isTouchingModel = false;
    private Vector3 pushDistin;

    public float dtMultiplier = 1.0f;
    public float _jumpForce = 5f;


    private int rotAng;
    private RaycastHit rayCout;
    private float veloY;

    private float jumpCD;

    public List<Node> nodes;

    // Start is called before the first frame update
    void Start()
    {
        pushDistin = _model.transform.position;
        nodes = new List<Node>(FindObjectsOfType<Node>());
    }

    private void FixedUpdate()
    {
        
        
    }

    private void Update()
    {
        isCollidingVertically(false);
        getKeyRotation(false);
        countingDown();

        Debug.Log(veloY);

        if(isCollidingVertically(false) || _cController.isGrounded)
        {
            
            
            
            if(isPressingWASD() && !Input.GetKey(KeyCode.Space))
            {
                transform.rotation = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y + rotAng, 0);
                
                _animator.SetBool("isRun", true);
            }
            else
            {
                
                _animator.SetBool("isRun", false);
            }

            if(jumpCD <= 0)
            {
                if (Input.GetKey(KeyCode.Space))
                {
                    
                    _animator.SetTrigger("isJumpTrig");
                    Debug.Log("JUMP");
                    veloY += _jumpForce;

                    jumpCD = 2;
                }
            }
        }

        if(Input.GetMouseButtonDown(0)) // Left
        {
            _animator.SetTrigger("isAttack");
            attackFlee();
        }
        if (Input.GetMouseButtonDown(1))
        {
            _animator.SetTrigger("isPush");
            pushLerp();
        }

        _model.transform.position = LERP(_model.transform.position, pushDistin, Time.deltaTime);
        _cController.Move(new Vector3(0f, veloY, 0f) * Time.deltaTime * dtMultiplier);
        

    }

    private void getKeyRotation(bool isDebugging)
    {
        if (Input.GetKey(KeyCode.A))
            rotAng = 270;
            
        if (Input.GetKey(KeyCode.D))
            rotAng = 90;

        if (Input.GetKey(KeyCode.W))
        {
            rotAng = 0;
            rotAng += (Input.GetKey(KeyCode.D)) ? 45 : 0;
            rotAng += (Input.GetKey(KeyCode.A)) ? -45 : 0;
        }
        if (Input.GetKey(KeyCode.S))
        {
            rotAng = 180;
            rotAng += (Input.GetKey(KeyCode.D)) ? -45 : 0;
            rotAng += (Input.GetKey(KeyCode.A)) ? 45 : 0;
        }

        if (Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.A))
            rotAng = 0;
        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.S))
            rotAng = 0;
       
        if (isDebugging)
            Debug.Log("rotAng: " + rotAng);
    }

    bool isCollidingVertically(bool isDebugging)
    {
        Physics.Raycast(transform.position, Vector3.down, out rayCout);

        if (isDebugging)
            Debug.Log("rayCout.distance: " + rayCout.distance);

        if (rayCout.distance <= 0.1)
        {
            return true;
        }

        return false;
    }


    private bool isPressingWASD()
    {
        if(Input.GetKey(KeyCode.W) 
            || Input.GetKey(KeyCode.A)
            || Input.GetKey(KeyCode.S)
            || Input.GetKey(KeyCode.D))
        {
            return true;
        }
        return false;
    }

    private void countingDown()
    {
        if(jumpCD > 0)
            jumpCD -= Time.deltaTime;

        if(veloY > 0)
            veloY += -9.8f * Time.deltaTime * dtMultiplier;
    }

    private void pushLerp()
    {
        Vector3 _dist = _model.transform.position - transform.position;
        if((_dist.z <= 2.3 && _dist.z >= -2.3) && (_dist.x <= 1.8 && _dist.x >= -1.8))
        {
            Vector3 pushVec = new Vector3(_dist.x, 0.0f, _dist.z).normalized;
            pushDistin = _model.transform.position + pushVec * 3.0f;
        }
    }

    private void attackFlee()
    {
        for(int i = 0; i < nodes.Count; i ++)
        {
            if(Vector3.Distance(transform.position, nodes[i].transform.position) <= 1.6)
            {
                nodes[i].GetComponent<ChickenMovement>().fleeTime = 3.0f;
            }
        }
    }

    private Vector3 LERP(Vector3 p0, Vector3 p1, float t)
    {
        return (1.0f - t) * p0 + t * p1;
    }

    
}
