package main

import (
	"fmt"
	"github.com/gin-gonic/gin"
	"loggin/internal/database"
	"loggin/internal/handlers"
)

func main() {
	database.ConnectDatabase()
	server := gin.Default()

	server.GET("/api/v1/healthstatus", func(ctx *gin.Context) {
		ctx.JSON(200, gin.H{
			"message": "i'm alive ;D",
		})
	})

	server.GET("/api/v1/logs", func(ctx *gin.Context) {
		handlers.WebsocketHandler(ctx)
	})

	fmt.Println("Servidor rodando em http://localhost:8000")
	server.Run(":8000")
}
