const dotenv = require("dotenv");
const config = require("config");

module.exports.getConfig = async (log) => {
    // Load any ENV vars from local .env file
    if (process.env.NODE_ENV !== "production") {
        dotenv.config();
    }

    // load database configuration
    const databaseConfig = config.get("database");
    log("Database config loaded");
    log(databaseConfig);
    return {
        database: {
            connectionString: databaseConfig.connectionString,
            databaseName: databaseConfig.databaseName,
        },
    };
};
