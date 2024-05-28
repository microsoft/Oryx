# Flask + React + Postgres Starter 

This is a minimal sample Flask and React starter code that demonstrates how both frameworks can be used together in a single page web Application.

The code is based on https://github.com/dternyak/React-Redux-Flask.

## Tutorial

## 1. Setting Up The Project

1. Clone the reponsitory
```bash
git clone [TODO INSERT URL]
cd flask-react-postgres
```

2. Install requirements.txt
```bash
pip install -r requirements.txt
```

3. Import the project folder into VS Code
```bash
code .
```

## 2. Running The Code Locally

1. Build the react.js front-end.
```bash
npm install
npm run build
```
2. Create the PostgreSQL database
```bash
python manage.py create_db
```
3. Start the Flask server
```bash
python manage.py runserver
```
4. Check ```localhost:5000``` in your browser to view the web application.

## 3. Deploying The Code To Azure

1. Go to the extensions tab on VS Code

2. Install the recommended extensions that show up 

3. Access Azure through (1) Guest Mode, (2) Creating a free Azure account or (3) signing into Azure with an existing account

4. Create an App Service instance

5. Create a PostgreSQL database with Azure Database for Postgres and connect it to the App Service instance

6. Deploy the code to your newly created App Service instance


# Contributing

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Feedback

* Request a new feature on [GitHub](CONTRIBUTING.md).
* File a bug in [GitHub Issues](https://) [TODO FIX LINK].
* [Tweet](https://twitter.com/microsoft) us with any other feedback.

## Bundled Extensions

The code ships with a set of recommended Visual Studio Code extensions that will empower the developement process of your Flask + React web application. These extensions include rich language support (code completion, go to definition) for both Python and JavaScript, as well as quick deploy to Azure from within VS Code. When the project is imported into VS Code, a notifcation will appear giving you the option to install these extensions. 

List of bundled extensions:

* [Python Extension for Visual Studio Code](https://marketplace.visualstudio.com/items?itemName=ms-python.python)
* [Azure App Service Extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azureappservice)

## License

Copyright (c) Microsoft Corporation. All rights reserved.

Licensed under the [MIT](LICENSE.txt) License.
