using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DisPlatform : MonoBehaviour
{
    public PlayerMove player;
    public float fadeSpeed; // 사라지는 속도
    private bool isFading = false; // 사라지는 중인지 여부
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
        color.a -= fadeSpeed * Time.deltaTime; // 투명도를 서서히 감소시킴
        tilemap.color = color;

        if (color.a <= 0) // 투명도가 0 이하가 되면 비활성화
        {
            isFading = false;
            gameObject.SetActive(false);
        }
    }

    void FadeIn()
    {
        gameObject.SetActive(true); // 게임 오브젝트 활성화
        Color color = tilemap.color;
        color.a = 1f; // 알파 값을 1 (완전히 불투명)으로 설정
        tilemap.color = color; // 변경된 색상 적용
    }

    void OnDisable()
    {
        Invoke("FadeIn", 2); //비활성화되면 2초뒤 재생성
    }
}
