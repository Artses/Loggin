package services

import (
	"loggin/internal/database"
	"github.com/nxadm/tail"
)

func GetLog() (*tail.Tail, error) {
    path := GetLogPath()
    log, err := tail.TailFile(path, tail.Config{Follow: true})
    if err != nil {
        return nil, err
    }
    return log, nil
}

func GetLogPath()string{
    db, err := database.ConnectDatabase()

    if err != nil {
        panic(err)
    }

    defer db.Close()

    row := db.QueryRow("select path from log limit 1")

    var name string
    row.Scan(&name)

    return name
}