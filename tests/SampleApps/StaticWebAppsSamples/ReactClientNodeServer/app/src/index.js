import React from 'react';
import ReactDOM from 'react-dom';
import './index.css';
import logo from './logo.svg';
import './App.css';
import * as serviceWorker from './serviceWorker';
import axios from 'axios';
//import { BrowserRouter } from 'react-router-dom';

// ReactDOM.render(<App />, document.getElementById('root'));

class FetchDemo extends React.Component {
    state = {
      userName: ''
    }
  
    componentDidMount() {
      axios.get(`/api/${this.props.subreddit}`)
        .then(res => {
          console.log(res.data);
          var userName = 'Hey ' + res.data.userDetails + '!';
          this.setState({userName});
        });
    }
  
    render() {
      return (
        <div className="App">
        <header className="App-header">
          <img src={logo} className="App-logo" alt="logo" />
          <p>
            If you login with Github below, I'll say hi :)
          </p>
          <a className="App-link" href="/login"> Login with Github </a>
          <a className="App-link" href="/logout"> Logout</a>
          <p>{this.state.userName}</p>
        </header>
      </div>
      );
    }
  }
  
  ReactDOM.render(
    <FetchDemo subreddit="getList"/>,
    document.getElementById('root')
  );

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();
