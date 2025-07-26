using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGravity : MonoBehaviour
{
    public GameManager gameManager;
    public PlayerMove player;
    public bool isGravity = true;
    public bool isGravityReversed = false;
    private SpriteRenderer playerSprite; // 플레이어의 SpriteRenderer 참조

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("character").GetComponent<PlayerMove>();
        playerSprite = player.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(gameManager.stageIndex > 16)
        {
            if (isGravity)
            {
                if (Input.GetKeyUp(KeyCode.Space))
                {
                    player.PlaySound("GRAVITY");
                    isGravityReversed = !isGravityReversed;
                    isGravity = false;
                    playerSprite.flipY = !playerSprite.flipY;
                    if (isGravityReversed)
                    {
                        player.rigid.gravityScale = -0.5f;
                    }
                    else
                    {
                        player.rigid.gravityScale = 0.5f;
                    }

                }
            }
            
        }
        else
        {
            player.rigid.gravityScale = 1;
        }
    }
    void FixedUpdate()
    {
        //중력 상태에 따라 레이캐스트 방향 결정.
        Vector3 raycastDirection = Vector3.down; //기본값은 아래쪽
        if (isGravityReversed)
        {
            raycastDirection = Vector3.up; //중력이 반전되면 위쪽.
        }

        // 플레이어가 땅으로 "떨어지는" 방향으로 움직이는지 확인하기 위한 속도 방향을 결정합니다.
        float checkVelocity = player.rigid.velocity.y;
        if (isGravityReversed)
        {
            // 중력이 반전된 경우, velocity.y가 양수일 때 플레이어는 "아래로" 움직이는 것입니다.
            checkVelocity *= -1;
        }

        //착지여부 확인
        if (checkVelocity < 0)
        {
            RaycastHit2D rayHit = Physics2D.BoxCast(player.coll.bounds.center, player.coll.bounds.size, 0f, raycastDirection, 1f, LayerMask.GetMask("Platform"));

            if (rayHit.collider == null)
            {
                isGravity = false; 
            }
            else
            {
                isGravity = true; 
            }
        }
    }
}
