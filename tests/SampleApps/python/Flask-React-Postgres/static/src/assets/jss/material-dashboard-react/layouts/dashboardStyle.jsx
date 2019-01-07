import {
  transition,
} from "assets/jss/material-dashboard-react.jsx";

const appStyle = theme => ({
  wrapper: {
    position: "relative",
    top: "0",
    height: "100vh",
    overflow: "auto",
    float: "right",
    ...transition,
    maxHeight: "100%",
    width: "70%",
    overflowScrolling: "touch"
  },
  content: {
    marginTop: "70px",
    padding: "30px 15px",
    minHeight: "calc(100vh - 123px)"
  },
  container: {
    marginTop: "70px"
  }
});

export default appStyle;
