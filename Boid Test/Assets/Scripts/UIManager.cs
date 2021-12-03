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
    

    private void DisplayCount()
    {
        countText.text = "Fish caught: " + count;
    }
    public void IncreaseCount()
    {
        count++;
        DisplayCount();
    }
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        count = 0;
        DisplayCount();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
