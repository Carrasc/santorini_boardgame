using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuilderSelectedAnimation : MonoBehaviour
{
    public float speed;

    public void Update()
    {
        float y = Mathf.PingPong(Time.time * speed, 0.1f);
        transform.position = new Vector3(transform.position.x, y, transform.position.z);
        
    }

    private void OnDisable()
    {
        transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
    }
}
