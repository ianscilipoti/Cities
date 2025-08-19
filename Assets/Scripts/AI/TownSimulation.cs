using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class TownSimulation
{
    public Dictionary<string, TownResident> residents;
    private const string SAVE_FILE_NAME = "town_simulation_";
    private int seed;

    public TownSimulation (int numResidents, int inSeed)
    {
        residents = new Dictionary<string, TownResident>();
        for (int i = 0; i < residents.Count; i ++) {
            //generate random hash
            residents.Add(i + "", new TownResident(i+""));
        }
        seed = inSeed;
    }

    public TownSimulation (List<TownResident> newResidents, int inSeed) {
        residents = new Dictionary<string, TownResident>();
        foreach (TownResident res in newResidents)
        {
            residents.Add(res.hash, res);
        }
        seed = inSeed;
    }

    public TownSimulation(int inSeed)
    {
        if (HasSaveFile(inSeed))
        {
            LoadFromFile(inSeed);
        }
        else
        {
            Debug.LogError("could not load from this save file");
        }
        seed = inSeed;
    }

    [System.Serializable]
    private class ResponseObject
    {
        public string character1Update;
        public string character2Update;
    }

    public async Task<bool> simulateTown (int numInteractions, bool save = true)
    {
        List<Task<Dictionary<TownResident, string>>> interactionTasks = new List<Task<Dictionary<TownResident, string>>>();
        int counter = 0;
        while (counter < numInteractions)
        {
            foreach (KeyValuePair<string, TownResident> kvp in residents)
            {
                counter++;
                if (kvp.Value.neighborHashes.Count == 0)
                {
                    Debug.LogWarning("resident has no neighbors");
                    continue;
                }

                interactionTasks.Add(simulateInteraction(kvp.Key, kvp.Value.neighborHashes[Random.Range(0, kvp.Value.neighborHashes.Count)]));
            }
        }

        Dictionary<TownResident, string>[] results = await Task.WhenAll(interactionTasks);
        Debug.Log(numInteractions + " simulations complete");
        foreach (Dictionary<TownResident, string> completedTask in results)
        {
            if (completedTask != null)
            {
                foreach (KeyValuePair<TownResident, string> kvp in completedTask)
                {
                    kvp.Key.backstory.Add(kvp.Value);
                }
            }
            else
            {
                Debug.LogWarning("simulation task failed");
            }
        }
        if (save)
        {
            SaveToFile();
        }
        return true;
    }

    //returns update to the backstory for two characters
    public async Task<Dictionary<TownResident,string>> simulateInteraction (string res1Hash, string res2Hash)
    {
        int residentAind = Random.Range(0, residents.Count - 1);
        int residentBind = Random.Range(0, residents.Count - 1);

        if (residentAind == residentBind)
        {
            residentBind = (residentBind + 1) % residents.Count; 
        }

        TownResident resA = residents[res1Hash];
        TownResident resB = residents[res2Hash];

        string devPrompt = $"You are simulating interactions between fictional human residents in a virtual city. \n" +
            $"The simulation is not a roleplay with the user — it is a behind-the-scenes event generator. \n" +
            $"Your task in every request is to:\n" +
            $"1. Use the provided character profiles to imagine a brief, natural, and interaction between the selected residents.\n" +
            $"2. Don't be too on the nose. While including details about profession / personality, the way they influence interactions should be natural and subtle.\n" +
            $"3. When a character refers to someone else in the story, use their full name. \n" +
            $"4. Keep the output short and sweet (< 120 words). \n" +
            $"5. Not all the neighbors will like each other. In some cases, their interests will conflict. Not all conflict resolves.\n" +
            $"6. Each character update should be formatted in first person past tense - as if they were telling the story of what happened.\n" +
            $"7. Return output ONLY in structured JSON:\n" +
            $"{{\n     \"character1Update\": \"Text to append to their backstory\",\n     \"character2Update\": \"Text to append to their backstory\"\n}}\n" +
            $"Follow the formatting exactly, without additional commentary or non-JSON text.";

        string fullBackstoryA = "";
        foreach(string entry in resA.backstory)
        {
            fullBackstoryA += entry + "\n";
        }
        string fullBackstoryB = "";
        foreach (string entry in resB.backstory)
        {
            fullBackstoryB += entry + "\n";
        }

        string userPrompt = $"Character A:\n" +
            $"Name: {resA.name}\n" +
            $"Age: {resA.age}\n" +
            $"Occupation: {resA.occupation}\n" +
            $"Personality: {resA.personality}\n" +
            $"Backstory summary: {fullBackstoryA}\n\n" +
            $"Character B:\nName: {resB.name}\n" +
            $"Age: {resB.age}\n" +
            $"Occupation: {resB.occupation}\n" +
            $"Personality: {resB.personality}\n" +
            $"Backstory summary: {fullBackstoryB}\n\n" +
            $"The updates for both characters should recount the same event from their two perspectives.";

        try
        {
            CancellationTokenSource source = new CancellationTokenSource(System.TimeSpan.FromSeconds(20));
            CancellationToken token = source.Token;
            string response = await AiService.ChatAsync(devPrompt, userPrompt, token);
            var obj = JObject.Parse(response);

            Dictionary<TownResident, string> updates = new Dictionary<TownResident, string>();

            updates[resA] = (string)obj["character1Update"];
            updates[resB] = (string)obj["character2Update"];

            return updates;
        }
        catch (System.Exception e)
        {
            Debug.LogError("AI call failed: " + e);
            return null;
        }
    }

    public bool SaveToFile()
    {
        try
        {
            // Create a serializable data structure
            var saveData = new TownSimulationSaveData
            {
                residents = residents,
                saveTimestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                seed = seed,
                version = "1.0"
            };

            // Convert to JSON
            string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            
            // Get the save path
            string savePath = Path.Combine(Application.persistentDataPath, GetSaveName(seed));
            
            // Write to file
            File.WriteAllText(savePath, json);
            
            Debug.Log($"Town simulation saved to: {savePath}");
            Debug.Log($"Saved {residents.Count} residents");
            
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save town simulation: {e.Message}");
            return false;
        }
    }

    public bool LoadFromFile(int seedToLoad)
    {
        try
        {
            string savePath = Path.Combine(Application.persistentDataPath, GetSaveName(seedToLoad));
            
            if (!HasSaveFile(seedToLoad))
            {
                Debug.LogWarning($"No save file found at: {savePath}");
                return false;
            }
            
            // Read the JSON file
            string json = File.ReadAllText(savePath);
            
            // Deserialize the data
            var saveData = JsonConvert.DeserializeObject<TownSimulationSaveData>(json);
            
            if (saveData?.residents != null)
            {
                residents = saveData.residents;
                Debug.Log($"Loaded {residents.Count} residents from save file");
                Debug.Log($"Save timestamp: {saveData.saveTimestamp}");
                Debug.Log($"Save seed: {saveData.seed}");
                Debug.Log($"Save version: {saveData.version}");
                return true;
            }
            else
            {
                Debug.LogError("Save file is corrupted or empty");
                return false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load town simulation: {e.Message}");
            return false;
        }
    }

    public static bool HasSaveFile(int seedToLoad)
    {
        string savePath = Path.Combine(Application.persistentDataPath, GetSaveName(seedToLoad));
        return File.Exists(savePath);
    }

    public static string GetSaveName (int forSeed)
    {
        return SAVE_FILE_NAME + forSeed + ".json";
    }

    /// <summary>
    /// Gets the save file path
    /// </summary>
    /// <returns>The full path to the save file</returns>
    //public string GetSaveFilePath()
    //{
    //    return Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
    //}

    /// <summary>
    /// Deletes the save file
    /// </summary>
    /// <returns>True if deletion was successful, false otherwise</returns>
    //public bool DeleteSaveFile()
    //{
    //    try
    //    {
    //        string savePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
    //        if (File.Exists(savePath))
    //        {
    //            File.Delete(savePath);
    //            Debug.Log("Save file deleted successfully");
    //            return true;
    //        }
    //        return false;
    //    }
    //    catch (System.Exception e)
    //    {
    //        Debug.LogError($"Failed to delete save file: {e.Message}");
    //        return false;
    //    }
    //}
}

[System.Serializable]
public class TownSimulationSaveData
{
    public Dictionary<string, TownResident> residents;
    public int seed;
    public string saveTimestamp;
    public string version;
}

