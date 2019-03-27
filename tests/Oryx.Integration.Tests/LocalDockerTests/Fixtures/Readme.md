### NOTES:
1. Database server related containers are created only once for different database types (i.e. there's only one
   container for MySQL, one container for PostgreSQL, etc.)
2. All database server types have the same table structure and sample data setup. This is so that we can reuse
   lot of code and make tests more readable and easy to manage.
3. All the samples used in these tests ONLY read data. Since the objective is to just make sure an app is able
   to connect to database, we want to keep things simple. If we introduced 'write' operation too, then sharing
   a database instance across multiple tests is error-prone.
