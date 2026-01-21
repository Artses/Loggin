const socket = new WebSocket("ws://localhost:8000/api/v1/wslogs");
const pathUrl = "http://localhost:8000/api/v1/path"
const logDiv = document.createElement("div");
const ul = document.createElement("ul");

socket.onopen = async () => {
    var path = fetchLogPath()
    socket.send(path);
};

async function fetchLogPath() { 
    try {
        const response = await fetch(pathUrl);
        if (!response.ok) {
            throw new Error(`Erro: ${data.message}`);
        }
        console.log("Resposta recebida do servidor:", response);
        const data = await response.json();
        return data.path;

    } catch (error) {
        console.error("Erro ao buscar o caminho do log:", error);
    }
}

socket.onmessage = function (event) {
    const li = document.createElement("li");
    li.textContent = event.data; 
    appendLogMessage(li);    
    ul.appendChild(li);
};

logDiv.appendChild(ul);
document.body.appendChild(logDiv);