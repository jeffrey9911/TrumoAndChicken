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

    

    public enum behaviour
    {
        Flee = 0,
        Arival = 1
    }
    public behaviour _behav;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch(_behav)
        {
            case behaviour.Flee:
                fleeWithAnim(3.0f);
                break;
            case behaviour.Arival:
                ArrivalBehaviour();
                break;
            default:
                break;
        }


        
        actGravity(false);

        _movementVector = _movementVector * _desiredVelocity * Time.deltaTime * 0.2f ;
        _characterController.Move(_movementVector + gravVelo);
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

                _desiredVelocity = Vector3.Distance(_target.transform.position, transform.position) * 0.5f;

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
        
        _movementVector.y = 0;
    }

}
