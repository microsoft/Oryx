//@ts-nocheck

import React from 'react';

import './empty-cart-view.css';

export default React.createClass({

    displayName: 'empty-cart-view',

    render() {
        return (
            <div className="gs-cartview-empty">
                <div className="gs-cartview-empty-tagline">Hello</div>
                <img src="/img/Computer-with-stickers.png" />
                <a className="gs-cartview-empty-browse" href="/browse">Browse Gnomes</a>
            </div>
        );
    }
});
