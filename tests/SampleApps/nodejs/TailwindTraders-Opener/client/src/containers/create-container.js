import React from 'react';
import { Container } from 'flux/utils';

import HeaderView from '../views/header-view/header-view';
import SearchBoxView from '../views/search-box-view/search-box-view';
import StickerListView from '../views/sticker-list-view/sticker-list-view';
import ExpandedItemView from '../views/expanded-item-view/expanded-item-view';

import createStore from '../stores/create-store';
import cartStore from '../stores/cart-store';

import { createExpandItemAction, createCloseExpandedItemAction } from '../actions/create-actions';

import './base.css';

class CreateContainer extends React.Component {
    render() {
        let expandedItem;
        if (this.state.create.expandedItem) {
            expandedItem = <ExpandedItemView item={this.state.create.expandedItem} createCloseExpandedItemAction={createCloseExpandedItemAction}/>;
        }
        return (
            <div>
                <HeaderView pageName="create" cartCount={this.state.cart.items.length}/>
                <SearchBoxView placeholder={this.state.create.defaultKeyword} />
                <StickerListView items={this.state.create.items} createExpandItemAction={createExpandItemAction} />
                {expandedItem}
            </div>
        );
    }
}

CreateContainer.getStores = () => [ createStore, cartStore ];

CreateContainer.calculateState = () => ({
    create: createStore.getState(),
    cart: cartStore.getState()
});

export default Container.create(CreateContainer);
