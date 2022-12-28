import { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Items } from './features/items/Items';
import { listAsync as getItems } from './features/items/itemSlice';
import { Lists } from './features/lists/Lists';
import { listAsync, selectActiveList } from './features/lists/listSlice';

function Display() {
    const dispatch = useDispatch();

    // Load all lists
    useEffect(() => {
        dispatch(listAsync());
    }, [dispatch]);

    // get the currently selected list
    const activeList = useSelector(selectActiveList);

    // update items for new list
    useEffect(() => {
        if (activeList && activeList.id) dispatch(getItems(activeList.id));
    }, [activeList, dispatch])

    return (
        <article className='row'>
            <section className='col-3'>
                <h2>Lists</h2>
                <Lists />
            </section>
            <section className='col-9'>
                {activeList.name ? <h2>{activeList.name} items</h2> : <div></div>}
                <Items />
            </section>
        </article>
    )
}

export default Display;