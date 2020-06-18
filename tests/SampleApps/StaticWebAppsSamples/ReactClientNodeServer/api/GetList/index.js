function getUserInfo(req){
    const clientPrincipalHeader = 'x-ms-client-principal';
    if (req.headers[clientPrincipalHeader] == null) {
        return null;
    }

    const buffer = Buffer.from(req.headers[clientPrincipalHeader], 'base64');
    const serializedJson = buffer.toString('ascii');
    return JSON.parse(serializedJson);
}

module.exports = async function (context, req) {
    context.log('JavaScript HTTP trigger function processed a request.');
    
    const user = getUserInfo(req);
    if (user === null)
    {
        user = {}
    }

    context.res = {
        body: {},
        headers: {
            'Content-Type': 'application/json'
        }
    }
};
