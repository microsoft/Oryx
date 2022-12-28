const mongoose = require('mongoose');

const schema = new mongoose.Schema({
    id: mongoose.Types.ObjectId,
    listId: {
        type: mongoose.Schema.Types.ObjectId,
        required: true
    },
    name: {
        type: String,
        required: true
    },
    userId: {
        type: String,
        required: true
    },
    description: String,
    state: {
        type: String,
        required: true,
        default: 'active'
    },
    dueDate: Date,
    completedDate: Date,
}, {
    timestamps: {
        createdAt: "createdDate",
        updatedAt: "updatedDate"
    }
});

module.exports.TodoItemModel = mongoose.model("TodoItem", schema, "TodoItem");