using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuilderSelectedAnimation : MonoBehaviour
{
    public float speed;

    private float startingYPos = 0;

    public void Update()
    {
        float y = Mathf.PingPong(Time.time * speed, 0.1f);
        transform.position = new Vector3(transform.position.x, y, transform.position.z);
        
    }

    private void OnEnable()
    {
        startingYPos = transform.position.y;
    }

    private void OnDisable()
    {
        transform.position = new Vector3(transform.position.x, startingYPos, transform.position.z);
    }
}
