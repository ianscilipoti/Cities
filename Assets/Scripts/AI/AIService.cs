using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Linq;

public static class AiService
{
	private const string EndPoint = "https://api.openai.com/v1/responses";

	[Serializable] private class ContentPart { public string type; public string text; }
	[Serializable] private class InputMessage { public string role; public ContentPart[] content; }
	[Serializable] private class TextFormat { public Format format; public string verbosity; }
	[Serializable] private class Format { public string type; }
	[Serializable] private class Reasoning { public string effort; }

	[Serializable]
	private class Request
	{
		public string model;
		public InputMessage[] input;
		public TextFormat text;
		public Reasoning reasoning;
		public object[] tools;   // empty array okay
		public bool store;
	}

	public static async Task<string> ChatAsync(string developerPrompt, string userPrompt, CancellationToken ct)
	{
		var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
		if (string.IsNullOrEmpty(apiKey)) throw new InvalidOperationException("OPENAI_API_KEY not set");

		var req = new Request
		{
			model = "gpt-5-nano",
			input = new[]
			{
				new InputMessage {
					role = "developer",
					content = new[] { new ContentPart { type = "input_text", text = developerPrompt } }
				},
				new InputMessage {
					role = "user",
					content = new[] { new ContentPart { type = "input_text", text = userPrompt } }
				},
			},
			text = new TextFormat { format = new Format { type = "json_object" }, verbosity = "medium" },
			reasoning = new Reasoning { effort = "low" },
			tools = Array.Empty<object>(),
			store = true
		};

		var json = JsonUtility.ToJson(req);
		using (var www = new UnityWebRequest(EndPoint, UnityWebRequest.kHttpVerbPOST))
		{
			www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
			www.downloadHandler = new DownloadHandlerBuffer();
			www.SetRequestHeader("Content-Type", "application/json");
			www.SetRequestHeader("Authorization", "Bearer " + apiKey);
			www.timeout = 20;

			var op = www.SendWebRequest();
			while (!op.isDone)
			{
				if (ct.IsCancellationRequested) { www.Abort(); ct.ThrowIfCancellationRequested(); }
				await Task.Yield();
			}

			var body = www.downloadHandler.text;
			var innerJson = ExtractOutputTextLinq(body);

			if (www.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError($"{www.responseCode} {www.error}\n{body}");
			}
			return innerJson;
		}
	}

	private static string ExtractOutputTextLinq(string body)
	{
		var root = JObject.Parse(body);
        var texts = root["output"]?
            .Where(o => (string?)o["type"] == "message")
            .SelectMany(o => (JArray?)o["content"] ?? new JArray())
            .Where(c => (string?)c["type"] == "output_text")
            .Select(c => (string?)c["text"])
            .Where(t => !string.IsNullOrEmpty(t));
        return texts != null ? string.Join("\n", texts) : null;
    }
}