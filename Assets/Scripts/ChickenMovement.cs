using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChickenMovement : MonoBehaviour
{
    public GameObject _target;
    public Animator _animator;
    public CharacterController _characterController;

    private Vector3 _movementVector;
    private float _maxVelocity = 5.0f;
    private float _desiredVelocity;

    private Vector3 gravVelo;
    private RaycastHit rayCout;

    public bool isUnderAttack = false;

    public float fleeTime = 0;


    private float _segmentTimer = 0;
    private float _segmentTravelTime = 1.0f;
    private int _segmentIndex = 0;

    private List<catNode> catNodes;

    public List<avdCNode> avoidCollisionNodes;

    private Vector3 calculatedObjTransform;

    public enum behaviour
    {
        Flee = 0,
        Arival = 1,
        Grass = 2,
        Chase = 3
    }
    public behaviour _behav;
    // Start is called before the first frame update
    void Start()
    {
        avoidCollisionNodes = new List<avdCNode>(FindObjectsOfType<avdCNode>());
        catNodes = new List<catNode>(FindObjectsOfType<catNode>());
    }

    // Update is called once per frame
    void Update()
    {
        switch(_behav)
        {
            case behaviour.Flee:
                actGravity(false);
                fleeWithAnim(3.0f);
                break;
            case behaviour.Arival:
                actGravity(false);
                ArrivalBehaviour();
                break;
            case behaviour.Grass:
                catmullMove();
                break;
            case behaviour.Chase:
                catmullMove();
                actGravity(false);
                AvoidCollisionBehaviour();
                break;
            default:
                break;
        }


        
        

        _movementVector = _movementVector * _desiredVelocity * Time.deltaTime;
        if(_behav != behaviour.Grass)
        {
            _movementVector.y = 0;
            _characterController.Move(_movementVector + gravVelo);
        }
        else
        {
            transform.position = calculatedObjTransform;
        }
        
    }

    bool actGravity(bool isDebugging)
    {
        Physics.Raycast(transform.position, Vector3.down, out rayCout);

        if (isDebugging)
            Debug.Log("rayCout.distance: " + rayCout.distance);

        if (rayCout.distance <= 0.1 || _characterController.isGrounded)
        {
            gravVelo.y = 0.0f;
            return true;
        }
        gravVelo.y += -9.8f * Time.deltaTime;
        return false;
    }

    private void fleeWithAnim(float _escapeArea)
    {
        
        float _dist = Vector3.Distance(_target.transform.position, transform.position);


        if ((_escapeArea - _dist) > 0)
        {
            _movementVector = (_target.transform.position - transform.position).normalized * _maxVelocity * -1.0f;
            _animator.SetBool("Turn Head", false);
            _animator.SetBool("Run", true);

            _desiredVelocity = _escapeArea - _dist;
            _desiredVelocity *= 0.2f;

            Vector3 dir = _target.transform.position - transform.position;
            dir.y = 0;

            transform.forward = -dir;

        }
        else
        {
            _movementVector = Vector3.zero;
            _animator.SetBool("Run", false);
            _animator.SetBool("Turn Head", true);
        }

        _movementVector.y = 0;
    }

    private void ArrivalBehaviour()
    {
        
        float _dist = Vector3.Distance(_target.transform.position, transform.position);

        if(fleeTime <= 0)
        {
            if (_dist >= 1.5)
            {
                _movementVector = (_target.transform.position - transform.position).normalized * _maxVelocity;
                _animator.SetBool("Turn Head", false);
                _animator.SetBool("Run", true);

                _desiredVelocity = Vector3.Distance(_target.transform.position, transform.position) * 0.1f;

                Vector3 dir = _target.transform.position - transform.position;
                dir.y = 0;

                transform.forward = dir;
            }
            else
            {
                _movementVector = Vector3.zero;
                _animator.SetBool("Run", false);
                _animator.SetBool("Turn Head", true);
            }
        }
        else
        {
            fleeTime -= Time.deltaTime;
            fleeWithAnim(5.0f);
        }
        
        
    }


    private void AvoidCollisionBehaviour()
    {
        _animator.SetBool("Run", true);
        Transform targetTransform;

        int targetNode = 0;

        for (int i = 0; i < avoidCollisionNodes.Count; i++)
        {
            if (Vector3.Distance(transform.position, avoidCollisionNodes[i].transform.position) < 
                Vector3.Distance(transform.position, avoidCollisionNodes[targetNode].transform.position))
            {
                targetNode = i;
            }
        }

        targetTransform = avoidCollisionNodes[targetNode].transform;
        float _escapeArea = 2.0f;
        Vector3 _prjV = Vector3.Project(targetTransform.position, transform.position.normalized);

        float _dist = Vector3.Distance(targetTransform.position, transform.position);
        float _avoidForceMultiplier = 1.0f / Vector3.Distance(targetTransform.position, transform.position);

        _movementVector = (calculatedObjTransform - transform.position).normalized * _maxVelocity;
        _movementVector += (_dist < _escapeArea) ? (_prjV * _escapeArea * _avoidForceMultiplier * 10) : Vector3.zero;
        

        _desiredVelocity = Vector3.Distance(calculatedObjTransform, transform.position) * 0.1f;

        Vector3 dir = transform.position - calculatedObjTransform;
        dir.y = 0;

        transform.forward = -dir;
    }


    private void catmullMove()
    {
        _segmentTimer += Time.deltaTime * 0.2f;

        if (_segmentTimer > _segmentTravelTime)
        {
            _segmentTimer = 0f;
            _segmentIndex += 1;

            if (_segmentIndex >= catNodes.Count)
                _segmentIndex = 0;
        }

        float t = (_segmentTimer / _segmentTravelTime);

        if (catNodes.Count < 4)
        {
            transform.position = Vector3.zero;
            return;
        }


        Vector3 p0, p1, p2, p3;
        int p0_index, p1_index, p2_index, p3_index;

        p1_index = _segmentIndex;
        p0_index = (p1_index == 0) ? catNodes.Count - 1 : p1_index - 1;
        p2_index = (p1_index + 1) % catNodes.Count;
        p3_index = (p2_index + 1) % catNodes.Count;

        p0 = catNodes[p0_index].transform.position;
        p1 = catNodes[p1_index].transform.position;
        p2 = catNodes[p2_index].transform.position;
        p3 = catNodes[p3_index].transform.position;

        calculatedObjTransform = Catmull(p0, p1, p2, p3, t);
    }

    private Vector3 Catmull(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        return 0.5f * (2.0f * p1 + t * (-p0 + p2)
        + t * t * (2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3)
        + t * t * t * (-p0 + 3.0f * p1 - 3.0f * p2 + p3));
    }

}
