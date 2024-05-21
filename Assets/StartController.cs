using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class StartController : MonoBehaviour
{

    [SerializeField] Volume v;
    private float timer = 0f;
    bool started = false;

    private void Start()
    {
        v.weight = 1;
    }

    private void Update()
    {
        if (!started)
        {
            timer += Time.deltaTime;

            v.weight = Mathf.Lerp(1, 0, timer / 4);

            if(timer >= 4)
            {
                started = true;
            }
        }
    }


}
