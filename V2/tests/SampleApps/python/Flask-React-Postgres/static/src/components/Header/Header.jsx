import React from "react";
import classNames from "classnames";
import PropTypes from "prop-types";
// @material-ui/core components
import withStyles from "@material-ui/core/styles/withStyles";
import AppBar from "@material-ui/core/AppBar";
import Toolbar from "@material-ui/core/Toolbar";
import IconButton from "@material-ui/core/IconButton";
import Hidden from "@material-ui/core/Hidden";
// @material-ui/icons
import ExitToApp from "@material-ui/icons/ExitToApp";
// core components
import Button from "components/CustomButtons/Button.jsx";

import headerStyle from "assets/jss/material-dashboard-react/components/headerStyle.jsx";

function Header({ ...props }) {
  function logout(e) {
    e.preventDefault();
    props.logoutAndRedirect();
  }
  const { classes, color } = props;
  const appBarClasses = classNames({
    [" " + classes[color]]: color
  });
  return (
    <AppBar className={classes.appBar + appBarClasses}>
      <Toolbar className={classes.container}>
        <div className={classes.flex}>
        </div>
          <Button
            color={window.innerWidth > 959 ? "transparent" : "white"}
            justIcon={window.innerWidth > 959}
            simple={!(window.innerWidth > 959)}
            aria-label="Logout"
            className={classes.buttonLink}
            onClick={e => logout(e)}
          >
            {/* <p>Logout</p> */}
            <ExitToApp className={classes.icons} />
          </Button>
      </Toolbar>
    </AppBar>
  );
}

Header.propTypes = {
  classes: PropTypes.object.isRequired,
  color: PropTypes.oneOf(["primary", "info", "success", "warning", "danger"])
};

export default withStyles(headerStyle)(Header);
