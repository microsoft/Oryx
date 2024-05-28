import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import * as actionCreators from "../actions/data.jsx";
import DashboardPage from "../views/Dashboard/Dashboard.jsx";

function mapStateToProps(state) {
  return {
    data: state.data,
    token: state.auth.token,
    loaded: state.data.loaded,
    userName: state.auth.userName,
    isFetching: state.data.isFetching
  };
}

function mapDispatchToProps(dispatch) {
  return bindActionCreators(actionCreators, dispatch);
}

const Standup = connect(
  mapStateToProps,
  mapDispatchToProps
)(DashboardPage);

export default Standup;
