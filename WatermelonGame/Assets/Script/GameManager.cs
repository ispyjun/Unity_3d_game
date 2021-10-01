using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject donglePrefab;
    public Transform dongleGroup;
    public List<Dongle> donglePool;

    public GameObject effectPrefab;
    public Transform effectGroup;
    public List<ParticleSystem> effectPool;

    [Range(1, 30)]
    public int poolSize;
    public int poolCursor;
    public Dongle lastDongle;

    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip;


    public GameObject startGroup;
    public GameObject endGroup;
    public Text scoreText;
    public Text maxScoreText;
    public Text subScoreText;
    public GameObject line;
    public GameObject floor;

    public enum Sfx { LevelUp, Next, Attach, Button, Over};
    int sfxCursor;

    public int score;
    public int maxLevel;
    public bool isOver;


    void Awake()
    {
        //프레임 설정
        Application.targetFrameRate = 60;

        //오브젝트 풀 시작
        donglePool = new List<Dongle>();
        effectPool = new List<ParticleSystem>();
        for (int i = 0;i< poolSize; i++)
        {
            MakeDongle();
        }

        //최고점수 설정
        if (!PlayerPrefs.HasKey("MaxScore"))
        {
            PlayerPrefs.SetInt("MaxScore", 0);
        }
        maxScoreText.text = PlayerPrefs.GetInt("MaxScore").ToString();
    }
    public void GameStart()
    {
        //UI 컨트롤
        startGroup.SetActive(false);
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        line.gameObject.SetActive(true);
        floor.gameObject.SetActive(true);

        //효과음
        SfxPlay(Sfx.Button);
        //BGM 시작
        bgmPlayer.Play();
        // 동글 만들기
        Invoke("NextDongle", 1.5f);
    }

    Dongle MakeDongle()
    {
        //이펙트 생성
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        instantEffectObj.name = "Effect" + effectPool.Count;
        ParticleSystem instanEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instanEffect);

        //동글 생성
        GameObject instantDongleObj = Instantiate(donglePrefab, dongleGroup);
        instantDongleObj.name = "Dongle" + donglePool.Count;
        Dongle instanDongle = instantDongleObj.GetComponent<Dongle>();
        instanDongle.manager = this;
        instanDongle.effect = instanEffect;
        donglePool.Add(instanDongle);

        return instanDongle;
    }

    Dongle GetDongle() 
    {
        for (int i= 0;i< donglePool.Count; i++)
        {
            poolCursor = (poolCursor + 1) % donglePool.Count;
            if (!donglePool[poolCursor].gameObject.activeSelf)
            {
                return donglePool[poolCursor];
            }
        }

        return MakeDongle();
    }

    void NextDongle() 
    {
        if (isOver)
        {
            return;
        }

        lastDongle = GetDongle();
        lastDongle.level = Random.Range(0, maxLevel);
        lastDongle.gameObject.SetActive(true);

        SfxPlay(Sfx.Next);
        StartCoroutine("WaitNext");
    }

    IEnumerator WaitNext()
    {
        while (lastDongle != null)
        {
            yield return null;
        }
        yield return new WaitForSeconds(1.7f);

        NextDongle();
    }

    public void TouchDown() {

        if (lastDongle == null)
            return;
        lastDongle.Drag();
    }

    public void TouchUp() {
        if (lastDongle == null)
            return;

        lastDongle.Drop();
        lastDongle = null;
    }

    public void GameOver()
    {
        if (isOver)
        {
            return;
        }

        isOver = true;
        Debug.Log("게임 오버");
        bgmPlayer.Stop();
        StartCoroutine("GameOverRoutine");
    }

    IEnumerator GameOverRoutine()
    {
        //활성화된 모든 동글 가져오기
        Dongle[] dongles = FindObjectsOfType<Dongle>();

        //물리효과 비활성화
        for (int i = 0; i < dongles.Length; i++)
        {
            dongles[i].rigid.simulated = false;
        }
        // 하나씩 지우기
        for (int i = 0; i < dongles.Length; i++)
        {
            dongles[i].Hide(Vector3.up * 100);
            yield return new WaitForSeconds(0.1f);
        }
        /*for (int i = 0; i < donglePool.Count; i++)
        {
            if (donglePool[i].gameObject.activeSelf)
            {
                dongles[i].Hide(Vector3.up * 100);
                yield return new WaitForSeconds(0.1f);
            }
        }*/

        yield return new WaitForSeconds(1f);

        //점수 출력
        subScoreText.text = "점수 : " + scoreText.text;
        //최대점수 갱신
        int maxScore = Mathf.Max(PlayerPrefs.GetInt("MaxScore"), score);
        PlayerPrefs.SetInt("MaxScore", maxScore);


        //UI 출력
        endGroup.SetActive(true);
        SfxPlay(Sfx.Over);
    }

    public void Reset()
    {
        SfxPlay(Sfx.Button);
        StartCoroutine("ResetRoutine");
    }

    IEnumerator ResetRoutine()
    {
        yield return new WaitForSeconds(1f);

        //장면 다시 불러오기
        SceneManager.LoadScene(0);
    }
    public void SfxPlay(Sfx type) 
    {
        switch (type)
        {
            case Sfx.LevelUp:
                sfxPlayer[sfxCursor].clip = sfxClip[Random.Range(0, 3)];
                break;
            case Sfx.Next:
                sfxPlayer[sfxCursor].clip = sfxClip[3];
                break;
            case Sfx.Attach:
                sfxPlayer[sfxCursor].clip = sfxClip[4];
                break;
            case Sfx.Button:
                sfxPlayer[sfxCursor].clip = sfxClip[5];
                break;
            case Sfx.Over:
                sfxPlayer[sfxCursor].clip = sfxClip[6];
                break;
        }

        sfxPlayer[sfxCursor].Play();
        sfxCursor = (sfxCursor + 1) % sfxPlayer.Length;
    }

    void LateUpdate()
    {
        scoreText.text = score.ToString();
    }
}
