using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringBehaviour : MonoBehaviour
{
    public GameObject splinePostition;

    private Vector3 _movementVector;
    private float _maxVelocity = 5.0f;
    private float _desiredVelocity;


    private float _segmentTimer = 0;
    private float _segmentTravelTime = 1.0f;
    private int _segmentIndex = 0;

    public List<Node> nodes;

    private Vector3 calculatedObjTransform;

    public enum steeringBehaviour
    {
        ArrivalBehaviour = 0,
        FleeBehaviour = 1,
        AvoidCollisionBehaviour = 2
    }

    public enum spline
    {
        catmull = 0,
        bezier = 1,
        lerp = 2
    }

    public steeringBehaviour steeringMode;
    public spline splineMode;

    public bool isMoving = false;

    private void Start()
    {
        nodes = new List<Node>(FindObjectsOfType<Node>());
    }

    // Update is called once per frame
    private void Update()
    {
        if (isMoving)
        {
            switch (splineMode)
            {
                case spline.catmull:
                    catmullMove();
                    break;
                case spline.bezier:
                    bezierMove();
                    break;
                case spline.lerp:
                    lerpMove();
                    break;
            }
            splinePosUpdate();
            switch (steeringMode)
            {
                case steeringBehaviour.ArrivalBehaviour:
                    ArrivalBehaviour();
                    break;
                case steeringBehaviour.FleeBehaviour:
                    FleeBehaviour();
                    break;
                case steeringBehaviour.AvoidCollisionBehaviour:
                    AvoidCollisionBehaviour();
                    break;
            }
            _movementVector = _movementVector * _desiredVelocity * Time.deltaTime;
            transform.position += _movementVector;
        }
    }

    private void ArrivalBehaviour()
    {
        _movementVector = (calculatedObjTransform - transform.position).normalized * _maxVelocity;
        _desiredVelocity = Vector3.Distance(calculatedObjTransform, transform.position);
    }

    private void FleeBehaviour()
    {
        float _escapeArea = 10.0f;

        _movementVector = (calculatedObjTransform - transform.position).normalized * _maxVelocity * -1.0f;
        float _dist = Vector3.Distance(calculatedObjTransform, transform.position);

        _desiredVelocity = ((_escapeArea - _dist) >= 0) ? (_escapeArea - _dist) : 0;
    }

    private void AvoidCollisionBehaviour()
    {
        Transform targetTransform;

        int targetNode = 0;

        for(int i = 0; i < nodes.Count; i++)
        {
            if(Vector3.Distance(transform.position, nodes[i].transform.position) < Vector3.Distance(transform.position, nodes[targetNode].transform.position))
            {
                targetNode = i;
            }
        }

        targetTransform = nodes[targetNode].transform;
        float _escapeArea = 3.0f;
        Vector3 _prjV = Vector3.Project(targetTransform.position, transform.position.normalized);

        float _dist = Vector3.Distance(targetTransform.position, transform.position);
        float _avoidForceMultiplier = 1.0f / Vector3.Distance(targetTransform.position, transform.position);

        _movementVector = (calculatedObjTransform - transform.position).normalized * _maxVelocity;
        _movementVector += (_dist < _escapeArea) ? (_prjV * _escapeArea * _avoidForceMultiplier) : Vector3.zero;

        _desiredVelocity = Vector3.Distance(targetTransform.transform.position, transform.position);


    }

    private void catmullMove()
    {
        _segmentTimer += Time.deltaTime;

        if (_segmentTimer > _segmentTravelTime)
        {
            _segmentTimer = 0f;
            _segmentIndex += 1;

            if (_segmentIndex >= nodes.Count)
                _segmentIndex = 0;
        }

        float t = (_segmentTimer / _segmentTravelTime);
        
        if (nodes.Count < 4)
        {
            transform.position = Vector3.zero;
            return;
        }

        
        Vector3 p0, p1, p2, p3;
        int p0_index, p1_index, p2_index, p3_index;

        p1_index = _segmentIndex;
        p0_index = (p1_index == 0) ? nodes.Count - 1 : p1_index - 1;
        p2_index = (p1_index + 1) % nodes.Count;
        p3_index = (p2_index + 1) % nodes.Count;

        p0 = nodes[p0_index].transform.position;
        p1 = nodes[p1_index].transform.position;
        p2 = nodes[p2_index].transform.position;
        p3 = nodes[p3_index].transform.position;

        calculatedObjTransform = Catmull(p0, p1, p2, p3, t);
    }

    private Vector3 Catmull(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        return 0.5f * (2.0f * p1 + t * (-p0 + p2)
        + t * t * (2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3)
        + t * t * t * (-p0 + 3.0f * p1 - 3.0f * p2 + p3));
    }

    private void bezierMove()
    {
        _segmentTimer += Time.deltaTime;

        if (_segmentTimer > _segmentTravelTime)
        {
            _segmentTimer = 0f;
            _segmentIndex += 1;

            if (_segmentIndex >= 0)
                _segmentIndex = 0;
        }

        float t = _segmentTimer / _segmentTravelTime;

        if (nodes.Count < 4)
        {
            transform.position = Vector3.zero;
            return;
        }

        Vector3 p0, p1, p2, p3;
        int p0_index, p1_index, p2_index, p3_index;

        p0_index = _segmentIndex;
        p1_index = (p0_index + 1);
        p2_index = (p1_index + 1);
        p3_index = (p2_index + 1);

        p0 = nodes[p0_index].transform.position;
        p1 = nodes[p1_index].transform.position;
        p2 = nodes[p2_index].transform.position;
        p3 = nodes[p3_index].transform.position;

        calculatedObjTransform = Bezier(p0, p1, p2, p3, t);
    }

    private Vector3 Bezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        return ((p0) + t * (3.0f * -p0 + 3.0f * p1)
            + t * t * (3.0f * p0 - 6.0f * p1 + 3.0f * p2)
            + t * t * t * (-p0 + 3.0f * p1 - 3.0f * p2 + p3));
    }

    private void lerpMove()
    {
        _segmentTimer += Time.deltaTime;

        if (_segmentTimer > _segmentTravelTime)
        {
            _segmentTimer = 0f;
            _segmentIndex += 1;

            if (_segmentIndex >= nodes.Count)
                _segmentIndex = 0;
        }

        float t = _segmentTimer / _segmentTravelTime;

        if (nodes.Count < 2)
        {
            transform.position = Vector3.zero;
            return;
        }

        Vector3 p0, p1;
        int p0_index, p1_index;

        p1_index = _segmentIndex;
        p0_index = (p1_index == 0) ? nodes.Count - 1 : p1_index - 1;

        p0 = nodes[p0_index].transform.position;
        p1 = nodes[p1_index].transform.position;

        calculatedObjTransform = LERP(p0, p1, t);
    }

    public Vector3 LERP(Vector3 p0, Vector3 p1, float t)
    {
        return (1.0f - t) * p0 + t * p1;
    }

    private void splinePosUpdate()
    {
        splinePostition.transform.position = calculatedObjTransform;
    }
}