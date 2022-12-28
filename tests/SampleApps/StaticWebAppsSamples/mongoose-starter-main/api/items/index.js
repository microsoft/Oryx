const { getUserId } = require('../core.js');
const store = require('../models/store.js');

// Export our function
module.exports = async function (context, req) {
    // Get the current user
    const userId = getUserId(req);

    // If no current user, return 401
    if(!userId) {
        context.res.status = 401;
        return;
    }

    // setup our default content type (we always return JSON)
    context.res = {
        header: {
            "Content-Type": "application/json"
        }
    }

    // Connect to the database
    await store.connect(context.log);

    // Read the method and determine the requested action
    switch (req.method) {
        // If get, return all tasks
        case 'GET':
            await getItems(context, userId);
            break;
        // If post, create new task
        case 'POST':
            await createItem(context, userId);
            break;
        // If put, update task
        case 'PUT':
            await updateItem(context, userId);
            break;
        case 'DELETE':
            await deleteItem(context, userId);
            break;
    }
};

// Return all items
async function getItems(context, userId) {
    // Get the list ID from the URL
    const listId = context.bindingData.listId;
    // load all items from database filtered by userId
    const items = await store.itemStore.list(userId, listId);
    // return all items
    context.res.body = items;
}

// Create new item
async function createItem(context, userId) {
    // Read the uploaded item
    let item = context.req.body;
    // Add the userId
    item.userId = userId;
    // Add the listId
    item.listId = context.bindingData.listId;
    // Save to database
    item = await store.itemStore.create(item);
    context.log(item);
    // Set the HTTP status to created
    context.res.status = 201;
    // return new object
    context.res.body = item;
}

// Update an existing function
async function updateItem(context, userId) {
    // Grab the id from the URL (stored in bindingData)
    const id = context.bindingData.id;
    // Get the item from the body
    const item = context.req.body;
    // Get the listId from the requeest
    item.listId = context.bindingData.listId;
    // Add the userId
    item.userId = userId;
    // Update the item in the database
    const result = await store.itemStore.update(id, userId, item);
    // Check to ensure an item was modified
    if (result.nModified === 1) {
        context.res.body = item;
    } else {
        // Item not found, status 404
        context.res.status = 404;
    }
}

async function deleteItem(context, userId) {
    const id = context.bindingData.id;
    const result = await store.itemStore.delete(id, userId);
    context.res = {id};
}
