using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour, BeObserver
{
    private CharacterController characterController;
    public FolowCamera followCamera;
    public ArrowShoot m_ArrowShoot;

    private Animator m_Am;
    private PlayerInput m_Input; //準備獲取玩家輸入

    private static float playerHp = 300;//玩家生命
    private static float playerMaxHp = 300;//玩家生命最大值
    private static float playerMp = 100;//玩家MP
    private static float playerMaxMp = 100;//玩家MP最大值

    private float rotateSpeed = 10.0f;//轉向速度
    private float speed = 6.0f;//移動速度
    private float gravity = 20.0f;//重力
    private float rollSpeed = 15.0f;//翻滾速度
    private float statetime;//動畫進行時間(百分比)
    private float fallSpeed;//角色落下速度
    private float mouseSlide;//滑鼠滑動輸入
    private float normalMove;//一般狀態下的WASD輸入總和
    private float bowRightMove;//弓狀態下的WASD輸入總和
    private float lockDistancs = 15.0f;//一般攻擊自動鎖定距離
    private float attackMoveSpeed;//攻擊時的移動速度
    private float mpPlusTime;//自動回魔時間

    private List<FSMBase> monster;//存取怪物資訊

    private int sensitivity=12;//弓狀態下的滑鼠控制相機靈敏度

    ///effect
    public GameObject chargeEffect;
    private ParticleSystem chargeSystem;
    public GameObject novaEffect;
    private ParticleSystem novaSystem;
    public GameObject magicCircleEffect;
    private ParticleSystem magicCircleSystem;
    
    private bool isCharge=false;

    ///Audio
    public GameObject chargeAudio;
    private AudioSource chargeAudioSource;
    private float chargeAudioTime=0f;
    public GameObject RunAudio;
    private AudioSource runAudioSource;
    private float runAudioTime;

    AnimatorStateInfo stateinfo;//當前Animation存取
    AnimatorStateInfo nextStateinfo;//下個Animation存取
    AnimatorStateInfo nextStateinfoOne;//第1層的下個Animation存取

    readonly int hashAttack01 = Animator.StringToHash("attack01");
    readonly int hashAttack02 = Animator.StringToHash("attack02");
    readonly int hashAttack03 = Animator.StringToHash("attack03");
    readonly int hashAttack04 = Animator.StringToHash("attack04");
    readonly int hashSpecialAttack=Animator.StringToHash("specialAttack");
    readonly int hashSpecialAttack1_1 = Animator.StringToHash("specialAttack1_1");
    readonly int hashSpecialAttack1_2 = Animator.StringToHash("specialAttack1_2");
    readonly int hashSpecialAttack2_1 = Animator.StringToHash("specialAttack2_1");
    readonly int hashSpecialAttack2_2 = Animator.StringToHash("specialAttack2_2");
    readonly int hashSpecialAttack2_3 = Animator.StringToHash("specialAttack2_3");
    readonly int hashBattleRoll=Animator.StringToHash("BattleRoll");
    readonly int hashRoll = Animator.StringToHash("Roll");
    readonly int hashBattleIdle= Animator.StringToHash("BattleIdle");
    readonly int hashIdle = Animator.StringToHash("Idle");
    readonly int m_StateTime = Animator.StringToHash("StateTime");
    readonly int m_ChargeTime = Animator.StringToHash("ChargeTime");
    readonly int hashBowIdle = Animator.StringToHash("BowIdle");
    readonly int hashBowShoot = Animator.StringToHash("BowShoot");
    readonly int hashHurt = Animator.StringToHash("Hurt");
    readonly int hashDead = Animator.StringToHash("death");
    readonly int hashGetup = Animator.StringToHash("getup");
    readonly int hashRun = Animator.StringToHash("Run");
    readonly int hashBattleRun = Animator.StringToHash("BattleRun");
    readonly int hashBowWalk = Animator.StringToHash("BowWalk");

    /// 動畫播放狀態
    private bool attackState;//所有一般攻擊Animation
    private bool battleRollState;//戰鬥翻滾翻滾
    private bool battleRollIsNext;//下個是戰鬥翻滾
    private bool rollState;//一般翻滾
    private bool rollIsNext;//下個是一般翻滾
    private bool battleIdleIsNext;//下個Animation是battleIdle
    private bool idleIsNext;//下個是Animation是Idle
    private bool idleState;//Idle動作
    private bool isTrasition;//混接中
    private bool bowIsNext;//下個Animation是弓
    private bool bowShoot;//射擊動作
    private bool hurt;//受傷動作
    private bool hurtIsNext;//下個是受傷動作
    private bool runIsNext;//一般跑步
    private bool runState;//一般跑步
    private bool battleRunIsNext;//戰鬥跑步
    private bool battleRunState;//戰鬥跑步
    private bool bowWalk;//拉弓移動

    ///玩家技能啟用狀態
    private bool explodeArrowLock = true;
    private bool swordSkillLock = true;

    [HideInInspector] public bool dead;//死亡動畫
    [HideInInspector] public bool getup;//起身動畫

    /// 按鍵選擇布林值
    [HideInInspector] public bool relife=false;

    Vector3 move = Vector3.zero;//角色總移動量
    Vector3 targetVector;//自動鎖定的方向    

    Vector2 moveInput;//存取按鍵WASD，主要用在轉向，不太需要管Input.GetAxis的數值變化
    Vector2 runInput;//存取WASD，需要Input.GetAxis的數值變化來用在blend tree
    HashSet<FSMBase> reliveObserver = new HashSet<FSMBase>();//負責復活的觀察者
    public Vector3 currentCheckPoint;
    float charge;
    public enum PlayerState
    {
        live=0,
        dead=1,
    }
    public PlayerState playerCurrnetState;
    void Start()
    {
        Cursor.visible=false;//關掉鼠標
        characterController = GetComponent<CharacterController>();    
       
        m_Am = GetComponent<Animator>();
        m_Input = GetComponent<PlayerInput>();

        monster = new List<FSMBase>();
        GameObject[] allMonster =GameManager.Instance.allMonster;//將場景裡tag為Monster的物件存起來
        if(allMonster!=null || allMonster.Length>0)
        {           
            foreach(GameObject m in allMonster)
            {
                monster.Add(m.GetComponent<FSMBase>());
            }
        }
        playerCurrnetState = PlayerState.live;
        PlayerInput.Instance.playerCurrnetState = PlayerState.live;
        currentCheckPoint = new Vector3(152f,23f,-118f);

        chargeSystem = chargeEffect.GetComponent<ParticleSystem>();
        novaSystem = novaEffect.GetComponent<ParticleSystem>();
        magicCircleSystem = magicCircleEffect.GetComponent<ParticleSystem>();

        chargeAudioSource = chargeAudio.GetComponent<AudioSource>();
        runAudioSource = RunAudio.GetComponent<AudioSource>();
        
    }
void Update()
    {
        if(Input.GetKeyDown("f1"))
        {
            PlayerInput.Instance.BowUnlock();
            UnlockSkill(1);
        }
        if (Input.GetKeyDown("f2"))
        {
            UnlockSkill(2);
        }
        if (Input.GetKeyDown("f4"))
        {
            UnlockSkill(3);
        }
        if (Input.GetKeyDown("f3"))
        {
            PlayerHurt(100);
        }

        BowAngle();
        TargetSearch();
        Alert();
       
        moveInput = PlayerInput.Instance.MoveInput;
        runInput = PlayerInput.Instance.MoveInput;
    }
    void FixedUpdate()
    {
        stateinfo = m_Am.GetCurrentAnimatorStateInfo(0);
        nextStateinfo = m_Am.GetNextAnimatorStateInfo(0);
        isTrasition = m_Am.IsInTransition(0);
        nextStateinfoOne= m_Am.GetNextAnimatorStateInfo(1);

        Sword.Instance.isTransition = isTrasition;

        m_Am.SetFloat(m_StateTime, Mathf.Repeat(m_Am.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f));//讓statetime不斷從0數到1
        statetime = m_Am.GetFloat("StateTime");

        CalculateGravity();
        GetAttackState();
        GetCurrentState();
        GetNextState();

        if (playerCurrnetState == PlayerState.live)//自動回復MP
        {
            AutoMpPlus();
        }

        if (m_Input.bowState)   //弓狀態與一般狀態的基本參數改變
        {
            BowBasicValue();
        }
        else
        {
            NormalBasicValue();
        }
        ResetTrigger();
        CantRollToBow();        //弓模式旗標
        if (m_Input.bowState && !attackState)
        {
            m_Am.SetBool("BowBool",true);
        }
        else
        {
            m_Am.SetBool("BowBool", false);
            BowEffectReset();
        }

        if (m_Input.moveFlagH || m_Input.moveFlagV)       //移動旗標
        {
            if(statetime<=0.4f)
                ResetAttackTrigger();

            m_Am.SetBool("RunBool", true);
        }
        else
        {
            moveInput = Vector2.zero;
            m_Am.SetBool("RunBool", false);
        }

        if (m_Input.avoid && !bowShoot && !m_Input.bowAttack)      //迴避
        {   
            ResetAttackTrigger();          

            m_Am.SetTrigger("AvoidTrigger");

            charge = 0f;
            PlayerInput.Instance.bowState = false;
            m_Input.avoid = false;
        }
        if (m_Input.attack)   //左鍵攻擊
        {
            m_Am.ResetTrigger("SpecialAttackTrigger");
            m_Am.SetTrigger("AttackTrigger");
            m_Input.attack = false;
        }
        if(m_Input.specialAttack)   //右鍵攻擊
        {
            
            m_Am.ResetTrigger("AttackTrigger");
            m_Am.SetTrigger("SpecialAttackTrigger");
            
            m_Input.specialAttack = false;
        }
        if (m_Input.bowAttack)  //弓左鍵射擊
        {
            chargeAudioSource.Stop();
            magicCircleEffect.SetActive(false);
            m_Am.ResetTrigger("AttackTrigger");

            chargeSystem.Stop();
            if (charge >= 1.5f)
            {
                novaSystem.Play();
            }

            m_ArrowShoot.GetCharge(charge, playerMp);
            charge = 0f;
            m_Am.SetTrigger("BowAttackTrigger");
            m_Input.bowAttack = false;
        }
        if (m_Input.bowCharge && m_Input.bowState)
        {
            charge += Time.deltaTime;
            if(chargeSystem.isStopped)
                chargeSystem.Play();

            if (charge >= 1.5f)
            {
                magicCircleEffect.SetActive(true);
            }            

            if (charge > 2.0f)
                charge = 2.0f;
            if ((playerMp < 25) || explodeArrowLock)//玩家魔力不足25，或還未獲得爆炸箭時改用一般箭矢
                charge = 1.0f;
            //重播蓄力音效
            chargeAudioTime += Time.deltaTime;
            if((chargeAudioTime>0.2f) && !chargeAudioSource.isPlaying)
            {
                chargeAudioSource.Play();
                chargeAudioTime = 0f;
            }
            if (chargeAudioTime > 0.75f)
            {
                chargeAudioSource.Play();
                chargeAudioTime = 0f;
            }

            m_Am.SetFloat(m_ChargeTime, charge);
        }

        if ((m_Input.moveFlagH || m_Input.moveFlagV) && !hurt && !attackState && !battleRollState && !battleRollIsNext && !m_Input.bowState)
        {
            Rotating(moveInput.x, moveInput.y);
        }
        RunAudioPlay();
        
    }
    void OnAnimatorMove()
    {
 
        RaycastHit hit;
        Ray ray = new Ray(transform.position + Vector3.up, -Vector3.up);//在林克身上做一條與Y軸平行的雷射用以偵測四周
        if (Physics.Raycast(ray, out hit, 1.0f, Physics.AllLayers))
        {
            move = Vector3.ProjectOnPlane(m_Am.deltaPosition, hit.normal);
        }

        if (!attackState || battleRollIsNext)//讀取按鍵決定方向
            move = followCamera.horizontalVector * moveInput.y + followCamera.cameraRight * moveInput.x;
        else
            move = Vector3.zero;

        if (battleRollState || battleRollIsNext || rollIsNext || rollState)//翻滾時採用翻滾速度
        {
            move = transform.forward * rollSpeed * Time.deltaTime;
        }
        else
            move = Vector3.Normalize(move) * speed * Time.deltaTime;

        if (battleIdleIsNext || idleIsNext || bowIsNext)//轉換到Idle與弓狀態時減速
            move = transform.forward * 0.0f;

        if (hurt && !battleRollIsNext)//受傷時移動量為0
            move = Vector3.zero;

        move += fallSpeed * Vector3.up * Time.deltaTime;//加上落下速度

        if(attackState)
            move += m_Am.deltaPosition;//加上美術位移

        move += transform.forward*attackMoveSpeed*Time.deltaTime;//加上攻擊時的移動速度

        characterController.Move(move);
    }
    /// <summary>
    /// 重力計算
    /// </summary>
    void CalculateGravity()
    {        
        if (characterController.isGrounded)
        {
            fallSpeed = -gravity * 0.3f;
        }
        else
        {
            fallSpeed -= gravity * Time.deltaTime;
        }
    }
    /// <summary>
    /// 一般轉向
    /// </summary>
    /// <param name="moveH"></param>
    /// <param name="moveV"></param>
    void Rotating(float moveH, float moveV)
    {
        // 建立角色目標方向的向量                  
        Vector3 newDirectionVector = followCamera.horizontalVector * moveV + followCamera.cameraRight * moveH;       
        if(newDirectionVector != Vector3.zero)
        {
            Quaternion newRotation = Quaternion.LookRotation(newDirectionVector, Vector3.up);
            characterController.transform.rotation = Quaternion.Lerp(characterController.transform.rotation, newRotation, Time.deltaTime * rotateSpeed);
        }    
    }
    /// <summary>
    /// 以瞬間轉向為主的翻滾轉向
    /// </summary>
    /// <param name="moveH"></param>
    /// <param name="moveV"></param>
    void RollRotating()
    {
        if (moveInput.y > 0)          ///此處的moveV與moveH只需取最大值作為翻滾方向判斷來使用
            moveInput.y = 1;
        else if (moveInput.y < 0)
            moveInput.y = -1;
        if (moveInput.x > 0)
            moveInput.x = 1;
        else if (moveInput.x < 0)
            moveInput.x = -1;

        if (!m_Input.moveFlagH)
            moveInput.x = 0;
        if (!m_Input.moveFlagV)
            moveInput.y = 0;

        Vector3 newDirectionVector = (followCamera.horizontalVector * moveInput.y + followCamera.cameraRight * moveInput.x).normalized;
        if (newDirectionVector != Vector3.zero)
        {
            Quaternion newRotation = Quaternion.LookRotation(newDirectionVector, Vector3.up);
            characterController.transform.rotation = newRotation;
        }
    }
    /// <summary>
    /// 在Animation的event使用
    /// 一般攻擊時的自動轉向
    /// </summary>
    void AttackRotating()
    {
        targetVector.y = 0f;
        Quaternion newRotation= Quaternion.LookRotation(targetVector, Vector3.up);        
        characterController.transform.rotation = newRotation;
    }
    /// <summary>
    /// 將所有一般攻擊狀態取出，來判斷是否在一般攻擊中
    /// </summary>
    void GetAttackState()
    {                                 
        if(stateinfo.shortNameHash == hashAttack01 ||
           stateinfo.shortNameHash == hashAttack02 ||
           stateinfo.shortNameHash == hashAttack03 ||
           stateinfo.shortNameHash == hashAttack04 ||
           stateinfo.shortNameHash == hashSpecialAttack ||
           stateinfo.shortNameHash == hashSpecialAttack1_1 ||
           stateinfo.shortNameHash == hashSpecialAttack1_2 ||
           stateinfo.shortNameHash == hashSpecialAttack2_1 ||
           stateinfo.shortNameHash == hashSpecialAttack2_2 ||
           stateinfo.shortNameHash == hashSpecialAttack2_3 ||
           nextStateinfo.shortNameHash== hashAttack01)
        {
            attackState = true;
        }
        else        
            attackState = false;

        PlayerInput.Instance.attackState = attackState;
        if (nextStateinfo.shortNameHash == hashAttack01)
        {
            Sword.Instance.attackState = true;
        }
        else
        {
            Sword.Instance.attackState = false;
        }
    }
    /// <summary>
    /// 獲取當前Animation
    /// </summary>
    void GetCurrentState()
    {        
        if (stateinfo.shortNameHash == hashBattleRoll)
            battleRollState = true;
        else
            battleRollState = false;

        if (stateinfo.shortNameHash == hashRoll)
            rollState = true;
        else
            rollState = false;

        if (stateinfo.shortNameHash == hashRun)
            runState = true;
        else
            runState = false;

        if (stateinfo.shortNameHash == hashBattleRun)
            battleRunState = true;
        else
            battleRunState = false;

        if (stateinfo.shortNameHash == hashBowWalk)
            bowWalk = true;
        else
            bowWalk = false;

        if (stateinfo.shortNameHash == hashHurt)
            hurt = true;
        else
            hurt = false;

        if (stateinfo.shortNameHash == hashDead)
            dead = true;
        else
            dead = false;

        if (stateinfo.shortNameHash == hashIdle)
            idleState = true;
        else 
            idleState = false;

        //Sword.Instance.runIsNext = runIsNext;
        //Sword.Instance.battleRunIsNext = battleRunIsNext;   
        Sword.Instance.idleState = idleState;
        PlayerInput.Instance.rollState = battleRollState;        
        PlayerInput.Instance.hurt = hurt;
    }    
    /// <summary>
    /// 獲取下個Animation
    /// </summary>
    void GetNextState()
    {
        if (nextStateinfo.shortNameHash == hashBattleRoll)
            battleRollIsNext = true;
        else
            battleRollIsNext = false;

        if (nextStateinfo.shortNameHash == hashRoll)
            rollIsNext = true;
        else
            rollIsNext = false;

        if (nextStateinfo.shortNameHash == hashBattleIdle)
            battleIdleIsNext = true;
        else
            battleIdleIsNext = false;

        if (nextStateinfo.shortNameHash == hashIdle)
            idleIsNext = true;
        else
            idleIsNext = false;

        if (nextStateinfo.shortNameHash == hashBowIdle)
            bowIsNext = true;
        else
            bowIsNext = false;

        if (nextStateinfoOne.shortNameHash == hashBowShoot)
            bowShoot = true;
        else
            bowShoot = false;

        if (nextStateinfo.shortNameHash == hashRun)
            runIsNext = true;
        else
            runIsNext = false;

        if (nextStateinfo.shortNameHash == hashBattleRun)
            battleRunIsNext = true;
        else
            battleRunIsNext = false;

        if (nextStateinfo.shortNameHash == hashHurt)
            hurtIsNext = true;
        else
            hurtIsNext = false;

        PlayerInput.Instance.rollIsNext = battleRollIsNext;
        PlayerInput.Instance.hurtIsNext = hurtIsNext;
        PlayerInput.Instance.bowShoot = bowShoot;
        Sword.Instance.idleIsNext = idleIsNext;
        Sword.Instance.bowIsNext = bowIsNext;        
        Sword.Instance.runIsNext = runIsNext;
        Sword.Instance.battleRunIsNext = battleRunIsNext;
    }
    /// <summary>
    /// 重製迴避觸發
    /// </summary>
    void ResetTrigger()
    {      
        m_Am.ResetTrigger("AvoidTrigger");
        m_Am.ResetTrigger("BowAttackTrigger");
        m_Am.ResetTrigger("BowTrigger");
    }
    /// <summary>
    /// 重製攻擊觸發
    /// </summary>
    void ResetAttackTrigger()
    {
        m_Am.ResetTrigger("AttackTrigger");
        m_Am.ResetTrigger("SpecialAttackTrigger");
    }
    /// <summary>
    /// 控制角色弓狀態下的上下角度
    /// </summary>
    void BowAngle()
    {
        mouseSlide-=PlayerInput.Instance.MouseInput.y*sensitivity;

        if (mouseSlide > 710f)///一般狀態bowangel限制
            mouseSlide = 710f;
        else if (mouseSlide < -240f)
            mouseSlide = -240f;

        if (m_Input.bowState)///弓狀態bowangel限制
        {
            if (mouseSlide > 250f)
                mouseSlide = 250f;
            else if (mouseSlide < -240f)
                mouseSlide = -240f;
        }

        m_Am.SetFloat("BowAngle",mouseSlide+500f);
    }
    /// <summary>
    /// 一般狀態下的基礎參數
    /// </summary>
    void NormalBasicValue()
    {
        normalMove = (Mathf.Abs(runInput.x) + Mathf.Abs(runInput.y)) * 2;
        if (normalMove > 1f)
            normalMove = 1f;
        m_Am.SetFloat("RunBlend", Mathf.Abs(normalMove));

        speed = 10.0f;
    }
    /// <summary>
    /// 弓狀態下的基礎參數
    /// </summary>
    void BowBasicValue()
    {
        normalMove = runInput.y;
        bowRightMove = runInput.x;

        m_Am.SetFloat("BowTotalMoveInput",Mathf.Abs(normalMove + bowRightMove));
        m_Am.SetFloat("RunBlend", normalMove);
        m_Am.SetFloat("RightRunBlend", bowRightMove);
        
        speed = 4.0f;
    }
    /// <summary>
    /// 時翻滾不能馬上切換到弓狀態
    /// </summary>
    void CantRollToBow()
    {
        if(statetime<0.5f && battleRollState)
            PlayerInput.Instance.rollToBow = false;
        else
            PlayerInput.Instance.rollToBow = true;
    }
    /// <summary>
    /// 尋找距離小於3.0f的最短距離目標
    /// </summary>
    void TargetSearch()
    {
        Vector3 vec;
        Vector3 lastVec=transform.forward;
        float last = 100.0f;//比鎖定距離還長的隨意數值用來給迴圈的第一圈比較用
        for (int i = 0; i < monster.Count; i++)
        {
            vec = monster[i].transform.position - transform.position; //獲得鎖定的方向
            if (vec.magnitude >= lockDistancs || (monster[i].currentState == FSMState.Dead))
            {               
                continue;
            }
            if (vec.magnitude < last)
            {
                lastVec = vec;
                last = vec.magnitude;
            }
        }
        targetVector = lastVec;

    }
    /// <summary>
    /// 怪物靠近時進入戰鬥狀態
    /// </summary>
    void Alert()
    {
        float dis;
        for (int i = 0; i < monster.Count; i++)
        {
            if(monster[i].currentState == FSMState.Dead)
            {
                continue;
            }
            dis = (monster[i].transform.position - transform.position).magnitude;
            if(dis<25f)
            {
                m_Am.SetBool("BattleBool", true);
                return;
            }
            m_Am.SetBool("BattleBool", false);
        }
    }
    /// <summary>
    /// 玩家受傷
    /// </summary>
    public void PlayerHurt(int damage)
    {
        if (playerCurrnetState == PlayerState.dead)
            return;
        if (battleRollState || battleRollIsNext)
            return;

        BowEffectReset();

        AttackMoveStop();
        charge = 0f;

        HpReduce(damage);
        if (playerHp <= 0)
        {
            playerStateChange = PlayerState.dead;
            PlayerInput.Instance.playerCurrnetState= PlayerState.dead;
            m_Am.SetTrigger("dead");
        }
        else
        {
            m_Am.SetTrigger("HurtTrigger");
        }        

        PlayerInput.Instance.bowState = false;
    }
    public void HpReduce(int damage)
    {
        playerHp -= damage;
        if (playerHp >= playerMaxHp)
            playerHp = playerMaxHp;
        if(playerHp <= 0)
            playerHp = 0;

        UIMain.Instance().UpdateHpBar(playerHp / playerMaxHp);
    }
    static public void HpIncrease(float heal)
    {
        playerHp += heal;
        if (playerHp >= playerMaxHp)
            playerHp = playerMaxHp;

        UIMain.Instance().UpdateHpBar(playerHp / playerMaxHp);        
        GameManager.Instance.PlayParticleSystem(GameManager.Instance.healHpEffect);
    }
    public void MpReduce(int cost)
    {
        playerMp -= cost;
        if (playerMp >= playerMaxMp)
            playerMp = playerMaxMp;
        if (playerMp <= 0)
            playerMp = 0;

        UIMain.Instance().UpdateMpBar(playerMp / playerMaxMp);
    }
    static public void MpIncrease(float mana)
    {
        playerMp += mana;
        if (playerMp >= playerMaxMp)
            playerMp = playerMaxMp;

        UIMain.Instance().UpdateMpBar(playerMp / playerMaxMp);
        //GameManager.Instance.PlayParticleSystem(GameManager.Instance.healMpEffect);
    }
    void AutoMpPlus()
    {
        mpPlusTime += Time.deltaTime;

        if(mpPlusTime>=1.0f)
        {
            MpIncrease(2.0f);
            mpPlusTime = 0f;
        }
    }
    /// <summary>
    /// 開始攻擊中移動
    /// 給event使用
    /// </summary>
    void AttackMove()
    {
        attackMoveSpeed = 5f;
    }
    /// <summary>
    /// 結束攻擊中移動
    /// </summary>
    public void AttackMoveStop()
    {
        attackMoveSpeed = 0f;
    }
    /// <summary>
    /// 儲存要給誰觀察
    /// </summary>
    /// <param name="ob"></param>
    public void Subscribe(FSMBase ob)
    {
        reliveObserver.Add(ob);
    }
    /// <summary>
    /// playerCurrnetState發生改變時要做的行動
    /// </summary>
    public void NotifyDead()
    {
        foreach(FSMBase m in reliveObserver) 
        {
            m.PlayerIsDead();
        }   
        WorldEvManager.Instance.PlayerDead();

    }
    public void NotifyLife()
    {
        foreach(FSMBase m in reliveObserver) 
        {
            m.PlayerIsReLife();
        }   
    }
    /// <summary>
    /// 玩家生死狀態設定
    /// </summary>
    public PlayerState playerStateChange
    {
        set  
        {
            playerCurrnetState = value;
            if(playerCurrnetState == PlayerState.dead)
            {
                NotifyDead();        
            }  
            if(playerCurrnetState == PlayerState.live)
            {
                NotifyLife();        
            }             
        }
        get { return playerCurrnetState; }
    }
    /// <summary>
    /// 接觸記錄點時設定復活點
    /// </summary>
    /// <param name="point"></param>
    public void SetCheckPoint(CheckPoint point)
    {
        if(point!=null)
            currentCheckPoint = point.transform.position;       
    }
    /// <summary>
    /// 給外部用的啟動復活流程
    /// </summary>
    public void StarRelive()
    {
        StartCoroutine(ShowScreen());
    }
    /// <summary>
    /// 玩家復活流程
    /// </summary>
    /// <returns></returns>
    protected IEnumerator ShowScreen()
    {
        while (stateinfo.shortNameHash != hashDead || !isTrasition)
        {
            yield return null;
        }
        yield return StartCoroutine(GameOverUI.ScreenFadeOut(GameOverUI.FadeType.GameOver));
        while (GameOverUI.Instance.m_IsFading)
        {
            yield return null;
        }

        if (currentCheckPoint != null)
        {
            transform.position = currentCheckPoint;
        }
        else
        {
            Debug.LogError("There is no CheckPoint set");
        }
        m_Am.SetTrigger("getup");        
    }
    protected IEnumerator PlayerReliveRoutine()
    {      
        playerHp = playerMaxHp;
        UIMain.Instance().UpdateHpBar(playerHp / playerMaxHp);

        yield return StartCoroutine(GameOverUI.ScreenFadeIn(GameOverUI.FadeType.GameOver));
        while (GameOverUI.Instance.m_IsFading)
        {
            yield return null;
        }

        playerStateChange = PlayerState.live;
        PlayerInput.Instance.playerCurrnetState = PlayerState.live;
        
        yield return null;
    }
    /// <summary>
    /// 玩家復活按鈕
    /// </summary>
    public void RelifeButton()
    {
        StartCoroutine(PlayerReliveRoutine());
    }
    public void UnlockSkill(int number)
    {
        switch(number)
        {
            case 1:
                explodeArrowLock = false;
                StartCoroutine(UIMain.Instance().ExplodeArrowUnlock());
                break;
            case 2:
                m_Am.SetBool("SwordSkillLock01",true);
                StartCoroutine(UIMain.Instance().SwordSkillUnLock01());
                break;
            case 3:
                m_Am.SetBool("SwordSkillLock02", true);
                StartCoroutine(UIMain.Instance().SwordSkillUnLock02());
                break;
        }
        
    }
    void BowEffectReset()
    {
        charge = 0f;
        magicCircleEffect.SetActive(false);
        chargeAudioSource.Stop();
    }
    void RunAudioPlay()
    {
        ///一般移動腳步聲
        if (runState)
        {
            runAudioTime += Time.deltaTime;
            if (!runAudioSource.isPlaying)
            {
                runAudioSource.Play();
            }
            if (runAudioTime > 0.27f)
            {
                runAudioSource.Play();
                runAudioTime = 0f;
            }
        }
        ///戰鬥移動腳步聲
        if(battleRunState)
        {
            runAudioTime += Time.deltaTime;
            if (!runAudioSource.isPlaying)
            {
                runAudioSource.Play();
            }
            if (runAudioTime > 0.4f)
            {
                runAudioSource.Play();
                runAudioTime = 0f;
            }
        }
        ///拉弓移動腳步聲
        if (bowWalk)
        {
            runAudioTime += Time.deltaTime;
            if (!runAudioSource.isPlaying)
            {
                runAudioSource.Play();
            }
            if (runAudioTime > 0.4f)
            {
                runAudioSource.Play();
                runAudioTime = 0f;
            }
        }
    }
}
