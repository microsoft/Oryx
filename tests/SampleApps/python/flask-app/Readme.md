**How to run**  
The `Dockerfile` in this sample does a multi-stage build where its uses the build image to do a build
and then copies the output to the runtime image. It then runs the app using the installed web server in the runtime image to launch the app.
-   Build the image: `docker build -t basicflaskappsample .`
-   Create a container: `docker run --name web -d -p 8000:5000 basicflaskappsample`
-   Access `http://localhost:8000` and you should see something like `Hello World!`
-   You can view logs of the running container using `docker logs web`
    **Example output:**  
    ```
    Collecting gunicorn
    Downloading https://files.pythonhosted.org/packages/8c/da/b8dd8deb741bff556db53902d4706774c8e1e67265f69528c14c003644e6/gunicorn-19.9.0-py2.py3-none-any.whl (112kB)
    Installing collected packages: gunicorn
    Successfully installed gunicorn-19.9.0
    [2018-11-13 20:23:25 +0000] [11] [INFO] Starting gunicorn 19.9.0
    [2018-11-13 20:23:25 +0000] [11] [INFO] Listening at: http://0.0.0.0:5000 (11)
    [2018-11-13 20:23:25 +0000] [11] [INFO] Using worker: sync
    [2018-11-13 20:23:25 +0000] [14] [INFO] Booting worker with pid: 14
    [2018-11-13 20:23:25 +0000] [15] [INFO] Booting worker with pid: 15
    [2018-11-13 20:23:25 +0000] [16] [INFO] Booting worker with pid: 16
    [2018-11-13 20:23:25 +0000] [17] [INFO] Booting worker with pid: 17
    172.17.0.1 - - [13/Nov/2018:20:23:33 +0000] "GET / HTTP/1.1" 200 39 "-" "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36"
    172.17.0.1 - - [13/Nov/2018:20:23:44 +0000] "GET /foo HTTP/1.1" 404 233 "-" "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36"
    ```


