using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int totalPoint;
    public int stagePoint;
    public int stageIndex;
    public int health;
    public PlayerMove player;
    public GameObject[] Stages;

    //UI
    public Image[] UIHealth;
    public TextMeshProUGUI UIPoint;
    public TextMeshProUGUI UIStage;
    public GameObject RestartBtn;

    void Update()
    {
        UIPoint.text = (totalPoint + stagePoint).ToString();
    }

    public void NextStage()
    {
        //Change Stage
        if(stageIndex < Stages.Length-1)
        {
            Stages[stageIndex].SetActive(false);
            stageIndex++;
            Stages[stageIndex].SetActive(true);
            PlayerReposition();

            UIStage.text = "STAGE " + (stageIndex + 1);
        }
        else //Game Clear
        {
            //Player Control Lock
            Time.timeScale = 0;

            //Result UI
            Debug.Log("���� Ŭ����!");

            //Restart Button UI
            TextMeshProUGUI btnText = RestartBtn.GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = "Game Clear!";
            RestartBtn.SetActive(true);
        }

        //Calculate Point
        totalPoint += stagePoint;
        stagePoint = 0;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            if (health > 1)
            {
                //Player Reposition
                PlayerReposition();
            }

            //Health Down
            HealthDown();
            player.PlaySound("DAMAGED");
        }
    }

    void PlayerReposition()
    {
        player.transform.position = new Vector3(-13.5f, 2.5f, -1);
        player.VelocityZero();
    }

    public void HealthDown()
    {
        if (health > 1)
        {
            health--;
            UIHealth[health].color = new Color(1, 0, 0, 0.4f); 
        }
        else
        {
            //All Health UI Off
            UIHealth[0].color = new Color(1, 0, 0, 0.4f);

            //Player Die Effect
            player.OnDie();
            //Retry Button UI
            RestartBtn.SetActive(true);
        }
    }

    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
    public IEnumerator RespawnAfterDelay(Collider2D collision)
    {
        yield return new WaitForSeconds(2f); // ����� ������ ����
        if (!collision.gameObject.activeSelf) // ������Ʈ�� ��Ȱ��ȭ�� �������� Ȯ��
        {
            collision.gameObject.SetActive(true); // ��Ȱ��ȭ�� ���¶�� Ȱ��ȭ
        }
    }
}
