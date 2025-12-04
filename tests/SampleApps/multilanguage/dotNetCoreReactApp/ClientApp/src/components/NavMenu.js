import React, { Component } from 'react';
import { Link } from 'react-router-dom';
import './NavMenu.css';

export class NavMenu extends Component {
  static displayName = NavMenu.name;

  constructor (props) {
    super(props);

    this.toggleNavbar = this.toggleNavbar.bind(this);
    this.state = {
      collapsed: true
    };
  }

  toggleNavbar () {
    this.setState({
      collapsed: !this.state.collapsed
    });
  }

  render () {
    return (
      <header>
        <nav className="navbar navbar-expand-sm navbar-toggleable-sm navbar-light bg-white border-bottom box-shadow mb-3">
          <div className="container">
            <Link className="navbar-brand" to="/">dotNetCoreReactApp</Link>
            <button 
              className="navbar-toggler" 
              type="button" 
              onClick={this.toggleNavbar}
              aria-controls="navbarNav"
              aria-expanded={!this.state.collapsed}
              aria-label="Toggle navigation"
            >
              <span className="navbar-toggler-icon"></span>
            </button>
            <div className={`collapse navbar-collapse ${this.state.collapsed ? '' : 'show'}`} id="navbarNav">
              <ul className="navbar-nav ms-auto">
                <li className="nav-item">
                  <Link className="nav-link text-dark" to="/">Home</Link>
                </li>
                <li className="nav-item">
                  <Link className="nav-link text-dark" to="/counter">Counter</Link>
                </li>
                <li className="nav-item">
                  <Link className="nav-link text-dark" to="/fetch-data">Fetch data</Link>
                </li>
              </ul>
            </div>
          </div>
        </nav>
      </header>
    );
  }
}