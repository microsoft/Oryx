import React, { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { saveAsync, selectLists, selectActiveList, activateList } from "./listSlice";

function DisplayList(props) {
    const dispatch = useDispatch();

    const activeList = useSelector(selectActiveList);
    const [active, setActive] = useState('');
    const list = props.list;

    useEffect(() => {
        setActive(list.id === activeList.id ? 'active' : '');
    }, [activeList, list.id]);

    return (
        <button type="button" className={`list-group-item text-start list-group-item-action ${ active }`} onClick={() => dispatch(activateList(list))}>{list.name}</button>
    )
}

export function Lists() {
    const dispatch = useDispatch();
    const lists = useSelector(selectLists);
    
    const [newList, setNewList] = useState({ name: '' });

    function onSubmitNewList(event) {
        // prevent form from submitting
        event.preventDefault();
        // save data
        dispatch(saveAsync(newList));
        // reset item
        setNewList({ name: '' });
    }

    return (
        <section className="d-grid gap-4">
            <form onSubmit={onSubmitNewList} className="input-group">
                <input type="text" className="form-control" onChange={(event) => setNewList({ name: event.target.value })} value={newList.name} placeholder="create new list" />
                <button type="submit" className="btn"><i className="bi bi-save" title="save"></i></button>
            </form>

            <ul className="list-group">
                { lists.map(list => <DisplayList key={list.id} list={list} />) }
            </ul>
        </section>
    )
}