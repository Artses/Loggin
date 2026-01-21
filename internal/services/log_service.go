package services

import (
	"loggin/internal/database"
	"loggin/internal/models"

	"github.com/nxadm/tail"
)

func GetLog(path string) (*tail.Tail, error) {
	// tem que pegar a lista de path e abrir todos futuramente
	log, err := tail.TailFile(path, tail.Config{Follow: true})
	if err != nil {
		return nil, err
	}
	return log, nil
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
