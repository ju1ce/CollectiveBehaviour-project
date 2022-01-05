using System.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine.UI;
using Unity.Transforms;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public Text countText;

    private int count;
    private int totalFish;
    private float time; 
    

    private void DisplayCount()
    {
        countText.text = "Fish caught: " + count + "\nFish alive: " + (totalFish - count) + "\nExecution time: " + (time/Globals.timestep).ToString("n1") + "s";
    }
    public void IncreaseCount()
    {
        count++;
        DisplayCount();
    }

    public void SetTotalFish(int num)
    {
        totalFish = num;
    }
    // Start is called before the first frame update
    void Start()
    {
        //Time.timeScale = 10f;

        time = 0f;
        instance = this;
        count = 0;
        totalFish = Globals.TotalFish;
        DisplayCount();
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        DisplayCount();
    }
}
