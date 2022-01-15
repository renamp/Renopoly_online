using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public static float positionSpace = 1.425f + 1.3f + 1.425f;

    private static Vector3[] objStartPosition = new Vector3[4];
    private static Quaternion[] objStartRotation = new Quaternion[4];

    public static void startPosition(int index, GameObject objStartPoint)
    {
        if (index < 4)
        {
            objStartPosition[index] = objStartPoint.transform.position;
            objStartRotation[index] = objStartPoint.transform.rotation;
        }
    }

    public static List<PathPoint> jumpForward(int playerId, int playerPosition ) 
    {
        List<PathPoint> listMove = new List<PathPoint>();

        listMove.Add(new PathPoint("inclocal", 0.2f, new Vector3(0.0f, 0.5f, 0f)));
        listMove.Add(new PathPoint("inclocal", 0.2f, new Vector3(0.0f, 0.5f, 1.425f)));
        listMove.Add(new PathPoint("inclocal", 0.2f, new Vector3(0.0f, 0.0f, 1.3f)));

        if (playerPosition == 9 || playerPosition == 19 || playerPosition == 29 || playerPosition == 39)
        {
            Pose objPose = getPosition(playerId, playerPosition + 1);
            listMove.Add(new PathPoint("absworld", 0.2f, objPose.position + new Vector3(0f, 0.5f, 0f)));
            listMove.Add(new PathPoint("absworld", 0.2f, objPose.rotation));
            listMove.Add(new PathPoint("absworld", 0.2f, objPose.position));
        }
        else
        {
            listMove.Add(new PathPoint("inclocal", 0.2f, new Vector3(0.0f, -0.5f, 1.425f)));
            listMove.Add(new PathPoint("inclocal", 0.2f, new Vector3(0.0f, -0.5f, 0f)));
        }
        return listMove;
    }

    public static List<PathPoint> jumpToJail(int playerId)
    {
        List<PathPoint> listMove = new List<PathPoint>();
        Pose objPose = getPosition(playerId, 10);
        listMove.Add(new PathPoint("inclocal", 0.2f, new Vector3(0.0f, 1f, 0f)));
        listMove.Add(new PathPoint("absworld", 1.5f, objPose.position + new Vector3(0f, 1f, 0f)));
        listMove.Add(new PathPoint("absworld", 0.2f, objPose.rotation));
        listMove.Add(new PathPoint("absworld", 0.2f, objPose.position));
        return listMove;
    }

    public static List<PathPoint> jumpToPosition(int playerId, int position)
    {
        List<PathPoint> listMove = new List<PathPoint>();
        Pose objPose = getPosition(playerId, position);
        listMove.Add(new PathPoint("inclocal", 0.2f, new Vector3(0.0f, 1f, 0f)));
        listMove.Add(new PathPoint("absworld", 1.5f, objPose.position + new Vector3(0f, 1f, 0f)));
        listMove.Add(new PathPoint("absworld", 0.2f, objPose.rotation));
        listMove.Add(new PathPoint("absworld", 0.2f, objPose.position));
        return listMove;
    }

    public static Pose getPosition(int playerId, int playerPosition)
    {
        Pose objPos = new Pose();
        if (playerPosition == 40) playerPosition = 0;

        if (playerPosition < 10)
        {
            Vector3 jumpVect = new Vector3(0f, 0f, positionSpace) * (playerPosition - 0);

            if (playerId < 3)
                objPos.position = objStartPosition[0] + new Vector3(0f, 0f, 1.4f * ((float)playerId)) + jumpVect;
            else if (playerId < 6)
                objPos.position = objStartPosition[0] + new Vector3(1.4f, 0f, 1.4f * ((float)playerId - 3)) + jumpVect;
            objPos.rotation = objStartRotation[0];
        }
        else if(playerPosition < 20)
        {
            Vector3 jumpVect = new Vector3(positionSpace, 0f, 0f) * (playerPosition - 10);

            if (playerId < 3)
                objPos.position = objStartPosition[1] + new Vector3(1.4f * ((float)playerId), 0f, 0f) + jumpVect;
            else if (playerId < 6)
                objPos.position = objStartPosition[1] + new Vector3(1.4f * ((float)playerId - 3), 0f, -1.4f) + jumpVect;
            objPos.rotation = objStartRotation[1];
        }
        else if (playerPosition < 30)
        {
            Vector3 jumpVect = new Vector3(0f, 0f, positionSpace) * (playerPosition - 20);

            if (playerId < 3)
                objPos.position = objStartPosition[2] - new Vector3(0f, 0f, 1.4f * ((float)playerId)) - jumpVect;
            else if (playerId < 6)
                objPos.position = objStartPosition[2] - new Vector3(1.4f, 0f, 1.4f * ((float)playerId - 3)) - jumpVect;
            objPos.rotation = objStartRotation[2];
        }
        else if (playerPosition < 40)
        {
            Vector3 jumpVect = new Vector3(positionSpace, 0f, 0f) * (playerPosition - 30);

            if (playerId < 3)
                objPos.position = objStartPosition[3] - new Vector3(1.4f * ((float)playerId), 0f, 0f) - jumpVect;
            else if (playerId < 6)
                objPos.position = objStartPosition[3] - new Vector3(1.4f * ((float)playerId - 3), 0f, -1.4f) - jumpVect;
            objPos.rotation = objStartRotation[3];
        }

        return objPos;
    }

    public static void setStartPosition(int playerId, int playerPosition, Transform startPoint, Transform target)
    {
        if(playerPosition < 10) 
        {
            Vector3 posVect = new Vector3(0f, 0f, positionSpace) * playerPosition;
            if (playerId < 3)
                target.SetPositionAndRotation(startPoint.position + new Vector3(0f, 0f, 1.4f * ((float)playerId)) + posVect, startPoint.rotation);
            else if (playerId < 6)
                target.SetPositionAndRotation(startPoint.position + new Vector3(1.4f, 0f, 1.4f * ((float)playerId - 3)) + posVect, startPoint.rotation);
        }
        else //if(playerPosition < 20)
        {
            Vector3 posVect = new Vector3(positionSpace, 0f, 0f) * playerPosition;
            if (playerId < 3)
                target.SetPositionAndRotation(startPoint.position + new Vector3(1.4f * ((float)playerId), 0f, 0f) + posVect, startPoint.rotation);
            else if (playerId < 6)
                target.SetPositionAndRotation(startPoint.position + new Vector3(1.4f * ((float)playerId - 3), 0f, 1.4f) + posVect, startPoint.rotation);
        }
    }
    public static void setStartPosition(int playerId, Transform startPoint, Transform target)
    {
        setStartPosition(playerId, 0, startPoint, target);
    }
}
