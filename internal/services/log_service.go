package services

import (
	"bufio"
	"fmt"
	"io"
	"loggin/internal/database"
	"loggin/internal/models"
	"os"
	"time"
)

func GetLog(path string) ([]byte, error) {
	file, err := os.Open(path)

	if err != nil {
		return nil, err
	}

	defer file.Close()

	reader := bufio.NewReader(file)
	lineNum := 1

	for {
		var log []byte
		lineBytes, err := reader.ReadBytes('\n')

		if len(lineBytes) > 0 {
			prefix := []byte(fmt.Sprintf("Linha %d:", lineNum))

			log = append(prefix, lineBytes...)
		}

		lineNum++

		if err == io.EOF {
			time.Sleep(500 * time.Millisecond)
			continue
		}
		return log, nil
	}
}

func GetPaths() ([]models.Path, error) {
	db, err := database.ConnectDatabase()
	if err != nil {
		return nil, err
	}
	defer db.Close()

	rows, err := db.Query("SELECT id, path FROM log")
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	var paths []models.Path

	for rows.Next() {
		var p models.Path

		if err := rows.Scan(&p.ID, &p.Path); err != nil {
			return nil, err
		}

		paths = append(paths, p)
	}

	if err := rows.Err(); err != nil {
		return nil, err
	}

	return paths, nil
}

func AddLogPath(path string) error {
	db, err := database.ConnectDatabase()

	if err != nil {
		panic(err)
	}

	defer db.Close()

	_, err = db.Exec("insert into log (path) values (?)", path)
	if err != nil {
		return err
	}

	return nil
}
