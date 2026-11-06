using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
    public class BloodEffectController : MonoBehaviour
    {
        Rigidbody2D rigidbody2D;
        SpriteRenderer spriteRenderer; 
        public void Awake()
        { 
            rigidbody2D = GetComponent<Rigidbody2D>();
            spriteRenderer =  GetComponent<SpriteRenderer>();
        }

        public void Initialize()
        {
            spriteRenderer.color = Color.white;
            transform.position = Vector2.zero;
            rigidbody2D.linearVelocity = Vector2.zero;
            rigidbody2D.angularVelocity =0;
            
        }

        public void Show(Color color, Vector3 position, Vector3 direction)
        {

            Initialize();
            
            spriteRenderer.color = color;
        
            Vector3 newDir = Quaternion.Euler(0,0,Random.Range(-20,20)) * direction;
            
            transform.position = position;
        
            rigidbody2D.AddForce(newDir * Random.Range(4f,8f), ForceMode2D.Impulse);
            rigidbody2D.AddTorque(  Random.Range(7f,20f), ForceMode2D.Impulse);
            transform.localScale =Vector3.one*Random.Range(0.4f,0.8f);

            StartCoroutine(BloodEffectUpdate( )); 
        }
        
        IEnumerator BloodEffectUpdate()
        { 
            while (true)
            {

                if (transform.localScale.x < 0.01f)
                {
                    gameObject.SetActive(false);
                    yield break;
                }

                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, spriteRenderer.color.a * 0.8f);
                transform.localScale = transform.localScale * 0.7f;

                yield return new WaitForSeconds(0.3f);
            }
        }

    }
}