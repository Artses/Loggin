const socket = new WebSocket("ws://localhost:8000/api/v1/wslogs");
const pathUrl = "http://localhost:8000/api/v1/path"
const ul = document.createElement("ul");
var count = 0
socket.onopen = async () => {
    const res = await fetchLogPath();

    res.paths.forEach(element => {
        socket.send(element.Path);
        console.log(element.Path)
    });
};

async function fetchLogPath() { 
    try {
        const response = await fetch(pathUrl);

        if (!response.ok) {
            throw new Error(`Erro: ${response.status}`);
        }

        const data = await response.json();
        return data

    } catch (error) {
        console.error("Erro ao buscar o caminho do log:", error);
    }
}

socket.onmessage = function (event) {
    const li = document.createElement("li");
    li.textContent = event.data; 
    ul.appendChild(li);
};

for (let i = 0; i < count; i++ ){
    const logDiv = document.createElement("div");
    logDiv.appendChild(ul);
    document.body.appendChild(logDiv);
}