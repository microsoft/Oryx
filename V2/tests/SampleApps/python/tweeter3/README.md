tweeter3
=======

Tweeter3 is a basic example Django application that uses [Django Rest Framework](https://github.com/encode/django-rest-framework)


## Installation Instructions

1. Clone the project.
    ```shell
    $ git clone https://github.com/nnja/connect_python_azure
    ```
1. `cd` intro the project directory
    ```shell
    $ cd connect_python_azure
    ```
1. Create a new virtual environment using Python 3.7 and activate it.
    ```shell
    $ python3 -m venv env
    $ source env/bin/activate
    ```
1. Install dependencies from requirements.txt:
    ```shell
    (env)$ pip install -r requirements.txt
    ```
1. Migrate the database.
    ```shell
    (env)$ python manage.py migrate
    ```
1. *(Optionally)* load sample fixtures that will populate the database with a handful of users and tweeters.

    **Note:** If fixtures are loaded, a sample user named 'Bob' will always be logged in by default.
    ```shell
    (env)$ python manage.py loaddata initial_data
    ```
1. Run the local server via:
    ```shell
    (env)$ python manage.py runserver
    ```

### Done!
The local application will be available at <a href="http://localhost:8000" target="_blank">http://localhost:8000</a>, and the browsable api will be available at <a href="http://localhost:8000/api" target="_blank">http://localhost:8000/api</a>

If you need to update static assets, make sure to run collect static.
```shell
(env) $ python manage.py collectstatic
```

### Set up your environment variables

See `.env-sample` for more details.

The following environment variables must be present in production:

```shell
# Configure the PostgreSQL Database
DB_USER="db_user"
DB_PASSWORD="db_password"
DB_NAME="db_name"
DB_HOST="db_host"

DJANGO_SETTINGS_MODULE="tweeter3.settings.production"
SECRET_KEY="my-secret-key"

# For deployment purposes, configure the Azure App Service Hostname
AZURE_APPSERVICE_HOSTNAME="myhost"

# Optionally, configure email error reporting by setting SEND_ADMIN_EMAILS="true"
# and configuring an email server and admin email addresses.
```

Configure them in a `.env` file, and then export the secrets:
```shell
$ set -a; source .env; set +a
```

## Create a production database server, either PostgreSQL or MySQL.

### Create and configure an Azure PostgreSQL Server

```shell
# Make sure to configure a secure admin password
POSTGRES_ADMIN_PASSWORD="secret-admin-password"

az postgres server create -u tweeteruser -n $DB_HOST --sku-name B_Gen5_1 --admin-password $POSTGRES_ADMIN_PASSWORD --resource-group appsvc_rg_linux_centralus --location "Central US"

# Create a firewall rule for your local IP
# Make sure you double check the value of $MY_IP
MY_IP=$(curl -s ipecho.net/plain)

# If you get an error saying "sql: FATAL:  no pg_hba.conf entry for host", that means the firewall entry was not correct.
az postgres server firewall-rule create --resource-group appsvc_rg_linux_centralus --server-name $DB_HOST --start-ip-address=$MY_IP --end-ip-address=$MY_IP --name AllowLocalClient

# Create a firewall rule for other azure resources
az postgres server firewall-rule create --resource-group appsvc_rg_linux_centralus --server-name $DB_HOST --start-ip-address=0.0.0.0 --end-ip-address=0.0.0.0 --name AllowAllAzureIPs
```

Next, connect to the production postgres server, create the database and configure it for Django.

```shell
# Connect to the cloud based PostgreSQL database
$ PGPASSWORD=$POSTGRES_ADMIN_PASSWORD psql -v db_password="'$DB_PASSWORD'" -h $DB_HOST.postgres.database.azure.com -U tweeteruser@$DB_HOST postgres
```

Next in postgres run:
```sql
CREATE DATABASE tweeter;
CREATE USER tweeterapp WITH PASSWORD :db_password;
GRANT ALL PRIVILEGES ON DATABASE tweeter TO tweeterapp;
ALTER ROLE tweeterapp SET client_encoding TO 'utf8';
ALTER ROLE tweeterapp SET default_transaction_isolation TO 'read committed';
ALTER ROLE tweeterapp SET timezone TO 'UTC';
\q
```

Lastly, run migrations on your production database and optionally load fixtures.

```shell
# make sure all the production secrets are loaded in your current environment
set -a; source .env; set +a

# run production migrations
(env)$ python manage.py migrate

# Load fixtures, if desired.
(env)$ python manage.py loaddata initial_data
```

## Deploy to Azure App Service

### Configure Production web app settings from file

In the Visual Studio Code Azure App Service extension, create a new deployment. Select `linux` as the environment, and `Python 3.7` as the runtime.

**Note:** Don't deploy the code just yet.

Next, set up environment variables in the production App Service environment:

```shell
# Make sure environment variables are loaded, and AZURE_APPSERVICE_HOSTNAME is set to the name of your app service host.

# Optionally pipe to /dev/null to avoid printing the secret values in your terminal.
$ az webapp config appsettings set --resource-group appsvc_rg_linux_centralus --name $AZURE_APPSERVICE_HOSTNAME --settings $(grep -v '^#' .env | xargs)  > /dev/null

# To confirm the secrets were set successfully, run:
$ echo $?
# The value should be 0.
```

### Set deployment source

Next, set the deployment source to `LocalGit`.

Then, go ahead and kick off the deployment.

## (Optional) Create Azure Dev Ops CI/CD Pipeline

### Configure Azure Dev Ops from yaml file

Create two pipelines.

1. One for CI (Continuous Integration):
    - use the source yaml file: `.azure-ci-pipeline.yml`
    - set the environment variables to use for tests:
        - DJANGO_SETTINGS_MODULE=connect_python_azure.settings.development

1. One for CD (Continuous Deployment):
    - use the source yaml file: `.azure-deploy-pipeline.yml`
    - Set the following environment variables for deployment credentials:
        - DEPLOYMENT_PASSWORD
        - DEPLOYMENT_URL
        - DEPLOYMENT_USERNAME

