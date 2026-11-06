using System.Collections.Generic;
using DefaultNamespace;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FWeapon
{
    public class ThreadloomArena : MonoBehaviour
    {
        public WeaponController weapon;
        public WeaponLibrary library;
        public Transform player;
        public Transform muzzle;
        public Transform pointer;
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI poolText;
        public List<EnemyController> targets = new List<EnemyController>();
        public int Hits { get; private set; }
        public int Defeated { get; private set; }

        readonly Dictionary<EnemyController, Vector3> spawnPositions = new Dictionary<EnemyController, Vector3>();
        readonly Dictionary<EnemyController, Color> spawnColors = new Dictionary<EnemyController, Color>();
        readonly Dictionary<EnemyController, Vector3> spawnScales = new Dictionary<EnemyController, Vector3>();
        readonly Dictionary<EnemyController, float> respawnTimes = new Dictionary<EnemyController, float>();
        float nextFireTime;
        Vector3 aim = Vector3.right;
        void Start()
        {
            ScreenWorldBounds.Initialize(Camera.main);
            foreach (EnemyController target in targets)
            {
                spawnPositions[target] = target.transform.position;
                spawnColors[target] = target.GetComponent<SpriteRenderer>().color;
                spawnScales[target] = target.transform.localScale;
                respawnTimes[target] = 0;
                target.SetFillFullHP();
                target.OnHit += OnHit;
                target.OnDied += OnDied;
            }
        }

        void OnDestroy()
        {
            foreach (EnemyController target in targets)
            {
                if (target != null)
                {
                    target.OnHit -= OnHit;
                    target.OnDied -= OnDied;
                }
            }
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (keyboard != null)
            {
                Vector2 movement = Vector2.zero;
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                {
                    movement.y++;
                }

                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                {
                    movement.y--;
                }

                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                {
                    movement.x--;
                }

                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                {
                    movement.x++;
                }

                Vector3 position = player.position + (Vector3)movement.normalized * (4 * Time.deltaTime);
                position.x = Mathf.Clamp(position.x, -9.6f, 9.6f);
                position.y = Mathf.Clamp(position.y, -4.3f, 4.3f);
                player.position = position;
                if (keyboard.tabKey.wasPressedThisFrame)
                {
                    weapon.SwitchWeapon();
                }

                if (keyboard.rKey.wasPressedThisFrame)
                {
                    ResetTargets();
                }
            }

            if (mouse != null)
            {
                Vector3 cursor = Camera.main.ScreenToWorldPoint(new Vector3(mouse.position.ReadValue().x, mouse.position.ReadValue().y, 10));
                cursor.z = 0;
                SetAim(cursor - player.position);
            }

            if ((mouse != null && mouse.leftButton.isPressed) || (keyboard != null && keyboard.spaceKey.isPressed))
            {
                TryFire();
            }

            foreach (EnemyController target in targets)
            {
                if (!target.gameObject.activeSelf)
                {
                    if (respawnTimes[target] == 0)
                    {
                        respawnTimes[target] = Time.time + 2.5f;
                    }

                    if (Time.time >= respawnTimes[target])
                    {
                        Respawn(target);
                    }
                }
            }

            scoreText.text = "HITS  " + Hits.ToString("000") + "     CLEARED  " + Defeated.ToString("00");
            int active = 0;
            int total = 0;
            foreach (ProjectilePool pool in weapon.GetComponentsInChildren<ProjectilePool>())
            {
                total += pool.missiles.Count;
                foreach (ProjectileController projectile in pool.missiles)
                {
                    if (projectile.flying)
                    {
                        active++;
                    }
                }
            }

            poolText.text = "POOL  " + active.ToString("00") + " / " + total.ToString("00");
        }

        public void SetAim(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.001f)
            {
                return;
            }

            aim = direction.normalized;
            muzzle.position = player.position + aim * 0.55f;
            pointer.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg);
        }

        public bool TryFire()
        {
            if (Time.time < nextFireTime)
            {
                return false;
            }

            WeaponDefinition definition = library.Weaponlib[weapon.currentWeaponIndex];
            nextFireTime = Time.time + Mathf.Max(0.1f, definition.cooldown);
            weapon.Fire(muzzle, aim);
            return true;
        }

        public void ResetTargets()
        {
            foreach (EnemyController target in targets)
            {
                Respawn(target);
            }
        }

        void Respawn(EnemyController target)
        {
            target.transform.position = spawnPositions[target];
            target.gameObject.SetActive(true);
            target.SetFillFullHP();
            target.SetColor(spawnColors[target]);
            target.transform.localScale = spawnScales[target];
            respawnTimes[target] = 0;
        }

        void OnHit(Color color, Vector3 position, Vector3 direction)
        {
            Hits++;
        }

        void OnDied(Color color, Vector3 position, Vector3 direction)
        {
            Defeated++;
        }
    }
}
