using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : MonoBehaviour
{
    public static Bow Instance
    {
        get { return s_Instance; }
    }
    protected static Bow s_Instance;

    public Transform bow;
    public GameObject standArrow;
    //public GameObject prefab;
    [HideInInspector]
    public bool bowState;
    //武器消融相關
    public Material bowMaterial;    
    public float dissolveAmount = -1.0f;
    public float endDissolveAmount = 1.5f;
    public float dissolveTime = 1.0f;
    public float startDissolveAmount;      

    // Start is called before the first frame update
    private void Awake()
    {
        s_Instance = this;
        bowMaterial = bow.GetComponent<MeshRenderer>().material;       
        bowMaterial.SetFloat("_DissolveAmount", dissolveAmount);  
    }
    void Start()
    {
        
    }

    void Update()
    {
        bowState = PlayerInput.Instance.bowState;
        if (bowState && PlayerInput.Instance.bowTrigger)
            BowOn();
        else if(!bowState && PlayerInput.Instance.bowTrigger)
            BowOff();

    }
    void BowOn()
    {
        bow.gameObject.SetActive(true);
        UIMain.Instance().ScopeOpen();

        PlayerInput.Instance.bowTrigger = false;
        WeaponOnDissolve();
    }
    void BowOff()
    {
        //bow.gameObject.SetActive(false);
        UIMain.Instance().ScopeClose();

        standArrow.SetActive(false);
        PlayerInput.Instance.bowTrigger = false;
        WeaponOffDissolve();
    }
    /// <summary>
    /// �I�sArrowShoot Script��Shoot�禡
    /// </summary>
    void BowFire()
    {
        BroadcastMessage("Shoot");
    }
    IEnumerator WeaponOn(float v_start, float v_end, float duration)
    {
        Debug.Log("weaponOn");
        float time = 0.0f;
        while (time < duration )
        {
            dissolveAmount = Mathf.Lerp(v_start, v_end, time / duration );
            bowMaterial.SetFloat("_DissolveAmount", dissolveAmount);
            time += Time.deltaTime;
            yield return null;
        }
        dissolveAmount = v_end;
    }
    IEnumerator WeaponOff(float v_start, float v_end, float duration)
    {
        Debug.Log("weaponOff");
        float time = 0.0f;
        while (time < duration )
        {
            dissolveAmount = Mathf.Lerp(v_start, v_end, time / duration );
            bowMaterial.SetFloat("_DissolveAmount", dissolveAmount);
            time += Time.deltaTime;
            yield return null;
        }
        dissolveAmount = v_end;
        bow.gameObject.SetActive(false);
    }
    private void WeaponOnDissolve()
    {
        StartCoroutine(WeaponOn(dissolveAmount, endDissolveAmount, dissolveTime));
    }
    private void WeaponOffDissolve()
    {
        StartCoroutine(WeaponOff(dissolveAmount, startDissolveAmount, 0.25f));
    }
    private void ResetArrow()
    {
        standArrow.SetActive(true);
    }
}
