'use strict';

import React from 'react'; // The React variable is used once the JSX is transformed into pure JS
import ReactDOM from 'react-dom';

import CartContainer from '../containers/cart-container';

ReactDOM.render(<CartContainer />, document.getElementById('content'));
