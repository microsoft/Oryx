
def app(environ, start_response):
    data = b"Hello Gunicorn!\n"
    start_response("200 OK", [
        ("Content-Type", "text/plain"),
        ("Content-Type", str(len(data)))
    ])
    return iter([data])
