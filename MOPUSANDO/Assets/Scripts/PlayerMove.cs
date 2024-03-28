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

    public float MaxSpeed;
    public float jumpPower;
    public bool isDoubleJumping = false;
    public Coroutine boostingCoroutine;
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    CapsuleCollider2D coll;
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
        }
    }
    void Update()
    {
        if(gameManager.stageIndex < 10) //11스테이지부터는 자동점프이므로 점프가 필요없음
        {
            if (Input.GetButtonDown("Jump") && !anim.GetBool("isJumping")) //Jump
            {
                rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
                anim.SetBool("isJumping", true);
                PlaySound("JUMP");
            }
            else if (isDoubleJumping && Input.GetButtonDown("Jump")) //DoubleJump
            {
                rigid.AddForce(Vector2.up * jumpPower * 1.2f, ForceMode2D.Impulse);
                anim.SetBool("isJumping", true);
                PlaySound("JUMP");
                isDoubleJumping = false;
            }
        }
        

        //Stop Speed
        if (Input.GetButtonUp("Horizontal")) {
            rigid.velocity = new Vector2(5f * rigid.velocity.normalized.x, rigid.velocity.y); //5f는 나중에 빙판길 미끄럼용
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

        //미끄러지지않게하기
        if(h == 0 && (gameManager.stageIndex < 6 || gameManager.stageIndex > 10)) //7스테이지부터는 얼음땅이라서 프리즈빼기
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
            Debug.DrawRay(rigid.position + Vector2.right * h * 0.5f, Vector3.down, new Color(0, 1, 0));
            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position + Vector2.right * h * 0.5f , Vector3.down, 1, LayerMask.GetMask("Platform"));
            if (rayHit.collider != null)
            {
                if (rayHit.distance < 1f)
                {
                    anim.SetBool("isJumping", false);
                    isDoubleJumping = false;
                    if(gameManager.stageIndex >= 11 && rayHit.distance < 0.5f) //11스테이지 자동점프 설정
                    {
                        Jump();
                    }
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
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Item")
        {
            // Point
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
                    StopCoroutine(boostingCoroutine); //이전에있던 코루틴삭제
                }
                boostingCoroutine = StartCoroutine(gameManager.Boosting(collision));
            }

            //Deactive Item
            collision.gameObject.SetActive(false);

            //더블점프,부스터 일정시간뒤 재생성
            if (!collision.gameObject.activeSelf && collision.gameObject.name.Contains("DoubleJump") || collision.gameObject.name.Contains("Booster"))
            {
                //오브젝트가 비활성화될 때 실행되는 메서드
                StartCoroutine(gameManager.RespawnAfterDelay(collision));
            }

            //Sound
            PlaySound("ITEM");
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
        else if(gameManager.stageIndex >= 6 || gameManager.stageIndex <= 10) //빙판길
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

        //레이어 체인지
        gameObject.layer = 9;

        //투명도 조절
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);

        //Reaction Force
        if(gameManager.stageIndex < 6 || gameManager.stageIndex > 10)
        {
            int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
            rigid.AddForce(new Vector2(dirc, 1) * 7, ForceMode2D.Impulse);
        }
        else if(gameManager.stageIndex >= 6 || gameManager.stageIndex <= 10) //빙판길
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
