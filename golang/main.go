package main

import (
	"crypto/hmac"
	"crypto/sha256"
	"encoding/base64"
	"io"
	"log"
	"net/http"
)

func computeSignature(content, secret string) (string, error) {
	key := []byte(secret)
	h := hmac.New(sha256.New, key)

	_, err := io.WriteString(h, content)
	if err != nil {
		return "", err
	}

	hashedBytes := h.Sum(nil)
	return base64.StdEncoding.EncodeToString(hashedBytes), nil
}

func main() {
	// The secret should be stored in a secure way, this is just a sample
	secret := "The secret provided by intigriti"

	http.HandleFunc("/", func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodPost {
			w.WriteHeader(http.StatusNotFound)
			return
		}

		// Get the signature that intigriti calculated
		actualDigest := r.Header.Get("x-intigriti-digest")

		// Read the request body as json
		jsonBody, err := io.ReadAll(r.Body)
		defer func() { _ = r.Body.Close() }()

		if err != nil {
			log.Println(err.Error())
			w.WriteHeader(http.StatusBadRequest)
			return
		}

		// Recalculate the signature with the secret that intigriti provived
		expectedDigest, err := computeSignature(string(jsonBody), secret)
		if err != nil {
			log.Println(err.Error())
			w.WriteHeader(http.StatusBadRequest)
			return
		}

		// Compare the webhook digest
		if expectedDigest != actualDigest {
			w.WriteHeader(http.StatusUnauthorized)
			log.Println("Signature does not match")
			return
		}

		eventType := r.Header.Get("x-intigriti-event")

		log.Printf("Received event: %s -> %s", eventType, string(jsonBody))

		w.WriteHeader(http.StatusNoContent)
	})

	log.Println(http.ListenAndServe(":4991", nil))
}
