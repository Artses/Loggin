package main

import(
	"fmt"

	"github.com/gin-gonic/gin"
	"loggin/internal/handlers"
)

func main (){

	server := gin.Default()

	server.GET("/healthstatus", func(ctx *gin.Context){
		ctx.JSON(200, gin.H{
			"message": "i'm alive ;D",
		})
	})

	server.GET("/logs", handlers.SSEHandler)

	server.GET("/", func(ctx *gin.Context) {
		ctx.File("../web/index.html")
	})
	fmt.Println("Servidor rodando em http://localhost:8080")
	server.Run(":8080")
}