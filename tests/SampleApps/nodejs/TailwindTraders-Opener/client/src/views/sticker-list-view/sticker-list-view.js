import React from 'react';
import StickerView from '../sticker-view/sticker-view';
import './sticker-list-view.css';

export default React.createClass({

    displayName: 'sticker-list-view',

    propTypes: {
        createExpandItemAction: React.PropTypes.func,
        items: React.PropTypes.arrayOf(React.PropTypes.object).isRequired
    },

    render() {
        const items = this.props.items.map((item, key) => <StickerView key={key} item={item} createExpandItemAction={this.props.createExpandItemAction} />);
        return (
            <div className="gs-stickerlist">
                {items}
            </div>
        );
    }
});
