using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Mirror;
using System;

public class PathPoint 
{
    public Vector3 position { get; set; }
    public Quaternion rotation { get; set; }
    public string controlMethod { get; set; }
    public float seconds { get; set; }

    public bool doRotation, doPosition;

    public enum Method
    {
        IncWorld
    }

    public PathPoint()
    {
        createPathPoint("NONE", 1.0f, Vector3.zero, Quaternion.Euler(0, 0, 0));
        doRotation = false; doPosition = false;
    }
    public PathPoint(string controlMethod, float seconds, Quaternion rotation)
    {
        createPathPoint(controlMethod, seconds, Vector3.zero, rotation );
        doRotation = true; doPosition = false;
    }
    public PathPoint(string controlMethod, float seconds, Vector3 position, Quaternion rotation)
    {
        createPathPoint( controlMethod, seconds, position, rotation);
        doRotation = true; doPosition = true;
    }
    public PathPoint(string controlMethod, float seconds, Vector3 position)
    {
        createPathPoint(controlMethod, seconds, position, Quaternion.Euler(0, 0, 0));
        doRotation = false; doPosition = true;
    }

    public void createPathPoint(string controlMethod, float seconds, Vector3 position, Quaternion rotation)
    {
        this.controlMethod  = controlMethod;
        this.position       = position;
        this.rotation       = rotation;
        this.seconds        = seconds;
    }

}

public class PathQueue : NetworkBehaviour
{
    // test variables

    public List<PathPoint> path; // path for the moviment
    private float _speed;
    private bool _moving;
    private bool _startAcceptPosition, _startAcceptRotation;
    private PathPoint startPoint, targetPoint;
    private float deltaTime;
    
    public bool Moving { get { return _moving; } }

    // Start is called before the first frame update
    void Start()
    {
        if (isLocalPlayer && !isServer) return;

        path = new List<PathPoint>();
        _moving                     = false;
        _startAcceptPosition        = false;
        _startAcceptRotation        = false;
        targetPoint = new PathPoint();
        startPoint = new PathPoint();
    }

    // Update is called once per frame
    void Update()
    {
        if (isLocalPlayer && !isServer) return;

        //
        if (path.Count > 0)
        {
            if (!_startAcceptPosition && !_startAcceptRotation)
            {   // set target position
                switch (path[0].controlMethod.ToLower())
                {
                    case "incworld":
                        targetPoint.position = path[0].position + transform.position;
                        targetPoint.rotation = path[0].rotation * transform.rotation;
                        break;
                    case "inclocal":
                        GameObject tempPos = new GameObject();
                        tempPos.transform.SetPositionAndRotation(transform.position, transform.rotation);
                        tempPos.transform.Translate(path[0].position);
                        targetPoint.position = tempPos.transform.position;
                        targetPoint.rotation = path[0].rotation * transform.rotation;
                        Transform.Destroy(tempPos);
                        break;
                    case "forward":
                        break;
                    case "absworld":
                        targetPoint.position = path[0].position;
                        targetPoint.rotation = path[0].rotation;
                        break;
                }
                // set target rotation and seconds
                
                targetPoint.seconds = path[0].seconds / _speed;
                // set start position and rotation
                startPoint.position = transform.position;
                startPoint.rotation = transform.rotation;

                deltaTime = 0.0f;

                _startAcceptPosition = true;
                _startAcceptRotation = true;
            }

            // positioning or rotating
            if (deltaTime < targetPoint.seconds)
            {
                // calculate interpolation
                float interpolation = deltaTime / targetPoint.seconds;

                if (_startAcceptPosition && path[0].doPosition )   // Positioning
                    transform.position = Vector3.Lerp(startPoint.position, targetPoint.position, interpolation);
                if (_startAcceptRotation && path[0].doRotation)   // Rotation
                    transform.rotation = Quaternion.Lerp(startPoint.rotation, targetPoint.rotation, interpolation);
            }
            else
            {   // set final position
                if (path[0].doPosition)   // Positioning
                    transform.position = targetPoint.position;
                // set final rotation
                if (path[0].doRotation)   // Rotation
                    transform.rotation = targetPoint.rotation;

                _startAcceptPosition = false;
                _startAcceptRotation = false;
            }

            // end of positioning and rotation
            if (!_startAcceptPosition && !_startAcceptRotation)
            {
                if (path.Count > 0) path.RemoveAt(0);
            }
            else
                deltaTime += Time.deltaTime;            // update time counter
        }

        // Moving
        if (path.Count > 0 && (_startAcceptPosition|| _startAcceptRotation))
            _moving = true;
        // Not moving
        if(path.Count==0 && (!_startAcceptPosition && !_startAcceptRotation))
            _moving = false;
    }

    public void PathMove(List<PathPoint> pathPoints, float speed)
    {
        path.AddRange(pathPoints);
        _speed = speed;
    }
    public void PathMove(List<PathPoint> pathPoints)
    {
        PathMove(pathPoints, 1.0f);
    }
}
