package database

import (
	"database/sql"
	"log"

	_ "github.com/lib/pq"
)

var DB *sql.DB

func ConnectDatabase() (*sql.DB, error) {
	db, err := sql.Open("postgres", "postgres://postgres:password@localhost:5432/postgres?sslmode=disable")
	if err != nil {
		log.Fatalf("não foi possivel se conectar ao banco de dados: %v", err)
		return nil, err
	}
	return db, nil
}