using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleCamera : MonoBehaviour
{
    private Vector3 cameraStartingPos;
    private float shakeMaxX;
    private float shakeMinX;
    private float shakeMaxY;
    private float shakeMinY;
    private float shakeTimer;
    private bool shakeCameraNow;
    private Vector3 shakeVector;

    void Start()
    {
        cameraStartingPos = gameObject.transform.position;
        shakeMaxX = gameObject.transform.position.x + 0.1f;
        shakeMinX = gameObject.transform.position.x - 0.1f;
        shakeMaxY = gameObject.transform.position.y + 0.1f;
        shakeMinY = gameObject.transform.position.y + 0.1f;
        shakeTimer = 0.2f;
        shakeCameraNow = false;
        shakeVector = new Vector3(0.0f, 0.0f,gameObject.transform.position.z);
    }

    void Update()
    {
        if(shakeCameraNow)
        {
            if(shakeTimer > 0.0f)
            {
                shakeTimer -= Time.deltaTime;
                shakeVector.x = Random.Range(shakeMinX, shakeMaxY);
                shakeVector.y = Random.Range(shakeMinY, shakeMaxY);
                gameObject.transform.position = shakeVector;
            }
            else
            {
                shakeCameraNow = false;
                shakeTimer = 0.2f;
                //gameObject.transform.position = cameraStartingPos;
            }
        }
    }

    public void CameraShake()
    {
        shakeCameraNow = true;
    }
}
