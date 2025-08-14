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

//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using UnityEngine;
//using UnityEngine.Networking;
//using System.IO;

//using UnityEngine.Networking;
//using System.Collections;

//// Barebones helper to call a Chat Completions endpoint using API key from environment
//// Reads OPENAI_API_KEY at call time and returns the raw response body
//public static class AiService
//{
//	private const string endPoint = "https://api.openai.com/v1/responses";

//	[Serializable]
//	private class ChatMessage
//	{
//		public string role;
//		public ChatMessageContent content;
//	}

//	[Serializable]
//	private class ChatMessageContent
//	{
//		public string type;
//		public string text;
//	}

//	[Serializable]
//	private class TextFormat
//	{
//		public TextType format;
//	}

//	[Serializable]
//	private class TextType
//	{
//		public string type;
//	}

//	[Serializable]
//	private class Request
//	{
//		public string model;
//		public List<ChatMessage> input;
//		public TextFormat text;
//	}

//	public static async Task<string> ChatAsync(string developer, string user, CancellationToken ct)
//	{
//		var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
//		if (string.IsNullOrEmpty(apiKey))
//		{
//			throw new InvalidOperationException("OPENAI_API_KEY environment variable is not set.");
//		}

//		List<ChatMessage> messages = new List<ChatMessage>();
//		messages.Add(new ChatMessage
//		{
//			role = "developer",
//			content = new ChatMessageContent
//			{
//				type = "input_text",
//				text = developer
//			},
//		});
//		messages.Add(new ChatMessage
//		{
//			role = "user",
//			content = new ChatMessageContent
//			{
//				type = "input_text",
//				text = user
//			},
//		});

//		var payload = new Request
//		{
//			model = "gpt-5-nano",
//			input = messages,
//			text = new TextFormat
//			{
//				format = new TextType
//				{
//					type = "json_object"
//                }
//            }
//		};

//		var json = JsonUtility.ToJson(payload);
//		Debug.Log($"POST {endPoint} bodyLen={json.Length}");

//		//using (var request = new UnityWebRequest(endPoint, UnityWebRequest.kHttpVerbPOST))
//		//{
//		//	byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
//		//	request.uploadHandler = new UploadHandlerRaw(bodyRaw);
//		//	request.downloadHandler = new DownloadHandlerBuffer();
//		//	request.SetRequestHeader("Content-Type", "application/json");
//		//	request.SetRequestHeader("Authorization", "Bearer " + apiKey);
//		//	request.timeout = 5; // seconds

//		//	await SendWithAwait(request, ct);

//		//	string body = request.downloadHandler.text;
//		//	if (request.result != UnityWebRequest.Result.Success)
//		//	{
//		//		Debug.LogError($"AI request failed: {request.responseCode} {request.error}\n{body}");
//		//	}
//		//	else
//		//	{
//		//		Debug.Log($"AI request ok: {request.responseCode}, bodyLen={body?.Length ?? 0}");
//		//	}
//		//	return body;
//		//}
//		using (UnityWebRequest www = UnityWebRequest.PostWwwForm(endPoint, json))
//		{
//			www.SetRequestHeader("Content-Type", "application/json");
//			www.SetRequestHeader("Authorization", "Bearer " + apiKey);
//			var op = www.SendWebRequest();

//			while (!op.isDone)
//            {
//				await Task.Yield();
//			}

//			if (www.result != UnityWebRequest.Result.Success)
//			{
//				Debug.LogError(www.error);
//				return null;
//			}
//			else
//			{
//				Debug.Log("Form upload complete! " + www.downloadHandler.text);
//				return www.downloadHandler.text;
//			}
//		}

//	}


//	public static string LoadJsonPayload(string fileName)
//	{
//		string path = Path.Combine(Application.streamingAssetsPath, fileName);
//		return File.ReadAllText(path);
//	}

//	private static async Task SendWithAwait(UnityWebRequest request, CancellationToken ct)
//	{
//		var op = request.SendWebRequest();
//		while (!op.isDone)
//		{
//			if (ct.IsCancellationRequested)
//			{
//				request.Abort();
//				ct.ThrowIfCancellationRequested();
//			}
//			await Task.Yield();
//		}
//	}

//}