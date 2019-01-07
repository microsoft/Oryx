import React from "react";
import PropTypes from "prop-types";
import TextField from "@material-ui/core/TextField";
import Button from "@material-ui/core/Button";
import Paper from "@material-ui/core/Paper";
import { validateEmail } from "../../utils/misc";

import * as actionCreators from "actions/auth.jsx";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";

const style = {
  marginTop: 50,
  paddingBottom: 50,
  paddingTop: 25,
  width: "100%",
  textAlign: "center",
  display: "inline-block"
};

function mapStateToProps(state) {
  return {
    isRegistering: state.auth.isRegistering,
    registerStatusText: state.auth.registerStatusText,
  };
}

function mapDispatchToProps(dispatch) {
  return bindActionCreators(actionCreators, dispatch);
}

class RegisterPage extends React.Component {
  constructor(props) {
    super(props);
    const redirectRoute = "/dashboard";
    this.state = {
      email: "",
      password: "",
      first_name: "",
      last_name: "",
      email_error_text: null,
      password_error_text: null,
      redirectTo: redirectRoute,
      disabled: true
    };
  }

  isDisabled() {
    let email_is_valid = false;
    let password_is_valid = false;

    if (this.state.email === "") {
      this.setState({
        email_error_text: null
      });
    } else if (validateEmail(this.state.email)) {
      email_is_valid = true;
      this.setState({
        email_error_text: null
      });
    } else {
      this.setState({
        email_error_text: "Sorry, this is not a valid email"
      });
    }

    if (this.state.password === "" || !this.state.password) {
      this.setState({
        password_error_text: null
      });
    } else if (this.state.password.length >= 6) {
      password_is_valid = true;
      this.setState({
        password_error_text: null
      });
    } else {
      this.setState({
        password_error_text: "Your password must be at least 6 characters"
      });
    }

    if (email_is_valid && password_is_valid) {
      this.setState({
        disabled: false
      });
    }
  }

  changeValue(e, type) {
    const value = e.target.value;
    const next_state = {};
    next_state[type] = value;
    this.setState(next_state, () => {
      this.isDisabled();
    });
  }

  _handleKeyPress(e) {
    if (e.key === "Enter") {
      if (!this.state.disabled) {
        this.register(e);
      }
    }
  }

  register(e) {
    e.preventDefault();

    this.props.registerUser(
      this.state.first_name,
      this.state.last_name,
      this.state.email,
      this.state.password,
      this.state.redirectTo
    )
  }

  render() {
    return (
      <div
        className="col-md-6 col-md-offset-3"
        onKeyPress={e => this._handleKeyPress(e)}
      >
        <Paper style={style}>
          <form>
            <div className="text-center">
              <h2>Register</h2>
              {this.props.statusText && (
                <div className="alert alert-info">{this.props.statusText}</div>
              )}
              
              <div className="col-md-12">
                <TextField
                  label="First Name"
                  type="text"
                  onChange={e => this.changeValue(e, "first_name")}
                />
              </div>
              <div className="col-md-12">
                <TextField
                  label="Last Name"
                  type="text"
                  onChange={e => this.changeValue(e, "last_name")}
                />
              </div>
              <div className="col-md-12">
                <TextField
                  label="Email"
                  type="email"
                  onChange={e => this.changeValue(e, "email")}
                />
              </div>
              <div className="col-md-12">
                <TextField
                  label="Password"
                  type="password"
                  onChange={e => this.changeValue(e, "password")}
                />
              </div>

              <Button
                variant="contained"
                disabled={this.state.disabled}
                style={{ marginTop: 50 }}
                onClick={e => this.register(e)}
              >
                Submit
              </Button>
            </div>
          </form>
        </Paper>
      </div>
    );
  }
}

RegisterPage.propTypes = {
  loginUser: PropTypes.func,
  statusText: PropTypes.string
};

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(RegisterPage);