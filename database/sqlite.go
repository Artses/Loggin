package database

import (
	 "database/sql"
	 "log"
	 "path/filepath"

	_"github.com/mattn/go-sqlite3"
)

var DB *sql.DB

func InitDatabase(filepath string) {
	var erro error
	DB, erro = sql.Open("sqlite3", filepath)
	if erro != nil {
		log.Fatalf("error in database conn: %v", erro)
	}

	if erro = DB.Ping(); erro != nil {
		log.Fatalf("error in database ping: %v", erro)
	}
	createTable()
	log.Println("database connected")
}



func createTable() error {

	sqlite := `CREATE TABLE IF NOT EXISTS logs (id INTEGER PRIMARY KEY AUTOINCREMENT, 
	timestamp DATETIME,
	 name TEXT,
	 message TEXT);`
	 stmt, erro := DB.Prepare(sqlite)


	 if erro != nil {
		log.Printf("error in prepare query %v", erro)
	    return erro
	}
	
	_, erro = stmt.Exec()
	
	if erro != nil {
		log.Printf("error in exec query %v", erro)
		return erro
	}

	log.Println("table created")
	
	defer stmt.Close()

	return nil
}
