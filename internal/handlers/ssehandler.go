package handlers

import (
	"fmt"
	"net/http"
	"time"

	"github.com/gin-gonic/gin"
)


func SSEHandler(ctx *gin.Context){
	ctx.Header("Content-Type", "text/event-stream")
	ctx.Header("Cache-Control", "no-cache")
	ctx.Header("Connection", "keep-alive")
	ctx.Header("Pragma", "no-cache")

	flusher, ok := ctx.Writer.(http.Flusher)

	if !ok {
		ctx.String(http.StatusInternalServerError, "Streaming não suportado")
		return
	}
	
	for i:= 0; i < 100; i++{
		fmt.Fprintf(ctx.Writer, "data: %d\n\n", i)
		time.Sleep(10 * time.Second)
		flusher.Flush()
	}
}