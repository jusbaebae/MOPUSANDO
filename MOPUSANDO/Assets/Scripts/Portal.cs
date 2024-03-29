using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public Transform Eportal;
    public PlayerMove player;
    Vector2 startPos;
    Vector2 endPos;
    Vector2 desPos;
    public float speed;

    void Start()
    {
        player = GameObject.Find("character").GetComponent<PlayerMove>();
        startPos = new Vector2(transform.position.x, transform.position.y + 0.1f);
        endPos = new Vector2(transform.position.x, transform.position.y - 0.1f);
        desPos = endPos;
        speed = 0.2f;
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
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name.Contains("character"))
        {
            player.transform.position = Eportal.transform.position;
        }
    }
}
