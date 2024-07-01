// set your Azure OpenAI endpoint and API key
const AOAI_BASE_URL = "https://YOUR_SERVICE_NAME.openai.azure.com/";
const API_KEY = "YOUR_API_KEY";

let selectedModel = "GPT-4o";
let context = [
    {
        "role": "system",
        "content": "あなたはユーザーの質問に親切に簡潔に回答する優秀なアシスタントです"
    }
];

document.querySelectorAll(".model-button").forEach(button => {  
    button.addEventListener("click", () => {  
        selectedModel = button.getAttribute("data-model");  
        document.querySelectorAll(".model-button").forEach(btn => btn.style.backgroundColor = "#007BFF");  
        button.style.backgroundColor = "#0056b3";  
    });  
});  
  
document.getElementById("send-button").addEventListener("click", sendMessage);  
document.getElementById("user-input").addEventListener("keypress", function (e) {  
    if (e.key === "Enter") {  
        sendMessage();  
    }  
});  
  
function sendMessage() {  
    const userInput = document.getElementById("user-input").value;  
    if (!userInput) return;  
  
    addMessageToChatLog("user", userInput);  
    document.getElementById("user-input").value = "";  
  
    fetchAnswerFromAPI(userInput).then(response => {  
        addMessageToChatLog("bot", response);  
    }).catch(error => {  
        console.error("Error:", error);  
        addMessageToChatLog("bot", "エラーが発生しました。もう一度お試しください。");  
    });  
}  
  
function addMessageToChatLog(sender, message) {  
    const chatLog = document.getElementById("chat-log");  
    const messageDiv = document.createElement("div");  
    messageDiv.className = `message ${sender}`;  
    messageDiv.innerHTML = `<div class="message-content">${message}</div>`;  
    chatLog.appendChild(messageDiv);  
    chatLog.scrollTop = chatLog.scrollHeight;  
}  
  
async function fetchAnswerFromAPI(question) {  

    endpoint = AOAI_BASE_URL + 'openai/deployments/' + selectedModel + '/chat/completions?api-version=2024-02-15-preview'
    context.push(
        {
            "role": "user",
            "content": question
        }
    );

    payload = {
        "messages": context,
        "temperature": 0.7,
        "top_p": 0.95,
        "max_tokens": 800
    };

    const response = await fetch(endpoint, {  
        method: 'POST',  
        headers: {  
            'Content-Type': 'application/json',  
            'api-key': API_KEY  
        },  
        body: JSON.stringify(payload)  
    });  

    if (!response.ok) {  
        throw new Error("Network response was not ok");  
    }  
  
    const data = await response.json();
    let res_assistant = data.choices[0].message.content;    

    context.push(
        {
            "role": "assistant",
            "content": res_assistant
        }
    );

    return res_assistant;  
}  
