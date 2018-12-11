# Overview

This is a demo application created to show how  SQL Server can operate in a DevOps scenario where an application developer can checkin code to GitHub and then trigger a build in Red Hat Open Shift to deploy the changes automatically as pods (containers).  This demo was first shown at the Nordic Infrastructure Conference (NIC) 2017 in Oslo, Norway on Feb 3, 2017.  This demo application is notable for showing a few things:
* An entrypoint CMD which executes a import-data.sh script at runtime to use sqlcmd to execute a .sql script to create a database and populate initial schema into it.
* The import-data.sh script also uses bcp to bulk import the data found in the Products.csv file.
* A simple node application that acts as a web service to get the data out of the SQL Server database using FOR JSON auto to automatically format the data into JSON and return it in the response.

**IMPORTANT:** This project has been tested with SQL Server v.Next version CTP 1.4 (March 17, 2017 release).

# Running the Demo
## Setting up the application and building the image for the first time
First, create a folder on your host and then git clone this project into that folder:
```
git clone https://github.com/twright-msft/mssql-node-docker-demo-app.git
```
To run the demo you just need to build the container:
```
docker build -t node-web-app .
```

Then, you need to run the container:
```
docker run -e ACCEPT_EULA=Y -e SA_PASSWORD=Yukon900 -p 1433:1433 -p 8080:8080 -d node-web-app
```
Note: make sure that your password matches what is in the import-data.sh script.

Then you can connect to the SQL Server in the container by running a tool on your host or you can docker exec into the container and run sqlcmd from inside the container.
```
docker exec -it <container name|ID> /bin/bash
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Yukon900
```
To show the web service response, open up a browser and point it to http://localhost:8080.

Now that you have the demo application prepared, you can setup and configure OpenShift.

## Setting up OpenShift
For demo purposes, you can deploy an OpenShift environment into any major cloud provider using templates available in the marketplaces, you can use [OpenShift Online](https://www.openshift.com/features/), or you can deploy a Virtual Box-based OpenShift environment called [minishift](https://www.openshift.org/vm/) on your local development/demo machine.

You will also need to get the `oc` commandline utility [installed](https://docs.openshift.org/latest/cli_reference/get_started_cli.html#installing-the-cli) on your host/demo machine.

Once you have OpenShift set up and you have the oc command line utility installed, you need to login to your OpenShift environment as a cluster administrator, create a new project, and set the permissions to allow any user identity to run as root (required by the mssql user that runs the sqlservr.sh script in a container for now).

```
oc login
oc new-project demo
oadm policy add-scc-to-user anyuid -z default
```
Now that you have OpenShift set up, you are ready to do an initial test run of deploying into OpenShift.

## Deploying into OpenShift

At a terminal prompt in the root of the application folder enter this command:
```
oc new-app .
```

That will deploy the app into OpenShift.  You can now log into OpenShift and see a few artifacts:
* 

# Detailed Explanation
Here's a detailed look at each of the files in the project.  

## Dockerfile
The Dockerfile defines how the image will be built.  Each of the commands in the Dockerfile is described below.

The Dockerfile can define a base image as the first layer.  In this case, the Dockerfile uses the official Microsoft SQL Server Linux image that can be found on [Docker Hub](http://hub.docker.com/r/microsoft/mssql-server-linux).  The Dockerfile will pull the image with the 'latest' tag.  This image requires two environment variables to be passed to it at run time - `ACCEPT_EULA` and `SA_PASSWORD`.  The Microsoft SQL Server Linux inmage is in turn based on the official Ubuntu Linux image `Ubuntu:16.04`.

```
FROM microsoft/mssql-server-linux:latest
```

This RUN command will update all the installed packages in the image, install the curl utility if it is not already there and then install node.

``` 
RUN apt-get -y update  && \
        apt-get install -y curl && \
        curl -sL https://deb.nodesource.com/setup_6.x | bash - && \
        apt-get install -y nodejs
```

This installs the tedious driver for SQL Server which allows node applications to connect to SQL Server and run SQL commands.  This is an open source project to which Microsoft is now one of the main contributors.

[NPM package details](https://www.npmjs.com/package/tedious)

[Source Code](https://github.com/tediousjs/tedious)

```
RUN npm install tedious
```

This RUN command creates a new directory _inside_ the container at /usr/src/app and then sets the working directory to that directory.

``` 
RUN mkdir -p /usr/src/app
WORKDIR /usr/src/app
```

Then this command copies the package.json file from the source code in this project to the /usr/src/app directory _inside_ the container.  The RUN command npm install will install all the dependencies defined in the package.json file.

```
COPY package.json /usr/src/app/
RUN npm install
```

Then all the source code from the project is copied into the container image in the /usr/src/app directory.
```
COPY . /usr/src/app
```

In order for the import-data.sh script to be executable you need to run the chmod command to add +x (execute) to the file.
```
RUN chmod +x /usr/src/app/import-data.sh
```

The EXPOSE command defines which port the application will be accessible at from outside the container.
```
EXPOSE 8080
```

Lastly, the CMD command defines what will be executed when the container starts.  In this case, it will execute the entrypoint.sh script contained in the source code for this project.  The souce code including the entrypoint.sh is contained in the /usr/src/app directory which has also been made the working directory by the commands above.
```
CMD /bin/bash ./entrypoint.sh
```

## entrypoint.sh
The entrypoint.sh script is executed when the container first starts.  The script kicks off three things _simultaneously_:
* Start SQL Serevr using the sqlservr.sh script.  This script will look for the existence of the `ACCEPT_EULA` and `SA_PASSWORD` environment variables.  Since this will be the first execution of SQL Server the SA password will be set and then the sqlservr process will be started.  Note: Sqlservr runs as a process inside of a container, _not_ as a daemon.
* Executes the import-data.sh script contained in the source code of this project.  The import-data.sh script creates a database, populates the schema and imports some data.
* Runs npm start which will start the node application.
```
/opt/mssql/bin/sqlservr.sh & /usr/src/app/import-data.sh & npm start 
```

## import-data.sh
The import-data.sh script is a convenient way to delay the execution of the SQL commands until SQL Server is started.  Typically SQL Server takes about 5-10 seconds to start up and be ready for connections and commands.  Bringing the SQL commands into a separate .sh script from entrypoint.sh creates modularity between the commands that should be run at container start up time and the SQL commands that need to be run.  It also allow for the container start up commands to be run immediately and the SQL commands to be delayed.

This command causes a wait to allow SQL Server to start up.  Nintey seconds is a bit excessive, but will ensure that even if there are extraordinary delays that the scripts will not execute until SQL Server is up.  For demo purposes you may want to reduce this number.
```
sleep 90s
```

The next command uses the SQL Server command line utility sqlcmd to execte some SQL commands contained in the setup.sql file.  The commands can also be passed directly to sqlcmd via the -q parameter.  For better readibility if you have lots of SQL commands, it's best to create a separate .sql file and put all the SQL commands in it. 

**IMPORTANT:** Make sure to change your password here if you use something other than 'Yukon900'.

```
sqlcmd -S localhost -U sa -P Yukon900 -d master -i setup.sql
```

The setup.sql script will create a new database called `DemoData` and a table called `Products` in the default `dbo` schema.  This bcp command will import the data contained in the source code file Products.csv.
**IMPORTANT:** If you change the names of the database or the table in the setup.sql script, make sure you change them here too.
**IMPORTANT:** Make sure to change your password here if you use something other than 'Yukon900'.

```
bcp DemoData.dbo.Products in "/usr/src/app/Products.csv" -c -t',' -S localhost -U sa -P Yukon900
```

## setup.sql
The setup.sql defines some simple commands to create a database and some simple schema.  You could use a .sql file like this for other purposes like creating logins, assigning permissions, creating stored procedures, and much more.  When creating a database in production situations, you will probably want to be more specific about where the database files are created so that the database files are stored in persistent storage.  This SQL script creates a table with two columns - ID (integer) and ProductName (nvarchar(max)).
```
CREATE DATABASE DemoData;
GO
USE DemoData;
GO
CREATE TABLE Products (ID int, ProductName nvarchar(max));
GO
```

## Products.csv
This CSV data file contains some sample data to populate the Products table.  It has two columns - ID and ProductName separated by a comma.  The bcp command in the import-data.sh script uses this file to import the data into the Products table created by the setup.sql script file.
```
1,Car
2,Truck
3,Motorcycle
4,Bicycle
5,Horse
6,Boat
7,Plane
8,Scooter
9,Gopher
.... more data if you want ....
```

## server.js
The server.js file defines the node application that exposes the web service and retrieves the data from SQL Server and returns it to the requestor as a JSON response.

The require statements at the top of the file bring in some libraries like tedious and express and define some global variables which can be used by the rest of the application.
```
var express = require("express");
var app = express();
var connection = require('tedious').Connection;
var request = require('tedious').Request;
```
The app.get defines the _route_ for this application.  Any GET request that comes to the root of this application will be handled by this function.  This effectively creates a simple REST-style interface for returing data in JSON from a GET request.
```
app.get('/', function (req, res) {
```

The next set of commands define the connection parameters and creates a connection object.

**IMPORTANT:** Make sure to change your password here if you use something other than 'Yukon900'.

**IMPORTANT:** If you change the names of the database in the setup.sql script, make sure you change it here to.
```
var config = {
     userName: 'sa',
     password: 'Yukon900', // update me
     server: 'localhost',
     options: {
         database: 'DemoData'
     }
}
var conn = new connection(config);
```

This next comamnd defines the event handler function for the connection.on event.
```
conn.on('connect', function(err) {
```

Assuming the connection is made correctly, the next command sets up the query that will be executed.  This uses SQL Server's built in JSON functions to retrieve the data in JSON format for us so we don't have to write code to convert the data from a traditional rowset into JSON. Nice!

[More information on JSON in SQL server](https://msdn.microsoft.com/en-us/library/dn921897.aspx)

```
sqlreq = new request("SELECT * FROM Products FOR JSON AUTO", function(err, rowCount) {
```

The next set of commands set up the event handler function for the sql request row command which will be triggered for each row in a response.  In this case there will only be a single row and a single column because we are using `FOR JSON AUTO` to get the data returned in a single string of JSON data.   Assuming the request comes back with a row and a column value we simply return the JSON string (the column.value) directly to the browser in the response (res).
```
sqlreq.on('row', function(columns) { 
   columns.forEach(function(column) {  
      if (column.value === null) {  
         console.log('NULL');
      } else {  
         res.send(column.value);
      }  
   });
});
```

This is the command that actually sends in the SQL request:
```
conn.execSql(sqlreq); 
```

This command starts the app listening on port 8080.
**IMPORTANT:** If you change the port number in the Dockerfile EXPOSE command make sure you change it here too.

```
var server = app.listen(8080, function () {
    console.log("Listening on port %s...", server.address().port);
});
```

## Alternative approach using SQL scripts for seeding schema and data

So far, the steps above have described how to create a docker image of sqlserver that starts seeding on first run.
However, if we have a huge database to be seeded, the setup.sql could contain all the 
CREATE/INSERT SQL statements that are needed for seeding the database. We do not have to import it from csv file.

You could either add the CREATE/INSERT statements to setup.sql and have those run each time a container is created or you can create a new image that has the schema and data captured inside of it.  In that case, after starting a new container and executing the .sql script, we can commit the newly running container with seeded db as a new image using "docker commit" command.
```
docker commit <container_id> <docker image tag>
```

We could now use this new image in a new docker based project including in a Docker Compose app using a docker-compose.yml file.

Also node.js dependency can be removed in this case. Node.js is only used here as an example web service to show the data can be retrieved from the SQL Server.
