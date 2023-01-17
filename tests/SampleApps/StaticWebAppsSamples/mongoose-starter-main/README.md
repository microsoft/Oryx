# Static Web Apps - Mongoose starter

[![Playwright tests](https://github.com/staticwebdev/mongoose-starter/actions/workflows/playwright-dashboard.yml/badge.svg)](https://github.com/staticwebdev/mongoose-starter/actions/workflows/playwright-dashboard.yml)

This template is designed to be a starter for creating [React](https://reactjs.org) apps using [Azure Static Web Apps](https://docs.microsoft.com/azure/static-web-apps/overview?WT.mc_id=academic-45074-chrhar), with [Azure Cosmos DB API for Mongo DB](https://docs.microsoft.com/azure/cosmos-db/mongodb/mongodb-introduction?WT.mc_id=academic-45074-chrhar) as a database and a [Mongoose](https://mongoosejs.com/) client. It's built with the following:

- Azure resources
  - [Azure Static Web Apps](https://docs.microsoft.com/azure/static-web-apps/overview?WT.mc_id=academic-45074-chrhar)
  - [Azure Cosmos DB API for Mongo DB](https://docs.microsoft.com/azure/cosmos-db/mongodb/mongodb-introduction?WT.mc_id=academic-45074-chrhar)
- Application libraries
  - [React](https://reactjs.org/) and [Redux Toolkit](https://redux-toolkit.js.org/)
  - [Mongoose](https://mongoosejs.com/)
  - [Azure Functions](https://docs.microsoft.com/azure/azure-functions/functions-overview?WT.mc_id=academic-45074-chrhar)
- Development libraries
  - [Azure Static Web Apps CLI](https://docs.microsoft.com/azure/static-web-apps/local-development?WT.mc_id=academic-45074-chrhar)

## Azure deployment

Please refer to the [documentation](https://docs.microsoft.com/azure/static-web-apps/add-mongoose?WT.mc_id=academic-45074-chrhar) for information on creating the appropriate server resources and deploying the project.

> **Important**
>
> Two environmental variables are required for the project:
>
> - `AZURE_COSMOS_CONNECTION_STRING`: The connection string to the database server
> - `AZURE_COSMOS_DATABASE_NAME`: The name of the database
>
> These can be stored in [application settings](https://docs.microsoft.com/azure/static-web-apps/add-mongoose?WT.mc_id=academic-45074-chrhar#configure-database-connection-string) in Azure Static Web Apps. When developing locally, the project will default to using MongoDB running on localhost.

## Local development

You can run the project locally using containers by following the [local setup instructions](./local-setup.md).

## Project structure

This starter project is designed to be a template for React with Redux and hosted on Azure Static Web Apps. It uses the [Redux Toolkit](https://redux-toolkit.js.org/). The project is a Todo application using authentication for Azure Static Web Apps. Todo items are collected in lists, and are scoped to individual users. To use the application:

1. Login using GitHub by clicking the login link.
1. Create a list.
1. Create todo items for the list.

### package.json scripts

- **dev**: Starts the SWA CLI, Azure Functions and the React app. The application will be available on [http://localhost:4280](http://localhost:4280)

### src/app

Contains [store.js](./src/app/store.js), which manages global state for the application.

### src/features

Contains three "features", one each for [items](./src/features/items/), [lists](./src/features/lists/) and [user](./src/features/user/). *lists* and *items* contain a [slice](https://redux-toolkit.js.org/api/createSlice) to manage their respective state and a React component.

### api

Root folder for Azure Functions. All [new serverless functions](https://docs.microsoft.com/azure/static-web-apps/add-api?tabs=react#create-the-api?WT.mc_id=academic-45074-chrhar) are added to this directory.

#### api/config

Contains the configuration for the database. Two environmental variables are required for the project:

- `AZURE_COSMOS_CONNECTION_STRING`: The connection string to the database server
- `AZURE_COSMOS_DATABASE_NAME`: The name of the database

These can be stored in [application settings](https://docs.microsoft.com/azure/static-web-apps/add-mongoose?WT.mc_id=academic-45074-chrhar#configure-database-connection-string) in Azure Static Web Apps. When developing locally, the project will default to using MongoDB running on localhost. You can change the default values by updating *default.json*.

#### api/models

Contains the two Mongoose models, TodoItemModel and TodoListModel. It also contains *store.js*, which exposes helper functions for [CRUD](https://en.wikipedia.org/wiki/Create,_read,_update_and_delete) operations.
