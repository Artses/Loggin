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
