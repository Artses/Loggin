package models

import "time"

type Log struct {
	ID int64
	Timestamp time.Time
	Name string
	Message string	
}