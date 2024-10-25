using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class ScoreManaging : MonoBehaviour
{
    public float score = 0f, reward = 0f;
    private TextMeshProUGUI ScoreText;
    public static ScoreManaging Instance { get; private set; }
    void Awake()
    {
        ScoreText = this.gameObject.GetComponent<TextMeshProUGUI>();    
        if (Instance == null){Instance = this;}    
    }

    // Update is called once per frame
    void Update()
    {
       ScoreText.text = "Score: " + score.ToString() + "\nReward: " + reward.ToString(); 
    }
}
