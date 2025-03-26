using System.Collections;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI; // Nav mesh ���

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
    
    public float attackRange = 2.0f; // �����Ÿ�
    public float attackDelay = 2.0f; // ���ݵ�����
    private float nextAttackTime = 0.0f; // ���� ���� �ð� ����
    public Transform[] patrolPoints; // ���� ��� ������
    private int currentPoint = 0; // ���� ���� ��� ����
    public float moveSpeed = 2.0f;
    private float trackingRange = 7f; // ���� ����
    private bool isAttack = false; // ���� ����
    public float zombiehp = 10; // ���� HP
    private float distanceToTarget; // Ÿ�ϰ��� �Ÿ� ��� ��
    private bool isWaiting = false; // ���� ��ȯ �� ��� ���� ����
    public float idleTime = 2.0f; // �� ���� ��ȯ �� ��� �ð�

    private Coroutine stateRoutine; 

    private Animator animator;

    public Transform attackPos;
    public Vector3 boxSize = new Vector3(1.0f, 1.0f, 1.0f);
    public float castDistance = 5.0f;
    public LayerMask targetLayer;

    private NavMeshAgent agent;

    // ���� �Ѿ���� ����
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
        // �Ǽ��� ������Ʈ�� ������ ��� ��� -> ����ó�� ������ ������ �ؾ���!
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

    public void ChangeState(ZombieState newState) //�ݺ��ϴٰ� stateRountine�� ���߰� newState�� ���� �ٽ� startCoroutine / ���� �ڵ��� ��� �Լ� ���߽� �Լ��� ���̴� ��찡 ũ��. / �帧 �����ϱ�
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
        Debug.Log(gameObject.name + "�����");
  
        
        while (currentState == ZombieState.Idle)
        {
            animator.SetBool("IsWalk", false);

            // �����°� ���º�ȭ�� �ٽ� ��ž�ڷ�ƾ�ϰ� ���� ���� ���¸� ��ŸƮ�ڷ�ƾ�Ѵ�.

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
        Debug.Log(gameObject.name + "������");
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

                // �޽ø�ũ�� ��������� ������ �ض�
                if (agent.isOnOffMeshLink)
                {
                    StartCoroutine(JumpAcrossLink());    
                }

                if (Vector3.Distance(transform.position, targetPoint.position) < 0.3f)
                {
                    currentPoint = (currentPoint + 1) % patrolPoints.Length;
                }

                // �������°� ��ȭ�� �ٽ� ��ž�ڷ�ƾ�ϰ� ���� ���� ���¸� ��ŸƮ�ڷ�ƾ�Ѵ�.

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
        Debug.Log(gameObject.name + "������");
        animator.SetBool("IsRun", true);

        while (currentState == ZombieState.Chase)
        {
            
            Vector3 direction = (target.position - transform.position).normalized; // ���ⱸ�ϱ�
            agent.speed = 5;
            agent.isStopped = false;
            agent.destination = target.position;

            // �������°� ��ȭ�� �ٽ� ��ž�ڷ�ƾ�ϰ� ���� ���� ���¸� ��ŸƮ�ڷ�ƾ�Ѵ�.
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
        Debug.Log(gameObject.name + "������");
        transform.LookAt(PlayerManager.Instance.transform.position);
        animator.SetTrigger("Attack");
        agent.isStopped = true;
        agent.destination = PlayerManager.Instance.transform.position;
        Invoke("AttackPlayer", 0.5f);

        yield return new WaitForSeconds(1.0f); // ���ݼӵ�

 
        float distance = Vector3.Distance(transform.position, PlayerManager.Instance.transform.position);
        
        if (distance > attackRange) // ���ݹ��� ����� ����, �ƴҽ� ����
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
        // �迭�� �Ǿ 2���� �������� ���°� ����, �迭���ְ� �غ���
        Vector3 orgin = attackPos.position; // ĳ������ �Ǻ��� ���̱⶧���� ���� ����
        Vector3 direction = attackPos.forward;
        float damage = 0.5f;
        DebugBox(orgin, direction);

        RaycastHit[] hits;
        hits = Physics.BoxCastAll(orgin, boxSize / 2, direction, Quaternion.identity, castDistance, targetLayer);

        foreach (RaycastHit hit in hits)
        {
            
            if (hit.collider.tag == "Player")
            {
                Debug.Log(hit.collider.tag + ": ������");
                hit.collider.gameObject.GetComponent<PlayerManager>().Damaged(damage);

                // �÷��̾� �ǰ� 0 ���ϸ� Ÿ���� �ٽ� ���� - �հ����ٰ� �����س���

            }
        }
    }
    public void TakeDamage(float damage)
    {
        Debug.Log(gameObject.name + ":" + damage + "������ �޴���");
        zombiehp -= damage;
        animator.SetTrigger("Damage");
        ZombieDamagedSound();
        trackingRange = 30.0f; // �Ѹ����� �������� �þ�鼭 �����ϱ�
        gameObject.layer = 10;
        moveSpeed = 0;
        
        // StartCoroutine(StunZombie(0.2f)); // ���� ���Ͻð� 
        Invoke("DamageOff", 0.4f); // ���� �����ð�
        
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
        Debug.Log(gameObject.name + "���");
        animator.SetTrigger("Die");
        ZombieDieSound();
        gameObject.GetComponent<CapsuleCollider>().enabled = false;
        yield return new WaitForSeconds(2.0f);

        gameObject.SetActive(false); ;
        

    }

    private IEnumerator JumpAcrossLink()
    {
        Debug.Log(gameObject.name + "���� ����");
        // ����������
        isJumping = true;
        // ������ �޽ð� ���� �����̰�
        agent.isStopped = true;

        // NavMeshLink�� ���۰� �� ��ǥ�� ��������
        OffMeshLinkData linkData = agent.currentOffMeshLinkData;
        Vector3 startPose = linkData.startPos;
        Vector3 endPose = linkData.endPos;

        // ���� ��� ���(�������� �׸��� ����)
        float elsapedTime = 0;
        while (elsapedTime < jumpDuration)
        { 
            float t = elsapedTime / jumpDuration;
            Vector3 currentPosition = Vector3.Lerp(startPose, endPose, t);
            currentPosition.y += Mathf.Sin(t * Mathf.PI) * jumpHeight; // ������ ���
            transform.position = currentPosition;
            
            yield return null;
        }
        // �������� ��ġ
        transform.position = endPose;

        // �޽ø�ũ �Ϸ� ���� ���·� ��� �簳
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
