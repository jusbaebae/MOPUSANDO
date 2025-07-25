using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DisPlatform : MonoBehaviour
{
    public PlayerMove player;
    public float fadeSpeed; // ������� �ӵ�
    private bool isFading = false; // ������� ������ ����
    private Tilemap tilemap;

    void Start()
    {
        tilemap = GetComponent<Tilemap>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            isFading = true;
        }
    }
    void Update()
    {
        if (isFading)
        {
            FadeOut();
        }
    }

    void FadeOut()
    {
        Color color = tilemap.color;
        color.a -= fadeSpeed * Time.deltaTime; // ������ ������ ���ҽ�Ŵ
        tilemap.color = color;

        if (color.a <= 0) // ������ 0 ���ϰ� �Ǹ� ��Ȱ��ȭ
        {
            isFading = false;
            gameObject.SetActive(false);
        }
    }

    void FadeIn()
    {
        gameObject.SetActive(true); // ���� ������Ʈ Ȱ��ȭ
        Color color = tilemap.color;
        color.a = 1f; // ���� ���� 1 (������ ������)���� ����
        tilemap.color = color; // ����� ���� ����
    }

    void OnDisable()
    {
        Invoke("FadeIn", 2); //��Ȱ��ȭ�Ǹ� 2�ʵ� �����
    }
}
