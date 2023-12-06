using Microsoft.AspNetCore.Mvc;
using RestSharp;
using System.Text.Json;
using src.Models;

namespace iobcloud.chatbase.service.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class ChatBotController : ControllerBase
{
    private readonly ILogger<ChatBotController> _logger;
    private readonly string idchat = "96b2efd9-9da1-4213-b43e-2641c6e7de44";

    public ChatBotController(ILogger<ChatBotController> logger)
    {
        _logger = logger;
    }

    [HttpPost(Name = "Create")]
    public async Task<Object> Create (string chatbotname, string documentoID)
    {
        string documentText = buscarDocumento(documentoID);
        string idChatBot = criarChatBot(chatbotname, documentText);

        return idChatBot;
    }

    [HttpPost(Name = "SendMessage")]
    public async Task<Object> SendMessage (string chatbotID, string message)
    {
        string resposta = enviarMessagem(chatbotID, message);

        return resposta;
    }

    private string buscarDocumento (string idDocumento)
    {
        var options = new RestClientOptions("http://kubemaster.iob.com.br:31311/api/Documentos/v1/" + idDocumento);
        var client = new RestClient(options);
        var request = new RestRequest("");
        var response = client.Get(request);

        var document = JsonSerializer.Deserialize<Documento>(response.Content);
        return document.value.content;
    }

    private string criarChatBot (string chatbotname, string source)
    {
        var options = new RestClientOptions("https://www.chatbase.co/api/v1/create-chatbot");
        var client = new RestClient(options);
        var request = new RestRequest("");
        request.AddHeader("accept", "application/json");
        request.AddHeader("authorization", "Bearer " + idchat);
        request.AddHeader("content-type", "application/json");
        request.AddJsonBody(JsonSerializer.Serialize(new { chatbotName = chatbotname, sourceText = source }), false);
        var response = client.Post(request);

        return response.Content;
    }

    private string enviarMessagem (string chatbotID, string message)
    {
        var mensagens = new List<Mensagem>();
        mensagens.Add(new Mensagem{ role = "user", content = message});

        var options = new RestClientOptions("https://www.chatbase.co/api/v1/chat");
        var client = new RestClient(options);
        var request = new RestRequest("");
        request.AddHeader("accept", "application/json");
        request.AddHeader("authorization", "Bearer " + idchat);
        request.AddJsonBody(JsonSerializer.Serialize(new { 
            stream = false, 
            temperature = 0,
            messages = mensagens,
            chatId = chatbotID
        }), false);
        var response = client.Post(request);
        
        return response.Content;
    }
}
