using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceScript : MonoBehaviour
{
    public int value;  // if 0 is moving else store value

    private int resultDelay;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("slowUpdate");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator slowUpdate()
    {
        while (true) {

            if (value == 0)
            {
                if (GetComponent<Rigidbody>().angularVelocity == Vector3.zero && GetComponent<Rigidbody>().velocity == Vector3.zero)
                {
                    if (resultDelay < 10) resultDelay++;
                }
                else
                {
                    resultDelay = 0;
                }

                if (resultDelay == 4)
                {
                    if (transform.forward.y > 0.92) value = 1;
                    else if (transform.forward.y < -0.92) value = 6;
                    else if (transform.up.y > 0.92) value = 5;
                    else if (transform.up.y < -0.92) value = 2;
                    else if (transform.right.y > 0.92) value = 4;
                    else if (transform.right.y < -0.92) value = 3;
                    else value = -1;
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}
