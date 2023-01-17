// Get current user
function getUserId(req) {
    // Retrieve client info from request header
    const header = req.headers['x-ms-client-principal'];
    // The header is encoded in Base64, so we need to convert it
    const encoded = Buffer.from(header, 'base64');
    // Convert from Base64 to ascii
    const decoded = encoded.toString('ascii');
    // Convert to a JSON object and return the userId
    return JSON.parse(decoded).userId;
}
exports.getUserId = getUserId;
