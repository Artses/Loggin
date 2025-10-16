package services

import(
	"github.com/nxadm/tail"
)

func GetLog(path string) (*tail.Tail, error) {
    log, err := tail.TailFile(path, tail.Config{Follow: true})
    if err != nil {
        return nil, err
    }
    return log, nil
}

//func PrintaLog(*tail.Tail) return str *Fazer com que o log seja salvo 1 por um de foma modular e montar ele apenas quando for enviar pro front
