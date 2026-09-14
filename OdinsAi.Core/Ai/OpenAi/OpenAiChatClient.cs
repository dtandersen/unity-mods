using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OdinsAi.Tool;

namespace OdinsAi.Ai.OpenAi
{
    /// <summary>
    /// Talks to an OpenAI-compatible <c>/v1/chat/completions</c> endpoint: builds the
    /// request payload, posts it through <see cref="IChatTransport"/>, and maps the
    /// response back to an <see cref="AiCompletion"/>.
    /// </summary>
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
                throw new AiClientException($"The AI service returned HTTP {reply.StatusCode}: {reply.Body}");
            }

            return Parse(reply.Body);
        }

        private string BuildBody(IReadOnlyList<ChatMessage> messages, IReadOnlyList<ToolDefinition> tools)
        {
            var payload = new JObject
            {
                ["model"] = _options.Model,
                ["messages"] = new JArray(messages.Select(ToWireMessage)),
            };

            if (tools != null && tools.Count > 0)
            {
                payload["tools"] = new JArray(tools.Select(ToWireTool));
                payload["tool_choice"] = "auto";
            }

            return payload.ToString(Formatting.None);
        }

        private static JObject ToWireMessage(ChatMessage message)
        {
            var wire = new JObject
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
                wire["tool_calls"] = new JArray(message.ToolCalls.Select(call => new JObject
                {
                    ["id"] = call.Id,
                    ["type"] = "function",
                    ["function"] = new JObject
                    {
                        ["name"] = call.Name,
                        ["arguments"] = JsonConvert.SerializeObject(call.Arguments.Values),
                    },
                }));
            }

            return wire;
        }

        private static JObject ToWireTool(ToolDefinition tool)
        {
            var properties = new JObject();
            var required = new JArray();

            foreach (var parameter in tool.Parameters)
            {
                properties[parameter.Name] = new JObject
                {
                    ["type"] = "string",
                    ["description"] = parameter.Description,
                };

                if (parameter.Required)
                {
                    required.Add(parameter.Name);
                }
            }

            return new JObject
            {
                ["type"] = "function",
                ["function"] = new JObject
                {
                    ["name"] = tool.Name,
                    ["description"] = tool.Description,
                    ["parameters"] = new JObject
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
            JObject root;
            try
            {
                root = JObject.Parse(json);
            }
            catch (Exception ex)
            {
                throw new AiClientException("The AI service returned a malformed response.", ex);
            }

            var choices = root["choices"] as JArray;
            var message = choices != null && choices.Count > 0 ? choices[0]["message"] : null;
            if (message == null)
            {
                throw new AiClientException("The AI service response contained no message.");
            }

            return new AiCompletion(
                (string)message["content"],
                ParseToolCalls(message["tool_calls"] as JArray));
        }

        private static IReadOnlyList<ToolCall> ParseToolCalls(JArray array)
        {
            if (array == null || array.Count == 0)
            {
                return Array.Empty<ToolCall>();
            }

            var calls = new List<ToolCall>();
            foreach (var item in array)
            {
                var function = item["function"];
                var id = (string)item["id"] ?? $"call_{calls.Count + 1}";
                var name = (string)function?["name"];
                calls.Add(new ToolCall(id, name, ParseArguments((string)function?["arguments"])));
            }

            return calls;
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
                var arguments = JObject.Parse(json);
                foreach (var property in arguments.Properties())
                {
                    values[property.Name] = property.Value.Type == JTokenType.Null
                        ? null
                        : property.Value.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new AiClientException($"The AI service returned malformed tool arguments: {json}", ex);
            }

            return new ToolArguments(values);
        }
    }
}