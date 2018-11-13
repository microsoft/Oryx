**How to run**  
The `Dockerfile` in this sample does a multi-stage build where its uses the build image to do a build
and then copies the output to the runtime image. It then runs the app using the installed web server in the runtime image to launch the app.
-   Build the image: `docker build -t djangoapp .`
-   Create a container: `docker run --name web -d -p 8001:5000 djangoapp`
-   Access `http://localhost:8001/boards` or `http://localhost:8001/uservoice` urls to get `Hello World!` messages from the respective app modules.
-   Static files are served through the `Whitenoise` middleware. You can visit `http://localhost:8001/staticfiles/css/boards.css` or  `http://localhost:8001/staticfiles/css/uservoice.css` to see that the css files have been 'collected' from their respective app folders into one single `staticfiles` folder.
-   You can view logs of the running container using `docker logs web`
    **Example:**  
    ```
    Collecting gunicorn
    Downloading https://files.pythonhosted.org/packages/8c/da/b8dd8deb741bff556db53902d4706774c8e1e67265f69528c14c003644e6/gunicorn-19.9.0-py2.py3-none-any.whl (112kB)
    Installing collected packages: gunicorn
    Successfully installed gunicorn-19.9.0
    [2018-11-13 21:02:21 +0000] [13] [INFO] Starting gunicorn 19.9.0
    [2018-11-13 21:02:21 +0000] [13] [INFO] Listening at: http://0.0.0.0:5000 (13)
    [2018-11-13 21:02:21 +0000] [13] [INFO] Using worker: sync
    [2018-11-13 21:02:21 +0000] [16] [INFO] Booting worker with pid: 16
    [2018-11-13 21:02:22 +0000] [17] [INFO] Booting worker with pid: 17
    [2018-11-13 21:02:22 +0000] [18] [INFO] Booting worker with pid: 18
    [2018-11-13 21:02:22 +0000] [19] [INFO] Booting worker with pid: 19
    Not Found: /
    172.17.0.1 - - [13/Nov/2018:21:02:23 +0000] "GET / HTTP/1.1" 404 2182 "-" "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36"
    172.17.0.1 - - [13/Nov/2018:21:02:27 +0000] "GET /uservoice/ HTTP/1.1" 200 58 "-" "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36"
    172.17.0.1 - - [13/Nov/2018:21:02:32 +0000] "GET /boards/ HTTP/1.1" 200 55 "-" "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36"
    172.17.0.1 - - [13/Nov/2018:21:02:43 +0000] "GET /staticfiles/css/uservoice.css HTTP/1.1" 200 0 "-" "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36"
    172.17.0.1 - - [13/Nov/2018:21:02:50 +0000] "GET /staticfiles/css/boards.css HTTP/1.1" 200 0 "-" "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36"
    ```