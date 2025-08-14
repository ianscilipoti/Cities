using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json.Linq;

public class TownSimulation
{
    public Resident[] residents;


    public TownSimulation(int numResidents)
    {
        residents = new Resident[numResidents];
        for (int i = 0; i < residents.Length; i ++) {
            residents[i] = new Resident();
        }
    }

    [System.Serializable]
    private class ResponseObject
    {
        public string character1Update;
        public string character2Update;
    }

    public async Task<bool> simulateInteraction ()
    {
        int residentAind = Random.Range(0, residents.Length - 1);
        int residentBind = Random.Range(0, residents.Length - 1);

        if (residentAind == residentBind)
        {
            residentBind = (residentBind + 1) % residents.Length; 
        }

        Resident resA = residents[Random.Range(0, residents.Length - 1)];
        Resident resB = residents[Random.Range(0, residents.Length - 1)];

        string devPrompt = $"You are simulating interactions between fictional human residents in a virtual city. \n" +
            $"The simulation is not a roleplay with the user — it is a behind-the-scenes event generator. \n" +
            $"Your task in every request is to:\n" +
            $"1. Use the provided character profiles and recent histories to imagine a brief, natural, and personality-consistent interaction between the selected residents.\n" +
            $"2. Ensure interactions make sense given their prior relationship, personalities, and occupations.\n" +
            $"3. Incorporate small talk, local events, and evolving relationships — these should be consistent with prior summaries when provided.\n" +
            $"4. Avoid overly dramatic or unrealistic events unless instructed otherwise; tone should feel like everyday neighborly encounters.\n" +
            $"5. Return output ONLY in structured JSON:\n" +
            $"{{\n     \"character1Update\": \"Text to append to their backstory\",\n     \"character2Update\": \"Text to append to their backstory\"\n}}\n" +
            $"Follow the formatting exactly, without additional commentary or non-JSON text.";

        string userPrompt = $"Character A:\n" +
            $"Name: {resA.name}\n" +
            $"Age: {resA.age}\n" +
            $"Occupation: {resA.occupation}\n" +
            $"Personality: {resA.personality}\n" +
            $"Backstory summary: {resA.backstory}\n\n" +
            $"Character B:\nName: {resB.name}\n" +
            $"Age: {resB.age}\n" +
            $"Occupation: {resB.occupation}\n" +
            $"Personality: {resB.personality}\n" +
            $"Backstory summary: {resB.backstory}\n\n" +
            $"Base the conversation on their personalities, occupations, age, and backstory. \n";

        try
        {
            CancellationTokenSource source = new CancellationTokenSource(System.TimeSpan.FromSeconds(20));
            CancellationToken token = source.Token;
            string response = await AiService.ChatAsync(devPrompt, userPrompt, token);
            var obj = JObject.Parse(response);

            resA.backstory += obj["character1Update"];
            resB.backstory += obj["character2Update"];

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("AI call failed: " + e);
            return false;
        }
    }

    public class Resident
    {
        public string name;
        public int age;
        public string backstory;
        public string occupation;
        public string personality;

        private static string[] names = { "James", "John", "Robert", "David", "William", "Richard", "Thomas", "Christopher", "Anthony" };
        private static string[] occupations = { "plumber", "hair stylist", "AI engineer", "billionare", "cleaner", "bicycles" };
        private static string[] personalities = { "grumper", "comedian", "nihilist", "boring", "free will user", "scared" };

        public Resident()
        {
            name = names[Random.Range(0, names.Length - 1)] + " " + names[Random.Range(0, names.Length - 1)] + " " + names[Random.Range(0, names.Length - 1)];
            age = Random.Range(8, 90);
            backstory = "";
            occupation = occupations[Random.Range(0, occupations.Length - 1)];
            personality = personalities[Random.Range(0, personalities.Length - 1)];
        }
    }
}

