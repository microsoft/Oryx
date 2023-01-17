import React, { useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { selectActiveList } from "../lists/listSlice";
import { removeAsync, saveAsync, selectListItems } from "./itemSlice";

function ItemsDisplay(props) {
    const dispatch = useDispatch();
    const [newItem, setNewItem] = useState({ name: '' });

    function onSubmitNewItem(event) {
        // prevent form from submitting
        event.preventDefault();
        // set the listId
        newItem.listId = props.listId;
        // save data
        dispatch(saveAsync(newItem));
        // reset item
        setNewItem({ name: '' });
    }

    return (
        <section className="d-grid gap-2">
            <form onSubmit={onSubmitNewItem} className="input-group">
                <input type="text" className="form-control" onChange={(event) => setNewItem({ name: event.target.value })} value={newItem.name} placeholder="create new item" />
                <button type="submit" className="btn"><i className="bi bi-save" title="save"></i></button>
            </form>

            <section className="accordion" id="item-display">
                <section className="accordion-item">
                    <h2 className="accordion-header">
                        <button className="accordion-button" type="button" data-bs-toggle="collapse" data-bs-target="#active-items" aria-expanded="true" aria-controls="active-items">
                            Active items
                        </button>
                    </h2>
                    <section className="accordion-collapse collapse show" id="active-items">
                        <section className="accordion-body">
                            <ul className="list-group">
                                {props.activeItems.map(item => <ItemDisplay key={item.id} item={item} {...props} />)}
                            </ul>
                        </section>
                    </section>
                </section>
                <section className="accordion-item">
                    <h2 className="accordion-header">
                        <button className="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#done-items" aria-expanded="true" aria-controls="done-items">
                            Done items
                        </button>
                    </h2>
                    <section className="accordion-collapse collapse" id="done-items">
                        <section className="accordion-body">
                            <ul className="list-group">
                                {props.doneItems.map(item => <ItemDisplay key={item.id} item={item} {...props} />)}
                            </ul>
                        </section>
                    </section>
                </section>
            </section>
        </section>
    )
}

function ItemDisplay(props) {
    const dispatch = useDispatch();

    const [editItem, setEditItem] = useState(null);

    function onRemove(item) {
        dispatch(removeAsync(item));
    }

    function onToggleState(item) {
        const updatedItem = { ...item };
        updatedItem.state = item.state === 'active' ? 'done' : 'active';
        dispatch(saveAsync(updatedItem));
    }

    function onStartEdit(item) {
        setEditItem({ ...item });
    }

    function onEndEdit() {
        setEditItem(null);
    }

    function onSubmitEditForm(event) {
        // prevent form from submitting
        event.preventDefault();
        // call parent event handler
        dispatch(saveAsync(editItem));
        // end editing
        onEndEdit();
    }

    if (editItem && editItem.id === props.item.id) {
        // Editing this item
        return (
            <li className="list-group-item" key={props.item.id}>
                <form className="d-flex justify-content-between align-items-center input-group" onSubmit={onSubmitEditForm}>
                    <input className="form-control" type="text" value={editItem.name} onChange={(event) => setEditItem({ ...editItem, name: event.target.value })} />
                    <button type="submit" className="btn"><i className="bi bi-save" title="save"></i></button>
                    <button className="btn" onClick={() => onEndEdit()}><i className="bi bi-x" title="cancel"></i></button>
                </form>
            </li>
        )
    } else {
        // No edit item, just display it
        return (
            <li className="list-group-item d-flex justify-content-between align-items-center" key={props.item.id}>
                <span onDoubleClick={() => onStartEdit(props.item)}>{props.item.name}</span>
                <span>
                    <button className="btn" onClick={() => onToggleState(props.item)}><i className="bi bi-check-circle" title="toggle state"></i></button>
                    <button className="btn" onClick={() => onStartEdit(props.item)}><i className="bi bi-pencil" title="edit"></i></button>
                    <button className="btn" onClick={() => onRemove(props.item)}><i className="bi bi-trash" title="delete"></i></button>
                </span>
            </li>
        )
    }
}

export function Items() {
    const items = useSelector(selectListItems);
    const activeList = useSelector(selectActiveList);

    const activeItems = items.filter(i => i.state === 'active');
    const doneItems = items.filter(i => i.state === 'done');
    if (activeList.name) {
        // A list is selected, display items
        return (
            <ItemsDisplay doneItems={doneItems} activeItems={activeItems} listId={activeList.id} />
        )
    } else {
        // No list selected, display message
        return (
            <h4>Select a list from the left to display items</h4>
        )
    }
}