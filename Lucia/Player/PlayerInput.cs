using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public static PlayerInput Instance
    {
        get { return s_Instance; }
    }

    protected static PlayerInput s_Instance;


    private Vector2 m_Movement;//存取WASD輸入
    private Vector2 m_Mouse;//存取滑鼠滑動

    private bool skillWindowIsOpen = false;//技能視窗是否開啟
    public GameObject WindLine;
    public bool isPlayingTimeline = false; 

    ///提供布林值給PlayerControl判斷
    [HideInInspector] public bool moveFlagH = false;
    [HideInInspector] public bool moveFlagV = false;
    [HideInInspector] public bool attack = false;
    [HideInInspector] public bool specialAttack = false;
    [HideInInspector] public bool avoid = false;
    [HideInInspector] public bool isTrasition = false;
    [HideInInspector] public bool bowState = false;
    [HideInInspector] public bool bowTrigger = false;
    [HideInInspector] public bool bowAttack;
    [HideInInspector] public bool bowCharge;
    ///從PlayerControl判斷的布林值
    [HideInInspector] public bool cantBowState;
    [HideInInspector] public bool attackState;
    [HideInInspector] public bool rollState;
    [HideInInspector] public bool rollIsNext;
    [HideInInspector] public bool rollToBow = false;
    [HideInInspector] public bool bowShoot;
    [HideInInspector] public bool hurt;
    [HideInInspector] public bool hurtIsNext;
    [HideInInspector] public PlayerControl.PlayerState playerCurrnetState;

    ///弓箭能力鎖
    private bool bowLock = true;

    public Vector2 MoveInput
    {
        get
        {
            if ((playerCurrnetState == PlayerControl.PlayerState.dead))
                return Vector2.zero;

            return m_Movement;
        }
    }
    public Vector2 MouseInput
    {
        get
        {
            if (playerCurrnetState == PlayerControl.PlayerState.dead)
                return Vector2.zero;

            return m_Mouse;
        }
    }
    private void Awake()
    {
        s_Instance = this;
    }
    void Start()
    {
        
    }
    void Update()
    {
        ///技能視窗
        if (Input.GetButtonDown("SkillWindow"))
        {
            skillWindowIsOpen = UIMain.Instance().OpenSkillWindow();           
            m_Movement = Vector2.zero;
            m_Mouse = Vector2.zero;
        }
        if(skillWindowIsOpen || isPlayingTimeline)
        {
            m_Movement = Vector2.zero;
            m_Mouse = Vector2.zero;
            moveFlagH = false;
            moveFlagV = false;
            return;
        }
        ///微風尋路按鈕
        if(Input.GetButtonDown("WindSeek"))
        {           
            WindLine.GetComponent<WindSeek>().PlayWindSeek();
        }

        ///WASD輸入
        m_Movement.Set(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (Input.GetButton("Horizontal"))
        {
            moveFlagH = true;
        }
        else
        {
            moveFlagH = false;
        }
        if (Input.GetButton("Vertical"))
        {
            moveFlagV = true;
        }
        else
        {
            moveFlagV = false;
        }
        ///滑鼠滑動
        m_Mouse.Set(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        ///劍攻擊
        if (Input.GetButtonDown("Fire1") && !(playerCurrnetState == PlayerControl.PlayerState.dead))
            attack = true;
        if (Input.GetButtonDown("Fire2"))
            specialAttack = true;

        ///弓射擊
        if (Input.GetButtonUp("Fire1") && bowState)
            bowAttack = true;
        if (Input.GetButton("Fire1") && bowState)
            bowCharge = true;
        else
            bowCharge = false;
        
        if (Input.GetButtonDown("Avoid") && !bowShoot && !FolowCamera.Instance.isSwitch)                  
            avoid = true;

        ///弓狀態判定
        
        if (!bowLock)
        {
            CantBow();
            if (Input.GetButtonDown("Switch") && !bowState && !FolowCamera.Instance.isSwitch && cantBowState
                && rollToBow)
            {
                FolowCamera.Instance.SwitchSet();
                bowTrigger = true;
                bowState = true;
            }
            else if (Input.GetButtonDown("Switch") && bowState && !FolowCamera.Instance.isSwitch && !bowShoot)
            {
                FolowCamera.Instance.SwitchSet();
                bowTrigger = true;
                bowState = false;
            }
        }
    }
    /// <summary>
    /// 不能切到弓的狀態
    /// 從PlayerControl的Scrip來獲得attackState和rollIsNext
    /// </summary>
    void CantBow()
    {
        cantBowState=!attackState && !rollIsNext && !hurt && !hurtIsNext;
    }
    public void BowUnlock()
    {
        bowLock = false;
        StartCoroutine(UIMain.Instance().BowUnlock());
    }    
    //void BowStateCancel()
    //{
    //    bowState = false;
    //}
}
