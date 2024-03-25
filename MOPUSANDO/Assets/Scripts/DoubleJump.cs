using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleJump : MonoBehaviour
{
    Vector2 startPos;
    Vector2 endPos;
    Vector2 desPos;
    public float speed;

    void Start()
    {
        startPos = new Vector2(transform.position.x, transform.position.y + 0.3f);
        endPos = new Vector2(transform.position.x, transform.position.y - 0.3f);
        desPos = endPos;
        speed = 0.5f;
    }
    void FixedUpdate()
    {
        transform.position = Vector2.MoveTowards(transform.position, desPos, Time.deltaTime * speed);

        if (Vector2.Distance(transform.position, desPos) <= 0.05f)
        {
            if (desPos == endPos) desPos = startPos;
            else desPos = endPos;
        }
    }
}
