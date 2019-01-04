// @ts-nocheck
import React from 'react';
import classname from 'classname';
import Icon from 'react-fa';
import PropTypes from 'prop-types';

import './header-view.css';

export default React.createClass({
    displayName: 'header-view',

    propTypes: {
        pageName: PropTypes.string.isRequired,
        cartCount:PropTypes.number.isRequired
    },

    render() {
        return (
            <div className="gs-header">
                <div className="gs-header-decorator">
                    <a href="/"><img src="/img/Logo.png" /></a>
                    <div className="margin-left">
                        <a href="/cart" className={classname({
                            'gs-header-navbar-cart': this.props.pageName !== 'cart',
                            'gs-header-navbar-cart-active': this.props.pageName === 'cart'
                        })}>
                            <Icon name="shopping-cart" className="gs-header-navbar-cart-icon" />
                            <div>View Cart ({this.props.cartCount})</div>
                        </a>
                    </div>
                </div>
            </div>
        );
    }
});
