package services

import (
	"loggin/internal/database"
	"github.com/nxadm/tail"
)

func GetLog() (*tail.Tail, error) {
    path := GetLogPath()
    // tem que pegar a lista de path e abrir todos futuramente
    log, err := tail.TailFile(path, tail.Config{Follow: true})
    if err != nil {
        return nil, err
    }
    return log, nil
}

func GetLogPath() string {
    db, err := database.ConnectDatabase()

    if err != nil {
        panic(err)
    }

    defer db.Close()

    row := db.QueryRow("select path from log limit 1")

    var name string
    row.Scan(&name)

    return name

    //fazer com que isto pegue todos os paths futuramente e retorne uma lista
}

func AddLogPath(path string) error {
    db, err := database.ConnectDatabase()

    if err != nil{
        panic(err)
    }

    defer db.Close()

    db.QueryRow("insert into log (path) values (?)", path)

    return nil
}