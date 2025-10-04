package main

import (
	"fmt"
	"github.com/gin-gonic/gin"
	"log"
	"loggin/database"
	"loggin/internal/handlers"
	"loggin/internal/models"
	"time"
)

func main() {

	// funções para database sqlite
	//inicializar o banco de dados
	database.InitDatabase("logs.db")
	//fechar o banco de dados ao fim da aplicação
	defer database.CloseDatabase()

	

	//inserir log no banco de dados
	logInserido := models.Log{
		Timestamp: time.Now(),
		Name:      "SYSTEM LOG",
		Message:   "Log de sistema inserido no banco de dados",
	}

	erro := database.InsertLog(logInserido)
	if erro != nil {
		log.Printf("error inserting log: %v", erro)
	} else {
		log.Println("log inserted successfully")
	}

	// fim funções para database sqlite

	server := gin.Default()

	server.GET("/healthstatus", func(ctx *gin.Context) {
		ctx.JSON(200, gin.H{
			"message": "i'm alive ;D",
		})
	})

	server.GET("/wslogs", func(ctx *gin.Context) {
		handlers.WebsocketHandler(ctx)
	})

	server.GET("/", func(ctx *gin.Context) {
		ctx.File("../web/index.html")
	})

	fmt.Println("Servidor rodando em http://localhost:8000")
	server.Run(":8000")
}
