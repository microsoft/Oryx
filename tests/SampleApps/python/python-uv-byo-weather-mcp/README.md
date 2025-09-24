# Python MCP SDK Azure Functions with uv example

This example demonstrates how to set up an Azure Functions Python custom handler app that uses uv.

As of 2025-09-18, Azure Functions does not natively support uv projects because they don't contain a requirements.txt file in the root directory. Here are the limitations:

- uv is not installed in the Azure Functions environment, so the `defaultExecutablePath` in `host.json` cannot point to `uv`. It must instead be set to `python`.
- To run the function app locally, you must run it in a uv virtual environment where the dependencies are installed.
- To deploy the function app to Azure, you must create a `requirements.txt` file that contains the up-to-date dependencies.

## Prerequisites

- uv

## Running locally

### uv run

1. Run `uv run func start`
    This will create the virtual environment, install dependencies, and start the function app.

### Activating the virtual environment manually

1. Run `uv sync` to create the virtual environment and install dependencies.
1. Activate the virtual environment:
    - On Windows: `.\.venv\Scripts\activate`
    - On macOS/Linux: `source .venv/bin/activate`
1. Run `func start` to start the function app.

## Deploying to Azure

As of 2025-09-18, Azure Functions does not natively support uv projects because they don't contain a requirements.txt file in the root directory. To deploy the project, you'll need to ensure that a requirements.txt file is created and contains the up-to-date dependencies.

1. Run `uv export --format requirements-txt > requirements.txt` to create a requirements.txt file with the current dependencies.
1. Use the Azure Functions Core Tools to deploy the function app:
   ```bash
   func azure functionapp publish <YourFunctionAppName>
   ```

Ensure that you run the `uv export` command each time you update your dependencies to keep the requirements.txt file current.

