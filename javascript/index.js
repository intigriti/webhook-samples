const express = require("express")
const crypto = require("crypto")
const bodyParser = require("body-parser")

const app = express()
const port = 7709

// The secret should be stored in a secure way, this is just a sample
const secret = "The secret provided by Intigriti"

app.use(bodyParser.json())
app.post("/", (req, res) => {

    // Get the signature that Intigriti calculated
    let actualDigest = req.headers["x-intigriti-digest"]

    // Recalculate the signature with the secret that Intigriti provided
    let expectedDigest = computeSignature(JSON.stringify(req.body), secret)

    // Check signature
    if (expectedDigest !== actualDigest) {
        res.status(401).end()
        return
    }
    let eventType = req.headers['x-intigriti-event']

    console.log("Received event -> " + eventType)
    console.log(req.body)

    res.status(204).end()

    function computeSignature(body, secret) {
        let hmac = crypto.createHmac('SHA256', secret)
        hmac.update(body)
        return hmac.digest().toString('base64')
    }
})

app.listen(port, () => console.log(`Server running on port ${port}`))