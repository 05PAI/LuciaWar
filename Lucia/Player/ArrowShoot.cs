using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowShoot : MonoBehaviour
{
    Transform cameraTrasform;
    PlayerControl player;

    public GameObject prefab;
    public GameObject standArrow;

    private float charge;//�W�O�ɶ�(��Player)
    private float playerMp;
    private int normalCost=10;//�@�����
    private int explodeCost = 25;//�z���bMP����

    private ArrowLoad load;
    private Arrow arrow;

    private Vector3 targetDirection;//�ǬP�ؼФ�V(�۾����e��)
    private Vector3 targetPosition;
    private Vector3 normalPosition;
    private Vector3 arrowDirection;//�b�ڭ����V
    private Vector3 arrowPosition;//�b�ڥͦ���V
    private float targetDistance;//�ؼШ���v�����Z��

    private void Awake()
    {
        load = new ArrowLoad();
        load.creatArrow(prefab, 30);

        player = gameObject.GetComponentInParent<PlayerControl>();
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        cameraTrasform = Camera.main.transform;
        normalPosition = cameraTrasform.position + cameraTrasform.forward * 80f;
        arrowDirection = normalPosition - transform.position;

        Ray r = new Ray(cameraTrasform.position, cameraTrasform.forward);
        if (Physics.Raycast(r, out RaycastHit hit, 100f))
        {            
            targetPosition = hit.point;
            targetDirection = targetPosition - transform.position;
            
            targetDistance = targetDirection.magnitude;
            if (targetDistance < 15f)                      //�ǬP�ؼ����Ӫ��
            {
                targetPosition=cameraTrasform.position + cameraTrasform.forward * 15f;                
            }
            targetDirection = targetPosition - transform.position;
            arrowDirection = targetDirection;
        }
        
    }
    void Shoot()
    {
        if (playerMp < 10)
            return;

        standArrow.SetActive(false);

        GameObject go = load.LoadArrow();        
        arrow=go.GetComponent<Arrow>();
        if ((charge < 1.5f) || (playerMp<= explodeCost))          //�M�w�o�@�b�O���q�b�٬O�z���b
        {
            arrow.IsNormal();
            player.MpReduce(normalCost);
        }
        else if(playerMp> explodeCost)
        {
            arrow.IsExplode();
            player.MpReduce(explodeCost);
        }
        
        go.transform.position = transform.position;//�վ�b�ڦ�m���}����m
        go.transform.forward = arrowDirection;//�վ�b�ګe�謰�}�o�e��
        go.SetActive(true);
    }
    public void GetCharge(float ch, float mp)
    {
        charge = ch;
        playerMp = mp;
    }
}
