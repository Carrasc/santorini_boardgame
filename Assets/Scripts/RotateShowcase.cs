using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateShowcase : MonoBehaviour
{
    [SerializeField] private Transform myTransform;
    [SerializeField] private float speed = 0.2f;

    // Update is called once per frame
    void Update()
    {
        myTransform.Rotate(new Vector3(0f, speed * Time.deltaTime, 0f));
    }

    private void OnDisable()
    {
        myTransform.localEulerAngles = new Vector3(0, 0, 20f);
    }
}
