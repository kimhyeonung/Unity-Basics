using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace FWeapon
{
    public enum WeaponType // 프로젝타일의움직임 정의
    {
        Normal=0,
        Piercing=1,
        Homing=2,
    }

    public enum FirePatternType
    {
        Normal=0,
        Burst=1,
        Spread=2,
        Weave=3,
        Triple=4,
    }

    public class RuntimeWeapon
    {
        public string weaponID;
        public WeaponType weaponType;
        public FirePatternType fireType; 
        
        public ProjectilePool projectilePool;
        public WeaponDefinition  definition;
        public IFirePattern    firePattern;

       public RuntimeWeapon(string id, WeaponType type, FirePatternType fireType,IFirePattern firePattern, ProjectilePool projectilePool, WeaponDefinition definition)
        {
            this.weaponID = id;
            this.weaponType  = type;
            this.fireType = fireType;
            this.projectilePool = projectilePool;
            this.definition = definition;
            this.firePattern = firePattern;
        }
    }
    
    
    public class WeaponController : MonoBehaviour
    {  
        [Header("Settings")]  
        public ProjectilePool projectilePoolPrefab =null; // 풀링 프리펩 가져오기
        [SerializeField] private WeaponLibrary weaponLibList = null;   // 기획자 정의한 웨폰 SO 들을 들고오자
        
        
        
        [SerializeField] private TextMeshProUGUI TextUI_weaponID = null;
        readonly Dictionary<string, RuntimeWeapon> weapons = new(); // 웨폰 ID로..그 많은 데이터들을 효율적으로 저장해놓자.

        //스위치 웨폰을 위한 변수들
        public int currentWeaponIndex = 0;
        public List<string> weaponIDList = new List<string>();
        
       

       
        void Awake() 
        {
            if (weaponLibList == null || weaponLibList.Count == 0)
            {
               Debug.LogError("WeaponController"+"weaponLibrary List is empty");

               return;
            }

            if (projectilePoolPrefab == null)
            {
                Debug.LogError("WeaponController"+"MissilePoolPrefab is null");
            }
            
            BuildWeapons();
            currentWeaponIndex = 0;
            ChangeUIWeaponName(weaponIDList[currentWeaponIndex]);
        }

        void ChangeUIWeaponName(string name)
        {
            if(TextUI_weaponID) TextUI_weaponID.text = name;
        }

        void BuildWeapons()
        {
            foreach (var w in weaponLibList.Weaponlib) // 등록한 스크립터블오브젝트들을
            {
                 if (!weapons.ContainsKey(w.weaponID)) // 딕셔너리에..이미 등록해놓은 ID면 패스
                {
                    // 탄환체 pooling 
                    ProjectilePool pool = Instantiate(projectilePoolPrefab, transform);
                    pool.name = "MissilePool_" + w.weaponType;
                    pool.Build(w, 20);
                    
                    // 발사방식에 맞는 IFirePattern을 생성해주자.
                    IFirePattern pattern = FirePatternFactory.CreateFirePattern(w.firePatternType);
                    
                    // 런타임웨폰데이터 생성
                    RuntimeWeapon runtimeWeapon = new RuntimeWeapon(w.weaponID,w.weaponType, w.firePatternType,pattern,  pool,w);
                    
                    // 딕셔너리에 저장
                    weapons.Add(w.weaponID,runtimeWeapon);
                    
                    // 스위칭 웨폰을 위해서 ID리스트를 딕셔너리 저장순서로 저장
                    weaponIDList.Add(w.weaponID);
                   
                }
            }

        }

        RuntimeWeapon GetWeapon(string weaponID)
        {
            weapons.TryGetValue(weaponID, out RuntimeWeapon runtimeWeapon);
            return runtimeWeapon;
        }

       public void Fire( Transform startPos, Vector3 direction)
        { 
            
           var weapon = GetWeapon(weaponIDList[currentWeaponIndex] );
           if(weapon == null)return;
           
           var f = weapon.firePattern;
           var p = weapon.projectilePool;
           
           StartCoroutine(f.Fire(p,startPos, direction,weapon.definition.BurstCount));
        } 

        public void SwitchWeapon( )
        {
             currentWeaponIndex++;
            if (currentWeaponIndex >= weapons.Count) currentWeaponIndex = 0; 
            
            ChangeUIWeaponName(weaponIDList[currentWeaponIndex]);
        }
    }
}
