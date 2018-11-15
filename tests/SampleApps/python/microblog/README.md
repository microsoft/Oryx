# Welcome to Microblog!

This is an example application featured in my [Flask Mega-Tutorial](https://blog.miguelgrinberg.com/post/the-flask-mega-tutorial-part-i-hello-world). See the tutorial for instructions on how to work with it.

## Database servers

**MySql**  
docker run --name mysql -d -e MYSQL_RANDOM_ROOT_PASSWORD=yes -e MYSQL_DATABASE=microblog -e MYSQL_USER=microblog -e MYSQL_PASSWORD=Passw0rd! mysql/mysql-server:5.7

**Postgres**  
docker run --name postgres -d -e POSTGRES_DB=microblog -e POSTGRES_USER=microblog -e POSTGRES_PASSWORD=Passw0rd! postgres

**Microsoft SQL Server**  
docker run --name mssql -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=Passw0rd!' -p 1433:1433 -d microsoft/mssql-server-linux:2017-CU12

## Run Web app
docker run --name web -d -p 8000:5000 -e SECRET_KEY=my-secret-key -e MAIL_SERVER=smtp.googlemail.com -e MAIL_PORT=587 -e MAIL_USE_TLS=true -e MAIL_USERNAME=kiranmicroblog -e MAIL_PASSWORD=OryxPassw0rd! --link postgres:dbserver -e DATABASE_URL=postgresql+psycopg2://microblog:Passw0rd!@dbserver/microblog microblog

**Note:** Change the '--link' and 'DATABASE_URL' values according to the database server you are testing against.  

**Database Urls for different database servers**  
-   mysql+pymysql://microblog:Passw0rd!@dbserver/microblog
-   postgresql+psycopg2://microblog:Passw0rd!@dbserver/microblog
-   mssql+pyodbc://Larry:Passw0rd!@dbserver/microblog?driver=ODBC+Driver+17+for+SQL+Server

