// @ts-nocheck
import React from 'react';
import Icon from 'react-fa';
import { createAddToCartAction } from '../../actions/cart-actions';

import './expanded-item-view.css';

export default React.createClass({

    displayName: 'expanded-item-view',

    propTypes: {
        createCloseExpandedItemAction: React.PropTypes.func.isRequired,
        item: React.PropTypes.object.isRequired
    },

    onCloseClicked() {
        this.props.createCloseExpandedItemAction();
    },

    onAddToCartClicked() {
        createAddToCartAction(this.props.item);
        this.props.createCloseExpandedItemAction();
    },

    render() {
        // TODO: replace the shopping cart with higher res version
        return (
            <div className="gs-expandeditemview">
                <div className="gs-expandeditemview-container">
                    <div className="gs-expandeditemview-header">
                        <div className="gs-expandeditemview-header-title">{this.props.item.name}</div>
                        <Icon name="close" size="2x" className="gs-expandeditemview-header-close" onClick={this.onCloseClicked} />
                    </div>
                    <div className="gs-expandeditemview-imagecontainer">
                        <div className="gs-expandeditemview-image" style={{ backgroundImage: `url(${this.props.item.image})` }} ></div>
                    </div>
                    <div className="gs-expandeditemview-checkout" onClick={this.onAddToCartClicked}>
                        <Icon name="shopping-cart" className="gs-expandeditemview-checkout-icon" />
                        <span>Add to cart</span>
                    </div>
                </div>
            </div>
        );
    }
});
