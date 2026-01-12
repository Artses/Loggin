package main

import (
	"fmt"
	"loggin/internal/database"
	"loggin/internal/handlers"
	"loggin/internal/services"

	"github.com/gin-gonic/gin"
)

func main() {
	database.ConnectDatabase()
	server := gin.Default()

	server.GET("/healthstatus", func(ctx *gin.Context){
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

	server.GET("/path", func(ctx *gin.Context) {
		ctx.JSON(200, gin.H{
			"path": services.GetLogPath(),
		})
	})

	fmt.Println("Servidor rodando em http://localhost:8000")
	server.Run(":8000")
}