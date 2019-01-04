import React from 'react';

import './common.css';

export class CheckoutView extends React.Component {
    render() {
        return (
            <div className="gs-feedback">
                <form className="gs-feedback-container" action="/feedback" method="post">
                    <div className="gs-feedback-heading">
                        <img src="/img/Check-mark-large.png" />
                        <div>Your Order is Complete!</div>
                    </div>
                    <textarea placeholder="Please leave us your feedback here!" className="gs-feedback-commentarea" name="feedback" />
                    <div className="gs-feedback-submit-container">
                        <a href="/browse">Browse for more stickers</a>
                        <button type="submit">Send</button>
                    </div>
                </form>
            </div>
        );
    }
}

export default CheckoutView;
