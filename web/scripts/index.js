const socket = new WebSocket("ws://localhost:8000/api/v1/wslogs");
const logsUl = document.getElementById("logs");
const pathUrl = "http://localhost:8000/api/v1/path"

socket.onopen = () => {
    var path;
    async () => {
        path = fetchLogPath()
    };
    socket.send(path);
    const li = document.createElement("li");
    logsUl.insertBefore(li, logsUl.firstChild);
};

async function fetchLogPath() {
    try {
        const response = await fetch(pathUrl);
        if (!response.ok) {
            throw new Error(`Erro: ${data.message}`);
        }
        const data = await response.json();
        return data.path;

    } catch (error) {
        console.error("Erro ao buscar o caminho do log:", error);
    }
}

socket.onmessage = function (event) {
    const li = document.createElement("li");
    li.textContent = event.data;
    logsUl.insertBefore(li, logsUl.firstChild);
};
