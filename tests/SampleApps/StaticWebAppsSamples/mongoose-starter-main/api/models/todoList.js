const mongoose = require('mongoose');

const schema = new mongoose.Schema({
    name: {
        type: String,
        required: true,
    },
    description: String,
    userId: {
        type: String,
        required: true,
    },
}, {
    timestamps: {
        createdAt: "createdDate",
        updatedAt: "updatedDate"
    }
});

module.exports.TodoListModel = mongoose.model("TodoList", schema, "TodoList");