package handlers

import (
	"fmt"
	"loggin/internal/services"
	"net/http"

	"github.com/gin-gonic/gin"
	"github.com/gorilla/websocket"
)

var upgrader = websocket.Upgrader{
	CheckOrigin: func(ctx *http.Request) bool {
		return true
	},
}

func WebsocketHandler(ctx *gin.Context){
	conn, err := upgrader.Upgrade(ctx.Writer, ctx.Request, nil)

	if err != nil {
		fmt.Printf("Erro ao fazer upgrade para websocket: %v\n", err)
		return 
	}
	defer conn.Close()

	for{
		mt,message,err := conn.ReadMessage()

		if err != nil{
			fmt.Printf("Erro ao ler mensagem: %v\n", err)
			break
		}

		path := (string(message))

		logs, err := services.GetLog(path)

		if err != nil {
			fmt.Printf("Erro ao abrir o arquivo de log: %v\n", err)
			break
		}

// tirar isso daq e deixar o logs.go resposavel por montar a mensagem e apenas enviar uma string para cá com a mensagem construida

		var lineCount = 0
		for line := range logs.Lines {
			if line == nil {
				continue
			}
			lineCount++
			err = conn.WriteMessage(mt, []byte(fmt.Sprintf("Linha: %v Texto: %v\n", lineCount,line.Text)))
			if err != nil {
				fmt.Printf("Erro ao escrever mensagem: %v\n", err)
				break
			}
		}
		
		if err != nil{
			fmt.Printf("Erro ao escrever mensagem: %v\n", err)
		}
	}
}
