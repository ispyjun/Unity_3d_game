using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool isTouchTop;
    public bool isTouchBottom;
    public bool isTouchRight;
    public bool isTouchLeft;

    public AudioClip audioBoom;
    public AudioClip audioAttack;
    public AudioClip audioDamaged;
    public AudioClip audioItem;

    public int life;
    public int score;

    public float speed;
    public int power;
    public int maxPower;
    public int boom;
    public int maxBoom;
    public float maxShotDelay;
    public float curShotDelay;

    public GameObject bulletObjA;
    public GameObject bulletObjB;
    public GameObject boomEffect;
    public GameManager gameManager;
    public ObjectManager objectManager;

    public bool[] joyControl;
    public bool isControl;
    public bool isButtonA;
    public bool isButtonB;
    public bool isHit;
    public bool isBoomTime;

    Animator anim;

    public GameObject[] followers;
    public bool isRespawnTime;
    SpriteRenderer spriteRenderer;

    AudioSource audioSource;


    void Awake()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        Unbeatable();
        Invoke("Unbeatable", 1.5f);
    }

    void Unbeatable()
    {
        isRespawnTime = !isRespawnTime;
        if (isRespawnTime)
        {
            spriteRenderer.color = new Color(1, 1, 1, 0.5f);
            for(int i=0;i< followers.Length; i++)
            {
                followers[i].GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);
            }
        }
        else
        {
            spriteRenderer.color = new Color(1, 1, 1, 1);
            for (int i = 0; i < followers.Length; i++)
            {
                followers[i].GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
            }
        }
    }

    void Update()
    {
        Move();
        Fire();
        Reload();
        Boom();
    }

    public void JoyPanel(int type)
    {
        for(int i = 0; i < 9; i++)
        {
            joyControl[i] = i == type;
        }
    }

    public void JoyDown()
    {
        isControl = true;
    }

    public void JoyUp()
    {
        isControl = false;
    }

    void Move()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (joyControl[0]) { h = -1; v = 1; }
        if (joyControl[1]) { h = 0; v = 1; }
        if (joyControl[2]) { h = 1; v = 1; }
        if (joyControl[3]) { h = -1; v = 0; }
        if (joyControl[4]) { h = 0; v = 0; }
        if (joyControl[5]) { h = 1; v = 0; }
        if (joyControl[6]) { h = -1; v = -1; }
        if (joyControl[7]) { h = 0; v = -1; }
        if (joyControl[8]) { h = 1; v = -1; }

        if ((isTouchRight && h == 1) || (isTouchLeft && h == -1) || !isControl)
            h = 0;
        
        if ((isTouchTop && v == 1) || (isTouchBottom && v == -1) || !isControl)
            v = 0;
        Vector3 curPos = transform.position;
        Vector3 nextPos = new Vector3(h, v, 0) * speed * Time.deltaTime;

        transform.position = curPos + nextPos;

        //애니메이션
        if (Input.GetButtonDown("Horizontal") || Input.GetButtonUp("Horizontal"))
        {
            anim.SetInteger("Input", (int)h);
        }
    }

    public void ButtonADown()
    {
        isButtonA = true;
    }
    public void ButtonAUp()
    {
        isButtonA = false;
    }
    public void ButtonBDown()
    {
        isButtonB = true;
    }
    public void ButtonBUp()
    {
        isButtonB = false;
    }

    void Fire()
    {
        //if (!Input.GetButton("Fire1"))
        //   return;

        if (!isButtonA)
            return;

        if (curShotDelay < maxShotDelay)
            return;

        switch (power)
        {
            //Power one
            case 1:
                GameObject bullet = objectManager.MakeObj("bulletPlayerA"); 
                bullet.transform.position = transform.position;

                Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
                rigid.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                break;
            case 2:
                GameObject bulletR = objectManager.MakeObj("bulletPlayerA");
                bulletR.transform.position = transform.position + Vector3.right * 0.1f;
                GameObject bulletL = objectManager.MakeObj("bulletPlayerA");
                bulletL.transform.position = transform.position + Vector3.left * 0.1f;

                Rigidbody2D rigidR = bulletR.GetComponent<Rigidbody2D>();
                Rigidbody2D rigidL = bulletL.GetComponent<Rigidbody2D>();
                rigidR.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                rigidL.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                break;
            default:
                GameObject bulletRR = objectManager.MakeObj("bulletPlayerA");
                bulletRR.transform.position = transform.position + Vector3.right * 0.25f;
                GameObject bulletLL = objectManager.MakeObj("bulletPlayerA");
                bulletLL.transform.position = transform.position + Vector3.left * 0.25f;
                GameObject bulletCC = objectManager.MakeObj("bulletPlayerB");
                bulletCC.transform.position = transform.position;

                Rigidbody2D rigidRR = bulletRR.GetComponent<Rigidbody2D>();
                Rigidbody2D rigidLL = bulletLL.GetComponent<Rigidbody2D>();
                Rigidbody2D rigidCC = bulletCC.GetComponent<Rigidbody2D>();
                rigidRR.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                rigidLL.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                rigidCC.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                break;
        }
        
        curShotDelay = 0;
        PlaySound("ATTACK");
    }

    void Reload()
    {
        curShotDelay += Time.deltaTime;
    }

    public void Boom()
    {
        //if (!Input.GetButton("Fire2")){
        //    return;
        //}

        if (!isButtonB)
            return;

        if (isBoomTime)
        {
            return;
        }

        if (boom == 0)
            return;

        boom--;
        isBoomTime = true;
        gameManager.UpdateBoomIcon(boom);

        boomEffect.SetActive(true);
        Invoke("OffBoomEffect", 1f);

        GameObject[] enemiesS = objectManager.GetPool("enemyS");
        GameObject[] enemiesM = objectManager.GetPool("enemyM");
        GameObject[] enemiesL = objectManager.GetPool("enemyL");
        GameObject[] enemiesB = objectManager.GetPool("boss");

        for (int index = 0; index < enemiesS.Length; index++)
        {
            if (enemiesS[index].activeSelf)
            {
                Enemy enemyLogic = enemiesS[index].GetComponent<Enemy>();
                enemyLogic.OnHit(30);
            }
        }
        for (int index = 0; index < enemiesM.Length; index++)
        {
            if (enemiesM[index].activeSelf)
            {
                Enemy enemyLogic = enemiesM[index].GetComponent<Enemy>();
                enemyLogic.OnHit(30);
            }
        }
        for (int index = 0; index < enemiesL.Length; index++)
        {
            if (enemiesL[index].activeSelf)
            {
                Enemy enemyLogic = enemiesL[index].GetComponent<Enemy>();
                enemyLogic.OnHit(30);
            }
        }
        for (int index = 0; index < enemiesB.Length; index++)
        {
            if (enemiesB[index].activeSelf)
            {
                Enemy enemyLogic = enemiesB[index].GetComponent<Enemy>();
                enemyLogic.OnHit(30);
            }
        }

        GameObject[] bulletsA = objectManager.GetPool("bulletEnemyA");
        GameObject[] bulletsB = objectManager.GetPool("bulletEnemyB");
        GameObject[] bulletsBA = objectManager.GetPool("bulletBossA");
        GameObject[] bulletsBB = objectManager.GetPool("bulletBossB");

        for (int index = 0; index < bulletsA.Length; index++)
        {
            if (bulletsA[index].activeSelf)
            {
                bulletsA[index].SetActive(false);
            }
        }
        for (int index = 0; index < bulletsB.Length; index++)
        {
            if (bulletsB[index].activeSelf)
            {
                bulletsB[index].SetActive(false);
            }
        }
        for (int index = 0; index < bulletsBA.Length; index++)
        {
            if (bulletsBA[index].activeSelf)
            {
                bulletsBA[index].SetActive(false);
            }
        }
        for (int index = 0; index < bulletsBB.Length; index++)
        {
            if (bulletsBB[index].activeSelf)
            {
                bulletsBB[index].SetActive(false);
            }
        }

        PlaySound("BOOM");
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Border")
        {
            switch (collision.gameObject.name)
            {
                case "Top":
                    isTouchTop = true;
                    break;
                case "Bottom":
                    isTouchBottom = true;
                    break;
                case "Right":
                    isTouchRight = true;
                    break;
                case "Left":
                    isTouchLeft = true;
                    break;
            }
        }
        else if(collision.gameObject.tag == "EnemyBullet" || collision.gameObject.tag == "Enemy" )
        {
            if (isRespawnTime)
                return;

            if (isHit)
                return;

            isHit = true;
            life--;
            PlaySound("DAMAGED");
            gameManager.UpdateLifeIcon(life);
            gameManager.CallExplosion(transform.position, "P");

            if (life == 0)
            {
                PlaySound("OVER");
                gameManager.GameOver();
                
            }
            else
            {
                power = 1;
                boom = 2;
                gameManager.UpdateBoomIcon(boom);
                gameManager.RespawnPlayer();
                
            }
            gameObject.SetActive(false);
            followers[0].SetActive(false);
            followers[1].SetActive(false);

            if (collision.gameObject.tag == "Enemy")
            {
                GameObject bossGo = collision.gameObject;
                Enemy enemyBoss = bossGo.GetComponent<Enemy>();
                if (enemyBoss.enemyName == "B")
                {
                    return;
                }
                else
                {
                    collision.gameObject.SetActive(false);
                }
            }

            //Destroy(collision.gameObject);
        }
        else if (collision.gameObject.tag == "Item")
        {
            Item item = collision.gameObject.GetComponent<Item>();
            switch (item.type)
            {
                case "Coin":
                    score += 500;
                    break;
                case "Power":
                    if (power == maxPower)
                        score += 1200;
                    else
                    {
                        score += 200;
                        power++;
                        AddFollower();
                    }
                    break;
                case "Boom":
                    if (boom == maxBoom)
                        score += 1000;
                    else
                    {
                        score += 200;
                        boom++;
                        gameManager.UpdateBoomIcon(boom);
                    }
                    break;
            }
            //gameObject.SetActive(false);
            collision.gameObject.SetActive(false);
            PlaySound("ITEM");
            //Destroy(collision.gameObject);
        }
    }

    void AddFollower()
    {
        if (power == 4)
            followers[0].SetActive(true);
        else if (power == 5)
            followers[1].SetActive(true);
    }
    void OffBoomEffect()
    {
        boomEffect.SetActive(false);
        isBoomTime = false;
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Border")
        {
            switch (collision.gameObject.name)
            {
                case "Top":
                    isTouchTop = false;
                    break;
                case "Bottom":
                    isTouchBottom = false;
                    break;
                case "Right":
                    isTouchRight = false;
                    break;
                case "Left":
                    isTouchLeft = false;
                    break;
            }
        }
    }

    void PlaySound(string action)
    {
        switch (action)
        {
            case "ATTACK":
                audioSource.clip = audioAttack;
                break;
            case "DAMAGED":
                audioSource.clip = audioDamaged;
                break;
            case "BOOM":
                audioSource.clip = audioBoom;
                break;
            case "ITEM":
                audioSource.clip = audioItem;
                break;
        }
        audioSource.Play();
    }
}
