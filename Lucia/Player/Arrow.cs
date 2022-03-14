using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public GameObject explodeArrow;
    public GameObject normalArrow;

    private float arrowSpeed = 50.0f;//�b�ڭ���t��
    private float gravity=3f ;//���O
    private float liveTime=0.0f;//�s�b�ɶ�
    private float fallSpeed=0.0f;//�Y���t��
    private float explodeRadius = 15f;//�z���b�|

    private ParticleSystem normalParticle;
    private ParticleSystem explodeParticle;

    private AudioSource normalAudio;
    private AudioSource explodeAudio;

    private bool explodeFlag=false;//�O�_���z���b

    private List<FSMBase> monster;//�s���Ǫ���T
    private SphereCollider collider;

    public GameObject normalHitEffect;
    public GameObject explodeEffect;

    // Start is called before the first frame update
    private void Awake()
    {

    }
    void Start()
    {
        monster = new List<FSMBase>();
        GameObject[] allMonster = GameManager.Instance.allMonster;//�N������tag��Monster������s�_��
        if (allMonster != null || allMonster.Length > 0)
        {
            foreach (GameObject m in allMonster)
            {
                monster.Add(m.GetComponent<FSMBase>());
            }
        }

        collider = transform.GetComponent<SphereCollider>();

        normalParticle = normalHitEffect.GetComponent<ParticleSystem>();
        normalAudio = normalHitEffect.GetComponent<AudioSource>();
        explodeParticle = explodeEffect.GetComponent<ParticleSystem>();
        explodeAudio = explodeEffect.GetComponent<AudioSource>();
}
    // Update is called once per frame
    void Update()
    {
        liveTime += Time.deltaTime;//�p��s�b�ɶ�
        if (liveTime >= 10f)//�s�b�W�L10���ɮ���
        {
            ArrowDestory();
        }

        transform.position += transform.forward * arrowSpeed * Time.deltaTime;//�b�ک��e����t��
        if (liveTime >= 1f)
        {
            fallSpeed += gravity * Time.deltaTime;//���O�p��
        }
        transform.position-= fallSpeed * Vector3.up*Time.deltaTime;//�b�ڼY���t��

    }
    private void OnTriggerEnter(Collider other)
    {       
            if (explodeFlag)
            {
                //ArrowExplode();
                ExplodeTest();
            }
            else
            {
                ArrowDestory();
            }       
    }
    /// <summary>
    /// �b�ڮ���
    /// </summary>
    void ArrowDestory()
    {
        //liveTime = 0.0f;//��l�Ʀs�b�ɶ�
        //fallSpeed = 0.0f;//��l�ƽb�ڪ��Y���t��
        collider.enabled = false;

        //particleNormal =normalHitEffect.GetComponent<ParticleSystem>();
        StartCoroutine(DestoryTime());
    }
    protected IEnumerator DestoryTime()
    {
        arrowSpeed = 0.0f;
        gravity = 0.0f;

        normalParticle.Play();
        normalAudio.Play();
        while (normalParticle.IsAlive())
        {
            yield return null;
        }

        liveTime = 0.0f;
        fallSpeed = 0.0f;
        arrowSpeed = 40f;
        gravity = 3.0f;
        collider.enabled = true;
        gameObject.SetActive(false);
    }
    public void IsExplode()
    {
        explodeFlag = true;
        transform.name = "ExplosiveArrow";
        normalArrow.SetActive(false);
        explodeArrow.SetActive(true);
    }
    public void IsNormal()
    {
        explodeFlag = false;
        transform.name = "Arrow(Clone)";
        normalArrow.SetActive(true);
        explodeArrow.SetActive(false);
    }
    /// <summary>
    /// trigger���z���b
    /// </summary>
    void ExplodeTest()
    {
        //liveTime = 0.0f;//��l�Ʀs�b�ɶ�
        //fallSpeed = 0.0f;//��l�ƽb�ڪ��Y���t��
        
        StartCoroutine(ExplodeTime());
    }
    protected IEnumerator ExplodeTime()
    {
        explodeArrow.SetActive(false);    
        if(liveTime<0.3f)
        {
            StartCoroutine(FolowCamera.Instance.CameraShake(1.0f, 1.35f));
        }
        else if(liveTime<0.6f)
        {
            StartCoroutine(FolowCamera.Instance.CameraShake(0.5f, 1.35f));
        }
        else if (liveTime < 0.9f)
        {
            StartCoroutine(FolowCamera.Instance.CameraShake(0.25f, 1.35f));
        }

        collider.radius = 9.0f;
        arrowSpeed = 0.0f;
        gravity = 0.0f;
        explodeParticle.Play();
        explodeAudio.Play();
        yield return new WaitForSeconds(0.2f);

        collider.enabled = false;
        while (explodeParticle.IsAlive())
        {
            yield return null;
        }

        liveTime = 0.0f;
        fallSpeed = 0.0f;

        collider.radius = 0.5f;
        arrowSpeed = 40f;
        gravity = 3.0f;
        gameObject.SetActive(false);
        collider.enabled = true;
    }
}
