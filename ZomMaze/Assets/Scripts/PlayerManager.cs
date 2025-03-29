 using System.Collections;
using System.Data;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.EventSystems;
using UnityEngine.Jobs; // NameSpace : �Ҽ� - ����� �̸��� �������ֱ�����
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum WeaponMode
{ 
    Pistol,
    Rifle,
    Shotgun,
    None
}


// ����� �������� ������ �����غ���
// �������� ���� �������� ���� : ������ �޸𸮸� ���� �����Ѵٴ� ���� �˱�.

public class PlayerManager : MonoBehaviour
{
    // �̱���
    public static PlayerManager Instance { get; private set; }

    private float moveSpeed = 3.0f; // �÷��̾� �̵��ӵ�
    private float mouseSensitivty = 100;
    public Transform cameraTransform; // ī�޶� Ʈ������
    public CharacterController characterController;
    public Transform playerHead; // �÷��̾� �Ӹ� ��ġ(1��Ī ���)
    public float thridPersonDistansce = 3.0f; // 3��Ī ��忡�� �÷��̾�� ī�޶��� �Ÿ�
    public Vector3 thridPersonOffset = new Vector3(0f, 1.5f, 0f); // 3��Ī ��忡�� ī�޶� ������
    public Transform playerLookObj; // �÷��̾� �þ� ��ġ

    public float zoomDistance = 1.0f; // ī�޶� Ȯ��� ���� �Ÿ�(3��Ī ��忡�� ���)
    public float zoomSpeed = 5.0f; // Ȯ����Ұ� �Ǵ� �ӵ�
    public float defaultFov = 60.0f; // �⺻ ī�޶� �þ߰�
    public float zooomFov = 30.0f; // Ȯ�� �� ī�޶� �þ߰� (1��Ī ��忡�� ���)

    private float currentDistance; // ���� ī�޶���� �Ÿ�(3��Ī ���)
    private float targetDistance; // ��ǥ ī�޶� �Ÿ�
    private float targetFov; // ��ǥ FOV
    private Coroutine zoomCoroutine; // �ڷ�ƾ�� ����Ͽ� Ȯ�� ��� ó��
    private Camera mainCamera; // ī�޶� ������Ʈ

    private float pitch = 0.0f; // ���Ʒ� ȸ�� ��
    private float yaw = 0.0f; // �¿� ȸ�� ��

    private bool isFirstPerson = false; // 1��Ī ��� ����
    private bool isrotaterAroundPlayer = true; // ī�޶� �÷��̾� ������ ȸ���ϴ��� ����
        
    public float gravity = -9.81f; // �߷� ���� ����
    public float jumpHeight = 2.0f;
    private Vector3 velocity;
    private bool isGround; // ���鿡 ����ִ���

    private Animator animator;
    private float horizontal;
    private float vertical;
    private bool isRunning = false;
    public float walkSpeed = 3.0f;
    public float runSpeed = 10.0f;
    private bool isAim = false;
    private bool isFire = false;
    private bool isReload = false;
    public bool isStop = false;

        
    public GameObject rifleFamas;
    public GameObject pistol;
    private bool isPistol = false;
    public float damage = 1;

    public Transform startPoint;

    //private int animationSpeed = 1;  // �ִϸ��̼� ���ǵ� ���� animator.speed = animationSpeed;
    public Transform aimTarget;
    private float weaponMaxDistance = 100.0f;
    public LayerMask TargetLayerMask;
    public LayerMask TargetLayerMask2;
    public LayerMask TargetLayerMask3;

    public MultiAimConstraint multiAimConstraint;

    private bool isEquip = false;
    public Vector3 boxSize = new Vector3(1.0f, 1.0f, 1.0f);
    public float castDistance = 0.5f;
    public LayerMask itemLayer;
    public Transform itemGetPos;

    public GameObject crossHairObj;
    private bool isGetGun = false;
    private bool isUseGun = false;

    public GameObject rifleIcon;
    public GameObject pistolIcon;
    public Text riflebulletText;
    public Text pistolbulletText;
    private int fireBulletCount = 0;
    private int pistolBulletCount = 10;
    private int saveBulletCount = 0;

    private float rifleFireDelay = 0.2f;
    private float pistolFireDelay = 0.5f;

    public float playerHp = 5.0f;

    public GameObject flashLightObj;
    private bool isFlashLightON = false;
    public AudioClip audioFlasgLihgt;


    // ���⺰ ����
    private WeaponMode currentWeaponMode = WeaponMode.Rifle;

    public float recoilStrength = 0.2f;
    public float maxRecoilAngle = 15.0f;
    public float currentRecoil = 0.0f;
    // ī�޶� ��� ���� �ݵ�
    public float shakeDuration = 0.01f;
    public float shakeMagnitude = 0.01f;

    private Vector3 originalCameraPosition;
    private Coroutine cameraShakeCoroutine;

    private bool isOPen;
    public Transform doorCheck;
    public float doorCheckDistance = 3.0f;

    public LayerMask cameraCollision;
    public LayerMask cameraCollision2;
    public Transform front;

    public Transform effectPos;

    public GameObject playerPrefab;
    public GameObject bloodSign;
    public GameObject bulletMark;

    public GameObject questText;

    public Transform pistolShot;
    public Transform rifleShot;

    public ParticleSystem pistolParticle;
    public ParticleSystem rifleParticle;

    public GameObject blood;



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if(Instance != this)
        {

             Destroy(gameObject);

        }
    }

    void Start()
    {   
        // Ŀ�� ���
        Cursor.lockState = CursorLockMode.Locked;
        transform.position = startPoint.position;
        currentDistance = thridPersonDistansce;
        targetDistance = thridPersonDistansce;
        targetFov = defaultFov;
        mainCamera = cameraTransform.GetComponent<Camera>();
        mainCamera.fieldOfView = defaultFov;
        animator = GetComponent<Animator>();
        rifleFamas.SetActive(false);
        pistol.SetActive(false);
        characterController = GetComponent<CharacterController>();
        crossHairObj.SetActive(false);
        rifleIcon.SetActive(false);
        pistolIcon.SetActive(false);
        riflebulletText.gameObject.SetActive(false);
        pistolbulletText.gameObject.SetActive(false);
        flashLightObj.SetActive(false);
        Image blood = bloodSign.GetComponent<Image>();
        currentWeaponMode = WeaponMode.None;

        /* �ڵ�� �Ȱ� ����
        RenderSettings.fog = true; // �Ȱ�ȿ�� Ȱ��ȭ
        RenderSettings.fogColor = Color.blue; // �Ȱ��� �� ����
        RenderSettings.fogDensity = 0.01f; // �Ȱ��� �е� ����
        RenderSettings.fogStartDistance = 10f; // �Ȱ� ���� �Ÿ��� ����Ÿ� ����(Liner��忡�� ���)
        RenderSettings.fogEndDistance = 50f;
        RenderSettings.fogMode = FogMode.Exponential; // ���� �Լ� ��� �Ȱ�
        

        if (mainCamera != null) // ī�޶��� clear Flags�� solid color�� �����ϰ�, ������ �Ȱ������� ����
        {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = RenderSettings.fogColor;
        }
        */
    }

    void Update()
    {
        PlayerMove();
        PlayerRun();
        CameraSet();
        Aim();
        WeaponChange();
        Reload();
        Fire();
        ItemGet();
        Die();
        ActionFlashLight();        
        CameraRecoil();
        DoorOpen();
        // animation.layer�� ���� ���� �ֱ�
        // AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(1);

    }

    public void PlayerMove()
    {
        mouseSensitivty = FindAnyObjectByType<MouseSensitivityManager>().GetMouseSensitivity();
        // ���콺 �Է��� �޾� ī�޶�� �÷��̾� ȸ�� ó��
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivty * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivty * Time.deltaTime;
        
        yaw += mouseX;
        pitch -= mouseY;
        // ���콺 ������ �� ���ư����ʵ��� ����
        pitch = Mathf.Clamp(pitch, -45, 45);
        flashLightObj.transform.rotation = Quaternion.Euler(pitch, yaw, 0);
    }

    void CameraSet()
    {
        isGround = characterController.isGrounded;
        isrotaterAroundPlayer = false; // ���� ���۽� 3��Ī���� ������
        
        // �÷��̾ �������� ���� �ٰ��ϵ��� ����
        if (isGround && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            isFirstPerson = !isFirstPerson;
            Debug.Log(isFirstPerson ? "1��Ī ���" : "3��Ī ���");
        }

        if (Input.GetKey(KeyCode.C)) // CŰ�� �ڴ����� �������� �ֺ� �þ� Ȯ�� ����
        {
            isrotaterAroundPlayer = !isrotaterAroundPlayer;

            Debug.Log(isrotaterAroundPlayer ? "ī�޶� ������ ȸ���մϴ�." : "�÷��̾��� �þ� Ȯ�ΰ���.");

        }

        if (isFirstPerson)
        {
            FirstPersonMovement();
        }
        else
        {
            ThirdPersonMovement();
        }

    }

    void CameraRecoil()        // �ѽ�� �ݵ� ǥ��, ũ�ν���� Ŀ���ٰ� �پ����ٰ��� ǥ��
    {

        if (currentRecoil > 0)
        {
            currentRecoil -= recoilStrength * Time.deltaTime;
            currentRecoil = Mathf.Clamp(currentRecoil, 0, maxRecoilAngle);
            Quaternion currentRotation = Camera.main.transform.rotation;
            Quaternion recoilRotation = Quaternion.Euler(-currentRecoil, 0, 0);
            Camera.main.transform.rotation = currentRotation * recoilRotation; // ī�޶� �����ϴ� �ڵ带 �����Ѵ�. -> �̰� ���������� ī�޶�� �ö��� �ʴ´�.
        }
    }

    void ApplyRecoil()
    {
        Quaternion currentRotation = Camera.main.transform.rotation; // ���� ī�޶� ���� ȸ���� ��������
        Quaternion recoilRotation = Quaternion.Euler(-currentRecoil, 0, 0); // �ݵ��� ����ؼ� x�� ���� ȸ���� �߰�
        Camera.main.transform.rotation = currentRotation * recoilRotation; // ���� ȸ�� ���� �ݵ��� ���Ͽ� ���ο� ȸ������ �ֱ�
        currentRecoil += recoilStrength; // �ݵ� ���� ����
        currentRecoil = Mathf.Clamp(currentRecoil, 0, maxRecoilAngle); // �ݵ����� ����
    }

    void StartCameraShake()
    {
        if (cameraShakeCoroutine != null)
        {
            StopCoroutine(cameraShakeCoroutine);
        }
        cameraShakeCoroutine = StartCoroutine(CameraShake(shakeDuration, shakeMagnitude));
    }

    IEnumerator CameraShake(float duration, float magnitude)
    {
        float elapsed = 0.0f;
        Vector3 orginalPosition = Camera.main.transform.position;

        while(elapsed < duration)
        {
            float offsetX = Random.Range(-1.0f, 1.0f) * magnitude;
            float offsetY = Random.Range(-1.0f, 1.0f) * magnitude;

            Camera.main.transform.position = orginalPosition + new Vector3(offsetX, offsetY, 0);
            
            elapsed += Time.deltaTime;

            yield return null;  
        }
        Camera.main.transform.position = orginalPosition;
    }

 
    public void Aim()
    {
        // ������ �������� �ڷ�ƾ ��� / ���콺�� ��������
        if ((Input.GetMouseButton(1) && isGetGun && isUseGun && !isReload) || (Input.GetMouseButton(1) && isPistol && isUseGun && !isReload))
        {
            isAim = true;            
            multiAimConstraint.data.offset = new Vector3(-30, 0, 0);
            crossHairObj.SetActive(true);
            animator.SetLayerWeight(1, 1); // ���̾ 1�� ����


            //Ȯ�� ��� �Ÿ��� ���� ������ - �ڷ�ƾ�� �ϰ��ִ��� �ƴ���, �ߺ�����
            if (zoomCoroutine != null)
            {
                // �ڷ�ƾ �����
                StopCoroutine(zoomCoroutine);
            }
            // 1��Ī ����϶�
            if (isFirstPerson)
            {
                // Ÿ���� ��ġ�� zooomFov�� �ϰ� 1��Ī����� �ڷ�ƾ���� ����
                SetTargetFov(zooomFov);
                // ������ ������Ʈ �ֱ�

                playerPrefab.SetActive(false);
                rifleFamas.SetActive(false);
                pistol.SetActive(false);
                zoomCoroutine = StartCoroutine(ZoomFieldOfView(targetFov));
            }
            else
            {   // �װ� �ƴ϶�� Ÿ���� ��ġ�� zoomDistance�ϰ� 3��Ī ����� �ڷ�ƾ���� ����
                SetTargetDistance(zoomDistance);
                zoomCoroutine = StartCoroutine(ZoomCamera(targetDistance));
            }


        }

        // ���콺�� ������
        if ((Input.GetMouseButtonUp(1) && isGetGun && isUseGun) || (Input.GetMouseButtonUp(1) && isPistol && isUseGun))
        {
            isAim = false;
            crossHairObj.SetActive(false);
            multiAimConstraint.data.offset = new Vector3(0, 0, 0);
            animator.SetLayerWeight(1, 0);
            currentRecoil = 0;

            //Ȯ�� ��� �Ÿ��� ���� ������ - �ڷ�ƾ�� �ϰ��ִ��� �ƴ���, �ߺ�����
            if (zoomCoroutine != null)
            {
                // �ڷ�ƾ �����
                StopCoroutine(zoomCoroutine);
            }
            // 1��Ī ����϶�
            if (isFirstPerson)
            {
                // Ÿ���� ��ġ�� defaultFov �ϰ� 1��Ī����� �ڷ�ƾ���� ����
                SetTargetFov(defaultFov);
                //������ ����
                
                playerPrefab.SetActive(true);
                if (currentWeaponMode == WeaponMode.Rifle)
                {
                    rifleFamas.SetActive(true);
                }
                else if (currentWeaponMode == WeaponMode.Pistol)
                { 
                    pistol.SetActive(true);
                }
                zoomCoroutine = StartCoroutine(ZoomFieldOfView(targetFov));
            }
            else
            {   // �װ� �ƴ϶�� Ÿ���� ��ġ�� thridPersonDistansce�ϰ� 3��Ī ����� �ڷ�ƾ���� ����
                SetTargetDistance(thridPersonDistansce);
                zoomCoroutine = StartCoroutine(ZoomCamera(targetDistance));

            }

        }
    }

    public void PlayerRun()
    {

        // bool�� ���� ����ƮŰ�� ������ ��ȭ�Ǵ� ���� ����
        if (Input.GetKey(KeyCode.LeftShift) && (horizontal !=0 || vertical !=0)) // �������� 0�� �ƴ϶������ ����
        {
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }



        // ��Ȳ�� �ִϸ��̼� ����
        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);
        animator.SetBool("IsRunning", isRunning);


        moveSpeed = isRunning ? runSpeed : walkSpeed;
    }

    public void Fire()
    {        

        if (Input.GetMouseButton(0) && !isReload)
        {
            if (isAim && !isFire)
            {
                isFire = true;

                if (currentWeaponMode == WeaponMode.Pistol)
                {

                    damage = 0.5f;
                    weaponMaxDistance = 50.0f;
                    recoilStrength = 0.1f;
                    if (pistolBulletCount > 0)
                    {
                        pistolBulletCount--;
                        pistolbulletText.text = $"{pistolBulletCount}/{"��"}";
                        pistolbulletText.gameObject.SetActive(true);
                        pistolParticle.transform.rotation = transform.rotation;
                        pistolParticle.Play();



                        StartCoroutine(FireWithDelay(pistolFireDelay));
                        animator.SetTrigger("Fire");
                        SoundManager.instance.PlaySFX("PistolSound", gameObject.transform.position);
                        

                        ApplyRecoil();
                        StartCameraShake();

                        // ���ѼҸ��� �ٲٱ�

                        // orgin = ���� ��ġ
                        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);

                        // ����ĳ��Ʈ�� ���� �浹ü ���� - ������ ����
                        RaycastHit[] hits = Physics.RaycastAll(ray, weaponMaxDistance, TargetLayerMask);

                        if (hits.Length > 0)
                        {
                            // �θ��� ����
                            for (int i = 0; i < hits.Length && i < 2; i++)
                            {
                                Debug.Log("Hit : " + hits[i].collider.gameObject.name);
                                Debug.DrawLine(ray.origin, hits[i].point, Color.red, 1.0f);

                                if (hits[i].collider.gameObject.CompareTag("Zombie"))
                                {
                                    blood.transform.position = hits[i].point;
                                    Instantiate(blood);
                                    hits[i].collider.gameObject.GetComponent<ZombieManager>().TakeDamage(damage);

                                }
                                else
                                {
                                    Debug.DrawLine(ray.origin, ray.origin + ray.direction * weaponMaxDistance, Color.green, 1.0f);
                                }

                            }


                        }

                        RaycastHit hitWall;

                        if (Physics.Raycast(ray, out hitWall, weaponMaxDistance, TargetLayerMask2))
                        {
                            Debug.Log("Hit : " + hitWall.collider.gameObject.name);
                            Debug.DrawLine(ray.origin, hitWall.point, Color.red, 1.0f);

                            if (hitWall.collider.gameObject.layer == 8)
                            {
                                ParticleManager.Instance.ParticlePlay(ParticleType.PistolFire, hitWall.point, Vector3.one);
                                SoundManager.instance.PlaySFX("WallHitSound", hitWall.point);
                                GameObject bulletImpact = Instantiate(bulletMark);
                                bulletImpact.transform.position = hitWall.point;
                                bulletImpact.transform.rotation = hitWall.collider.transform.rotation;
                                Destroy(bulletImpact, 1.0f);
                            }
                            else
                            {
                                Debug.DrawLine(ray.origin, ray.origin + ray.direction * weaponMaxDistance, Color.green, 1.0f);
                            }

                        }

                        RaycastHit hitGround;

                        if (Physics.Raycast(ray, out hitGround, weaponMaxDistance, TargetLayerMask3))
                        {
                            Debug.Log("Hit : " + hitGround.collider.gameObject.name);
                            Debug.DrawLine(ray.origin, hitGround.point, Color.red, 1.0f);

                            if (hitGround.collider.gameObject.layer == 9)
                            {
                                ParticleManager.Instance.ParticlePlay(ParticleType.PistolFire, hitGround.point, Vector3.one);
                                SoundManager.instance.PlaySFX("WallHitSound", hitGround.point);
                                GameObject bulletImpact = Instantiate(bulletMark);
                                bulletImpact.transform.position = hitGround.point;
                                bulletImpact.transform.rotation = hitGround.collider.transform.rotation;
                                Destroy(bulletImpact, 1.0f);
                            }
                            else
                            {
                                Debug.DrawLine(ray.origin, ray.origin + ray.direction * weaponMaxDistance, Color.green, 1.0f);
                            }

                        }

                    }
                    else
                    {
                        // ���ѼҸ� �ѹ��� �����ϱ�
                        isFire = false;

                        if (Input.GetMouseButtonDown(0))
                        {
                            SoundManager.instance.PlaySFX("EmptyBullet", gameObject.transform.position);
                        }

                        return;
                    }

                }



                if (currentWeaponMode == WeaponMode.Rifle)
                {
                    damage = 1.0f;
                    weaponMaxDistance = 100.0f;
                    recoilStrength = 0.4f;
                   
                    if (fireBulletCount > 0)
                    {
                        fireBulletCount--;
                        rifleParticle.transform.rotation = transform.rotation;
                        rifleParticle.Play();

                        riflebulletText.text = $"{fireBulletCount}/{saveBulletCount}";
                        riflebulletText.gameObject.SetActive(true);

                        StartCoroutine(FireWithDelay(rifleFireDelay));
                        animator.SetTrigger("Fire");
                        SoundManager.instance.PlaySFX("FamasShot", gameObject.transform.position);


                        ApplyRecoil();
                        StartCameraShake();                        

                        // orgin = ���� ��ġ
                        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);

                        // ����ĳ��Ʈ�� ���� �浹ü ���� - ������ ����
                        RaycastHit[] hits = Physics.RaycastAll(ray, weaponMaxDistance, TargetLayerMask);

                        if (hits.Length > 0)
                        {
                            // �θ��� ����
                            for (int i = 0; i < hits.Length && i < 2; i++)
                            {
                                Debug.Log("Hit : " + hits[i].collider.gameObject.name);
                                Debug.DrawLine(ray.origin, hits[i].point, Color.red, 1.0f);

                                if (hits[i].collider.gameObject.CompareTag("Zombie"))
                                {
                                    blood.transform.position = hits[i].point;
                                    Instantiate(blood);
                                    // ��ƼŬ ������ �Ҹ� �־��
                                    hits[i].collider.gameObject.GetComponent<ZombieManager>().TakeDamage(damage);

                                }
                                else
                                {
                                    Debug.DrawLine(ray.origin, ray.origin + ray.direction * weaponMaxDistance, Color.green, 1.0f);
                                }

                            }


                        }

                        RaycastHit hitWall;

                        if (Physics.Raycast(ray, out hitWall, weaponMaxDistance, TargetLayerMask2))
                        {
                            Debug.Log("Hit : " + hitWall.collider.gameObject.name);
                            Debug.DrawLine(ray.origin, hitWall.point, Color.red, 1.0f);

                            if (hitWall.collider.gameObject.layer == 8)
                            {
                                ParticleManager.Instance.ParticlePlay(ParticleType.WeaponFire, hitWall.point, Vector3.one);
                                SoundManager.instance.PlaySFX("WallHitSound", hitWall.point);
                                GameObject bulletImpact = Instantiate(bulletMark);
                                bulletImpact.transform.position = hitWall.point;
                                bulletImpact.transform.rotation = hitWall.collider.transform.rotation;
                                Destroy(bulletImpact, 3.0f);


                            }
                            else
                            {
                                Debug.DrawLine(ray.origin, ray.origin + ray.direction * weaponMaxDistance, Color.green, 1.0f);
                            }

                        }

                        RaycastHit hitGround;

                        if (Physics.Raycast(ray, out hitGround, weaponMaxDistance, TargetLayerMask3))
                        {
                            Debug.Log("Hit : " + hitGround.collider.gameObject.name);
                            Debug.DrawLine(ray.origin, hitGround.point, Color.red, 1.0f);

                            if (hitGround.collider.gameObject.layer == 9)
                            {
                                ParticleManager.Instance.ParticlePlay(ParticleType.WeaponFire, hitGround.point, Vector3.one);
                                SoundManager.instance.PlaySFX("WallHitSound", hitGround.point);
                                GameObject bulletImpact = Instantiate(bulletMark);
                                bulletImpact.transform.position = hitGround.point;
                                bulletImpact.transform.rotation = hitGround.collider.transform.rotation;
                                Destroy(bulletImpact, 3.0f);
                            }
                            else
                            {
                                Debug.DrawLine(ray.origin, ray.origin + ray.direction * weaponMaxDistance, Color.green, 1.0f);
                            }

                        }

                    }
                    else
                    {

                        // ���ѼҸ� �ѹ��� �����ϱ�
                        isFire = false;
                        if (Input.GetMouseButtonDown(0))
                        {
                            SoundManager.instance.PlaySFX("EmptyBullet", gameObject.transform.position);
                        }
                        return;
                    }
                }





            }
        }

    }

    public void Reload()
    {
        // �����ϸ� ���� �ڵ����� Ǯ�����ϴ� ��� �����غ���
        if ((Input.GetKeyDown(KeyCode.R) && isGetGun && !isFire && saveBulletCount > 0 && fireBulletCount < 30) || (Input.GetKeyDown(KeyCode.R) && isPistol && !isFire && pistolBulletCount<10))
        {
            isReload = true;
            isAim = false;

            if (currentWeaponMode == WeaponMode.Rifle)
            {

                if (fireBulletCount + saveBulletCount < 30)
                {
                    animator.SetTrigger("Reload");

                    fireBulletCount = saveBulletCount + fireBulletCount;
                    saveBulletCount = 0;
                    riflebulletText.text = $"{fireBulletCount}/{saveBulletCount}";
                    riflebulletText.gameObject.SetActive(true);

                    StartCoroutine(FinishReload());

                }
                else if (fireBulletCount + saveBulletCount >= 30)
                {
                    animator.SetTrigger("Reload");
                    saveBulletCount = saveBulletCount - (30 - fireBulletCount);
                    fireBulletCount = 30;
                    riflebulletText.text = $"{fireBulletCount}/{saveBulletCount}";
                    riflebulletText.gameObject.SetActive(true);

                    StartCoroutine(FinishReload());

                }
            }

            if (currentWeaponMode == WeaponMode.Pistol)
            {
                if (pistolBulletCount  < 10)
                {

                    animator.SetTrigger("Reload");                    
                    pistolBulletCount = 10;
                    pistolbulletText.text = $"{pistolBulletCount}/{"��"}";
                    pistolbulletText.gameObject.SetActive(true);
                    StartCoroutine(FinishReload());
                }
            
            }
        }   
    }

    private IEnumerator FinishReload() // reload �������� ���ϰ� �ϱ�
    {
        yield return new WaitForSeconds(1.5f);
        isReload = false;
    }

    public void WeaponChange()
    {

        if (Input.GetKeyDown(KeyCode.Alpha1) && isGetGun && !isAim && !isReload)
        {            

            // ���� Ű�� ������ �Լ� ������ϰ�
            if (currentWeaponMode == WeaponMode.Rifle)
            {
                return;
            }

            currentWeaponMode = WeaponMode.Rifle;
            animator.SetTrigger("IsWeaponChange");
            rifleFamas.SetActive(true);
            pistol.SetActive(false);
            rifleIcon.SetActive(true);
            pistolIcon.SetActive(false);
            riflebulletText.text = $"{fireBulletCount}/{saveBulletCount}";
            riflebulletText.gameObject.SetActive(true);
            pistolbulletText.text = $"{pistolBulletCount}/{"��"}";
            pistolbulletText.gameObject.SetActive(false);
            isPistol = false;
            isUseGun = true;

            walkSpeed = 2.5f;
            runSpeed = 4.0f;


        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && !isAim && !isReload)
        {
            if (currentWeaponMode == WeaponMode.Pistol)
            {
                return;
            }

            isPistol = true;

            currentWeaponMode = WeaponMode.Pistol; 
            animator.SetTrigger("IsWeaponChange");
            rifleIcon.SetActive(false);
            pistolIcon.SetActive(true);
            riflebulletText.text = $"{fireBulletCount}/{saveBulletCount}";
            riflebulletText.gameObject.SetActive(false);
            pistolbulletText.text = $"{pistolBulletCount}/{"��"}";
            pistolbulletText.gameObject.SetActive(true);
            pistol.SetActive(true);
            rifleFamas.SetActive(false);
            isUseGun = true;

            walkSpeed = 3f;
            runSpeed = 6.0f;

        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && !isAim && !isReload)
        {
            if (currentWeaponMode == WeaponMode.None)
            {
                return;
            }
            currentWeaponMode = WeaponMode.None;
            animator.SetTrigger("IsWeaponChange");
            rifleFamas.SetActive(false);
            pistol.SetActive(false);
            rifleIcon.SetActive(false);
            pistolIcon.SetActive(false);
            riflebulletText.gameObject.SetActive(false);
            pistolbulletText.gameObject.SetActive(false);
            isUseGun = false;

            walkSpeed = 3f;
            runSpeed = 7.0f;
        }
    }



        
    void ItemGet()
    {
        if (Input.GetKeyDown(KeyCode.E) && !isEquip)
        {
            isEquip = true;
            StartCoroutine(OneEquip(0.6f));
            Vector3 orgin = itemGetPos.position; // ĳ������ �Ǻ��� ���̱⶧���� ���� ����
            Vector3 direction = itemGetPos.forward;
            RaycastHit[] hits;
            hits = Physics.BoxCastAll(orgin, boxSize / 2, direction, Quaternion.identity, castDistance, itemLayer);
            DebugBox(orgin, direction);


            foreach (RaycastHit hit in hits)
            {

                if (hit.collider.name == "WeaponFamas")
                {
                    animator.SetTrigger("GetItem");
                    SoundManager.instance.PlaySFX("GetItem", gameObject.transform.position);
                    Debug.Log("Item : " + hit.collider.name);
                    StartCoroutine(DisableAfterDelay(hit.collider.gameObject, 0.5f));                    
                    fireBulletCount = 30;

                    isGetGun = true;

                }

                if (hit.collider.CompareTag("Bullet"))
                {
                    animator.SetTrigger("GetItem");
                    SoundManager.instance.PlaySFX("GetItem", gameObject.transform.position);
                    Debug.Log("Item : " + hit.collider.name);
                    StartCoroutine(DisableAfterDelay(hit.collider.gameObject, 0.5f));                    

                    saveBulletCount += 30;

                    if (saveBulletCount >= 120)
                    {
                        saveBulletCount = 120;

                    }
                    riflebulletText.text = $"{fireBulletCount}/{saveBulletCount}";


                }

                if (hit.collider.CompareTag("Heal"))
                {

                    if (playerHp < 30)
                    {
                        playerHp += 5;

                        if (playerHp > 30)
                        {
                            playerHp = 30;
                        }
                        animator.SetTrigger("GetItem");
                        Debug.Log("Item : " + hit.collider.name);
                        SoundManager.instance.PlaySFX("GetHeal", gameObject.transform.position);
                        StartCoroutine(DisableAfterDelay(hit.collider.gameObject, 0.5f));                        
                    }

                                        

                }

            }
        }
    }
    IEnumerator OneEquip(float delay)
    {
        
        yield return new WaitForSeconds(delay);
        isEquip = false;
    }


    IEnumerator DisableAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
    }


    void ActionFlashLight()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {     
            SoundManager.instance.PlaySFX("FlashLight", flashLightObj.transform.position);
            isFlashLightON = !isFlashLightON;
            flashLightObj.SetActive(isFlashLightON);
    

        }

    }


    void FirstPersonMovement()
    {
        if (!isStop)
        {
            // ���������� ���� ����� ��ü���� ������ �ִϸ��̼� ������ ���� float ����
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");


            // ī�޶� ��ġ���� ���� �÷��̾� ������ ����
            Vector3 moveDirection = cameraTransform.forward * vertical + cameraTransform.right * horizontal;

            moveDirection.y = 0;
            characterController.Move(moveDirection * moveSpeed * Time.deltaTime);


            // ī�޶���ġ�� �÷��̾��� �Ӹ��� ����
            cameraTransform.position = playerHead.position;
            // ī�޶� ���� ����
            cameraTransform.rotation = Quaternion.Euler(pitch, yaw, 0);
            // �÷��̾��� ������ ī�޶��� ȸ�� ������ ����
            transform.rotation = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0);

            UpdateAimTarget();
        }

    }

    void ThirdPersonMovement()
    {

        // ���������� ���� ����� ��ü���� ������ �ִϸ��̼� ������ ���� float ����
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        characterController.Move(move * moveSpeed * Time.deltaTime);


        UpdateCameraPosition();

    }

    void UpdateCameraPosition()
    {
        if (isrotaterAroundPlayer)
        {
            // ī�޶� �÷��̾� �����ʿ��� ȸ���ϵ��� ����
            Vector3 direction = new Vector3(0, 0, -currentDistance);
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

            // ī�޶� �÷��̾��� �����ʿ��� ������ ��ġ�� �̵�
            cameraTransform.position = transform.position + thridPersonOffset + rotation * direction;

            // ī�޶� �÷��̾��� ��ġ�� ���󰡵��� ����
            cameraTransform.LookAt(transform.position + new Vector3(0, thridPersonOffset.y, 0));

            CameraCollide(cameraCollision);
            CameraCollide(cameraCollision2);



        }
        else
        {
            if (!isStop)
            {

                // �÷��̾ ���� ȸ���ϴ� ���
                transform.rotation = Quaternion.Euler(0f, yaw, 0);
                Vector3 direction = new Vector3(0, 0, -currentDistance);
                cameraTransform.position = playerLookObj.position + thridPersonOffset + Quaternion.Euler(pitch, yaw, 0) * direction;
                cameraTransform.LookAt(playerLookObj.position + new Vector3(0, thridPersonOffset.y, 0));

                CameraCollide(cameraCollision);
                CameraCollide(cameraCollision2);
                UpdateAimTarget();
            }
        
        }


    }

    void CameraCollide(LayerMask layer)
    {
        // ī�޶� ���� �浹�ҽ� ī�޶� ������ �̵���Ű�� �ڵ�
        RaycastHit hit;
        Vector3 rayDir = cameraTransform.position - front.position;
        if (Physics.Raycast(front.position, rayDir, out hit, currentDistance, layer))
        {

            cameraTransform.position = hit.point;

        }

    }

    public void SetTargetDistance(float distance)
    { 
        targetDistance = distance;
    }

    public void SetTargetFov(float fov)
    {
        targetFov = fov;
    }

    IEnumerator ZoomCamera(float targetDistance) // 3��Ī
    {
        while (Mathf.Abs(currentDistance - targetDistance) > 0.01f) // ���� �Ÿ����� ��ǥ �Ÿ��� �ε巴�� �̵�, Mathf.Abs - ����
        { 
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomSpeed);
            yield return null; 
        
        }

        currentDistance = targetDistance; // ��ǥ �Ÿ��� ������ �� ���� ����
    
    }

    IEnumerator ZoomFieldOfView(float tartgetFov) // 1��Ī
    {
        while (Mathf.Abs(mainCamera.fieldOfView - targetFov) > 0.01f)
        { 
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, tartgetFov, Time.deltaTime * zoomSpeed);
            yield return null;
        }

        mainCamera.fieldOfView = targetFov;

    }

    IEnumerator FireWithDelay(float fireDelay)
    {        
        yield return new WaitForSeconds(fireDelay);
        isFire = false;

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Goal"))
        {
            animator.SetTrigger("IsWin");
            MenuManager.Instance.SetFirstLoadFalse();
            MenuManager.Instance.nextSceneName = "LoadingScene";
            MenuManager.Instance.StartCoroutine(MenuManager.Instance.FadeInAndLoadScene("LoadingScene"));

        }

        if (other.gameObject.CompareTag("EndGoal"))
        {
            MenuManager.Instance.nextSceneName = "EndScene";
            MenuManager.Instance.StartCoroutine(MenuManager.Instance.FadeInAndLoadScene("EndScene"));
        }
        // �����۰� �浹������ �÷��̾� �ȿ� ������Ʈ�� �ְ� ���� �� -> �������� �԰� �����°� ����
        // other.gameObject.transform.SetParent(transform); 
        // other.gameObject.transform.SetParent(null); 

        if (other.gameObject.CompareTag("Quest"))
        { 
            questText.SetActive(false);
            other.gameObject.SetActive(false);
        }
    }
    
    void DoorOpen()
    {
        if (Input.GetKeyDown(KeyCode.E) && !isOPen)
        {
            isOPen = true;
            StartCoroutine(OpenDoor(1.0f));
            RaycastHit hit;            
            Debug.DrawLine(doorCheck.position, doorCheck.forward, Color.green, 1.0f);
            if (Physics.Raycast(doorCheck.position, doorCheck.forward, out hit, doorCheckDistance))
            {

                if (hit.collider.gameObject.CompareTag("Door"))
                {  
                    animator.SetTrigger("OpenDoor");

                    Debug.DrawLine(doorCheck.position, doorCheck.forward, Color.red, 2.0f);
                    Debug.Log("Hit : " + hit.collider.gameObject.name);

                    hit.collider.gameObject.transform.GetComponent<DoorManager>().ChangeDoorOpen();

                }

            }


        }
    }

    IEnumerator OpenDoor(float delay)
    { 
        yield return new WaitForSeconds(delay);
        isOPen = false;
    }




    public void Damaged(float damage)
    {
        if (playerHp > 0)
        {             
            animator.SetTrigger("Damage");
            StartCoroutine(Blood());
            GetComponent<CharacterController>().enabled = false;
            GetComponent<CharacterController>().enabled = true;
            playerHp -= damage;
            gameObject.layer = 10;
            Invoke("DamagedOff", 1.5f);
        }
    }

    private IEnumerator Blood()
    {
        bloodSign.SetActive(true);

        yield return new WaitForSeconds(1f);

        bloodSign.SetActive(false);

    
    }

    void DamagedOff()
    {
        gameObject.layer = 6;
    }



    public void Die()
    {
        if (playerHp <= 0)
        {            
            isStop = true;
            animator.SetTrigger("Die");
            // �׾����� �������̵��� �����غ� - ������ ���콺�� ���ư��� �㸮�� �����̴���������
            walkSpeed = 0;
            runSpeed = 0;
            GameSettingUImanager.instance.gameOverMenu();

        }
    }

        

    void UpdateAimTarget()
    {
        // ī�޶� �߽����� ���콺 ����Ʈ�� ���� �߻�
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        aimTarget.position = ray.GetPoint(10.0f);
    }

    public void WeaponChangeSoundOn()
    {
        SoundManager.instance.PlaySFX("ChangeGun",gameObject.transform.position);
    }

    public void FireSound()
    {
        SoundManager.instance.PlaySFX("FamasShot",rifleFamas.transform.position);
    }

    public void DamagedSound()
    {
        SoundManager.instance.PlaySFX("PlayerDamagedSound", gameObject.transform.position);
    }

    public void GetItemSound()
    {
        SoundManager.instance.PlaySFX("GetItem", gameObject.transform.position);
    }

    public void ReloadSound()
    {
        if (currentWeaponMode == WeaponMode.Rifle)
        {
            SoundManager.instance.PlaySFX("ReLoadSound", gameObject.transform.position);
        }
        else if (currentWeaponMode == WeaponMode.Pistol)
        {
            SoundManager.instance.PlaySFX("PistolReloadSound", gameObject.transform.position);
        }
            
    }

    public void FootSound()
    {
        SoundManager.instance.PlaySFX("StepSound",gameObject.transform.position);
    }

    public void WalkSound()
    {
        SoundManager.instance.PlaySFX("WalkSound", gameObject.transform.position);
    }

    public void OpenDoorSound()
    {
        SoundManager.instance.PlaySFX("OpenDoor", gameObject.transform.position);
    }

    //BoxCast �׸���
    void DebugBox(Vector3 origin, Vector3 direction)
    {
        Vector3 endPoint = origin + direction * castDistance;

        Vector3[] corners = new Vector3[8];
        corners[0] = origin + new Vector3(-boxSize.x, -boxSize.y, -boxSize.z) / 2;
        corners[1] = origin + new Vector3(boxSize.x, -boxSize.y, -boxSize.z) / 2;
        corners[2] = origin + new Vector3(-boxSize.x, boxSize.y, -boxSize.z) / 2;
        corners[3] = origin + new Vector3(boxSize.x, boxSize.y, -boxSize.z) / 2;
        corners[4] = origin + new Vector3(-boxSize.x, -boxSize.y, boxSize.z) / 2;
        corners[5] = origin + new Vector3(boxSize.x, -boxSize.y, boxSize.z) / 2;
        corners[6] = origin + new Vector3(-boxSize.x, boxSize.y, boxSize.z) / 2;
        corners[7] = origin + new Vector3(boxSize.x, boxSize.y, boxSize.z) / 2;

        Debug.DrawLine(corners[0], corners[1], Color.green, 3.0f);
        Debug.DrawLine(corners[1], corners[3], Color.green, 3.0f);
        Debug.DrawLine(corners[3], corners[2], Color.green, 3.0f);
        Debug.DrawLine(corners[2], corners[0], Color.green, 3.0f);
        Debug.DrawLine(corners[4], corners[5], Color.green, 3.0f);
        Debug.DrawLine(corners[5], corners[7], Color.green, 3.0f);
        Debug.DrawLine(corners[7], corners[6], Color.green, 3.0f);
        Debug.DrawLine(corners[6], corners[4], Color.green, 3.0f);
        Debug.DrawLine(corners[0], corners[4], Color.green, 3.0f);
        Debug.DrawLine(corners[1], corners[5], Color.green, 3.0f);
        Debug.DrawLine(corners[2], corners[6], Color.green, 3.0f);
        Debug.DrawLine(corners[3], corners[7], Color.green, 3.0f);
        Debug.DrawRay(origin, direction * castDistance, Color.green);


    }
    
    /*
    // ���� �ε�� �� ȣ��Ǵ� �Լ�
    void OnSceneLoad(Scene scene, LoadSceneMode mode)
    { 
        Debug.Log("Loaded Scene : " + scene.name);
        // �ʱ�ȭ�� �ؾ��Ѵ�.
        // �÷��̾�, AI, Item, Weapon
    
    }
    */
}
