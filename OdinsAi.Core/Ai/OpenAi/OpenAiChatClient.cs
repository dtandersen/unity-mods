using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OdinsAi.Tool;

namespace OdinsAi.Ai.OpenAi
{
    /// <summary>
    /// Talks to an OpenAI-compatible <c>/v1/chat/completions</c> endpoint: builds the
    /// request payload, posts it through <see cref="IChatTransport"/>, and maps the
    /// response back to an <see cref="AiCompletion"/>.
    /// </summary>
    /// <remarks>
    /// Deliberately avoids <c>JObject</c>/<c>JArray</c> building and the
    /// <c>JToken.ToString(Formatting)</c> overload: Unity's linker strips unused
    /// Newtonsoft members from the game's copy, and that overload is missing at
    /// runtime. Plain dictionaries + typed DTOs only use the core
    /// <see cref="JsonConvert"/> surface the game itself relies on.
    /// </remarks>
    public sealed class OpenAiChatClient : IAiClient
    {
        private readonly IChatTransport _transport;
        private readonly OpenAiOptions _options;

        public OpenAiChatClient(IChatTransport transport, OpenAiOptions options)
        {
            _transport = transport;
            _options = options;
        }

        public async Task<AiCompletion> CompleteAsync(
            IReadOnlyList<ChatMessage> messages,
            IReadOnlyList<ToolDefinition> tools,
            CancellationToken cancellationToken = default)
        {
            var request = new HttpRequest(
                _options.Endpoint,
                new Dictionary<string, string> { ["Authorization"] = $"Bearer {_options.ApiKey}" },
                BuildBody(messages, tools));

            HttpReply reply;
            try
            {
                reply = await _transport.PostAsync(request, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AiClientException("Could not reach the AI service.", ex);
            }

            if (reply.StatusCode < 200 || reply.StatusCode >= 300)
            {
                throw new AiClientException(
                    $"The AI service returned HTTP {reply.StatusCode}: {Truncate(reply.Body, 300)}");
            }

            return Parse(reply.Body);
        }

        private static string Truncate(string value, int max)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= max)
            {
                return value;
            }

            return value.Substring(0, max) + "...(truncated)";
        }

        private string BuildBody(IReadOnlyList<ChatMessage> messages, IReadOnlyList<ToolDefinition> tools)
        {
            var payload = new Dictionary<string, object>
            {
                ["model"] = _options.Model,
                ["messages"] = messages.Select(ToWireMessage).ToList(),
            };

            if (tools != null && tools.Count > 0)
            {
                payload["tools"] = tools.Select(ToWireTool).ToList();
                payload["tool_choice"] = "auto";
            }

            return JsonConvert.SerializeObject(payload);
        }

        private static Dictionary<string, object> ToWireMessage(ChatMessage message)
        {
            var wire = new Dictionary<string, object>
            {
                ["role"] = message.Role.ToString().ToLowerInvariant(),
            };

            if (message.Content != null)
            {
                wire["content"] = message.Content;
            }

            if (message.ToolCallId != null)
            {
                wire["tool_call_id"] = message.ToolCallId;
            }

            if (message.ToolCalls != null && message.ToolCalls.Count > 0)
            {
                wire["tool_calls"] = message.ToolCalls.Select(call => new Dictionary<string, object>
                {
                    ["id"] = call.Id,
                    ["type"] = "function",
                    ["function"] = new Dictionary<string, object>
                    {
                        ["name"] = call.Name,
                        ["arguments"] = JsonConvert.SerializeObject(call.Arguments.Values),
                    },
                }).ToList();
            }

            return wire;
        }

        private static Dictionary<string, object> ToWireTool(ToolDefinition tool)
        {
            var properties = new Dictionary<string, object>();
            var required = new List<string>();

            foreach (var parameter in tool.Parameters)
            {
                properties[parameter.Name] = new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = parameter.Description,
                };

                if (parameter.Required)
                {
                    required.Add(parameter.Name);
                }
            }

            return new Dictionary<string, object>
            {
                ["type"] = "function",
                ["function"] = new Dictionary<string, object>
                {
                    ["name"] = tool.Name,
                    ["description"] = tool.Description,
                    ["parameters"] = new Dictionary<string, object>
                    {
                        ["type"] = "object",
                        ["properties"] = properties,
                        ["required"] = required,
                    },
                },
            };
        }

        private static AiCompletion Parse(string json)
        {
            ChatResponse response;
            try
            {
                response = JsonConvert.DeserializeObject<ChatResponse>(json);
            }
            catch (Exception ex)
            {
                throw new AiClientException("The AI service returned a malformed response.", ex);
            }

            var message = response?.Choices != null && response.Choices.Count > 0
                ? response.Choices[0]?.Message
                : null;

            if (message == null)
            {
                throw new AiClientException("The AI service response contained no message.");
            }

            return new AiCompletion(message.Content, ParseToolCalls(message.ToolCalls));
        }

        private static IReadOnlyList<ToolCall> ParseToolCalls(List<ResponseToolCall> calls)
        {
            if (calls == null || calls.Count == 0)
            {
                return Array.Empty<ToolCall>();
            }

            var result = new List<ToolCall>();
            for (var i = 0; i < calls.Count; i++)
            {
                var call = calls[i];
                var id = string.IsNullOrEmpty(call.Id) ? $"call_{i + 1}" : call.Id;
                result.Add(new ToolCall(id, call.Function?.Name, ParseArguments(call.Function?.Arguments)));
            }

            return result;
        }

        private static ToolArguments ParseArguments(string json)
        {
            var values = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(json))
            {
                return new ToolArguments(values);
            }

            try
            {
                var parsed = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (parsed != null)
                {
                    foreach (var pair in parsed)
                    {
                        values[pair.Key] = pair.Value?.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new AiClientException($"The AI service returned malformed tool arguments: {json}", ex);
            }

            return new ToolArguments(values);
        }

        private sealed class ChatResponse
        {
            [JsonProperty("choices")]
            public List<ChatChoice> Choices { get; set; }
        }

        private sealed class ChatChoice
        {
            [JsonProperty("message")]
            public ResponseMessage Message { get; set; }
        }

        private sealed class ResponseMessage
        {
            [JsonProperty("content")]
            public string Content { get; set; }

            [JsonProperty("tool_calls")]
            public List<ResponseToolCall> ToolCalls { get; set; }
        }

        private sealed class ResponseToolCall
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("function")]
            public ResponseFunction Function { get; set; }
        }

        private sealed class ResponseFunction
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("arguments")]
            public string Arguments { get; set; }
        }
    }
}