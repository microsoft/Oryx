const { getUserId } = require('../core.js');
const store = require('../models/store.js');

module.exports = async function (context, req) {
    context.log('List function called.');
    let userId = null;

    try {
        userId = getUserId(req);
        // If no current user, return 401
        if(!userId) {
            context.res.status = 401;
            return;
        }
    } catch {
        // Error, return unauthorized
        context.res.status = 401;
        return;
    }


    // set return value to JSON
    context.res = {
        header: {
            "Content-Type": "application/json"
        }
    }

    // connect to the database
    await store.connect(context.log);

    // Read the method and determine requested action
    switch (req.method) {
        case 'GET':
            // return all lists
            await getLists(context, userId);
            break;
        case 'POST':
            // create new list
            await createList(context, userId);
            break;
    }
}

async function getLists(context, userId) {
    const lists = await store.listStore.list(userId);
    context.res.body = lists;
}

async function createList(context, userId) {
    let newList = context.req.body;
    newList.userId = userId;
    newList = await store.listStore.create(newList);
    context.res.status = 201;
    context.res.body = newList;
}
