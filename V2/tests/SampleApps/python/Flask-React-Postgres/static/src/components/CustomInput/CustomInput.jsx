import React from "react";
import classNames from "classnames";
import PropTypes from "prop-types";
// @material-ui/core components
import withStyles from "@material-ui/core/styles/withStyles";
import FormControl from "@material-ui/core/FormControl";
import Input from "@material-ui/core/Input";
// @material-ui/icons
import Clear from "@material-ui/icons/Clear";
import Check from "@material-ui/icons/Check";
// core components
import customInputStyle from "assets/jss/material-dashboard-react/components/customInputStyle.jsx";

function CustomInput({ ...props }) {
  const {
    classes,
    formControlProps,
    id,
    value,
    ref,
    inputProps,
    error,
    success
  } = props;

  const underlineClasses = classNames({
    [classes.underlineError]: error,
    [classes.underlineSuccess]: success && !error,
    [classes.underline]: true
  });

  return (
    <FormControl
      {...formControlProps}
      className={formControlProps.className + " " + classes.formControl}
    >
      <Input
        classes={{
          disabled: classes.disabled,
          underline: underlineClasses
        }}
        id={id}
        value={value}
        ref={ref} 
        disableUnderline={true}
        {...inputProps}
      />
      {error ? (
        <Clear className={classes.feedback + " " + classes.labelRootError} />
      ) : success ? (
        <Check className={classes.feedback + " " + classes.labelRootSuccess} />
      ) : null}
    </FormControl>
  );
}

CustomInput.propTypes = {
  classes: PropTypes.object.isRequired,
  id: PropTypes.string,
  value: PropTypes.string,
  inputProps: PropTypes.object,
  formControlProps: PropTypes.object,
  error: PropTypes.bool,
  success: PropTypes.bool
};

export default withStyles(customInputStyle)(CustomInput);
