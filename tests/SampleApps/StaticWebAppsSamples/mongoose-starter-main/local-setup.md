# Local setup

To support running locally, the starter project is configured with a [dev container](https://code.visualstudio.com/docs/remote/create-dev-container?WT.mc_id=academic-45074-chrhar). The container has the following resources:

- Node.js
- Azure Functions Core Tools
- MongoDB

To run the project, you will need the following:

- [Docker](https://docs.docker.com/engine/install/)
- [Visual Studio Code](https://code.visualstudio.com?WT.mc_id=academic-45074-chrhar)
- [Remote - Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers&WT.mc_id=academic-45074-chrhar)

## Setup

1. Clone the repository you created earlier when [deploying to Azure](https://docs.microsoft.com/azure/static-web-apps/add-mongoose?WT.mc_id=academic-45074-chrhar), or [create a copy from the template](https://github.com/login?return_to=/staticwebdev/mongoose-starter/generate) and clone your copy

  ```bash
  git clone <YOUR REPOSITORY URL HERE>
  cd <YOUR REPOSITORY NAME>
  ```

1. Open the project in Visual Studio Code

  ```bash
  code .
  ```

1. When prompted inside Visual Studio Code, select **Reopen in Container**. The container will build, and Visual Studio Code wil refresh.

1. Inside Visual Studio Code, open a terminal window by selecting **View** > **Terminal**, and execute the following code to install the packages and run the site

  ```bash
  npm dev:install
  npm run dev
  ```

  Your project will now start!

1. Navigate to [http://localhost:4280](http://localhost:4280) to use your site

> **NOTE** You might be prompted to open a different port. The Azure Static Web Apps CLI will host the project on port [4280](http://localhost:4280).
