using System.Collections.Generic;
using UnityEngine;

public class TownResident
{
    public string name;
    public string hash;
    public int age;
    public List<string> backstory;
    public string occupation;
    public string personality;

    public List<string> neighborHashes;

    private static string[] names = { "James", "John", "Robert", "David", "William", "Richard", "Thomas", "Christopher", "Anthony" };
    private static string[] occupations = { "plumber", "hair stylist", "gardener", "billionare", "cleaner", "bicycles" };
    private static string[] personalities = { "sad", "funny", "nihilist", "boring", "temperamental", "accusatory" };

    public TownResident(string newHash)
    {
        name = names[Random.Range(0, names.Length - 1)] + " " + names[Random.Range(0, names.Length - 1)] + " " + names[Random.Range(0, names.Length - 1)];
        age = Random.Range(8, 90);
        backstory = new List<string>();
        occupation = occupations[Random.Range(0, occupations.Length - 1)];
        personality = personalities[Random.Range(0, personalities.Length - 1)];
        neighborHashes = new List<string>();
        hash = newHash;
    }
}