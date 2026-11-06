using System;
using System.Collections;
using DefaultNamespace;
using UnityEngine;
using Random = UnityEngine.Random;


//1. 여러개의 적들을 관리 ( 생성, 위치, 명령)
//2. 
public class EnemyManager : MonoBehaviour
{
    public int maxEnemyCount = 3;
    public EnemyController enemyPrefab;
    public Transform parentCleaner;
    private EnemyController[] enemies = null;
    [SerializeField] private Transform player;
    [SerializeField] [Range (0.5f,10f)]private float moveSpeed = 1.4f;
    
    [SerializeField] [Range(0,2f)] private float enemyPositionArrageOffset = 0.4f;
    [SerializeField] private Transform enemySpawnPosition;
    [SerializeField] private ScoreSystem scoreSystem;

    [SerializeField] private BloodEffectPool bloodEffect;
    
    
    
    void Awake()
    {

        if (enemyPrefab == null)
        {
            Debug.LogError("EnemyManager enemyPrefab is null");

            return;
        }

        if (parentCleaner == null)
        {
            Debug.LogError("EnemyManager parentCleaner is null");

            return;
        }

        if (player == null)
        {
            Debug.LogError("Player is null");

            return;
        }

        if (bloodEffect == null)
        {
            Debug.LogError("BloodEffectPool is null");

            return;
        }
 

        enemies = new EnemyController[maxEnemyCount];
 
            
        // 생성
        for (int i = 0; i < maxEnemyCount; i++)
        {
            enemies[i] = Instantiate(enemyPrefab );
            enemies[i].transform.SetParent(parentCleaner); 
            enemies[i].SetTarget(player);
            enemies[i].SetFillFullHP();
            enemies[i].SetMoveSpeed(moveSpeed);
            enemies[i].SetColor(new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
            enemies[i].OnDied += HandleEnemyDieEvent;
            enemies[i].OnHit += HandleEnemyHitEvent;
        }

        //좌표 설정
        OnArrageAllEnemiesPosition(); 
    }

    // 구독시 생명주기가 다를 수 있어서 더이상 이벤트를 받을 필요가 없는 시점에는 구독을 해제하는 습관을 가져야한다. 
    public void OnDestroy()
    { 
        if (enemies == null) return;

        for (int i = 0; i < maxEnemyCount; i++)
        {
            if (enemies[i] != null) enemies[i].OnDied -= HandleEnemyDieEvent;
            if (enemies[i] != null) enemies[i].OnHit -= HandleEnemyHitEvent;
        } 
    }


    public void HandleEnemyDieEvent(Color color,Vector3 hitPoint, Vector3 direction)
    {
        //1. 죽는 이팩트 처리
        
        bloodEffect.Show(color, hitPoint, direction, BloodEffectPool.BloodEffectType.large);
        
        //2.  죽는 사운드 효과음 처리 
        
        //3.  죽을 때 점수처리..
        scoreSystem.AddScore(10);
    }

    public void HandleEnemyHitEvent(Color color,Vector3 hitPoint, Vector3 direction)
    {
        //1. 맞을 때 이팩트 처리
         
        bloodEffect.Show(color, hitPoint, direction, BloodEffectPool.BloodEffectType.small);
        
        
        //2. 맞을 때 사운드 처리  
    }

    
    

    // 제가 0번키를 누를때마다..적들이 위치로...정렬. 
    public void OnArrageAllEnemiesPosition()
    { 
      //  int enemyArrangeRowX = Random.Range(1, 20);
       // int enemyArrangeRowY = Random.Range(1, 20);

        float rad = 0;
        float radianOffset = (2 * Mathf.PI) / (float)maxEnemyCount;
        float radius =8F;
        
        Vector3 center = player.position;
        for (int i = 0; i < maxEnemyCount; i++)
        {
           // var pos = new Vector3((i % enemyArrangeRowX) * enemyPositionArrageOffset, (i / enemyArrangeRowY) * enemyPositionArrageOffset, 0);

           rad += radianOffset;
           var pos = new Vector3(Mathf.Sin(rad) *radius, Mathf.Cos(rad)*radius, 0);
            enemies[i].transform.position =center + pos;
            enemies[i].gameObject.SetActive(true);
        }
    }
    
     
       

     
}
