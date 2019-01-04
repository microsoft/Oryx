import React from 'react';
import { searchImage } from '../../utils/api/create-api';

import './search-box-view.css';

const SEARCH_BOX_SIZE = 40;

export default React.createClass({
    displayName: 'search-box-view',

    propTypes: {
        placeholder: React.PropTypes.string
    },

    onKeyPressed(event) {
        if (event.key === 'Enter') {
            searchImage(event.target.value);
        }
    },

    render() {
        return (
            <div className="gs-create-search">
                <input
                    type="text"
                    className="gs-create-search-input"
                    placeholder={this.props.placeholder}
                    size={SEARCH_BOX_SIZE}
                    onKeyPress={this.onKeyPressed}
                    />
            </div>
        );
    }
});
