using UnityEngine;
using System.Collections;

public class NewMonoBehaviour : MonoBehaviour
{
    // Use this for initialization
    async void Start()
    {
        RunTest();
    }

    async void RunTest()
    {
        Debug.Log("Has OPENAI_API_KEY: " + !string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("OPENAI_API_KEY")));
        TownSimulation sim = new TownSimulation(2);
        bool res = await sim.simulateInteraction();
    }
}
