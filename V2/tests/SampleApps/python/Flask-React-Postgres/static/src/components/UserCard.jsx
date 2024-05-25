import React from "react";
import { COMPLETED, IN_PROGRESS } from "../constants/index";
import moment from "moment";

import List from "@material-ui/icons/List";
import CheckCircleOutline from "@material-ui/icons/CheckCircleOutline";
import AssignmentLate from "@material-ui/icons/AssignmentLate";
// core components
import GridItem from "components/Grid/GridItem.jsx";
import CustomTabs from "components/CustomTabs/CustomTabs.jsx";

import * as actionCreators from "actions/data.jsx";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";

function mapStateToProps(state) {
  return {
    token: state.auth.token
  };
}

function mapDispatchToProps(dispatch) {
  return bindActionCreators(actionCreators, dispatch);
}

class UserCard extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      slideIndex: 0
    };

    this.date =
      props.tasks.length > 0
        ? moment.utc(props.tasks[0].date).format("MMMM DD, YYYY")
        : moment().format("MMMM DD, YYYY");
    
    this.state = {
        tasks: this.props.tasks
    }
  }

  updateTaskIdCallback(task, id) {
    task.id = id;
    this.state.tasks.push(task);
    this.updateTasks();
  }

  addTask(task_val, status) {
    const newTask = { id: null, task: task_val, date: new Date(), status: status };
    this.props.storeTask(this.props.token, this.props.email, newTask, this.updateTaskIdCallback.bind(this));
  }

  deleteTask(task) {
    this.props.deleteTask(this.props.token, task.id);

    const currentIndex = this.state.tasks.indexOf(task);
    this.state.tasks.splice(currentIndex, 1);
    this.updateTasks();
  }

  editTask(task, task_val, status) {
    this.props.editTask(this.props.token, task.id, task_val, status);

    const index = this.state.tasks.indexOf(task);
    this.state.tasks[index].status = status;
    this.state.tasks[index].task = task_val;
    this.updateTasks();
  }

  updateTasks() {
    this.setState({
        tasks: this.state.tasks
    })
  }

  render() {
    return (
        <GridItem xs={12} sm={12} md={8}>
          <CustomTabs
            tasks={this.state.tasks}
            email={this.props.email}
            canEdit={this.props.canEdit}
            addTask={this.addTask.bind(this)}
            deleteTask={this.deleteTask.bind(this)}
            editTask={this.editTask.bind(this)}
            updateTasks={this.updateTasks.bind(this)}

            title={this.props.name}
            headerColor="primary"
            tabs={[
              {
                tabName: "All",
                tabIcon: List,
                newTasksCompleted: false,
                tasksToShow: [COMPLETED, IN_PROGRESS]
              },
              {
                tabName: "Completed",
                tabIcon: CheckCircleOutline,
                newTasksCompleted: true,
                tasksToShow: [COMPLETED]
              },
              {
                tabName: "TODO",
                tabIcon: AssignmentLate,
                newTasksCompleted: false,
                tasksToShow: [IN_PROGRESS]
              }
            ]}
          />
        </GridItem>
    );
  }
}

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(UserCard);