using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FolowCamera : MonoBehaviour
{
    public static FolowCamera Instance
    {
        get { return s_Instance; }
    }

    protected static FolowCamera s_Instance;

    public float cameraSpeed = 20.0f;
    public Transform lookTarget;
    public float followDistance;
    public Vector2 minMaxFollowDistance;
    public float cameraHeight;
    public float relativeDistance = 2.0f;
    public float bowStateY = 2.25f;

    public bool isSwitch = false;

    private float horizontalAngle;
    private float verticalAngle;
    private float zeroPoint;
    private float distance;

    Quaternion playerRotate;

    Vector2 mouseInput;
    Vector2 moveInput;

    Vector3 lookTargetPosition;
    Vector3 cameraForward;
    Vector3 cameraPosition;
    Vector3 relativeVector;//相對於一般視角的弓視角向量

    Vector3 relativeForward;//相對於一般視角的弓視角正前方
    Vector3 bowPosition;//弓視角下的攝影機位置
    Vector3 normalPosition;//一般視角下的攝影機位置
    Vector3 nextPosition;//下一個攝影機位置
    Vector3 lastPosition;//上一個攝影機位置
    Vector3 direct;//上一個攝影機位置到下一個攝影機位置的向量
    Vector3 shakePosition;//震動量

    [HideInInspector] public Vector3 horizontalVector;//攝影機正前方向量
    [HideInInspector] public Vector3 cameraRight;//攝影機側面向量
    public LayerMask checkHitLayerMask;

    private void Awake()
    {
        s_Instance = this;
    }
    void Start()
    {
        horizontalVector = lookTarget.transform.forward;
        lookTargetPosition = lookTarget.position + new Vector3(0.0f, 2.0f, 0.0f);

        relativeVector = Quaternion.AngleAxis(155f, Vector3.up) * horizontalVector;
    }

    void Update()
    {
        UpdateCamera();
    }

    void UpdateCamera()
    {
        mouseInput = PlayerInput.Instance.MouseInput;
        moveInput = PlayerInput.Instance.MoveInput / cameraSpeed;
        horizontalAngle = mouseInput.x;
        verticalAngle += mouseInput.y;
        if(PlayerInput.Instance.bowState)
            BowVisionLimit();
        else
            NormalVisionLimit();

        CameraRotate();//隨時更新一般狀態的攝影機位置與轉向
        BowCameraRotate();//隨時更新弓狀態的攝影機位置與轉向

        if (PlayerInput.Instance.bowState)
        {
            lastPosition = normalPosition;
            nextPosition = bowPosition;
            
            Switch();

            playerRotate = Quaternion.LookRotation(horizontalVector);
            lookTarget.rotation = playerRotate;
        }
        else
        {
            lastPosition = bowPosition;
            nextPosition = normalPosition;

            Switch();

            WallDetect(); //牆壁檢測
        }
        transform.forward = cameraForward;
        transform.position = cameraPosition+shakePosition;
    }

    void OnDrawGizmos()
    {

        Gizmos.color = new Color(1.0f, 0.0f, 0.0f);
        Gizmos.DrawLine(cameraRight, cameraRight * 3);
        Gizmos.color = new Color(0.0f, 1.0f, 0.0f);
        Gizmos.DrawLine(transform.position, cameraPosition);
    }
    /// <summary>
    /// 牆壁偵測
    /// </summary>
    void WallDetect()
    {
        lookTargetPosition = lookTarget.position + new Vector3(0.0f, 2.0f, 0.0f);
        if (Physics.SphereCast(lookTargetPosition, 0.5f ,-cameraForward, out RaycastHit rh, followDistance, checkHitLayerMask))
        {
            Vector3 hitRayDir = rh.point - lookTarget.position;
            float hitRayLength = hitRayDir.magnitude;

            Vector3 newCameraPosition = rh.point + rh.normal * 0.5f;//固定住攝影機的位置(不要再後退了) 
            
            if (hitRayLength < minMaxFollowDistance.x && (rh.transform.gameObject.layer==8))
            {
                float upDistance = minMaxFollowDistance.x - hitRayLength;
                cameraPosition = newCameraPosition + Vector3.up * upDistance;
            }
            else
            {
                cameraPosition = newCameraPosition;
            }
            cameraForward = lookTargetPosition - cameraPosition;
        }
    }
    void BowCameraRotate()
    {
        relativeVector = Quaternion.AngleAxis(horizontalAngle + moveInput.x, Vector3.up) * relativeVector;
        relativeVector.Normalize();
        relativeForward = Quaternion.AngleAxis(verticalAngle, -cameraRight) * relativeVector;
        relativeForward.Normalize();

        bowPosition = lookTarget.position + new Vector3(0.0f, bowStateY, 0.0f) + relativeForward * relativeDistance;
    }
    /// <summary>
    /// vector = Quaternion.AngleAxis(角度, 旋轉軸向量) * 欲旋轉向量;
    /// </summary>
    void CameraRotate()
    {
        if (PlayerInput.Instance.attackState || PlayerInput.Instance.bowState)
            moveInput.x = 0f;

        horizontalVector = Quaternion.AngleAxis(horizontalAngle + moveInput.x, Vector3.up) * horizontalVector;
        horizontalVector.Normalize();
        cameraRight = Vector3.Cross(Vector3.up, horizontalVector);
        cameraForward = Quaternion.AngleAxis(verticalAngle, -cameraRight) * horizontalVector;
        cameraForward.Normalize();

        normalPosition = lookTarget.position + new Vector3(0.0f, cameraHeight, 0.0f) - (cameraForward * followDistance);
    }
    /// <summary>
    /// 攝影機位置切換
    /// </summary>
    void Switch()
    {
        direct = nextPosition - lastPosition;//攝影機移動方向
        distance = direct.magnitude;//攝影機需移動的總距離
        direct.Normalize();//將向量單位只當作方向用
        zeroPoint = Mathf.Lerp(zeroPoint, distance, 0.1f);//攝影機移動距離，從0開始到總距離，並加上內插
        cameraPosition = lastPosition + zeroPoint * direct;

        if ((cameraPosition - nextPosition).magnitude < 0.05f)//快到位置時直接到點
        {
            cameraPosition = nextPosition;
            isSwitch = false;
        }
    }
    /// <summary>
    /// 開始弓切換的參數設定
    /// </summary>
    public void SwitchSet()
    {
        isSwitch = true;
        zeroPoint = 0f;
    }
    /// <summary>
    /// 一般狀態下的滑鼠滑動限制
    /// </summary>
    void NormalVisionLimit()
    {
        if (verticalAngle > 20.0f)
            verticalAngle = 20.0f;
        if (verticalAngle < -60.0f)
            verticalAngle = -60.0f;
    }
    /// <summary>
    /// 弓狀態下的滑鼠滑動限制
    /// </summary>
    void BowVisionLimit()
    {
        if(verticalAngle > 20.0f)
            verticalAngle = 20.0f;
        if (verticalAngle < -21f)
            verticalAngle = -21f;
    }
    public IEnumerator CameraShake(float shake,float time)
    {
        while (shake > 0.05f)
        {
            ///X軸震動
            //shakePosition = new Vector3((Random.Range(0f, shake)) - shake * 0.5f, 0f, 0f);
            ///Z軸震動
            shakePosition = new Vector3(0f,0f, (Random.Range(0f, shake)) - shake * 0.5f);

            shake = shake / time;
        
            yield return null;
        }
        
        shakePosition = Vector3.zero;
    }
}
