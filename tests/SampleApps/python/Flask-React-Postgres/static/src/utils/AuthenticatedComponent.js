import React from "react";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import history from "../history/history";
import * as actionCreators from "../actions/auth.jsx";
import PropTypes from "prop-types";
import axios from "axios";

function mapStateToProps(state) {
  return {
    token: state.auth.token,
    userName: state.auth.userName,
    isAuthenticated: state.auth.isAuthenticated
  };
}

function mapDispatchToProps(dispatch) {
  return bindActionCreators(actionCreators, dispatch);
}

export function requireAuthentication(Component) {
  class AuthenticatedComponent extends React.Component {
    componentWillMount() {
      this.checkAuth();
      this.setState({
        loaded_if_needed: false
      });
    }

    componentWillReceiveProps(nextProps) {
      this.checkAuth(nextProps);
    }

    checkAuth(props = this.props) {
      if (!props.isAuthenticated) {
        const token = localStorage.getItem("token");
        if (!token) {
          history.push("/");
        } else {
          axios
            .post("/api/is_token_valid", {
              token
            })
            .then(res => {
              if (res.status === 200) {
                this.props.loginUserSuccess(token);
                this.setState({
                  loaded_if_needed: true
                });
              } else {
                history.push("/");
              }
            });
        }
      } else {
        this.setState({
          loaded_if_needed: true
        });
      }
    }

    render() {
      return (
        <div>
          {this.props.isAuthenticated && this.state.loaded_if_needed ? (
            <Component {...this.props} />
          ) : null}
        </div>
      );
    }
  }

  AuthenticatedComponent.propTypes = {
    loginUserSuccess: PropTypes.func,
    isAuthenticated: PropTypes.bool
  };

  return connect(
    mapStateToProps,
    mapDispatchToProps
  )(AuthenticatedComponent);
}
