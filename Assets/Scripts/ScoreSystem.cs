using UnityEngine;

namespace DefaultNamespace
{ 
    public class ScoreSystem :  MonoBehaviour
    {
        public int score;
        public TMPro.TMP_Text scoreUIText;

        void Awake()
        {
            score = 0;
            if (scoreUIText == null)
            {
                Debug.LogError("ScoreSystem:: Awake  scoreUIText is null");
                return;
            }
        } 
        public void AddScore(int add)
        {
            score += add;
            scoreUIText.text = score.ToString();
        } 
    } 
}