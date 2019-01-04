import React from 'react';

import HeaderView from '../views/header-view/header-view';
import CheckoutView from '../views/checkout-view/checkout-view';

import './base.css';

class CheckoutContainer extends React.Component {
    render() {
        return (
            <div>
                <HeaderView pageName="cart" cartCount="0"/>
                <CheckoutView />
            </div>
        );
    }
}

export default CheckoutContainer;
