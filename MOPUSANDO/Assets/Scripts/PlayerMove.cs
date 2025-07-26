using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public GameManager gameManager;
    public AudioClip audioJump;
    public AudioClip audioAttack;
    public AudioClip audioDamaged;
    public AudioClip audioItem;
    public AudioClip audioPortal;
    public AudioClip audioGravity;

    public float MaxSpeed;
    public float jumpPower;
    public bool isDoubleJumping = false;
    public Coroutine boostingCoroutine;
    public Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    public CapsuleCollider2D coll;
    Animator anim;
    AudioSource audioSource;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        coll = GetComponent<CapsuleCollider2D>();
        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound(string action)
    {
        switch (action)
        {
            case "JUMP":
                audioSource.clip = audioJump;
                audioSource.Play();
                break;
            case "ATTACK":
                audioSource.clip = audioAttack;
                audioSource.Play();
                break;
            case "DAMAGED":
                audioSource.clip = audioDamaged;
                audioSource.Play();
                break;
            case "ITEM":
                audioSource.clip = audioItem;
                audioSource.Play();
                break;
            case "PORTAL":
                audioSource.clip = audioPortal;
                audioSource.Play();
                break;
            case "GRAVITY":
                audioSource.clip = audioPortal;
                audioSource.Play();
                break;
        }
    }
    void Update()
    {
        if(gameManager.stageIndex < 11) //11~16���������� �ڵ�����, 17~??���������� �߷¹����̹Ƿ� ������ �ʿ����
        {
            if (Input.GetButtonDown("Jump") && !anim.GetBool("isJumping")) //Jump
            {
                rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
                anim.SetBool("isJumping", true);
                PlaySound("JUMP");
            }
            else if (isDoubleJumping && Input.GetButtonDown("Jump")) //DoubleJump
            {
                rigid.velocity = Vector2.up * jumpPower;
                anim.SetBool("isJumping", true);
                PlaySound("JUMP");
                isDoubleJumping = false;
            }
        }
        
        //Stop Speed
        if (Input.GetButtonUp("Horizontal")) {
            rigid.velocity = new Vector2(5f * rigid.velocity.normalized.x, rigid.velocity.y); //5f�� ���߿� ���Ǳ� �̲�����
        }

        //Direction Sprite
        if (Input.GetButton("Horizontal"))
        {
            spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;
        }

        //Animation
        if(rigid.velocity.normalized.x == 0)
        {
            anim.SetBool("isWalking", false);
        }
        else
        {
            anim.SetBool("isWalking", true);
        }
    }
    void FixedUpdate()
    {
        //Move By Key Control
        float h = Input.GetAxisRaw("Horizontal");
        rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);

        if (rigid.velocity.x > MaxSpeed) //Right Max Speed
        {
            rigid.velocity = new Vector2(MaxSpeed, rigid.velocity.y);
        }
        else if (rigid.velocity.x < MaxSpeed * (-1)) //Left Max Speed
        {
            rigid.velocity = new Vector2(MaxSpeed * (-1), rigid.velocity.y);
        }

        //�̲��������ʰ��ϱ�
        if(h == 0 && (gameManager.stageIndex < 6 || gameManager.stageIndex > 10)) //7�����������ʹ� �������̶� �������
        {
            rigid.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }
        else
        {
            rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        //Landing Platform
        if(rigid.velocity.y < 0)
        {
            RaycastHit2D rayHit = Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector3.down, 1f, LayerMask.GetMask("Platform"));
            if (rayHit.collider != null)
            {
                anim.SetBool("isJumping", false);
                isDoubleJumping = false;
                if(gameManager.stageIndex >= 11 && gameManager.stageIndex <= 16) //11~16�������� �ڵ����� ����
                {
                    Invoke("Jump",0.01f);
                }
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Enemy")
        {
            //Attack
            if(rigid.velocity.y < 0 && transform.position.y > collision.transform.position.y)
            {
                OnAttack(collision.transform);
                PlaySound("ATTACK");
            }
            else //Damaged
            {
                OnDamaged(collision.transform.position);
                PlaySound("DAMAGED");
            }
        }
        if (collision.gameObject.tag == "Spike")
        {
            OnDamaged(collision.transform.position);
            PlaySound("DAMAGED");
        }
        if (collision.gameObject.name.Contains("DisPlatform"))
        {
            StopCoroutine(gameManager.RespawnAfterDelay(coll));
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
            if (collision.gameObject.tag == "Item")
            {
            //Point
            bool isBronze = collision.gameObject.name.Contains("Bronze");
            bool isSilver = collision.gameObject.name.Contains("Silver");
            bool isGold = collision.gameObject.name.Contains("Gold");

                if(isBronze)
                {
                    gameManager.stagePoint += 100;
                }
                else if(isSilver)
                {
                    gameManager.stagePoint += 300;
                } 
                else if (isGold)
                {
                    gameManager.stagePoint += 500;
                }
                else if (collision.gameObject.name.Contains("DoubleJump"))
                {
                    //DoubleJump
                    isDoubleJumping = true;
                    if(gameManager.stageIndex >= 11)
                    {
                        rigid.velocity = Vector2.up * jumpPower;
                    }
                }
                else if (collision.gameObject.name.Contains("Booster"))
                {
                    if (boostingCoroutine != null)
                    {
                        //Booster
                        StopCoroutine(boostingCoroutine); //�������ִ� �ڷ�ƾ����
                    }
                    boostingCoroutine = StartCoroutine(gameManager.Boosting());
                }

            collision.gameObject.SetActive(false);

                //��������,�ν��� �����ð��� �����
                if (!collision.gameObject.activeSelf && collision.gameObject.name.Contains("DoubleJump") || collision.gameObject.name.Contains("Booster"))
                {
                    //������Ʈ�� ��Ȱ��ȭ�� �� ����Ǵ� �޼���
                    StartCoroutine(gameManager.RespawnAfterDelay(collision));
                }
                PlaySound("ITEM");
            }
            else if (collision.gameObject.tag == "Portal")
            {
                PlaySound("PORTAL");
            }
            else if (collision.gameObject.tag == "Finish")
            {
                //Next Stage
                gameManager.NextStage();
            }
    }

    void OnAttack(Transform enemy)
    {
        //Point
        gameManager.stagePoint += 100;

        //Reaction Force
        if (gameManager.stageIndex < 6 || gameManager.stageIndex > 10)
        {
            rigid.AddForce(Vector2.up * 8, ForceMode2D.Impulse);
        }
        else if(gameManager.stageIndex >= 6 && gameManager.stageIndex <= 10) //���Ǳ�
        {
            rigid.AddForce(Vector2.up * 4, ForceMode2D.Impulse);
        }

        //Enemy Die
        EnemyMove enemyMove = enemy.GetComponent<EnemyMove>();
        enemyMove.OnDamaged();
    }

    void OnDamaged(Vector2 targetPos)
    {
        //Health Down
        gameManager.HealthDown();

        //���̾� ü����
        gameObject.layer = 9;

        //���� ����
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);

        //Reaction Force
        if(gameManager.stageIndex < 6 || gameManager.stageIndex > 10)
        {
            int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
            rigid.AddForce(new Vector2(dirc, 1) * 7, ForceMode2D.Impulse);
        }
        else if(gameManager.stageIndex >= 6 && gameManager.stageIndex <= 10) //���Ǳ�
        {
            int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
            rigid.AddForce(new Vector2(dirc, 1) * 3, ForceMode2D.Impulse);
        }
        
        //Animation
        anim.SetTrigger("isDamaged");

        Invoke("OffDamaged", 3);
    }
    void OffDamaged()
    {
        gameObject.layer = 8;
        spriteRenderer.color = new Color(1, 1, 1, 1);
    }

    public void OnDie()
    {
        //Sprite Alpha
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        //Sprite Flip Y
        spriteRenderer.flipY = true;
        //collider Disable
        coll.enabled = false;
        //Die Effect Jump
        rigid.AddForce(Vector2.up * 8, ForceMode2D.Impulse);
    }

    public void VelocityZero()
    {
        rigid.velocity = Vector2.zero;
    }

    public void Jump() 
    {
        rigid.velocity = Vector2.up * jumpPower;
        anim.SetBool("isJumping", true);
        PlaySound("JUMP");
    }
}
