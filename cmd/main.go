package main

import (
	"fmt"
	"loggin/internal/handlers"
	"github.com/gin-gonic/gin"
)

func main() {

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

	fmt.Println("Servidor rodando em http://localhost:8000")
	server.Run(":8000")
}