package main
import (
	"fmt"
	"strings"
	"net/http"
	"io/ioutil"
)

func main() {

	url := "https://h.api.redlink.com.ar/redlink/homologacion/enlacepagosseg/0/0/34/sesion/385"

	payload := strings.NewReader("{\"credenciales\":{\"usuario\":\"500\",\"email\":\"mauromarozzi@cormoran.com.ar\",\"contrasena\":\"Prueba123\"}}")

	req, _ := http.NewRequest("POST", url, payload)

	req.Header.Add("x-ibm-client-id", "eb1741fd-7117-4554-93e7-361804b34030")
	req.Header.Add("cliente", "200.2.127.227")
	req.Header.Add("requerimiento", "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789/*-")
	req.Header.Add("content-type", "application/json")
	req.Header.Add("accept", "application/json")

	res, _ := http.DefaultClient.Do(req)

	defer res.Body.Close()
	body, _ := ioutil.ReadAll(res.Body)

	fmt.Println(res)
	fmt.Println(string(body))


}