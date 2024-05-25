import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import * as actionCreators from "../actions/auth.jsx";
import Header from "../components/Header/Header.jsx";

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

const HeaderView = connect(
  mapStateToProps,
  mapDispatchToProps
)(Header);

export default HeaderView;
