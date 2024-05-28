# node-js-sample "webfrontend"

A barebones Node.js app using Express 4

## Running Locally

```sh
cd \Oryx\tests\SampleApps\nodejs\webfrontend-yarnlock
yarn install
node server.js
```

Your app should now be running on [localhost:80](http://localhost:80/).

If port 80 is occupied you will get error

```sh
events.js:183
      throw er; // Unhandled 'error' event
      ^

Error: listen EACCES 0.0.0.0:80
    at Object._errnoException (util.js:992:11)
    at _exceptionWithHostPort (util.js:1014:20)
    at Server.setupListenHandle [as _listen2] (net.js:1338:19)
    at listenInCluster (net.js:1396:12)
    at Server.listen (net.js:1480:7)
    at Function.listen (c:\repo\Oryx\tests\SampleApps\nodejs\webfrontend\node_modules\express\lib\application.js:618:24)
    at Object.<anonymous> (c:\repo\Oryx\tests\SampleApps\nodejs\webfrontend\server.js:14:18)
    at Module._compile (module.js:652:30)
    at Object.Module._extensions..js (module.js:663:10)
    at Module.load (module.js:565:32)
```
Change your port to some empty port (e.g. 85) and run "node server.js" again.
App should be running on localhost:85.