import React from 'react';

import './common.css';
import './feedback-view.css';

class FeedbackView extends React.Component {
    render() {
        return (
            <div className="gs-feedback">
                <div className="gs-feedback-container">
                    <div className="gs-feedback-heading">
                        <img src="/img/Check-mark-large.png" />
                        <div>Thanks for your feedback!</div>
                    </div>
                    <a href="/browse" className="gs-feedback-browse-button">
                        <div>Browse More Stickers</div>
                    </a>
                </div>
            </div>
        );
    }
}

export default FeedbackView;
