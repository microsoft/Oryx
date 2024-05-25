import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import * as actionCreators from "../actions/data.jsx";
import Dashboard from "../layouts/Dashboard/Dashboard.jsx";

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

const DashboardPage = connect(
  mapStateToProps,
  mapDispatchToProps
)(Dashboard);

export default DashboardPage;
