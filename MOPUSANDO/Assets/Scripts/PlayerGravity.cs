using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGravity : MonoBehaviour
{
    public GameManager gameManager;
    public PlayerMove player;
    public bool isGravity = true;
    public bool isGravityReversed = false;
    private SpriteRenderer playerSprite; // �÷��̾��� SpriteRenderer ����

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
        //�߷� ���¿� ���� ����ĳ��Ʈ ���� ����.
        Vector3 raycastDirection = Vector3.down; //�⺻���� �Ʒ���
        if (isGravityReversed)
        {
            raycastDirection = Vector3.up; //�߷��� �����Ǹ� ����.
        }

        // �÷��̾ ������ "��������" �������� �����̴��� Ȯ���ϱ� ���� �ӵ� ������ �����մϴ�.
        float checkVelocity = player.rigid.velocity.y;
        if (isGravityReversed)
        {
            // �߷��� ������ ���, velocity.y�� ����� �� �÷��̾�� "�Ʒ���" �����̴� ���Դϴ�.
            checkVelocity *= -1;
        }

        //�������� Ȯ��
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
