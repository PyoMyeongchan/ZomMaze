using System.Collections;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI; // Nav mesh 사용

public enum ZombieState
{
    Idle,
    Patrol,
    Chase,
    Attack,
    Die

}


public class ZombieManager : MonoBehaviour
{
    public ZombieState currentState = ZombieState.Idle;
    
    public float attackRange = 2.0f; // 사정거리
    public float attackDelay = 2.0f; // 공격딜레이
    private float nextAttackTime = 0.0f; // 다음 공격 시간 관리
    public Transform[] patrolPoints; // 순찰 경로 지점들
    private int currentPoint = 0; // 현재 순찰 경로 지점
    public float moveSpeed = 2.0f;
    private float trackingRange = 7f; // 추적 범위
    private bool isAttack = false; // 공격 상태
    public float zombiehp = 10; // 좀비 HP
    private float distanceToTarget; // 타켓과의 거리 계산 값
    private bool isWaiting = false; // 상태 전환 후 대기 상태 여부
    public float idleTime = 2.0f; // 각 상태 전환 후 대기 시간

    private Coroutine stateRoutine; 

    private Animator animator;

    public Transform attackPos;
    public Vector3 boxSize = new Vector3(1.0f, 1.0f, 1.0f);
    public float castDistance = 5.0f;
    public LayerMask targetLayer;

    private NavMeshAgent agent;

    // 벽을 넘어서오는 좀비
    private bool isJumping = false;
    private Rigidbody rb;
    public float jumpHeight = 2.0f;
    public float jumpDuration = 1.0f;
    private NavMeshLink[] navMeshLinks;

    private bool isStunned = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        ChangeState(currentState);
        rb = GetComponent<Rigidbody>();
        // 실수로 컴포넌트를 빼먹을 경우 대비 -> 예외처리 나머지 위에도 해야함!
        if (rb == null)
        { 
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;

        navMeshLinks = FindObjectsOfType<NavMeshLink>();

    }

    // Update is called once per frame
    void Update()
    {
        distanceToTarget = Vector3.Distance(transform.position, PlayerManager.Instance.transform.position);

    }

    public void ChangeState(ZombieState newState) //반복하다가 stateRountine을 멈추고 newState로 실행 다시 startCoroutine / 이전 코드의 경우 함수 남발시 함수가 꼬이는 경우가 크다. / 흐름 생각하기
    {
        if (isJumping) 
        { 
            return; 
        }

        if (stateRoutine != null)
        {
            StopCoroutine(stateRoutine);
        
        }
        currentState = newState;

        switch (currentState)
        { 
            case ZombieState.Idle:
                stateRoutine = StartCoroutine(Idle());
                break;
            case ZombieState.Patrol:
                stateRoutine = StartCoroutine(Patrol());
                break;
            case ZombieState.Chase:
                stateRoutine = StartCoroutine(Chase(PlayerManager.Instance.transform));
                break;
            case ZombieState.Attack:
                stateRoutine = StartCoroutine(Attack());
                break;
            case ZombieState.Die:
                stateRoutine = StartCoroutine(Die());
                break;
            

        }

    }

    private IEnumerator Idle()    
    {
        Debug.Log(gameObject.name + "대기중");
  
        
        while (currentState == ZombieState.Idle)
        {
            animator.SetBool("IsWalk", false);

            // 대기상태가 상태변화시 다시 스탑코루틴하고 다음 현재 상태를 스타트코루틴한다.

            float distance = Vector3.Distance(transform.position, PlayerManager.Instance.transform.position);

            if (distance < trackingRange && attackRange < distance)
            {
                ChangeState(ZombieState.Chase);
                yield break;
            }
            
            else if (distance < attackRange)
            {
                ChangeState(ZombieState.Attack);
                yield break;
            }
            
            yield return null;
        }
        
    }

    private IEnumerator Patrol()
    {
        Debug.Log(gameObject.name + "순찰중");
        animator.SetBool("IsWalk", true);


        while (currentState == ZombieState.Patrol)
        {
            if (patrolPoints.Length > 0)
            {
                moveSpeed = 1.0f;                
                Transform targetPoint = patrolPoints[currentPoint];
                Vector3 direction = (targetPoint.position - transform.position).normalized;
                agent.speed = moveSpeed;
                agent.isStopped = false;
                agent.destination = targetPoint.position;

                // 메시링크에 가까워지면 무엇을 해라
                if (agent.isOnOffMeshLink)
                {
                    StartCoroutine(JumpAcrossLink());    
                }

                if (Vector3.Distance(transform.position, targetPoint.position) < 0.3f)
                {
                    currentPoint = (currentPoint + 1) % patrolPoints.Length;
                }

                // 순찰상태가 변화시 다시 스탑코루틴하고 다음 현재 상태를 스타트코루틴한다.

                float distance = Vector3.Distance(transform.position, PlayerManager.Instance.transform.position);

                if (distance < trackingRange && attackRange < distance)
                {
                    ChangeState(ZombieState.Chase);
                    yield break;

                }                
                else if (distance < attackRange)
                {
                    ChangeState(ZombieState.Attack);
                    yield break;

                } 
                
                
            }
            yield return null;
        }
    }



    private IEnumerator Chase(Transform target)
    {
        Debug.Log(gameObject.name + "추적중");
        animator.SetBool("IsRun", true);

        while (currentState == ZombieState.Chase)
        {
            
            Vector3 direction = (target.position - transform.position).normalized; // 방향구하기
            agent.speed = 5;
            agent.isStopped = false;
            agent.destination = target.position;

            // 추적상태가 변화시 다시 스탑코루틴하고 다음 현재 상태를 스타트코루틴한다.
            float distance = Vector3.Distance(transform.position, target.position);

            
            if (distance < attackRange)
            {
                ChangeState(ZombieState.Attack);
                yield break;
            }
            
            if (distance >trackingRange)
            {
                ChangeState(ZombieState.Patrol);
                animator.SetBool("IsRun", false);
                yield break;

            }            
            
            yield return null;
        }
        
    }

    private IEnumerator Attack()   
    {
        Debug.Log(gameObject.name + "공격중");
        transform.LookAt(PlayerManager.Instance.transform.position);
        animator.SetTrigger("Attack");
        agent.isStopped = true;
        agent.destination = PlayerManager.Instance.transform.position;
        Invoke("AttackPlayer", 0.5f);

        yield return new WaitForSeconds(1.0f); // 공격속도

 
        float distance = Vector3.Distance(transform.position, PlayerManager.Instance.transform.position);
        
        if (distance > attackRange) // 공격범위 벗어나면 추적, 아닐시 공격
        {
            ChangeState(ZombieState.Chase);
            yield break;
        }
        else
        {
            ChangeState(ZombieState.Attack);

            yield break;

        }        

    }

    void AttackPlayer()
    {
        // 배열로 되어서 2번씩 데미지가 들어가는거 같음, 배열없애고 해보기
        Vector3 orgin = attackPos.position; // 캐릭터의 피봇이 발이기때문에 따로 설정
        Vector3 direction = attackPos.forward;
        float damage = 0.5f;
        DebugBox(orgin, direction);

        RaycastHit[] hits;
        hits = Physics.BoxCastAll(orgin, boxSize / 2, direction, Quaternion.identity, castDistance, targetLayer);

        foreach (RaycastHit hit in hits)
        {
            
            if (hit.collider.tag == "Player")
            {
                Debug.Log(hit.collider.tag + ": 공격중");
                hit.collider.gameObject.GetComponent<PlayerManager>().Damaged(damage);

                // 플레이어 피가 0 이하면 타켓을 다시 설정 - 먼곳에다가 저장해놓기

            }
        }
    }
    public void TakeDamage(float damage)
    {
        Debug.Log(gameObject.name + ":" + damage + "데미지 받는중");
        zombiehp -= damage;
        animator.SetTrigger("Damage");
        ZombieDamagedSound();
        trackingRange = 30.0f; // 총맞으면 추적범위 늘어나면서 오게하기
        gameObject.layer = 10;
        moveSpeed = 0;
        
        // StartCoroutine(StunZombie(0.2f)); // 좀비 스턴시간 
        Invoke("DamageOff", 0.4f); // 좀비 무적시간
        
        if (zombiehp <= 0)
        {
            ChangeState(ZombieState.Die);
        }
       
    }

    private IEnumerator StunZombie(float duration)
    {
        isStunned = true;
        agent.isStopped = true;
        yield return new WaitForSeconds(duration);
        isStunned = false;
        agent.isStopped = false;
    }

    void DamageOff()
    {
        gameObject.layer = 3;
    }

    private IEnumerator Die()
    {
        Debug.Log(gameObject.name + "사망");
        animator.SetTrigger("Die");
        ZombieDieSound();
        gameObject.GetComponent<CapsuleCollider>().enabled = false;
        yield return new WaitForSeconds(2.0f);

        gameObject.SetActive(false); ;
        

    }

    private IEnumerator JumpAcrossLink()
    {
        Debug.Log(gameObject.name + "좀비 점프");
        // 점프했을때
        isJumping = true;
        // 좀비의 메시가 멈춘 상태이고
        agent.isStopped = true;

        // NavMeshLink의 시작과 끝 좌표를 가져오기
        OffMeshLinkData linkData = agent.currentOffMeshLinkData;
        Vector3 startPose = linkData.startPos;
        Vector3 endPose = linkData.endPos;

        // 점프 경로 계산(포물선을 그리며 점프)
        float elsapedTime = 0;
        while (elsapedTime < jumpDuration)
        { 
            float t = elsapedTime / jumpDuration;
            Vector3 currentPosition = Vector3.Lerp(startPose, endPose, t);
            currentPosition.y += Mathf.Sin(t * Mathf.PI) * jumpHeight; // 포물선 경로
            transform.position = currentPosition;
            
            yield return null;
        }
        // 도착점에 위치
        transform.position = endPose;

        // 메시링크 완료 이전 상태로 경로 재개
        agent.CompleteOffMeshLink();
        agent.isStopped = false;
        isJumping = false;

    }

    void ZombieIdleSound()
    {
        SoundManager.instance.PlaySFX("ZombieIdle", gameObject.transform.position);
    }
    
    void ZombieAttackSound()
    {
        SoundManager.instance.PlaySFX("ZombieAttack", gameObject.transform.position);
    }

    void ZombieDamagedSound()
    {
        SoundManager.instance.PlaySFX("ZombieDamagedSound", gameObject.transform.position);
    }

    void ZombieDieSound()
    {
        SoundManager.instance.PlaySFX("ZombieDie", gameObject.transform.position);
    }

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
}
