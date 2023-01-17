const mongoose = require('mongoose');
const { getConfig } = require("../config/index.js");
const { TodoItemModel } = require("./todoItem.js");
const { TodoListModel } = require("./todoList.js");

async function configureMongoose(log) {
    // Configure JSON output to client
    // Removes version, sets _id to id
    mongoose.set("toJSON", {
        virtuals: true,
        versionKey: false,
        transform: (_, converted) => {
            converted.id = converted._id;
            delete converted._id;
        }
    });

    try {
        const db = mongoose.connection;
        db.on("connecting", () => log("Mongoose connecting..."));
        db.on("connected", () => log("Mongoose connected successfully!"));
        db.on("disconnecting", () => log("Mongoose disconnecting..."));
        db.on("disconnected", () => log("Mongoose disconnected successfully!"));
        db.on("error", (err) => log("Mongoose database error:", err));

        // Load configuration information
        const config = await getConfig(log);

        await mongoose.connect(
            config.database.connectionString,
            { dbName: config.database.databaseName, useNewUrlParser: true }
        );
    }
    catch (err) {
        log(`Mongoose database error: ${err}`);
        throw err;
    }
};

const itemStore = {
    list: async(userId, listId) => {
        return await TodoItemModel.find({ userId, listId });
    },
    create: async(item) => {
        return await TodoItemModel.create(item);
    },
    update: async(id, userId, item) => {
        return await TodoItemModel.updateOne({ _id: id, userId}, item);
    },
    delete: async(id, userId) => {
        return await TodoItemModel.deleteOne({ _id: id, userId });
    }
}

const listStore = {
    list: async(userId) => {
        return await TodoListModel.find({ userId }).exec();
    },
    create: async(list) => {
        return await TodoListModel.create(list);
    },
    update: async(id, userId, list) => {
        return await TodoListModel.updateOne({ _id: id, userId}, list).exec();
    },
    delete: async(id, userId) => {
        return await TodoListModel.deleteOne({ _id: id, userId }).exec();
    }
}

module.exports = {
    connect: async(log) => {
        if (mongoose.connection.readyState === 1) {
            log('Connection already established.');
            return;
        }

        await configureMongoose(log);
    },
    itemStore,
    listStore
}
