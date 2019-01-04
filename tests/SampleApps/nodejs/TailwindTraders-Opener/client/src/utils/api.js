import { v4 } from 'node-uuid'; // Yes this works in the browser too

// Temporary until we implement actual auth
if (!localStorage.token) {
    localStorage.token = v4();
}
const token = localStorage.token;

export function request({ url, method = 'GET', payload = {} }, cb = () => {}) {
    const req = new XMLHttpRequest();
    payload.token = token;

    req.onreadystatechange = () => {
        if (req.readyState === XMLHttpRequest.DONE) {
            if (req.status !== 200) {
                cb(`Error (${req.status}): ${req.response}`);
                return;
            }
            let response = req.response;
            try {
                response = JSON.parse(response);
            } catch (e) {
                // Squash parse errors
            }
            cb(undefined, response);
        }
    };

    console.log('Requesting: %s', url);

    if (method === 'POST' || method === 'PUT' || method === 'DELETE') {
        req.open(method, url);
        req.setRequestHeader('Content-Type', 'application/json');
        req.send(JSON.stringify(payload));
    } else if (method === 'GET') {
        const query = [];
        for (const param in payload) {
            if (payload[param]) {
                let paramValue = payload[param];
                if (Array.isArray(paramValue)) {
                    if (paramValue.length === 0) {
                        continue;
                    }
                    paramValue = paramValue.join(',');
                }
                query.push(`${param}=${paramValue}`);
            }
        }
        req.open(method, `${url}?${query.join('&')}`);
        req.send();
    } else {
        req.open(method, url);
        req.send(payload);
    }
}
