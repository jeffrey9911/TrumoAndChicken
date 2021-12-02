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

    private float _segmentTimer = 0;
    private float _segmentTravelTime = 1.0f;
    private int _segmentIndex = 0;

    public List<Node> nodes;

    private Vector3 calculatedObjTransform;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        fleeWithAnim();
        actGravity(false);

        _movementVector = _movementVector * _desiredVelocity * Time.deltaTime;
        //transform.position += _movementVector;
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

    private void fleeWithAnim()
    {
        float _escapeArea = 3.0f;

        _movementVector = (_target.transform.position - transform.position).normalized * _maxVelocity * -1.0f;
        float _dist = Vector3.Distance(_target.transform.position, transform.position);


        if ((_escapeArea - _dist) >= 0)
        {
            _animator.SetBool("Turn Head", false);
            _animator.SetBool("Run", true);

            _desiredVelocity = _escapeArea - _dist;

            Vector3 dir = _target.transform.position - transform.position;
            dir.y = 0;

            transform.forward = -dir;

        }
        else
        {
            _animator.SetBool("Run", false);
            _animator.SetBool("Turn Head", true);
        }

        _movementVector.y = 0;
    }


}
