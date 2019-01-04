import React from 'react';
import Icon from 'react-fa';
import { createAddToCartAction } from '../../actions/cart-actions';

import './sticker-view.css';

export default React.createClass({

    displayName: 'sticker-view',

    propTypes: {
        createExpandItemAction: React.PropTypes.func.isRequired,
        item: React.PropTypes.object.isRequired
    },

    onAddToCartClicked() {
        createAddToCartAction(this.props.item);
    },

    onCoverartClicked() {
        this.props.createExpandItemAction(this.props.item.id);
    },

    render() {
        return (
            <div className="gs-sticker">
                <div className="gs-sticker-image"
                    style={{ backgroundImage: `url(${this.props.item.image})` }}
                    onClick={this.onCoverartClicked} ></div>
                <div className="gs-sticker-metadata">
                    <div className="gs-sticker-metadata-title">{this.props.item.name}</div>
                    <div className="gs-sticker-metadata-author">
                        <Icon name="camera" className="gs-sticker-metadata-author-icon" />
                        <div>{this.props.item.author}</div>
                    </div>
                    <div className="gs-sticker-metadata-tags">
                        <Icon name="tag" size="lg" className="gs-sticker-metadata-tags-icon" />
                        <div>{this.props.item.tags.join(', ')}</div>
                    </div>
                    <div className="gs-sticker-metadata-bottomrow">
                        <div>{this.props.item.size.width} x {this.props.item.size.height}</div>
                        <div className="gs-sticker-metadata-cart" onClick={this.onAddToCartClicked}>
                            <Icon name="shopping-cart" className="gs-sticker-metadata-cart-icon" />
                            <div>Add to cart</div>
                        </div>
                    </div>
                </div>
            </div>
        );
    }
});
