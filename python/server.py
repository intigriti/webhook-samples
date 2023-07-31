from flask import Flask
from flask import request
import hmac
import hashlib
import base64
import logging

app = Flask(__name__)

logging.basicConfig(level=logging.INFO)

# The secret should be stored in a secure way, this is just a sample
secret = "The secret provided by intigriti"

def compute_signature(content, secret):
    secret_bytes = secret.encode('utf-8')
    hmacsha256 = hmac.new(secret_bytes, content.encode('utf-8'), hashlib.sha256)
    hashed_bytes = hmacsha256.digest()
    return base64.b64encode(hashed_bytes).decode('utf-8')


@app.route("/", methods=['POST'])
def receive_event():

    # Get the signature that Intigriti calculated
    actualDigest = request.headers['x-intigriti-digest'] 

    # Read the request body as utf-8 string
    json = request.data.decode('utf-8')

    # Recalculate the signature with the secret that Intigriti provided
    expectedDigest = compute_signature(json, secret)

    # Check signature
    if(actualDigest != expectedDigest) :
        return "Unauthorized", 401
    
    eventType = request.headers['x-intigriti-event'] 

    app.logger.info("Received event -> %s \n%s", eventType, json)

    return "", 204


app.run(host="localhost", port=4991)