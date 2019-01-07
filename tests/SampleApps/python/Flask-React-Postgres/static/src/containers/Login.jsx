import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import * as actionCreators from "../actions/auth.jsx";
import LoginView from "../layouts/Login/Login.jsx";

const mapStateToProps = state => {
  return {
    isAuthenticating: state.auth.isAuthenticating,
    statusText: state.auth.statusText
  };
};

const mapDispatchToProps = dispatch => {
  return bindActionCreators(actionCreators, dispatch);
};

const LoginPage = connect(
  mapStateToProps,
  mapDispatchToProps
)(LoginView);

export default LoginPage;
