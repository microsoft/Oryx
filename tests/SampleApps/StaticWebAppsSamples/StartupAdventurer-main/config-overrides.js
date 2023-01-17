const path = require("path");

module.exports = function override(config, env) {
	//do stuff with the webpack config...
	config.resolve.alias = {
		"~": path.resolve("./"),
		"@": path.resolve("./src"),
	};
	return config;
};
