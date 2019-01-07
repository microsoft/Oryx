import React from "react";
import PropTypes from "prop-types";
import withStyles from "@material-ui/core/styles/withStyles";
import UserCard from "components/UserCard.jsx";

import dashboardStyle from "assets/jss/material-dashboard-react/views/dashboardStyle.jsx";

import GridContainer from "components/Grid/GridContainer.jsx";


class Dashboard extends React.Component {
  componentDidMount() {
    this.fetchData();
  }

  fetchData() {
    const token = this.props.token;
    this.props.fetchProtectedData(token);
  }
  render() {
    return (
      <GridContainer>
        {!this.props.loaded ? (
          <h1>Loading data...</h1>
        ) : (
          [
            (!this.props.data.user_has_tasks ? 
              <UserCard canEdit={true} email={this.props.userName} name={this.props.data.data.first_name + " " + this.props.data.data.last_name} tasks={[]}/> :
            <span/> ),
            this.props.data.users.map(([key, value]) => (
              <UserCard
                canEdit={this.props.userName === key}
                email={key}
                name={value[0].first_name + " " + value[0].last_name}
                tasks={value}
              />
            ))
          ]
        )}
      </GridContainer>
    );
  }
}

Dashboard.propTypes = {
  classes: PropTypes.object.isRequired,
  userName: PropTypes.string,
  fetchProtectedData: PropTypes.func
};

export default withStyles(dashboardStyle)(Dashboard);
