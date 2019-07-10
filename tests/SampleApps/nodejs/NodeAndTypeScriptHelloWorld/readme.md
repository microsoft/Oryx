## Skeleton for Node.js applications written in TypeScript

### Development

```bash
npm run dev
```

### Running tests

```bash
npm test
```

### Linting

```bash
npm run lint
```

### Building a container

```bash
docker build .
```

### Oryx build this app
```bash
docker run -v c:\temp\demo:/app-original:ro oryxdevmcr.azurecr.io/public/oryx/build /bin/bash -c "cp -r /app-original /app && oryx build /app"
```