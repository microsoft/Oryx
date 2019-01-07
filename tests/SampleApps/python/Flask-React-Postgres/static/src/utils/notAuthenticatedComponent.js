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

export function requireNoAuthentication(Component) {
  class notAuthenticatedComponent extends React.Component {
    constructor(props) {
      super(props);
      this.state = {
        loaded: false
      };
    }

    componentWillMount() {
      this.checkAuth();
    }

    componentWillReceiveProps(nextProps) {
      this.checkAuth(nextProps);
    }

    checkAuth(props = this.props) {
      if (props.isAuthenticated) {
        history.push("/dashboard");
      } else {
        const token = localStorage.getItem("token");
        if (token) {
          axios
            .post("/api/is_token_valid", {
              token
            })
            .then(res => {
              if (res.status === 200) {
                this.props.loginUserSuccess(token);
                history.push("/dashboard");
              } else {
                this.setState({
                  loaded: true
                });
              }
            });
        } else {
          this.setState({
            loaded: true
          });
        }
      }
    }

    render() {
      return (
        <div>
          {!this.props.isAuthenticated && this.state.loaded ? (
            <Component {...this.props} />
          ) : null}
        </div>
      );
    }
  }

  notAuthenticatedComponent.propTypes = {
    loginUserSuccess: PropTypes.func,
    isAuthenticated: PropTypes.bool
  };

  return connect(
    mapStateToProps,
    mapDispatchToProps
  )(notAuthenticatedComponent);
}
